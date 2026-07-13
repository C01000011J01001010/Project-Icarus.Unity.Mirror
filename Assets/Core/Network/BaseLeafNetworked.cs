using FishNet.Connection;
using FishNet.Object;
using Core.EventBus;
using Core.Manager;
using Core.Update;
using UnityEngine;

namespace Core.Network
{
    public enum NetworkTickTarget
    {
        None,
        ServerOnly,
        ClientOnly,
        OwnerOnly,
        ServerAndClient
    }

    public abstract class BaseLeafNetworked : NetworkBehaviour
    {
        /// <summary>
        /// 상속받는 쪽에서 결정하도록
        /// </summary>
        protected abstract NetworkTickTarget networkTickTarget { get;}

        private bool _isRegistered = false;

        // ==========================================
        // 1. 유니티 로컬 생명주기 제어 (로컬에서 껐다 킬 때)
        // ==========================================
        protected virtual void OnEnable()
        {
            // 객체가 네트워크 상에 완전히 스폰된 상태라면 등록 시도
            if (base.IsSpawned) TryRegisterTick();
        }

        protected virtual void OnDisable()
        {
            TryUnregisterTick();
        }

        // ==========================================
        // 2. FishNet 네트워크 생명주기 제어 (진입점)
        // ==========================================

        // 서버로서 스폰되었을 때
        public override void OnStartServer() => TryRegisterTick();
        public override void OnStopServer() => TryUnregisterTick();

        // 클라이언트로서 스폰되었을 때
        public override void OnStartClient() => TryRegisterTick();
        public override void OnStopClient() => TryUnregisterTick();

        // 런타임에 서버가 나에게 소유권을 주거나 빼앗을 때!
        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);
            // 소유권이 바뀌었으므로 기존 틱을 해제하고, 내 새로운 권한에 맞춰 재등록 시도
            TryUnregisterTick();
            TryRegisterTick();
        }

        // ==========================================
        // 3. 등록 / 해제 공통 로직
        // ==========================================
        private void TryRegisterTick()
        {
            if (_isRegistered) return; // 중복 등록 방지
            if (!EvaluateNetworkCondition()) return; // 권한이 없으면 스킵

            _isRegistered = true;

            // 우리가 완성해둔 인터페이스 기반의 상향식(Bottom-up) 등록 진행
            if (this is ITickable tickable)
                EventBus<R_TickEvent>.Publish(new R_TickEvent(tickable, tickable.TickGroup, true));

            if (this is ILateTickable lateTickable)
                EventBus<R_LateTickEvent>.Publish(new R_LateTickEvent(lateTickable, lateTickable.LateTickGroup, true));

            if (this is IFixedTickable fixedTickable)
                EventBus<R_FixedTickEvent>.Publish(new R_FixedTickEvent(fixedTickable, fixedTickable.FixedTickGroup, true));
        
        }

        private void TryUnregisterTick()
        {
            if (!_isRegistered) return;
            _isRegistered = false;

            if (this is ITickable tickable)
                EventBus<R_TickEvent>.Publish(new R_TickEvent(tickable, tickable.TickGroup, false));

            if (this is ILateTickable lateTickable)
                EventBus<R_LateTickEvent>.Publish(new R_LateTickEvent(lateTickable, lateTickable.LateTickGroup, false));

            if (this is IFixedTickable fixedTickable)
                EventBus<R_FixedTickEvent>.Publish(new R_FixedTickEvent(fixedTickable, fixedTickable.FixedTickGroup, false));
        }

        private bool EvaluateNetworkCondition()
        {
            return networkTickTarget switch
            {
                NetworkTickTarget.None => false,
                NetworkTickTarget.ServerOnly => base.IsServerInitialized, // 💡 FishNet 최신 API (IsServer 대신 권장됨)
                NetworkTickTarget.ClientOnly => base.IsClientInitialized,
                NetworkTickTarget.OwnerOnly => base.IsOwner,
                NetworkTickTarget.ServerAndClient => base.IsServerInitialized || base.IsClientInitialized,
                _ => false
            };
        }
    }
}