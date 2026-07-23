using Core;
using Core.EventBus;
using Core.Interface;


namespace Icarus.Ui
{
    // ui에서 뒤늦게 요청하는 경우
    public struct RequestCompassCharacterEvent : IEvent { }

    // 화살표(캐릭터)를 위한 이벤트
    public struct SetCompassCharacterEvent : IEvent
    {
        public IYRotationProvider Target;
        public SetCompassCharacterEvent(IYRotationProvider target) => Target = target;
    }

    public class CompassArrowUI : BaseYRotationUI<SetCompassCharacterEvent, RequestCompassCharacterEvent>, ITickable
    {
        public TickGroup TickGroup => TickGroup.Ui;

        // 이벤트에서 인터페이스 추출
        protected override IYRotationProvider GetTargetFromEvent(SetCompassCharacterEvent evt) => evt.Target;

        // 화살표 회전 공식: Z = -WorldY
        protected override float CalculateZRotation(float worldYRotation) => -worldYRotation;

        public void Tick(float dt)
        {
            OnTick(dt);
        }
    }
}

