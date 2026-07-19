using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class DragManager : BaseGlobalManager, IGlobalManager
{
    private Canvas canvas;
    private RectTransform currentRect;
    private CanvasGroup canvasGroup;

    public bool IsDragging => currentRect != null;


    public IEnumerator Initialize()
    {
        yield break;
    }

    public void Exit()
    {
        
    }

    public void OnTouchEnter(DragTarget target, PointerEventData eventData, Canvas TargetCanvas)
    {
        if(target.moveObjRectTransform == null || target.moveObjCanvasGroup == null)
        {
            Debug.Log("드래그 대상을 움직이는 대상으로 함");
            currentRect = target.GetRectTransform();
            canvasGroup = target.GetCanvasGroup();
        }
        else // 움직이는 대상이 지정된 경우만 해당 컴포넌트를 사용
        {
            Debug.Log("드래그 대상을 움직이는 대상이 다름");
            currentRect = target.moveObjRectTransform;
            canvasGroup = target.moveObjCanvasGroup;
        }

        
        canvas = TargetCanvas;

        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;
    }

    public void Drag(PointerEventData eventData)
    {
        if (currentRect == null) return;

        // eventData.delta == 이동한 픽셀값, canvas.scaleFactor == 캔버스 확대비율
        currentRect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void EndDrag()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        currentRect = null;
        canvasGroup = null;
    }

    
}

