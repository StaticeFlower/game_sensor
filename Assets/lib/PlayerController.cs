using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 12f;
    public float xLimit = 8f;
    [Range(0f, .5f)] public float inputSmoothing = .12f;
    private float smoothVelocity;
    private float currentInput; // เพิ่มตัวแปรจำค่าแกนปัจจุบัน
    private WsCloudClient sensorClient;
    public static string ActiveMode => Object.FindFirstObjectByType<WsCloudClient>()?.ControlModeLabel ?? "Keyboard";

    private void Start() { sensorClient = GetComponent<WsCloudClient>(); }
    private void Update()
    {
        float keyboardAxis = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) keyboardAxis = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) keyboardAxis = 1f;
        }
        float target = Mathf.Abs(keyboardAxis) > .01f ? keyboardAxis : sensorClient != null ? sensorClient.MovementAxis : 0f;
        
        // แก้ไข: ใช้ currentInput แทน 0f เพื่อให้ SmoothDamp ต่อเนื่อง ไม่กระตุก
        currentInput = Mathf.SmoothDamp(currentInput, target, ref smoothVelocity, inputSmoothing);
        
        transform.Translate(Vector3.right * currentInput * moveSpeed * Time.deltaTime, Space.World);
        Vector3 position = transform.position; position.x = Mathf.Clamp(position.x, -xLimit, xLimit); transform.position = position;
    }
}