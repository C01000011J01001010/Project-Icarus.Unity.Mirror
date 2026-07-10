using Core.EventBus;
using Core.Hub;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(ManagerHub))]
    [RequireComponent(typeof(UiHub))]
    internal abstract class BaseContext<T> : MonoBehaviour where T : BaseContext<T>
    {
        protected static T _instance;
        public static T Inst => _instance;

#if UNITY_EDITOR
        public bool isInit { get; private set; }
#endif

        [Header("Hubs (2계층)")]
        internal ManagerHub managerHub { get; private set; }
        internal UiHub uiHub { get; private set; }

        // 💡 [변경됨] 단일 제네릭 클래스가 아닌, 비제네릭 인터페이스 리스트로 변경하여 
        // 씬에 배치될 복수의 ActorHub(CharacterHub, ProjectileHub 등)를 유연하게 포용합니다.
        private List<IActorHub> _actorHubs = new List<IActorHub>();

        protected virtual void Awake()
        {
            T thisInstance = this as T;
            if (!thisInstance.TryMakeSingleton(ref _instance))
            {
                Destroy(gameObject);
                return;
            }

            managerHub = GetComponent<ManagerHub>();
            uiHub = GetComponent<UiHub>();

            var foundActorHubs = GetComponents<IActorHub>();

            // 유저님이 제안하신 IPriority 규칙에 맞춰 우선순위(낮은 순) 정렬 후 리스트에 정착시킵니다.
            _actorHubs = foundActorHubs.OrderBy(hub => hub.Priority).ToList();
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public virtual IEnumerator Initialize()
        {
            yield return InitializeHubsSequence();
        }

        protected virtual IEnumerator InitializeHubsSequence()
        {
            // 0. 로딩 시작 알림
            EventBus<SystemLoadingEvent>.Publish(new SystemLoadingEvent(SystemLoadingEvent.State.Start, "시스템 초기화 준비...", 0f));

            // 1. ManagerHub 초기화 (1:1 시스템 매니저들 서비스 세팅)
            if (managerHub != null)
            {
                EventBus<SystemLoadingEvent>.Publish(new SystemLoadingEvent(SystemLoadingEvent.State.Progress, "매니저 시스템 로드 중...", 0.2f));
                yield return managerHub.Initialize();
            }

            // 2. 다중 ActorHub 일괄 순차 초기화
            // 💡 정렬된 순서대로 루프를 돌며 비동기 대기를 수행합니다.
            for (int i = 0; i < _actorHubs.Count; i++)
            {
                float progressRate = 0.3f + ((float)i / _actorHubs.Count * 0.4f); // 0.3 ~ 0.7 구간 진행률 계산
                string hubName = _actorHubs[i].GetType().Name;

                EventBus<SystemLoadingEvent>.Publish(new SystemLoadingEvent(SystemLoadingEvent.State.Progress, $"인게임 {hubName} 세팅 중...", progressRate));
                yield return _actorHubs[i].Initialize();
            }

            // 3. UiHub 초기화 (안전하게 모든 데이터가 완비된 시점에 뷰 로드)
            if (uiHub != null)
            {
                EventBus<SystemLoadingEvent>.Publish(new SystemLoadingEvent(SystemLoadingEvent.State.Progress, "UI 시스템 로드 중...", 0.9f));
                yield return uiHub.Initialize();
            }

            // 4. 로딩 완료 알림
            EventBus<SystemLoadingEvent>.Publish(new SystemLoadingEvent(SystemLoadingEvent.State.Complete, "초기화 완료!", 1.0f));

#if UNITY_EDITOR
            isInit = true;
#endif
        }
    }
}