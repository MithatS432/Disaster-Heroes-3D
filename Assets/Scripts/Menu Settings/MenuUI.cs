using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    public Slider volumeSlider;
    public Slider effectSlider;

    public bool isPaused = false;

    public GameObject pauseMenu;
    public GameObject optionsMenu;

    void Start()
    {
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);

        volumeSlider.value = AudioManager.MasterVolume;
        effectSlider.value = EffectManager.EffectQuality;

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ContinueGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        pauseMenu.SetActive(true);
        CursorManager.Instance.UnlockCursor();
    }

    public void SetVolume(float value)
    {
        AudioManager.SetVolume(value);
    }
    public void SetEffectQuality(float value)
    {
        EffectManager.SetEffectQuality(value);
    }


    public void ContinueGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        CursorManager.Instance.LockCursor();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !isPaused)
        {
            CursorManager.Instance.LockCursor();
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}