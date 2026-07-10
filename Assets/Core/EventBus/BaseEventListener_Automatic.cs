using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.EventBus
{
    /// <summary>
    /// 해제 로직을 자동화한 EventListener
    /// <para>기본 EventListener로 사용</para>
    /// <para>익명함수의 메모리 차지를 고려하지 않아도 되는 수준에 사용권장</para>
    /// </summary>
    public abstract class BaseEventListener_Automatic : MonoBehaviour
    {
        // 중복 등록 방지용 해시셋
        private HashSet<object> _subscribedHandlers = new();

        // '구독 해제 로직'을 담아둘 리스트
        private List<Action> _unsubscribeActions = new();

        // 1. 자식 클래스에서 이벤트를 등록할 때 호출하는 함수
        protected void SubscribeTo<T>(Action<T> handler) where T : struct, IEvent
        {
            // 중복 등록을 예방
            if (_subscribedHandlers.Add(handler))
            {
                // 이벤트 버스에 실제 등록
                EventBus<T>.Subscribe(handler);

                // 이 이벤트를 해제하는 '익명 함수(Closure)'를 만들어 리스트에 보관
                _unsubscribeActions.Add(() =>
                {
                    EventBus<T>.Unsubscribe(handler);
                    _subscribedHandlers.Remove(handler); // 깔끔한 상태 초기화
                });
            }
        }

        // 자식 클래스에게 강제하는 추상 메서드
        protected abstract void RegisterEvents();

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            // 자식 클래스가 해제를 신경 쓸 필요 없이
            // 부모클래스가 알아서 전부 해제함
            foreach (var unsubscribeAction in _unsubscribeActions)
            {
                unsubscribeAction?.Invoke();
            }

            // 리스트 비우기
            _unsubscribeActions.Clear();
        }
    }
}

