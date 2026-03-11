using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIlinker : MonoBehaviour
{
    private Canvas targetCanvas;        // 현재 씬의 Canvas
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene,LoadSceneMode mode)
    {
            targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas != null)
            {
                this.transform.SetParent(targetCanvas.transform, false);
            }
        
    }
}
