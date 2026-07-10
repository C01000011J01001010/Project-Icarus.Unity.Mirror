using UnityEngine;
using UnityEngine.InputSystem;

public class NewInputClickHandler : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 터치나 마우스 클릭 감지
        if (Mouse.current.leftButton.wasPressedThisFrame ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame))
        {
            Vector2 touchPosition = GetInputPosition();
            CheckForClick(touchPosition);
        }
    }

    Vector2 GetInputPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }
        return Mouse.current.position.ReadValue();
    }

    void CheckForClick(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"{hit.collider.gameObject.name} 클릭!");
        }
    }
}