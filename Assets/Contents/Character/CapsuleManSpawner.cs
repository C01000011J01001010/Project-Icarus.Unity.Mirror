using UnityEngine;
using FishNet.Object;

public class CapsuleManSpawner : NetworkBehaviour
{
    [Tooltip("스폰할 캡슐맨 프리팹을 여기에 넣으세요")]
    public GameObject capsuleManPrefab;

    [Tooltip("스폰 위치 결정")]
    [SerializeField] private Transform SpawnPoint;

    // 호스트(서버) 권한이 완전히 준비된 직후에만 실행되는 안전한 콜백
    public override void OnStartServer()
    {
        base.OnStartServer();

        // 1. 유니티의 기본 기능으로 씬 허공(예: 높이 5)에 캡슐맨을 생성합니다.
        Vector3 spawnPoint = SpawnPoint? SpawnPoint.position : new Vector3(0, 5, 0);
        GameObject spawnedCapsule = Instantiate(capsuleManPrefab, spawnPoint, Quaternion.identity);

        // 2. FishNet 서버에게 "이 객체를 스폰해라!" 라고 명령합니다.
        // 💡 씬 객체가 아니라 동적으로 스폰된 객체는 ObserverManager가 절대 렌더러를 끄지 않습니다!
        ServerManager.Spawn(spawnedCapsule); // 서버소유
        //ServerManager.Despawn

        Debug.Log("🎉 캡슐맨이 서버의 가호 아래 완벽하게 동적 스폰되었습니다!");
    }
}