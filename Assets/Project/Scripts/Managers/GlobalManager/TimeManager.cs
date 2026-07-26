using System;
using System.Collections;
using UnityEngine;
using static Constants;
using CoreEngine;

// 매니저 클래스중 가장 마지막에 초기화
public class TimeManager : BaseGlobalManager, IGlobalManager, ITickable
{
    // 현실 24분(1440초) = 게임 내 하루

    /// <summary>
    /// 게임 속 하루를 현실 시간 초로 환산한 값<br/>
    /// *현실의 1분은 게임속 1시간
    /// </summary>
    public static float DayLength { get; private set; } = 24 * 60;

    /// <summary>
    /// 플레이어 새게임을 시작하고 경과한 총 시간
    /// </summary>
    public double TotalGameSeconds { get; private set; }

    public TickGroup TickGroup => TickGroup.Initial;

    public event System.Action<int/*분*/> Event_OnGameMinutesPassed; // 현실 1초마다 발생
    public event System.Action<int/*분*/> Event_OnGameHoursPassed;   // 현실 1분마다 발생

    private float timerForMinute = 0f;
    private float timerForHour = 0f;

    

    //public void Exit()
    //{
    //    UpdateManager.UPDATE_Initial -= OnUpdate;
    //}

    //public IEnumerator Initialize()
    //{
    //    UpdateManager.UPDATE_Initial += OnUpdate;
    //    yield break;
    //}

    /// <summary>
    /// 타이틀 씬에서 플레이어 선택후 씬 전환시 실행
    /// </summary>
    public void OnPlayerGameStart()
    {
        TotalGameSeconds = LoadPlayerLastTotalTime();
        DateTime lastSaveTime = LoadPlayerLastPlayDataTime();
        TimeSpan offlineDelta = DateTime.UtcNow - lastSaveTime;
        UpdateTime((float)offlineDelta.TotalSeconds);
    }

    public double LoadPlayerLastTotalTime()
    {
        // TODO: 세이브된 최종 플레이 타임 로드
        return 0;
    }

    public DateTime LoadPlayerLastPlayDataTime()
    {
        // TODO: 마지막 플레이 했던 날짜, 시간 데이터 로드
        return DateTime.UtcNow;
    }

    private void OnPlayerSleep()
    {
        // 8시간 처리
        UpdateTime(8 * REAL_SECONDS_PER_GAME_HOUR);
    }

    private void UpdateTime(float deltaTime)
    {
        TotalGameSeconds += deltaTime;
        timerForMinute += deltaTime;
        timerForHour += deltaTime;

        if (timerForMinute >= REAL_SECONDS_PER_GAME_MINUTE)
        {
            int deltaGameMinutes = ((int)timerForMinute) / REAL_SECONDS_PER_GAME_MINUTE;
            timerForMinute -= (deltaGameMinutes * REAL_SECONDS_PER_GAME_MINUTE);
            Event_OnGameMinutesPassed?.Invoke(deltaGameMinutes);
        }

        if (timerForHour >= REAL_SECONDS_PER_GAME_HOUR)
        {
            int deltaGameHours = ((int)timerForHour) / REAL_SECONDS_PER_GAME_HOUR;
            timerForHour -= (deltaGameHours * REAL_SECONDS_PER_GAME_HOUR);
            Event_OnGameHoursPassed?.Invoke(deltaGameHours);
        }
    }


    // 게임속에서 현재가 몇 일째인지 반환
    public int GetCurrentDay() => (int)(TotalGameSeconds / DayLength) + 1;

    public void SetWorldTime(float dayLength)
    {
        DayLength = dayLength;
    }

    public void Tick(float deltaTime)
    {
        UpdateTime(Time.unscaledDeltaTime);
    }
}
