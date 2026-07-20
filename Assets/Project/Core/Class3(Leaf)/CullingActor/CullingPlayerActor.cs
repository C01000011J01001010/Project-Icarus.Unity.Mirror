using Core.EventBus;
using Core.Hub;
using Core.Manager.Culling;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core
{
    public class CullingPlayerActor : BaseActor, ITickable
    {
        // 플레이어 컨트롤러 또는 위치 추적 스크립트 내부
        private Vector3Int _lastGridKey = new Vector3Int(int.MinValue, 0, 0);

        public TickGroup TickGroup => TickGroup.Object;

        // UpdateManager에 등록된 Tick 등에서 실행
        public void Tick(float deltaTime)
        {
            Vector3Int currentGrid = CoreFacade.GetGridKey(transform.position);

            // 격자가 이전과 달라졌을 때만! (c거리 트리거의 정수화 버전)
            if (currentGrid != _lastGridKey)
            {
                _lastGridKey = currentGrid;

                // 허공에 "나 격자 바뀌었다!" 방송 (수백 개의 객체 Culling 로직이 단 한 번 실행됨)
                EventBus<CullingPlayerGridChangedEvent>.Publish(new CullingPlayerGridChangedEvent(currentGrid));

                Utility.Log($"여기 격자 바뀌었어요! 현재격자: {currentGrid}");
            }
        }
    }
}
