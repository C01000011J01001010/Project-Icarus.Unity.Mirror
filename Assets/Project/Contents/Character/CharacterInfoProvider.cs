using CoreEngine;
using CoreEngine.Interface;
using UnityEngine;

namespace Icarus.Character
{
    public interface ICharacterRotationProvider : IYRotationProvider { }
    public class CharacterInfoProvider : BaseSinglePublisher<ICharacterRotationProvider>, ICharacterRotationProvider
    {
        // 핵심 비즈니스 로직에만 집중!
        public float WorldYRotation => transform.eulerAngles.y;
    }
}