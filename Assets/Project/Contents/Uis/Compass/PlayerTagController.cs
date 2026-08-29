using CoreEngine;
using CoreEngine.Interface;
using CoreEngine.Helpers;
using Icarus.Character;
using System.Collections.Generic;
using UnityEngine;

namespace Icarus.Ui
{
    public class PlayerTagController : CoreMonoBehaviour, ITickable
    {
        [SerializeField] private PlayerTagCanvas _playerTagCanvasPrefab;
        [SerializeField, Range(1f, 30f)] private float _rotationSmoothSpeed = 15f;

        [Tooltip("UI 프리팹들이 생성될 부모 컨테이너\n(미지정 시 이 객체의 부모로 자동 할당)")]
        [SerializeField] private Transform _canvasContainer;

        private List<PlayerTagCanvas> _playerTagCanvasList = new();

        private InterfaceReceiver<IClientInputProvider> _receiver = new();
        protected IClientInputProvider Target => _receiver.Target;

        public TickGroup TickGroup => TickGroup.Ui;

        private void Awake()
        {
            // 인스펙터에서 할당을 깜빡했을 경우를 대비한 방어 코드 (기존 로직 유지)
            if (_canvasContainer == null)
            {
                _canvasContainer = transform.parent;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _receiver.Bind();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _receiver.Unbind();
        }

        public void Tick(float deltaTime)
        {
            if (SystemHelper.isUnityNull(Target)) return;

            int activeCount = Target.GetPlayerInputCount();

            // 접속 인원에 맞춰 UI 객체 풀(Pool) 동기화
            SyncCanvasPool(activeCount);

            // 활성화된 플레이어 입력 처리 및 UI 갱신
            for (int i = 0; i < activeCount; i++)
            {
                Vector2 clientInput = Target.GetPlayerInput(i);
                PlayerTagCanvas canvas = _playerTagCanvasList[i];

                if (clientInput.sqrMagnitude > 0.001f)
                {
                    canvas.SetVisual(true);

                    // 목표 각도 계산
                    float angle = Mathf.Atan2(clientInput.x, clientInput.y) * Mathf.Rad2Deg;
                    Quaternion targetRot = Quaternion.Euler(0, 0, -angle);

                    // Canvas에게 직접 회전하라고 명령 (캡슐화)
                    canvas.UpdateRotation(targetRot, _rotationSmoothSpeed, deltaTime);
                }
                else
                {
                    canvas.SetVisual(false);
                }
            }

            // 3. 남는 인원의 UI 객체는 비활성화 처리
            for (int i = activeCount; i < _playerTagCanvasList.Count; i++)
            {
                _playerTagCanvasList[i].SetVisual(false);
            }
        }

        // 💡 변경: 프로퍼티(Getter)에 있던 생성 로직을 독립적인 풀링 메서드로 분리
        private void SyncCanvasPool(int targetCount)
        {
            while (_playerTagCanvasList.Count < targetCount)
            {
                // Instantiate 오버로딩을 활용해 부모를 즉시 할당 (성능 최적화)
                PlayerTagCanvas newCanvas = Instantiate(_playerTagCanvasPrefab, _canvasContainer, false);
                newCanvas.Initialize();
                newCanvas.SetVisual(false); // 생성 직후에는 일단 꺼둠
                _playerTagCanvasList.Add(newCanvas);
            }
        }
    }
}
