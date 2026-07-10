using Core;
using UnityEngine.SceneManagement;




/// <summary>
/// Additive로 로드되는 개별 씬마다 존재하는 컨텍스트
/// 씬이 언로드될 때 자연스럽게 파괴되며 메모리를 정리
/// </summary>
internal class SceneContext : BaseContext<SceneContext>
{
    // Start()를 구현하지 않습니다! 
    // 자기 멋대로 초기화를 시작하면 SceneLoadManager의 통제를 벗어나기 때문입니다.

    // SceneContext의 Initialize()는 SceneLoadManager.LoadingScene() 완료 직전에 호출됩니다.

    protected override void Awake()
    {
        base.Awake();

        // 💡 유저님의 아이디어: 내 게임오브젝트가 속한 씬 프로퍼티를 그대로 디렉터에게 패스!
        Scene currentSceneDomain = gameObject.scene;

        // SceneFlowDirector에게 이 씬이 현재 활성화된 메인 도메인임을 선언
        SceneFlowDirector.RegisterCurrentScene(currentSceneDomain);
    }
}