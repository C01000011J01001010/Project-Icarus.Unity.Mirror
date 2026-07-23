using Core;
using Core.Interface;
using Icarus.Ui;

namespace Icarus.Camera
{
    public interface ICameraRotationProvider : IYRotationProvider { }
    public class CameraInfoProvider : BaseInterfaceProvider<ICameraRotationProvider>, ICameraRotationProvider
    {
        // 인터페이스 구현: 내 Transform의 Y 각도를 반환
        public float WorldYRotation => transform.eulerAngles.y;
    }
}


