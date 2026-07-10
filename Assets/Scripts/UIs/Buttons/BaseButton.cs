
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BaseButton : MonoBehaviour, IInitialize
{
    private Button _button;
    private Button.ButtonClickedEvent _onClick;


    public void Exit()
    {

    }

    public IEnumerator Initialize()
    {
        if(!TryGetComponent(out _button))
        {
            Debug.LogError("버튼 컴포넌트 캐싱 실패");
            yield break;
        }
        _onClick = _button.onClick;
        yield return null;
    }

    public IEnumerator LateInitialize()
    {
        yield break;
    }



    public bool IsInteractable => _button.interactable;

    

    /// <summary>
    /// 콜백함수는 오로지 하나만 넣을 수 있으며, 여러 개를 넣을 시 람다함수로 여러 함수를 묶어야함
    /// </summary>
    /// <param name="callback"></param>
    public virtual void SetCallback(UnityAction callback)
    {
        _onClick.RemoveAllListeners();
        _onClick.AddListener(callback);
    }

    public virtual void AddCallback(UnityAction callback)
    {
        _onClick.RemoveListener(callback);
        _onClick.AddListener(callback);
    }

    public virtual void ClearCallback()
    {
        _onClick.RemoveAllListeners();
    }

    public virtual void SetInteractable(bool isOn)
    {
        _button.interactable = isOn;
    }
}
