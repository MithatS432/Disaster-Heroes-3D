using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    [Header("Cursor")]
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (cursorTexture != null)
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);

        LockCursor();
    }

    void Update()
    {
        if (Cursor.visible && Input.GetMouseButtonDown(0))
        {
            MenuUI menu = FindFirstObjectByType<MenuUI>();
            if (menu != null && menu.isPaused)
            {
                PlayClickSound();
            }
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, AudioManager.MasterVolume);
        }
    }
}