using Core.Manager;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public abstract class VirtualCamera : MonoBehaviour//, IInitializable
{
    protected CinemachineCamera cinemachineCamera;

    public LateTickGroup LateTickGroup => throw new System.NotImplementedException();

    public virtual void Exit()
    {

    }

    public virtual IEnumerator Initialize()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        SetTrackingTarget();
        yield return null;
    }

    private void SetTrackingTarget(Transform trackingTarget = null, Transform lookAtTarget = null)
    {
        // 이미 타겟이 있으면 굳이 새로 찾아서 초기화하지 않음
        if (trackingTarget == null && cinemachineCamera.Target.TrackingTarget) return;

        // 매개변수가 없는 경우, 또는 null인 경우 타겟 검색
        if (trackingTarget == null) trackingTarget = FindTrackingTarget();
        if (lookAtTarget == null) lookAtTarget = FindLookAtTarget();

        // 타겟을 찾지 못했다면 바꾸지 않음
        if (trackingTarget == null) return;

        CameraTarget newTarget = new CameraTarget();
        newTarget.TrackingTarget = trackingTarget;

        // LookAtTarget을 분리시킨 경우에만 사용
        if(lookAtTarget)
        {
            newTarget.CustomLookAtTarget = lookAtTarget;
            newTarget.LookAtTarget = lookAtTarget;
        }


        cinemachineCamera.Target = newTarget;
    }
    protected abstract Transform FindTrackingTarget();
    protected virtual Transform FindLookAtTarget() => null;

    /// <summary>
    /// 카메라 활성/비활성
    /// </summary>
    public virtual void SetActive(bool active)
    {
        if (cinemachineCamera != null)
            cinemachineCamera.gameObject.SetActive(active);
    }


    public virtual void SetVerticalFOV(float value)
    {
        cinemachineCamera.Lens.FieldOfView = value;
    }

    /// <summary>
    /// 자식 클래스에서 상황별 업데이트 로직
    /// CameraManager에서 호출
    /// </summary>
    public virtual void LateTick(float dt) { }
}
