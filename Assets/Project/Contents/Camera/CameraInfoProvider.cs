using Core;
using Core.Interface;
using Icarus.Ui;

namespace Icarus.Camera
{
    public class CameraInfoProvider : 
        BaseInterfaceProvider<RequestCompassCameraEvent, SetCompassCameraEvent>, IYRotationProvider
    {
        // 인터페이스 구현: 내 Transform의 Y 각도를 반환
        public float WorldYRotation => transform.eulerAngles.y;

        protected override SetCompassCameraEvent GetPublishEvent()
        {
            return new SetCompassCameraEvent(this);
        }
    }
}


