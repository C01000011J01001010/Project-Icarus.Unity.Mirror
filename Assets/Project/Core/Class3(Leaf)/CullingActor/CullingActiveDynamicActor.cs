using Core.Manager.Culling;
using UnityEngine;

namespace Core
{
    public class CullingActiveDynamicActor : BaseCullingDynamicActor
    {
        public override CullingType cullingType => CullingType.ActiveDynamic;

        public override void SetVisualActive(bool isActive)
        {
            base.SetVisualActive(isActive);
            // 스스로 움직이는 객체는 스크립트 실행을 위해 Root(gameObject)를 끄지 않고 Renderer만 끕니다!
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].enabled != isActive)
                {
                    _renderers[i].enabled = isActive;
                }
            }
        }

        public override void SetPhysicsActive(bool isActive)
        {
            base.SetPhysicsActive(isActive);
            // 능동적 객체는 화면에 보이지 않아도 땅으로 떨어지거나 벽에 막혀야 하므로
            // Collider를 절대 끄지 않고 무시(Return)합니다.
            return;
        }
    }
}