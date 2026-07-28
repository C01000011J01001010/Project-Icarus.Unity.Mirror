using CoreEngine;
using CoreEngine.Interface;
using UnityEngine;

namespace Icarus.Character
{
    public interface ICharacterRotationProvider : IYRotationProvider { }
    public class CharacterInfoProvider : MonoBehaviour,
        ICharacterRotationProvider, IMapTargetProvider
    {
        // 핵심 비즈니스 로직에만 집중!
        public float WorldYRotation => transform.eulerAngles.y;

        public Vector3 WorldPosition => transform.position;

        private readonly InterfaceBinderContainer _binders = new();

        protected virtual void Awake()
        {
            _binders.Add(new InterfacePublisher<ICharacterRotationProvider>(this));
            _binders.Add(new InterfacePublisher<IMapTargetProvider>(this));
        }

        // 자식 클래스에서 재정의(override)할 일이 생길 수 있으므로 virtual로 열어둡니다.
        protected virtual void OnEnable() => _binders.BindAll();
        protected virtual void OnDisable() => _binders.UnbindAll();
    }
}