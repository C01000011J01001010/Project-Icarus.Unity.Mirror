using System;
using UnityEngine;


public abstract class BaseWindow_DataManage : BaseWindow_FullScreen
{
    protected enum PlayerSaveKey
    {

    }

    protected int DataCount = 0;

    protected override void Awake()
    {
        //AdjustContentCellSize();
        //// None을 제외한 개수
        //DataCount = Enum.GetValues(typeof(PlayerSaveKey)).Length - 1;
        //InitializePool(DataCount);
    }

    protected void OnEnable()
    {
        RefreshPopUp();
        ScrollToTop();
    }

    public override void RefreshPopUp()
    {
        RefreshPopUp(DataCount,
            () =>
            {
                //int index = 0;
                //foreach (PlayerSaveKey saveKey in Enum.GetValues(typeof(PlayerSaveKey)))
                {
                    //if (saveKey == PlayerSaveKey.None) continue;

                    //SavedPlayerDataPanel panel = ActiveObjList[index++].GetComponent<SavedPlayerDataPanel>();
                    //if (panel != null)
                    //{
                    //    panel.SetPanel(saveKey);
                    //}
                    //else
                    //{
                    //    Debug.LogAssertion($"{gameObject.name}의 스크립트 확인 바람");
                    //}

                }
            });
    }
}
