using UnityEngine;
using System;
using CoreEngine.Actor;
using CoreEngine.Helpers;

namespace Icarus.Character
{
    [Serializable]
    public class SharedActorMovementFeature : BaseActorFeature
    {
        [Header("이동 세팅")]
        [SerializeField] private float moveSpeed = 20f;
        [SerializeField] private float rotationSpeed = 1.0f;

        [Header("날갯짓 & 균형 세팅")]
        [SerializeField] private float flapForce = 12f;
        [SerializeField] private float flapTorque = 1f;
        [SerializeField] private float pGain = 20f;
        [SerializeField] private float dGain = 5f;
        [SerializeField] private float timeDelay = 1f;

        private float timeCount;
        private Rigidbody _rigidbody;
        private SharedActor _actor;

        protected override void OnInitialized()
        {
            _actor = _host as SharedActor;
            // GetComponent 중복 방지: 미리 캐싱된 Host의 자원을 신뢰하고 가져옵니다.
            _rigidbody = _host.gameObject.GetComponent<Rigidbody>();
        }

        public void ApplyFlap(bool isLeft)
        {
            _rigidbody.AddForce(_host.transform.up * flapForce, ForceMode.Impulse);
            float torqueDirection = isLeft ? -1f : 1f;
            _rigidbody.AddTorque(Vector3.forward * torqueDirection * flapTorque, ForceMode.Impulse);
            timeCount = 0;
        }

        public void FixedTick(float fixedDeltaTime)
        {
            Move(fixedDeltaTime);
            StabilizeRotation(fixedDeltaTime);
        }

        private void Move(float fixedDeltaTime)
        {
            Vector2 combinedInput = Vector2.zero;

            // 개선점: .Values를 제외하여 매 틱당 발생하는 GC 메모리 할당을 0으로 만듭니다.
            foreach (var kvp in _actor.ClientInputs)
            {
                combinedInput += kvp.Value;
            }

            if (combinedInput != Vector2.zero)
            {
                Vector3 moveDir = new Vector3(combinedInput.x, 0, combinedInput.y).normalized;
                RigidBodyHelper.SmoothLookAt(_rigidbody, moveDir, rotationSpeed, fixedDeltaTime);
                _rigidbody.AddForce(moveDir * moveSpeed, ForceMode.Force);
            }
        }

        private void StabilizeRotation(float fixedDeltaTime)
        {
            if (timeCount <= timeDelay)
            {
                timeCount += fixedDeltaTime;
                return;
            }
            Vector3 error = Vector3.Cross(_host.transform.up, Vector3.up);
            Vector3 torque = (error * pGain) - (_rigidbody.angularVelocity * dGain);
            _rigidbody.AddTorque(torque, ForceMode.Force);
        }
    }
}