using CoreEngine;
using UnityEngine;
using UnityEngine.UI;
using CoreEngine.Extentions;

[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
public class PlayerTagCanvas : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    // private Image _image; // 당장 사용하지 않는다면 캐싱만 해두거나 제거해도 무방합니다.

    private bool _isVisual = false;
    public bool IsVisual => _isVisual;

    protected void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
        // _image = GetComponentInChildren<Image>();
    }

    // 💡 변경: 부모 지정을 Instantiate 단계에서 처리하므로, 여기서는 Transform 초기화만 담당합니다.
    public void Initialize()
    {
        _rectTransform.SetAnchorPivotAndPosition(AnchorPreset.StretchAll);
        _rectTransform.localScale = Vector3.one;
        _rectTransform.localRotation = Quaternion.identity;
    }

    public void SetVisual(bool isActive)
    {
        // 💡 안전장치: 이미 같은 상태라면 중복해서 CanvasGroup을 조작하지 않음
        if (_isVisual == isActive) return;

        _canvasGroup.alpha = isActive ? 1f : 0f;
        _isVisual = isActive;
    }

    // 회전 로직(Slerp)을 캔버스 내부로 가져와 캡슐화 달성
    public void UpdateRotation(Quaternion targetRotation, float smoothSpeed, float deltaTime)
    {
        _rectTransform.localRotation = Quaternion.Slerp(
            _rectTransform.localRotation, targetRotation, deltaTime * smoothSpeed
        );
    }
}