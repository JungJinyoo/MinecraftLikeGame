using UnityEngine;
using UnityEngine.Rendering;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;   // Hashtable

public class MinecraftDayNightCycle : MonoBehaviourPunCallbacks
{
    public Material skyMat;
    public Light directionalLight;

    const float dayLengthSeconds = 1200f;
    float timeOfDay = 0f;
    public float timeSpeed = 20f;
    public Material blockmat;

    [Range(0f, 1f)] public float minNightBlend = 0.0f;
    [Range(0f, 1f)] public float maxNightBlend = 1.0f;


    // 밤으로 취급할 기준 ( 0 ~ 1 )
    [Range(0f, 1f)] public float nightBlendThreshold = 0.7f;  // 추가


    float initialSunX;



    // 외부에서 읽을 수 있는 현재 Blend 값 / 밤 여부
    public static float CurrentBlend { get; private set; } // 추가
    public static bool IsNight { get; private set; }


    //  방 공용으로 쓰는 키
    const string ROOM_DAY_START_KEY = "ROOM_DAY_START"; // 추가


    // 이 방의 낮/밤 시작 기준 (PhotonNetwork.Time 기준)
    double roomStartTime = 0; //    추가


    void Start()
    {
        initialSunX = directionalLight.transform.rotation.eulerAngles.x;

        // 기본 환경광 모드
        RenderSettings.ambientMode = AmbientMode.Flat;

        // Fog 활성화
        RenderSettings.fog = true;

        InitRoomStartTime();
    }


    // 방에 들어왔을 때도 다시 한번 시도 (씬이 나중에 로드될 수도 있으니까)
    public override void OnJoinedRoom()
    {
        InitRoomStartTime();
    }


   void InitRoomStartTime()
{
    if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
    {
        roomStartTime = Time.time;
        return;
    }

    var props = PhotonNetwork.CurrentRoom.CustomProperties;

    if (props.TryGetValue(ROOM_DAY_START_KEY, out object v))
    {
        roomStartTime = (double)v;
    }
    else if (PhotonNetwork.IsMasterClient)
    {
        // 마스터만 기준 시간 설정
        roomStartTime = PhotonNetwork.Time;

        Hashtable ht = new Hashtable();
        ht[ROOM_DAY_START_KEY] = roomStartTime;
        PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
    }
}


    void Update()
    {
        //  네트워크 시간 기반으로 timeOfDay 계산
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            double elapsed = PhotonNetwork.Time - roomStartTime;  // 방이 시작된 후 경과 시간(초)
            timeOfDay = (float)(elapsed * timeSpeed);

            // 0 ~ 24000 안에서 반복
            if (timeOfDay >= 24000f)
                timeOfDay %= 24000f;
        }               // if 부분 추가된것
        else
        {
            // 오프라인일 때는 그냥 기존 방식
            timeOfDay += Time.deltaTime * timeSpeed;
            if (timeOfDay >= 24000f) timeOfDay = 0f;
        }

        float blend = CalculateSkyBlend(timeOfDay);



        // static 프로퍼티에 저장
        CurrentBlend = blend;
        IsNight = (blend >= nightBlendThreshold);       // 추가



        skyMat.SetFloat("_Blend", blend);

        float worldBrightness = Mathf.Lerp(1f, 0.25f, blend);
        blockmat.SetFloat("_Brightness", worldBrightness);

        UpdateSunRotation();
        UpdateSunLightIntensity(blend);
        UpdateAmbientLight(blend);
        UpdateFog(blend);
    }

    void UpdateSunRotation()
    {
        float mcAngle = (timeOfDay / 24000f) * 360f - 90f;
        float sunAngle = initialSunX + mcAngle;

        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 0f, 0f);
    }

    void UpdateSunLightIntensity(float blend)
    {
        float dayIntensity = 1.0f;
        float nightIntensity = 1.0f;//0.08f; //밤에도 최소한의 빛 유지

        directionalLight.intensity = Mathf.Lerp(dayIntensity, nightIntensity, blend);
    }

    void UpdateAmbientLight(float blend)
    {
        // 낮 환경광
        Color dayAmbient = new Color(0.75f, 0.80f, 0.95f);

        // 밤 환경광
        Color nightAmbient = new Color(0.05f, 0.10f, 0.15f);

        RenderSettings.ambientLight = Color.Lerp(dayAmbient, nightAmbient, blend);
    }

    void UpdateFog(float blend)
    {
        // 낮 안개
        Color dayFog = new Color(0.75f, 0.85f, 1f);

        // 밤 안개
        Color nightFog = new Color(0.05f, 0.05f, 0.1f);

        RenderSettings.fogColor = Color.Lerp(dayFog, nightFog, blend);
        RenderSettings.fogDensity = Mathf.Lerp(0.002f, 0.01f, blend);
    }

    float CalculateSkyBlend(float t)
    {
        if (t < 11000f) return minNightBlend;
        if (t < 13000f) return Mathf.Lerp(minNightBlend, maxNightBlend, (t - 11000f) / 2000f);
        if (t < 23000f) return maxNightBlend;
        return Mathf.Lerp(maxNightBlend, minNightBlend, (t - 23000f) / 1000f);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
{
    if (propertiesThatChanged.ContainsKey(ROOM_DAY_START_KEY))
    {
        roomStartTime = (double)propertiesThatChanged[ROOM_DAY_START_KEY];
    }
}
}
