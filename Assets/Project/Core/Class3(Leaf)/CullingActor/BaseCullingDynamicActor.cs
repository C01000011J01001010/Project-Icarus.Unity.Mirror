using Core.EventBus;
using Core.Manager.Culling;
using UnityEngine;

namespace Core
{
    
    public abstract class BaseCullingDynamicActor : BaseCullingObjectActor, ITickable
    {
        private Vector3Int _lastGridKey;

        // 몬스터나 일반 오브젝트 그룹에 맞게 설정 (UpdateManager 연동)
        public virtual TickGroup TickGroup => TickGroup.Object;

        protected override void OnEnable()
        {
            base.OnEnable();
            // 🌟 매우 중요: 스폰 직후의 최초 격자 위치를 반드시 동기화해두어야 함
            _lastGridKey = CoreFacade.GetGridKey(transform.position);
        }

        public void Tick(float deltaTime)
        {
            // 매 프레임(또는 일정 틱마다) 현재 격자 계산
            Vector3Int currentGrid = CoreFacade.GetGridKey(transform.position);

            // 격자 선을 넘어갔다면!
            if (currentGrid != _lastGridKey)
            {
                // Manager에게 "나 옛날 격자에서 빼고 새 격자에 넣어줘!" 라고 방송
                EventBus<CullingObjectMovedEvent>.Publish(new CullingObjectMovedEvent(this, _lastGridKey, currentGrid));

                _lastGridKey = currentGrid;
            }
        }
    }
}