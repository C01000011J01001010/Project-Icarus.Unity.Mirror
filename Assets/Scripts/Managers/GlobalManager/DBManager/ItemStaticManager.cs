using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템의 정적 데이터 관리
/// </summary>
[AddComponentMenu(AssetMenu + "/ItemStaticManager")]
public class ItemStaticManager : BaseStaticDataManager<ItemData>, IGlobalManager
{
    protected override string Label => Constants.LABEL_ItemData;
}