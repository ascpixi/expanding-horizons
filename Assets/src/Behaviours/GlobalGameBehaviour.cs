using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalGameBehaviour : MonoBehaviour
{
    static GlobalGameBehaviour self;
    static bool frozen;

    void Awake()
    {
        if (self != null)
            throw new Exception("There can be no more than two GlobalGameBehaviour components in a single scene.");

        self = this;
    }

    void Start()
    {
        Debug.Log($"(scene changed: {SceneManager.GetActiveScene().name})");

        if (LevelData.InLevel && LevelData.Current.FadeIn) {
            TransitionManager.FadeIn();
        }
    }
    
    void Update()
    {
#if UNITY_STANDALONE
        if (Input.GetButtonDown("Toggle Fullscreen")) {
            ToggleFullscreen();
        }
#endif
    }
    
    void OnApplicationQuit() => DOTween.KillAll();
    
    public static bool Frozen {
        get => frozen;
        set {
            if (value && !frozen) {
                DOTween.PauseAll();
            }
            else if(!value && frozen) {
                DOTween.PlayAll();
            }

            frozen = value;
        }
    }
    
    /// <summary>
    /// Loads the specified scene by its build index.
    /// </summary>
    /// <param name="levelIndex">The target scene's level index.</param>
    public static void LoadScene(int levelIndex)
    {
        self.StartCoroutine(self.LoadLevelCoroutine(levelIndex));
    }

    /// <summary>
    /// Attempts to load the next scene.
    /// </summary>
    public static void LoadNextScene()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if(nextIndex >= SceneManager.sceneCountInBuildSettings) {
            Debug.LogError($"Attempted to load the next scene ({nextIndex}) while no such scene exists");
            return;
        }

        LoadScene(nextIndex);
    }

    /// <summary>
    /// Attempts to load the previous scene.
    /// </summary>
    public static void LoadPreviousScene()
    {
        int prevIndex = SceneManager.GetActiveScene().buildIndex - 1;
        if (prevIndex >= SceneManager.sceneCountInBuildSettings) {
            Debug.LogError($"Attempted to load the previous scene ({prevIndex}) while no such scene exists");
            return;
        }

        LoadScene(prevIndex);
    }

    /// <summary>
    /// Reloads the current scene.
    /// </summary>
    public static void RestartCurrentScene() => LoadScene(SceneManager.GetActiveScene().buildIndex);
    
    IEnumerator LoadLevelCoroutine(int levelIndex)
    {
        TransitionManager.FadeOut();

        var asyncLoad = SceneManager.LoadSceneAsync(levelIndex);
        asyncLoad!.allowSceneActivation = false;
        
        yield return TransitionManager.WaitForCompletion();
        
        Frozen = false; // reset freeze state on scene change
        asyncLoad.allowSceneActivation = true;
    }
    
    
#if UNITY_STANDALONE
    void ToggleFullscreen()
    {
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) {
            // For windowed mode, ensure that the screen has at least 50px of padding.
            var res = Screen.currentResolution;
            if (res.width - 100 < Screen.width || res.height - 100 < Screen.height) {
                Screen.SetResolution(
                    (int)Screen.safeArea.width - 100, (int)Screen.safeArea.height - 100,
                    FullScreenMode.Windowed
                );
                
                var display = Screen.mainWindowDisplayInfo;
                Screen.MoveMainWindowTo(in display, new(25, 75));
            }
            else {
                Screen.fullScreenMode = FullScreenMode.Windowed;
            }
        }
        else {
            Screen.SetResolution(
                Display.main.systemWidth, Display.main.systemHeight,
                FullScreenMode.FullScreenWindow
            );
        }
    }
#endif
}
