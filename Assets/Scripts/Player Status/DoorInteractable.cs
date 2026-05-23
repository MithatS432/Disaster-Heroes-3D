using UnityEngine;
using System.Collections;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 90f;

    public enum DoorAxis
    {
        Y,
        Z
    }

    [SerializeField] private DoorAxis rotationAxis = DoorAxis.Y;

    private bool isOpen;
    private Quaternion startRotation;



    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;

    [SerializeField] private float openSpeed = 2f;
    private Coroutine rotateRoutine;
    private Quaternion targetRotation;

    void Start()
    {
        if (doorPivot != null)
        {
            startRotation = doorPivot.localRotation;
        }
    }

    public void Interact(PlayerContoller player)
    {
        if (doorPivot == null) return;

        isOpen = !isOpen;

        float angle = isOpen ? openAngle : 0f;

        Vector3 axis = rotationAxis == DoorAxis.Y ? Vector3.up : Vector3.forward;

        targetRotation = startRotation * Quaternion.AngleAxis(angle, axis);

        if (audioSource != null)
            audioSource.PlayOneShot(isOpen ? openClip : closeClip);

        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);

        rotateRoutine = StartCoroutine(RotateDoor());
    }
    private IEnumerator RotateDoor()
    {
        while (Quaternion.Angle(doorPivot.localRotation, targetRotation) > 0.1f)
        {
            doorPivot.localRotation = Quaternion.Slerp(
                doorPivot.localRotation,
                targetRotation,
                Time.deltaTime * openSpeed
            );

            yield return null;
        }

        doorPivot.localRotation = targetRotation;
    }
}