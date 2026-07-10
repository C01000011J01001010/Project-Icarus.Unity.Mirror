
public enum ActorHubPriority
{
    System = 100,       // 맵, 환경 요소 등
    Character = 200,    // 캐릭터 스폰
    Vehicle = 300,      // 탈것 스폰
    Camera = 400,       // 카메라 타겟 세팅
    Controller = 500,   // 가장 마지막에 뇌(Player/AI) 연결
}