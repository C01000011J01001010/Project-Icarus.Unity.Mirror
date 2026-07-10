using Core.Hub;

public enum ControllerType
{
    PlayerController,
    InputSender,
}

public class ControllerActorHub : ActorHub<ControllerType>
{
    public override int Priority => (int)ActorHubPriority.Controller;
}