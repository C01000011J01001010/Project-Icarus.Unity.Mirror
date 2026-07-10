using Core.Hub;

public enum CharacterType
{
    CapsuleMan,
}

public class CharacterActorHub : ActorHub<CharacterType>
{
    public override int Priority => (int)ActorHubPriority.Character;
}
