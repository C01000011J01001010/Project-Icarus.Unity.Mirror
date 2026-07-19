using UnityEngine;

public class ItemViewer : BaseDataViewer_ForUi<ItemDataContainer, ItemData>
{
    public override void UpdateView()
    {
        // 아이템이 없으면 슬롯만 보이도록
        if(ConnectObject.Get() == null)
        {
            iconImage.color = Color.clear;
        }
        else
        {
            iconImage.color = Color.white;
        }
        base.UpdateView();
    }
}
