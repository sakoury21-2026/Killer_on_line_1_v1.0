using UnityEngine;
using System.Collections;

public class MySmartDoor : MonoBehaviour
{
    [Header("Door Movement")]
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float openSpeed = 5f;

    private bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private Coroutine currentCoroutine;

    private void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    public void Interact()
    {
        isOpen = !isOpen;

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(RotateDoor());
    }

    private IEnumerator RotateDoor()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.01f)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * openSpeed
            );

            yield return null;
        }

        transform.localRotation = targetRotation;
        currentCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2.5f);
    }
}
