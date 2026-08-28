 
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Window 객체 기본 클래스
/// </summary>
public abstract class BaseWindow : MonoBehaviour//, IInitialize
{
    //IPoolable prefab;

    // 에디터 연결
    public RectTransform contentTrans;
    public GridLayoutGroup contentGrid;
    public ScrollRect scrollRect;  // ScrollRect 컴포넌트를 연결

    protected List<GameObject>  ActiveObjList = new();
    protected Queue<GameObject> memoryPool = new();

    public bool IsInit { get; protected set; } = false;
    public void Exit()
    {
        
    }

    public IEnumerator Initialize()
    {
        yield break;
    }

    public IEnumerator LateInitialize()
    {
        yield break;
    }


    protected virtual void ChangeContentRectTransform()
    {
        //InitAnchor();
        //InitGridRayout();

        if (contentGrid != null)
        {
            Vector2 size = Vector2.zero;

            // 행개수 : ex) (자식개수가 4, 제약개수가 4) -> (3/4 + 1 = 1)
            int rowCount = ((ActiveObjList.Count - 1) / (contentGrid.constraintCount)) + 1;
            //Debug.Log($"rowCount : {rowCount}");

            int ColumnCount = contentGrid.constraintCount;
            //Debug.Log($"_columnCount : {_columnCount}");

            // 셀의 좌우 여백 및 한 행의 셀의 x축 크기 * 셀의 개수, 셀 사이의 여백을 더함
            size.x = contentGrid.padding.left + contentGrid.padding.right +
                contentGrid.cellSize.x * ColumnCount +
                contentGrid.spacing.x * (ColumnCount - 1);
            //Debug.Log($"size.x : {size.x}");

            size.y = contentGrid.padding.top + contentGrid.padding.bottom +
                contentGrid.cellSize.y * rowCount +
                contentGrid.spacing.y * (rowCount - 1);
            //Debug.Log($"size.y : {size.y}");

            contentTrans.sizeDelta = size;

            //Debug.Log("CONTENT 맞춤설정 완료");

        }
        else
        {
            Debug.LogWarning("contentGrid null");
        }

    }

    // 스크롤을 위로 올리는 함수
    public void ScrollToTop()
    {
        // 스크롤바를 통해 콘텐트를 제어
        //scrollRect.verticalScrollbar.value = 1;

        // 콘텐트를 제어하여 스크롤바를 제어
        scrollRect.verticalNormalizedPosition = 1f;  // 1f: 맨 위
    }

    /// <summary>
    /// RefreshPopUp(int totalCount, Action InitElementCallback)를 내부에서 호출해야 함
    /// </summary>
    public abstract void RefreshPopUp();
    public virtual void RefreshPopUp(int totalCount, Action InitElementCallback)
    {
        // 필요한 객체의 개수
        int NeededCount = totalCount - ActiveObjList.Count;

        // 객체가 더 필요한 경우 메모리풀에서 꺼냄
        if (NeededCount > 0)
        {
            Debug.Log($"객체 {NeededCount}개 활성화");
            for (int i = 0; i < NeededCount; i++)
            {
                GetObject();
            }
        }
        // 필요없는 만큼 환수함
        else if (NeededCount < 0)
        {
            NeededCount = (-NeededCount);

            Debug.Log($"객체 {NeededCount}개 비활성화");
            for (int i = 0; i < NeededCount; i++)
            {
                ReturnObject(ActiveObjList[0]);
            }
        }

        // 항목개수에 맞게 사이즈를 변경
        ChangeContentRectTransform();

        // 현재 활성화된 객체에 정보를 초기화
        InitElementCallback();
    }
    

    protected virtual void CreateNewObject(int orderInPool)
    {
        //GameObject obj = Instantiate(prefab);

        //if (obj != null)
        //{
        //    // 메모리풀의 대상을 메모리풀 안에서 관리함
        //    // 메모리풀이 donDestroy이면 객체도 donDestroy
        //    obj.transform.SetParent(contentTrans.transform, false);

        //    obj.SetActive(false);

        //    if (memoryPool != null)
        //    {
        //        memoryPool.Enqueue(obj);
        //    }
        //    else
        //    {
        //        Debug.LogError("메모리풀(Queue) 초기화 안됐음");
        //    }
        //}
        //else
        //{
        //    Debug.LogError("프리팹이 없음");
        //}
    }
    public virtual GameObject GetObject(Vector3 position)
    {
        if (memoryPool.Count == 0)
        {
            CreateNewObject(contentTrans.transform.childCount);
        }

        GameObject obj = memoryPool.Dequeue();
        obj.SetActive(true);
        ActiveObjList.Add(obj);

        obj.transform.position = position;
        return obj;
    }
    public  virtual GameObject GetObject()
    {
        if (memoryPool.Count == 0)
        {
            CreateNewObject(contentTrans.transform.childCount);
        }

        GameObject obj = memoryPool.Dequeue();
        obj.SetActive(true);
        ActiveObjList.Add(obj);
        return obj;
    }

    public virtual void ReturnObject(GameObject obj)
    {
        if(ActiveObjList.Contains(obj))
        {
            Debug.Log($"반환되는 객체 : {obj.name}");
            obj.SetActive(false);
            memoryPool.Enqueue(obj);
            ActiveObjList.Remove(obj);
            ChangeContentRectTransform(); // 객체가 회수됐으면 그만큼 스크롤 공간을 조정
        }
        else
        {
            Debug.LogAssertion("잘못된 반환");
        }
    }

    public virtual void ReturnAllObject()
    {
        int activeObjCount = ActiveObjList.Count;
        Debug.Log($"반환시작, 반환될 객체의 개수 : {ActiveObjList.Count}");
        for (int i = 0; i < activeObjCount; i++)
        {
            ReturnObject(ActiveObjList[0]);
        }
        ChangeContentRectTransform();
    }

    
}
