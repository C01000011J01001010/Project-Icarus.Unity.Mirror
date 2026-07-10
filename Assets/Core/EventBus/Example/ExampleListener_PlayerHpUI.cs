using Core.EventBus;
using UnityEngine;
using UnityEngine.UI;

namespace Core.EventBus.Example
{
    /// <summary>
    /// 메모리 해제 자동화 예시
    /// </summary>
    public class ExampleListener_PlayerHpUI : BaseEventListener_Automatic
    {
        [SerializeField] private int _targetPlayerId = 1; // 이 UI가 담당할 플레이어 ID
        [SerializeField] private Image _hpSliderImage;

        // 🎯 컴파일러가 강제하는 이벤트 등록 함수
        protected override void RegisterEvents()
        {
            SubscribeTo<ExampleEvent_PlayerDamaged>(OnPlayerDamaged);
        }

        // 이벤트가 들어왔을 때 실행될 로직
        private void OnPlayerDamaged(ExampleEvent_PlayerDamaged evt)
        {
            // 내 담당 플레이어가 아니면 무시 (필터링)
            if (evt.PlayerId != _targetPlayerId) return;

            // UI 갱신
            float hpRatio = (float)evt.CurrentHp / evt.MaxHp;
            _hpSliderImage.fillAmount = hpRatio;

            Debug.Log($"[PlayerHpUI] 플레이어 {evt.PlayerId}의 체력바를 {evt.CurrentHp}로 갱신했습니다.");
        }
    }
}

