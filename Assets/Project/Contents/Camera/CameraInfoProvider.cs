using Core;
using Core.Interface;

namespace Icarus.Camera
{
    public interface ICameraRotationProvider : IYRotationProvider { }
    // 💡 1. 상속의 자유: 이제 MonoBehaviour를 직접 상속받거나 다른 부모를 가질 수 있습니다.
    public class CameraInfoProvider : BaseSinglePublisher<ICameraRotationProvider>, ICameraRotationProvider
    {
        public float WorldYRotation => transform.eulerAngles.y;
    }
}


