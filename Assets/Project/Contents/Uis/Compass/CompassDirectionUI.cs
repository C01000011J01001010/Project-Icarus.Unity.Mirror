using Core;
using Core.EventBus;
using Core.Interface;

namespace Icarus.Ui
{
    // ui에서 뒤늦게 요청하는 경우
    public struct RequestCompassCameraEvent : IEvent { }

    

    // 나침반 원판(카메라)을 위한 이벤트
    public struct SetCompassCameraEvent : IEvent
    {
        public IYRotationProvider Target;
        public SetCompassCameraEvent(IYRotationProvider target) => Target = target;
    }

    /// <summary>
    /// 로컬 카메라에 따라 Compass Direction Canvas 회전
    /// </summary>
    public class CompassDirectionUI : BaseYRotationUI<SetCompassCameraEvent, RequestCompassCameraEvent>, ILateTickable
    {
        public LateTickGroup LateTickGroup => LateTickGroup.Ui;

        // 이벤트에서 인터페이스 추출
        protected override IYRotationProvider GetTargetFromEvent(SetCompassCameraEvent evt) => evt.Target;

        // 원판 회전 공식: Z = WorldY
        protected override float CalculateZRotation(float worldYRotation) => worldYRotation;

        public void LateTick(float dt)
        {
            OnTick(dt);
        }
    }
}

