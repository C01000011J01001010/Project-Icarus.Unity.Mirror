 
 
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class DragTarget : MonoBehaviour, IInitialize, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private DragManager _dragManager;
    public RectTransform moveObjRectTransform {  get; private set; }
    public CanvasGroup moveObjCanvasGroup {  get; private set; }

    /// <summary>
    /// 현재 객체를 포함하는 가장 가까운 캔버스
    /// </summary>
    private Canvas _canvas;

    public IEnumerator Initialize()
    {

        _canvas = transform.FindParentCanvas();
        if(_canvas is null) yield break;
        // GameManager temporarily disabled
        // _dragManager = GameManager.GetManager<DragManager>();
        // if(_dragManager is null) yield break;
    }

    public IEnumerator LateInitialize()
    {
        yield break;
    }

    public void Exit()
    {
        
    }


    /// <summary>
    /// IPointerDownHandler, 터치다운시 호출
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("터치/클릭 시작!");
        _dragManager.OnTouchEnter(this, eventData, _canvas);
    }

    /// <summary>
    /// IBeginDragHandler, 터치 후 드래그 시작할 때 호출
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("드래그 시작!");
        // 선택적으로 처리
    }

    /// <summary>
    /// IDragHandler, 드래그 중 계속 호출
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        _dragManager.Drag(eventData);
    }

    /// <summary>
    /// IEndDragHandler, 드래그 끝났을 때 호출
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        _dragManager.EndDrag();
    }


    private void Update()
    {
        //if (Input.GetMouseButtonUp(0) && _dragManager.IsDragging)
        //{
        //    Debug.LogWarning("드래그 상태가 중간에 끊겨 강제 종료! (마우스)");
        //    _dragManager.EndDrag();
        //}
    }

    /// <summary>
    /// 실제 드래그 대상과 이동하는 대상이 다를 수 있음
    /// </summary>
    /// <param name="value"></param>
    public void SetMoveObjAttribute(RectTransform rectTransform, CanvasGroup canvasGroup)
    {
        if (canvasGroup != null)
        {
            moveObjRectTransform = rectTransform;
            moveObjCanvasGroup = canvasGroup;
        }
        else
        {
            Debug.LogAssertion("캔버스 그룹이 없음");
        }
            
    }
    
    public RectTransform GetRectTransform() => GetComponent<RectTransform>();
    public CanvasGroup GetCanvasGroup() => GetComponent<CanvasGroup>();

    
}
