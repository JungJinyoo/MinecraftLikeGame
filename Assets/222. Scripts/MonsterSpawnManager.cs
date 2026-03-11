using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterSpawnManager : MonoBehaviour
{
    [Header("스폰 기준 플레이어")]
    public Transform player;

    [Header("몬스터 프리팹 이름들 (야간 스폰)")]
    public List<string> monsterPrefabNames = new List<string>();

    [Header("동물 프리팹 이름들 (항상 스폰)")]
    public List<string> animalPrefabNames = new List<string>();

    [Header("몬스터 스폰 설정")]
    public float monsterSpawnInterval = 5f;
    public int maxMonsters = 20;

    [Header("동물 스폰 설정")]
    public float animalSpawnInterval = 15f;
    public int maxAnimals = 10;

    [Header("스폰 거리 제한")]
    public float minDistance = 15f;
    public float maxDistance = 30f;

    private float monsterTimer = 0f;
    private float animalTimer = 0f;

    PhotonView pv;

    void Start()
    {
        pv = GetComponent<PhotonView>();

        if (!pv.IsMine)
        {
            enabled = false;
            return;
        }
    }

    void Awake()
    {
        player = transform.parent.transform;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        MonsterSpawnLogic();  
        AnimalSpawnLogic();
    }

    // -------------------------------
    // 🧟 몬스터 스폰 (밤 전용)
    // -------------------------------
    void MonsterSpawnLogic()
    {
        if (!MinecraftDayNightCycle.IsNight) return;

        monsterTimer += Time.deltaTime;
        if (monsterTimer < monsterSpawnInterval) return;

        int count = GameObject.FindGameObjectsWithTag("Monster").Length;
        if (count >= maxMonsters)
        {
            monsterTimer = 0f;
            return;
        }

        Vector3 pos = GetValidSpawnPosition(player.position);

        if (monsterPrefabNames.Count > 0)
        {
            string prefab = monsterPrefabNames[Random.Range(0, monsterPrefabNames.Count)];
            PhotonNetwork.Instantiate(prefab, pos, Quaternion.identity);
        }

        monsterTimer = 0f;
    }

    // -------------------------------
    // 🐑 동물 스폰 (낮/밤 상관 없음)
    // -------------------------------
    void AnimalSpawnLogic()
    {
        animalTimer += Time.deltaTime;
        if (animalTimer < animalSpawnInterval) return;

        int count = GameObject.FindGameObjectsWithTag("Animal").Length;
        if (count >= maxAnimals)
        {
            animalTimer = 0f;
            return;
        }

        Vector3 pos = GetValidSpawnPosition(player.position);

        if (animalPrefabNames.Count > 0)
        {
            string prefab = animalPrefabNames[Random.Range(0, animalPrefabNames.Count)];
            PhotonNetwork.Instantiate(prefab, pos, Quaternion.identity);
        }

        animalTimer = 0f;
    }

    // -------------------------------
    // 공통 스폰 위치 계산
    // -------------------------------
    Vector3 GetValidSpawnPosition(Vector3 center)
    {
        Vector3 pos;

        for (int i = 0; i < 20; i++)
        {
            Vector2 rnd2D = Random.insideUnitCircle.normalized * Random.Range(minDistance, maxDistance);

            pos = new Vector3(
                center.x + rnd2D.x,
                center.y + 20f,
                center.z + rnd2D.y
            );

            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 100f))
            {
                Vector3 finalPos = hit.point;

                if (finalPos.y > center.y + 15f || finalPos.y < center.y - 20f)
                    continue;

                return finalPos;
            }
        }

        return center + new Vector3(0, 2, maxDistance);
    }
}
