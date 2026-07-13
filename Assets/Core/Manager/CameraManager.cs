using Core;
using Core.Manager;
using Core.Update;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Manager
{
    public class CameraManager : BaseManager, ILateTickable//, IWorldInitializable
    {
        [SerializeField] int _priority = -1;
        public int Priority => _priority;

        MainCameraController _mainCamera;

        private List<VirtualCamera> _virtualCameras;
        private VirtualCamera _currentCamera;

        // ILateTickable 구현
        public LateTickGroup LateTickGroup => LateTickGroup.Camera;

        public void LateTick(float dt)
        {
            _currentCamera?.LateTick(dt);
        }

        public override void Exit()
        {
            OptionManager.OnGraphicOptionChanged -= OptionChanged;
        }
        
        public override IEnumerator Initialize(IModuleHub hub)
        {
            OptionManager.OnGraphicOptionChanged -= OptionChanged;
            OptionManager.OnGraphicOptionChanged += OptionChanged;

            _mainCamera = GetComponentInChildren<MainCameraController>();
            _virtualCameras = GetComponentsInChildren<VirtualCamera>().ToList();

            yield return _mainCamera.Initialize();
            foreach (var VCAM in _virtualCameras)
            {
                yield return VCAM.Initialize();
            }

            SwitchCamera<ThirdPersonCamera>();
            ResetCamera();
        }

        /// <summary>
        /// 특정 타입의 가상 카메라를 활성화
        /// </summary>
        public T SwitchCamera<T>() where T : VirtualCamera
        {
            foreach (var vcam in _virtualCameras)
            {
                if (vcam is T matched)
                {
                    SetActiveCamera(vcam);
                    return matched;
                }
            }
            Debug.LogWarning($"VirtualCamera of type {typeof(T)} not Contains");
            return null;
        }

        /// <summary>
        /// 현재 활성 카메라 반환
        /// </summary>
        public T GetCurrentCamera<T>() where T : VirtualCamera
        {
            if (_currentCamera is T matched) return matched;

            Debug.LogError($"{typeof(T).Name}은 {_currentCamera.GetType().Name}과 다름");
            return null;
        }

        private void SetActiveCamera(VirtualCamera target)
        {
            if (_currentCamera == target) return;

            // 비활성화
            if (_currentCamera != null)
                _currentCamera.SetActive(false);

            // 활성화
            _currentCamera = target;
            _currentCamera.SetActive(true);
        }

        public void ResetCamera()
        {
            OptionChanged(OptionManager.appliedGraphicOption);
        }


        public void OptionChanged(GraphicOptionValues value)
        {
            _currentCamera.SetVerticalFOV(value.fileldOfView);
        }
    }
}

