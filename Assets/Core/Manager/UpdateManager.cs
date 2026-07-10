using UnityEngine;

public delegate void DelegateUpdate(float delataTime);

/// <summary>
/// EventBus를 쓰지 않고 직접 연결해제
/// </summary>
public class UpdateManager : MonoBehaviour
{
    #region Event_Update
    // 프레임 시작 시 데이터를 초기화하는 단계의 업데이트
    public static event DelegateUpdate UPDATE_Initial;

    // 컨트롤러의 업데이트 진행! => 파악된 상황을 통해 캐릭터에 정보 전달
    public static event DelegateUpdate UPDATE_OnController;

    // 전달된 정보를 통해서 캐릭터가 활동
    public static event DelegateUpdate UPDATE_OnCharacter;

    // 캐릭터가 오브젝트에 관여하고 나서 오브젝트가 발동
    public static event DelegateUpdate UPDATE_Object;
    #endregion

    #region Event_LateUpdate
    public static event DelegateUpdate UPDATE_Camera;

    // 프레임 종료 시 변환된 데이터를 정리하는 단계의 업데이트
    public static event DelegateUpdate UPDATE_Post;
    #endregion

    #region Event_FixedUpdate
    // 매 FixedUpdate마다 물리적 업데이트를 해주는 것
    public static event DelegateUpdate UPDATE_Physics;
    #endregion

    public static void ClearEventUpdate()
    {
        UPDATE_Initial = null;
        UPDATE_OnController = null;
        UPDATE_OnCharacter = null;
        UPDATE_Object = null;
        UPDATE_Camera = null;
        UPDATE_Post = null;
        UPDATE_Physics = null;
    }

    private void FixedUpdate()
    {
        // GameManager의 일시정지 플래그를 확인
        //if (GameManager.IsUpdatePaused()) return;


        float fixedDeltaTime = Time.fixedDeltaTime;
        //if (GameManager.Inst != null && GameManager.Inst.IsInit && WorldManager.IsInit)
        {
            UPDATE_Physics?.Invoke(fixedDeltaTime);
        }
    }

    private void Update()
    {
        //if (GameManager.IsUpdatePaused()) return;

        float deltaTime = Time.deltaTime;
        //if (GameManager.Inst != null && GameManager.Inst.IsInit && WorldManager.IsInit)
        {
            UPDATE_Initial?.Invoke(deltaTime);
            UPDATE_OnController?.Invoke(deltaTime);
            UPDATE_OnCharacter?.Invoke(deltaTime);
            UPDATE_Object?.Invoke(deltaTime);
        }
    }

    private void LateUpdate()
    {
        //if (GameManager.IsUpdatePaused()) return;

        float deltaTime = Time.deltaTime;
        //if (GameManager.Inst != null && GameManager.Inst.IsInit && WorldManager.IsInit)
        {
            UPDATE_Camera?.Invoke(deltaTime);
            UPDATE_Post?.Invoke(deltaTime);
        }
    }
}