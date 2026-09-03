using CoreEngine;
using CoreEngine.Actor;
using CoreEngine.CameraSystem;
using CoreEngine.EventBus;
using CoreEngine.Manager;
using CoreEngine.Manager.Pool;
using CoreEngine.Network.FishNetExtension;
using FishNet.Object.Synchronizing;
using Icarus.Camera;
using Icarus.Controller;
using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Icarus.Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class SharedActor : BaseNetworkActor, IActorHost, IPoolable, IFixedTickable
    {
        public FixedTickGroup FixedTickGroup => FixedTickGroup.Physics;
        protected override NetworkTickTarget networkTickTarget => NetworkTickTarget.ServerOnly;

        private readonly SyncDictionary<int, Vector2> _clientInputs = new SyncDictionary<int, Vector2>();
        public SyncDictionary<int, Vector2> ClientInputs => _clientInputs;

        public IPoolReleaser Releaser { get; set; }

        [Header("🪽 부품(Features) 조립")]
        [SerializeField] private SharedActorMovementFeature _movementFeature = new();
        [SerializeField] private SharedActorStateFeature _stateFeature = new();
        [SerializeField] private SharedActorAnimationFeature _animationFeature = new();

        private RepeatEventProvider<SetCameraTargetEvent> _cameraTargetProvider;
        private RepeatEventProvider<SwitchCameraEvent> _cameraSwitchProvider;


        private CameraNetTarget _cameraTarget;
        private Rigidbody _rb;
        private bool _isSpawned;

        public override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody>();

            _movementFeature.Initialize(this);
            _stateFeature.Initialize(this);
            _animationFeature.Initialize(this);
        }

        public void OnSpawn()
        {
            // Ping-Pong 패턴: 카메라가 먼저 켜졌든 늦게 켜졌든 안전하게 통신
            _cameraTarget = GetComponentInChildren<CameraNetTarget>();
            if (_cameraTarget != null)
            {
                _cameraTarget.DecoupleAndFollow(transform);
                Func<SetCameraTargetEvent> cameraTargetEventFunc = () => new SetCameraTargetEvent(_cameraTarget, typeof(ThirdPersonCameraController));
                _cameraTargetProvider = new RepeatEventProvider<SetCameraTargetEvent>(cameraTargetEventFunc);
                _cameraTargetProvider.Bind();

                Func<SwitchCameraEvent> cameraSwitchEventFunc = () => new SwitchCameraEvent(typeof(ThirdPersonCameraController));
                _cameraSwitchProvider = new RepeatEventProvider<SwitchCameraEvent>(cameraSwitchEventFunc);
                _cameraSwitchProvider.Bind();
            }

            _stateFeature.StartState();
            _isSpawned = true;
        }

        public void OnDespawn()
        {
            _isSpawned = false;
            _clientInputs.Clear();
            _stateFeature.StopState();

            _cameraTarget?.ReturnToParent(transform);
            _cameraTargetProvider?.Unbind();
            _cameraSwitchProvider?.Unbind();

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            // ServerManager.Spawn() 처리가 완전히 끝나서 서버 권한이 확정된 시점입니다.
            EventBus<SharedActorMoveEvent>.Subscribe(OnSharedActorMove);
            EventBus<SharedActorFlapEvent>.Subscribe(OnSharedActorWingFlap);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            // ServerManager.Despawn()이 호출되어 객체가 풀로 돌아갈 때 자동으로 귀를 닫습니다.
            EventBus<SharedActorMoveEvent>.Unsubscribe(OnSharedActorMove);
            EventBus<SharedActorFlapEvent>.Unsubscribe(OnSharedActorWingFlap);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!base.IsServerInitialized)
            {
                _rb.isKinematic = true;
            }
        }

        private void OnSharedActorMove(SharedActorMoveEvent evt) => _clientInputs[evt.ClientId] = evt.MoveVector;
        private void OnSharedActorWingFlap(SharedActorFlapEvent evt) => _movementFeature.ApplyFlap(evt.IsLeft);

        public void FixedTick(float fixedDeltaTime)
        {
            if (!this.IsServerInitialized || !_isSpawned) return;

            _stateFeature.FixedTick(fixedDeltaTime);
            _movementFeature.FixedTick(fixedDeltaTime);
        }

        public bool TryGetFeature<T>(out T feature) where T : class, IActorFeature
        {
            if (_movementFeature is T move) { feature = move; return true; }
            if (_stateFeature is T state) { feature = state; return true; }
            if (_animationFeature is T anim) { feature = anim; return true; }
            feature = null; return false;
        }
    }
}