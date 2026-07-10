using Core.EventBus;
using UnityEngine;

namespace Core.EventBus.Example
{
    /// <summary>
    /// 메모리 해제 수동조작 예시
    /// </summary>
    public class ExampleListener_PlayerSoundEffect : BaseEventListener_Manual
    {
        [SerializeField] private int _targetPlayerId = 1;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _hitSound;

        protected override void RegisterEvents()
        {
            SubscribeTo<ExampleEvent_PlayerDamaged>(PlayHitSound);
        }

        protected override void UnregisterEvents()
        {
            UnsubscribeFrom<ExampleEvent_PlayerDamaged>(PlayHitSound);
        }

        private void PlayHitSound(ExampleEvent_PlayerDamaged evt)
        {
            if (evt.PlayerId != _targetPlayerId) return;

            // 체력이 깎였을 때 피격 사운드 재생
            if (_audioSource != null && _hitSound != null)
            {
                _audioSource.PlayOneShot(_hitSound);
                Debug.Log($"[PlayerSoundEffect] 플레이어 {evt.PlayerId} 피격 사운드 재생!");
            }
        }
    }
}
