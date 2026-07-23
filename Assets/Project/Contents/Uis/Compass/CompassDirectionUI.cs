using Core;
using Core.EventBus;
using Core.Interface;
using Icarus.Camera;

namespace Icarus.Ui
{
    /// <summary>
    /// 카메라 y축 회전을 적용하는 ui
    /// </summary>
    public class CompassDirectionUI : BaseYRotationUI<ICameraRotationProvider>, ILateTickable
    {
        public LateTickGroup LateTickGroup => LateTickGroup.Ui;


        // 원판 회전 공식: Z = WorldY
        protected override float CalculateZRotation(float worldYRotation) => worldYRotation;

        public void LateTick(float dt)
        {
            OnTick(dt);
        }
    }
}

