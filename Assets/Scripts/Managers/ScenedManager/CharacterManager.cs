//using Core;
//using Core.EventBus;
//using Core.Manager;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class CharacterManager : BaseManager, ITickable, IFixedTickable
//{
//    //[SerializeField] private int _priority;
//    //public int Priority => _priority;

//    public TickGroup TickGroup => TickGroup.Character;

//    public FixedTickGroup FixedTickGroup => FixedTickGroup.Physics;

//    List<BaseCharacter> _characterList;

//    public override void Exit()
//    {
//        EventBus<R_TickEvent>.Publish(new R_TickEvent(this, TickGroup.Character, false));
//        EventBus<R_FixedTickEvent>.Publish(new R_FixedTickEvent(this, FixedTickGroup.Physics, false));
//    }

//    public override IEnumerator Initialize()
//    {
//        _characterList = new();

//        EventBus<R_TickEvent>.Publish(new R_TickEvent(this, TickGroup.Character, true));
//        EventBus<R_FixedTickEvent>.Publish(new R_FixedTickEvent(this, FixedTickGroup.Physics, true));

//        yield return null;
//    }

//    //public IEnumerator LateInitialize()
//    //{
//    //    yield break;
//    //}

//    public void AddList(BaseCharacter character)
//    {
//        if (!_characterList.Contains(character))
//        {
//            _characterList.Add(character);
//        }
//    }

//    public void RemoveList(BaseCharacter character)
//    {
//        _characterList.Remove(character);
//    }

//    public void Tick(float deltaTime)
//    {
//        for (int i = _characterList.Count - 1; i >= 0; --i)
//        {
//            _characterList[i].Tick(deltaTime);
//        }
//    }

//    public void FixedTick(float fixedDeltaTime)
//    {
//        for (int i = _characterList.Count - 1; i >= 0; --i)
//        {
//            _characterList[i].FixedTick(fixedDeltaTime);
//        }
//    }
//}
