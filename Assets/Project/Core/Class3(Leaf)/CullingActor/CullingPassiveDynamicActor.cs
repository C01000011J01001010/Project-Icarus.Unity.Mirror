using Core.Manager.Culling;
using UnityEngine;

namespace Core
{
    public class CullingPassiveDynamicActor : BaseCullingDynamicActor
    {
        public override CullingType cullingType => CullingType.PassiveDynamic;

        public override void SetVisualActive(bool isActive)
        {
            base.SetVisualActive(isActive);
            // 시각적으로 꺼질 때 오브젝트 통째로 끄기
            if (gameObject.activeSelf != isActive)
            {
                gameObject.SetActive(isActive);
            }
        }

        public override void SetPhysicsActive(bool isActive)
        {
            base.SetPhysicsActive(isActive);
            // 물리 연산(Collider)만 끄고 켭니다. (시각은 켜져있으나 거리가 멀어졌을 때 작동)
            for (int i = 0; i < _colliders.Length; i++)
            {
                if (_colliders[i] != null && _colliders[i].enabled != isActive)
                {
                    _colliders[i].enabled = isActive;
                }
            }
        }
    }
}