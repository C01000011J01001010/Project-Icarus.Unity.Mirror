using UnityEngine;
using CoreEngine;
using CoreEngine.Actor;
using System;

namespace CoreEngine.Actor
{
    /// <summary>
    /// Animatior 컴포넌트를 제어하기 위한 컴포넌트
    /// </summary>
    [Serializable]
    public abstract class BaseAnim : BaseActorFeature
    {
        [SerializeField]
        protected Animator _animator;
        public Animator animator
        {
            get
            {
                if (_animator != null) return _animator;
                _animator = Host.GetComponentInChildren<Animator>();
                return _animator;
            }
        }

        protected abstract void GetAnimPrarmHash();

        protected void SetParam(int ParamHash, bool value) => animator.SetBool(ParamHash, value);
        protected void SetParam(int ParamHash, float value) => animator.SetFloat(ParamHash, value);
        protected void SetParam(int ParamHash, int value) => animator.SetInteger(ParamHash, value);
        protected void SetParam(int ParamHash) => animator.SetTrigger(ParamHash);
    }
}

