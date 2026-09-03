using UnityEngine;
using System;
using CoreEngine.Actor;

namespace Icarus.Character
{
    [Serializable]
    public class SharedActorAnimationFeature : BaseActorFeature
    {
        private Animator _animator;

        protected override void OnInitialized()
        {
            _host.TryGetComponent(out _animator);
        }

        public void PlayFlap(bool isLeft)
        {
            if (_animator == null) return;
            _animator.SetTrigger(isLeft ? "FlapLeft" : "FlapRight");
        }
    }
}