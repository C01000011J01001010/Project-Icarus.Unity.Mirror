using Core;
using Icarus.Ui;
using Core.Interface;


namespace Icarus.Character
{
    public class CharacterInfoProvider : 
        BaseInterfaceProvider<RequestCompassCharacterEvent, SetCompassCharacterEvent>, IYRotationProvider
    {
        // 인터페이스 구현: 내(항아리) Transform의 Y 각도를 반환
        public float WorldYRotation => transform.eulerAngles.y;

        protected override SetCompassCharacterEvent GetPublishEvent()
        {
            return new SetCompassCharacterEvent(this);
        }
    }
}