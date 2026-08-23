using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.1f;
    private Vector2 lookInput;
    private float verticalRotation;
    private bool canLook = true; // האם השחקן רשאי להזיז את המבט מתחיל ב"אמת" כדי שהמצלמה תפעל כרגיל בתחילת המשחק
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (!canLook) // אם תנועת המבט נעולה
        {
            return; // עוצר את כל האפדייט לפני סיבוב המצלמה
        }
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    public void SetLookEnabled(bool enabled) // מפעילה או מכבה את תנועת המבט
    {
        canLook = enabled; // שומרת את מצב המבט החדש
        lookInput = Vector2.zero; // מוחקת תנועת עכבר שנשארה
    }
}
