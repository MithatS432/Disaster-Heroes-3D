using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerContoller : MonoBehaviour
{
    [Header("References")]
    private MenuUI menuUI;
    private IInteractable currentInteractable;

    [Header("Components")]
    private Rigidbody rb;
    private Animator anim;
    private AudioSource audioSource;
    private Camera playerCamera;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 200f;
    [SerializeField] private Transform cameraPivot;
    private float xRotation = 0f;

    [Header("Player Stats")]
    public float horizantalInput;
    public float verticalInput;

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    public float jumpForce = 8f;

    private bool isGrounded;
    private bool isRunning;
    private Vector3 moveInput;

    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float rayDistance = 0.5f;

    public GameObject gameplayUI;
    public GameObject inventoryUI;
    public GameObject missionsUI;
    private bool inventoryOpen;


    [Header("Collectibles")]
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private CanvasGroup[] collectibleUI;
    [SerializeField] private int totalCollectibles = 6;
    private bool[] collected;
    [SerializeField] private Image crosshair;


    [Header("Room Transitions")]
    [Tooltip("Tooltip for room transition")]
    public TextMeshProUGUI roomNameText;
    public float roomOpenDistance = 3f;
    [SerializeField] private LayerMask roomLayer;


    [Header("Audio Clips")]
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip runSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip inventorySound;
    [SerializeField] private AudioClip roomOpenSound;




    void Start()
    {
        menuUI = FindFirstObjectByType<MenuUI>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        playerCamera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inventoryOpen = false;

        collected = new bool[totalCollectibles];
    }
    void Update()
    {
        if (GameManager.Instance.isGameStarted == false)
        {
            return;
        }

        if (menuUI != null && menuUI.isPaused)
            return;

        // INPUT
        horizantalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        moveInput = new Vector3(horizantalInput, 0f, verticalInput);
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // MOUSE LOOK
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        CheckGround();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            bool wasMoving = moveInput.magnitude > 0.1f;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            float volumeMultiplier = wasMoving ? 1f : 0.5f;

            audioSource.PlayOneShot(jumpSound, volumeMultiplier * AudioManager.MasterVolume);

            anim.SetTrigger("Jump");
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            inventoryOpen = !inventoryOpen;

            gameplayUI.SetActive(!inventoryOpen);
            inventoryUI.SetActive(!inventoryOpen);
            missionsUI.SetActive(!inventoryOpen);
            audioSource.PlayOneShot(inventorySound, AudioManager.MasterVolume);
        }

        currentInteractable = GetInteractable();

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }


        UpdateCrosshair();

        float inputMagnitude = moveInput.magnitude;
        float animSpeed = isGrounded ? inputMagnitude * (isRunning ? 1f : 0.5f) : 0f;
        anim.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);

    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.isGameStarted)
        {
            return;
        }


        Vector3 move = transform.TransformDirection(moveInput).normalized;
        float speed = isRunning ? runSpeed : moveSpeed;

        if (move.magnitude > 0.1f)
        {
            if (Physics.Raycast(transform.position, move, out RaycastHit hit, rayDistance, wallLayer))
            {
                move = Vector3.zero;
            }
        }

        Vector3 targetVelocity = move * speed;

        rb.linearVelocity = new Vector3(
            targetVelocity.x,
            rb.linearVelocity.y,
            targetVelocity.z
        );

        if (isGrounded && rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                -2f,
                rb.linearVelocity.z
            );
        }
    }

    private void LateUpdate()
    {
        if (playerCamera == null) return;

        playerCamera.transform.SetPositionAndRotation(
            cameraPivot.position,
            cameraPivot.rotation
        );
    }
    private void CheckGround()
    {
        isGrounded = Physics.SphereCast(
            groundCheckPoint.position,
            groundCheckRadius,
            Vector3.down,
            out _,
            0.3f,
            groundLayer
        );
    }

    #region Room Passings and Room Open Doors
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Room2"))
        {
            roomNameText.text = "Bulunduğunuz Konum Banyo Kısmı";
        }
        else if (other.gameObject.CompareTag("Room1"))
        {
            roomNameText.text = "Bulunduğunuz Konum Oturma Odası";
        }
        else if (other.gameObject.CompareTag("Room3"))
        {
            roomNameText.text = "Bulunduğunuz Konum Çoçuk Odası";
        }
        else if (other.gameObject.CompareTag("Room4"))
        {
            roomNameText.text = "Bulunduğunuz Konum Mutfak Kısmı";
        }
        else if (other.gameObject.CompareTag("Room5"))
        {
            roomNameText.text = "Bulunduğunuz Konum Yatak Odası";
        }
    }
    #endregion



    #region Audio Handling
    public void FootstepSound()
    {
        AudioClip clip = isRunning ? runSound : walkSound;
        audioSource.PlayOneShot(clip, 0.5f * AudioManager.MasterVolume);
    }

    public void JumpSound()
    {
        audioSource.PlayOneShot(jumpSound, AudioManager.MasterVolume);
    }

    public void PickupItemSound()
    {
        audioSource.PlayOneShot(pickupSound, AudioManager.MasterVolume);
    }
    #endregion





    #region Interactable Handling
    public bool IsCollected(int id)
    {
        return collected[id];
    }

    public void MarkCollected(int id)
    {
        collected[id] = true;
    }

    public void TriggerCollectUI(int id)
    {
        UpdateUI(id);
    }
    private void UpdateUI(int id)
    {
        CanvasGroup cg = collectibleUI[id];

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        Image[] images = cg.GetComponentsInChildren<Image>();

        foreach (Image img in images)
        {
            Color c = img.color;
            c.a = 1f;
            img.color = c;
        }
    }
    private IInteractable GetInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            }
            return interactable;
        }

        return null;
    }
    private void UpdateCrosshair()
    {
        if (crosshair == null) return;

        Color targetColor = (currentInteractable != null) ? Color.green : Color.white;

        crosshair.color = Color.Lerp(crosshair.color, targetColor, 0.15f);
    }
    #endregion
}