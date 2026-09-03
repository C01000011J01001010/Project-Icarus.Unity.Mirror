using CoreEngine.Actor;
using CoreEngine.DesignPattern.StateMachine;
using System;
using System.Collections.Generic;

namespace Icarus.Character
{
    public enum SharedActorStateType { Idle, Moving, Flapping }

    [Serializable]
    public class SharedActorStateFeature : BaseStateController<SharedActorStateType>
    {
        protected override Dictionary<SharedActorStateType, BaseState<SharedActorStateType>> ProductState()
        {
            // 각 상태 인스턴스를 생성할 때 Host를 넘겨주어 상태 내부에서 물리/애니메이션 부품에 접근하게 함
            return new Dictionary<SharedActorStateType, BaseState<SharedActorStateType>>
            {
                // { SharedActorStateType.Idle, new IdleState(_host) },
                // { SharedActorStateType.Moving, new MovingState(_host) },
                // { SharedActorStateType.Flapping, new FlappingState(_host) }
            };
        }
    }
}