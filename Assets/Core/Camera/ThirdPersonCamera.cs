using Core.Character;
using Core.EventBus;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public struct CallCameraTargetEvent : IEvent
{
    ISetCharacter _characterSetter;
    public CallCameraTargetEvent(ISetCharacter characterSetter) => _characterSetter = characterSetter;
}

[RequireComponent(typeof(CinemachineThirdPersonFollow))]
class ThirdPersonCamera : VirtualCamera, ISetCharacter
{
    [Header("CameraZoom")]
    [Tooltip("카메라 줌인 줌아웃 적용 비율")]
    [Range(0.05f, 0.2f)]
    public float zoomRate = 0.1f;

    [Tooltip("카메라 줌인 줌아웃 적용 속도")]
    [Range(5f, 10f)]
    public float zoomSpeed = 10f;

    [Range(1 , _zoomBoundary)]
    public  float ZoomInLimit = 1;

    [Range(_zoomBoundary, 4f)]
    public float ZoomOutLimit = 4;

    private const float _zoomBoundary = 2;
    private float _targetZoomDistance;

    public CinemachineThirdPersonFollow thirdPersonFollow {  get; private set; }

    public ICharacter followTarget;

    public override void Exit()
    {

    }

    public override IEnumerator Initialize()
    {
        yield return base.Initialize();
        thirdPersonFollow = GetComponent<CinemachineThirdPersonFollow>();
        _targetZoomDistance = thirdPersonFollow.CameraDistance;

        yield return null;
    }

    protected override Transform FindTrackingTarget()
    {
        //PlayerCharacter target = WorldManager.GetObject<PlayerCharacter>();
        // 내가 타겟으로할 캐릭터가 있는지 확인함
        EventBus<CallCameraTargetEvent>.Publish(new CallCameraTargetEvent(this));
        if (followTarget != null)
        {
            //GameObject targetObj = followTarget.transform.FindObjectInChildrenWithTag(Constants.TAG_CameraRoot);
            //return targetObj.transform;
            return (followTarget as MonoBehaviour).transform;
        }
        else
        {
            Debug.LogAssertion($"{name} is Failed => FindTrackingTarget");
            return null;
        }
    }


    public void OnZoom(float delta)
    {
        _targetZoomDistance -= delta * zoomRate;
        _targetZoomDistance = Mathf.Clamp(_targetZoomDistance, ZoomInLimit, ZoomOutLimit);
    }

    public override void Tick(float deltaTime)
    {
        // Lerp는 근사값을 만드니 현재 속도와 목표속도의 차이가 offset 이하면 Lerp를 생략 
        float speedOffset = 0.1f;

        // 가속 or 감속 시
        if (thirdPersonFollow.CameraDistance < _targetZoomDistance - speedOffset ||
            thirdPersonFollow.CameraDistance > _targetZoomDistance + speedOffset)
        {
            // 선형보간
            thirdPersonFollow.CameraDistance = 
                Mathf.Lerp(thirdPersonFollow.CameraDistance, _targetZoomDistance,
                Time.deltaTime * zoomSpeed);
        }
        else
        {
            // 속도차가 speedOffset 이하면 근사값을 정확한 값으로 변경
            thirdPersonFollow.CameraDistance = _targetZoomDistance;
        }
    }

    public void SetCharacter<TCharacter>(TCharacter character) 
        where TCharacter : MonoBehaviour, ICharacter
    {
        followTarget = character;
    }
}