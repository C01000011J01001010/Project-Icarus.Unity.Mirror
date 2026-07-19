using System.Collections;
using UnityEngine;

public class AudioManager : BaseGlobalManager, IGlobalManager
{
    // alt+ shift + . -> 선택박스에 추가
    // alt+ shift + , -> 선택박스에서 제거
    // alt+ shift + 화살표(위아래) -> 여러개의 커서
    // ctrl + alt + 마우스 클릭 -> 여러개의 커서
    // Home -> 맨앞으로 이동

    public void Exit()
    {

    }

    public IEnumerator Initialize()
    {
        yield break;
    }

    
}
