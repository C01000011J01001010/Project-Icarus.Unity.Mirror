using System;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BaseSequenceInputDetector : MonoBehaviour
{
    protected static BaseSequenceInputDetector CurrentCommand;
    // 입력할 커맨드 리스트
    protected abstract Key[] sequence { get; } //= new Key[] { Key.W, Key.A, Key.S, Key.D };

    // 입력 제한 시간
    protected abstract float timeLimit { get; set; } // = 1.0f;

    protected int currentIndex = 0;
    protected float timeTaken;

    //protected void OnEnable()
    //{
    //    GameManager.UPDATE_EVENT_Post -= CALLBACK_UPDATE;
    //    GameManager.UPDATE_EVENT_Post += CALLBACK_UPDATE;
        
    //    Keyboard.current.onTextInput -= OnTextInput;
    //    Keyboard.current.onTextInput += OnTextInput;
    //}

    //protected void OnDisable()
    //{
    //    GameManager.UPDATE_EVENT_Post -= CALLBACK_UPDATE;
    //    Keyboard.current.onTextInput -= OnTextInput;
    //}


    protected void CALLBACK_UPDATE()
    {
        timeTaken += Time.deltaTime;
        // 타임아웃 체크
        if (currentIndex > 0 && (timeTaken > timeLimit))
        {
            ResetSequence();
        }
    }

    
    protected void OnTextInput(char inputChar)
    {
        Key expectedKey = sequence[currentIndex];

        // 입력 키 비교 (대소문자 구분 없이)
        Key actualKey = CharToKey(inputChar);
        if (actualKey == expectedKey)
        {
            currentIndex++;

            if (currentIndex >= sequence.Length)
            {
                // 성공 시 이벤트 발생
                Debug.Log("Sequence Matched!");
                OnSequenceMatched();
                ResetSequence();
            }
        }
        else
        {
            ResetSequence(); // 틀리면 리셋
        }
    }

    protected void ResetSequence()
    {
        currentIndex = 0;
        timeTaken = 0f;
    }

    protected virtual void OnSequenceMatched()
    {
        // TODO: 원하는 이벤트 실행
        Debug.Log("Triggering special event!");
    }

    protected abstract Key CharToKey(char c);
    //{
    //    c = char.ToLower(c); // 소문자로 통일
    //    switch (c)
    //    {
    //        case 'w': return Key.W;
    //        case 'a': return Key.A;
    //        case 's': return Key.S;
    //        case 'd': return Key.D;
    //        default: return Key.None;
    //    }
    //}
}
