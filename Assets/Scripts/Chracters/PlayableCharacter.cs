using System.Collections;
using UnityEngine;


public class PlayableCharacter : BaseCharacter, IScenedGameObject
{
    public event System.Action Event_OnControllTargetSet;
    public event System.Action Event_OnControllTargetRemoved;


    public override void Exit()
    {

    }

    public override IEnumerator Initialize()
    {
        //TryAddCharacterModule<CharacterTileChecker>();
        //TryAddCharacterModule<CharacterActionController>();
        //TryAddCharacterModule<CharacterQuestBook>();

        yield return base.Initialize();
        yield return null;
    }

    public override IEnumerator LateInitialize()
    {
        yield return base.LateInitialize();

        // 현재 조작가능한 캐릭터가 없다면 해당 캐릭터를 사용하도록 함
        //PlayerCotroller user = WorldManager.GetObject<PlayerCotroller>();
        //if (user.curTargetCharacter == null)
        //{
        //    user.SetControllTarget(this);
        //}
    }

    // 새로 연결되는 캐릭터만 해당하니 이벤트 등록 안함
    public virtual void OnControllTargetSet()
    {
        Event_OnControllTargetSet?.Invoke();
    }

    // 연결된 캐릭터만 제거하는거니 이벤트로 등록 안함
    public virtual void OnControllTargetRemoved()
    {
        Event_OnControllTargetRemoved?.Invoke();
    }
}
