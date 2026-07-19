using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseWindow_FullScreen : BaseWindow
{
    public RectTransform viewPortRectTrans;

    protected virtual void Awake()
    {
        AdjustContentCellSize();
    }


    protected virtual void AdjustContentCellSize()
    {
        // rect.size를 프리팹으로 연결한 상태에서 사용하면 로드된 실제 크기와 다른 값을 불러올 수 있음(로드 전 크기)
        // 이는 레이아웃 계산이 완료되지 않았기에 생기는 문제로 해당 연산을 먼저 처리하도록 만들어야함
        LayoutRebuilder.ForceRebuildLayoutImmediate(viewPortRectTrans);
        Vector2 contentCellSize = viewPortRectTrans.rect.size;

        // y는 스크롤축이니 그대로 유지
        contentCellSize.y = contentGrid.cellSize.y; 

        // x축에서 여백 제외한 모든 공간을 값으로 정함
        contentCellSize.x = contentCellSize.x
            - contentGrid.padding.left
            - contentGrid.padding.right
            - contentGrid.spacing.x * (contentGrid.constraintCount - 1);
        contentCellSize.x /= contentGrid.constraintCount;

        contentGrid.cellSize = contentCellSize;
    }

    protected override void ChangeContentRectTransform()
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
}
