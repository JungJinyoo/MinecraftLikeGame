using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    //AudioSource[] audio;
    public AudioSource sfx;
    public AudioSource bgm;

    public AudioClip[] sfxaudioclip;
    public AudioClip[] LobbyBGMaudioclip;
    public AudioClip[] GameBGMaudioclip;
    public AudioClip[] Nowplaylist;
    private int index=0;
    public SliderCtrl sliderCtrl;
    //추가
    public GameObject myui;
    private Canvas targetCanvas;
    void Start()
    {
        PlayLobbyBGM();
    }

    void Awake()
    {
        //audio = GetComponents<AudioSource>();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void sfxPlayOneShot(AudioClip clip)
    {
        sfx.PlayOneShot(clip);
    }

    void btnSoundplay()
    {
        sfx.Play();
    }
    public void playRandsound()
    {
        int num = Random.Range(0, sfxaudioclip.Length);
        sfx.clip = sfxaudioclip[num];
        btnSoundplay();
    }
    public void PlayLobbyBGM()
    {
        Nowplaylist = LobbyBGMaudioclip;
        index = 0;
        PlayNext();
    }
    public void PlayGameBGM()
    {
        Nowplaylist = GameBGMaudioclip;
        index = 0;
        PlayNext();
    }

    void PlayNext()
    {
        if (Nowplaylist.Length == 0) return;

        bgm.clip = Nowplaylist[index];
        bgm.Play();

        StartCoroutine(WaitSong());
    }
    IEnumerator WaitSong()
    {
        yield return new WaitForSeconds(bgm.clip.length);

        index = (index + 1) % Nowplaylist.Length;
        PlayNext();
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 이름으로 구분
        if (scene.name == "scLOBY")
        {
            PlayLobbyBGM();
        }
        else if (scene.name == "Minecrafttest")
        {
            PlayGameBGM();                 
        }
        Button[] btns = FindObjectsOfType<Button>();

        foreach (Button b in btns)
        {
            b.onClick.AddListener(() => SoundManager.Instance.btnSoundplay());
        }
        if(scene.name=="Minecrafttest")
        {
            //캔버스 때오기용
            targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas != null)
            {
                if(myui!=null)
                myui.transform.SetParent(targetCanvas.transform, false);
            }
        }
        foreach (Button b in btns)
        {
            b.onClick.AddListener(() => SoundManager.Instance.btnSoundplay());
        }

    }
    //마스터
    public void SetMasterVolume()
    {
        AudioListener.volume = sliderCtrl.sliders[0].slider.value;
    }

    // 브금
    public void SetBGMVolume()
    {
        bgm.volume = sliderCtrl.sliders[1].slider.value;;
    }

    // sfx
    public void SetSFXVolume()
    {
        sfx.volume = sliderCtrl.sliders[2].slider.value;;
    }
    //사운드캔버스ui 때오기용
    public void keepui()
    {
        myui.SetActive(true);
        myui.transform.SetParent(null, false);
        DontDestroyOnLoad(myui);
        myui.SetActive(false);
        Debug.Log("떄오기");
    }


    
}
