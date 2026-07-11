using UnityEngine;
using FishNet.Object;

public class MeshCCTV : MonoBehaviour
{
    private MeshRenderer _renderer;
    private NetworkObject _nob;

    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _nob = GetComponent<NetworkObject>();
    }

    void Update()
    {
        // 렌더러가 꺼지는 그 순간을 포착!
        if (_renderer != null && !_renderer.enabled)
        {
            Debug.LogError("🚨 범인 검거! 누군가 방금 MeshRenderer를 껐습니다!");

            // FishNet 정상 스폰 여부 확인
            if (_nob != null && !_nob.IsSpawned)
            {
                Debug.LogError("👉 힌트: FishNet에 정식 스폰(Spawn)되지 않은 객체라서 FishNet이 강제로 끈 것입니다. ServerManager.Spawn()을 호출했는지 확인하세요.");
            }

            // 유니티 에디터를 이 프레임에서 강제로 일시정지 시킵니다!
            Debug.Break();
        }
    }
}