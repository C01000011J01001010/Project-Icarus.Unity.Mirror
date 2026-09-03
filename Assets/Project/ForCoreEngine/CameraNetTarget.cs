using UnityEngine;
using CoreEngine;
using CoreEngine.CameraSystem;


namespace Icarus.Camera
{
    public class CameraNetTarget : CameraTargetProvider
    {
        private Transform _followTarget;

        // 부모의 회전 간섭을 피하기 위해 런타임에 계층을 분리합니다.
        public void DecoupleAndFollow(Transform followTarget)
        {
            _followTarget = followTarget;
            transform.SetParent(null);
        }

        // 오브젝트 풀로 반환될 때 다시 원래 부모의 자식으로 들어갑니다.
        public void ReturnToParent(Transform originalParent)
        {
            _followTarget = null;
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        private void LateUpdate()
        {
            // 부모-자식 관계가 끊어졌으므로, 위치(Position)만 수동으로 따라갑니다.
            if (_followTarget != null)
            {
                transform.position = _followTarget.position;
            }
        }

        public static implicit operator Transform(CameraNetTarget _this)
        {
            return _this.Target;
        }
    }

}
