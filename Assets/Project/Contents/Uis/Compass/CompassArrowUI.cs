using CoreEngine;
using CoreEngine.EventBus;
using CoreEngine.Interface;
using Icarus.Character;


namespace Icarus.Ui
{
    /// <summary>
    /// 캐릭터 y축 회전을 적용하는 ui
    /// </summary>
    public class CompassArrowUI : BaseYRotationUI<ICharacterRotationProvider>, ITickable
    {
        public TickGroup TickGroup => TickGroup.Ui;

        // 화살표 회전 공식: Z = -WorldY
        protected override float CalculateZRotation(float worldYRotation) => -worldYRotation;

        public void Tick(float dt)
        {
            OnTick(dt);
        }
    }
}

