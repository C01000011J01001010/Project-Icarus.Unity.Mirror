using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Manager;

public class CharacterManager : MonoBehaviour, IScenedManager
{
    [SerializeField] private int _priority;
    public int Priority => _priority;

    public bool IsActive => throw new System.NotImplementedException();

    List<BaseCharacter> _characterList;

    public void Exit()
    {
        UpdateManager.UPDATE_OnCharacter -= CALLBACK_Update;
        UpdateManager.UPDATE_Physics -= CALLBACK_FixedUpdate;
    }

    public IEnumerator Initialize()
    {
        _characterList = new();
        UpdateManager.UPDATE_OnCharacter -= CALLBACK_Update;
        UpdateManager.UPDATE_OnCharacter += CALLBACK_Update;
        UpdateManager.UPDATE_Physics -= CALLBACK_FixedUpdate;
        UpdateManager.UPDATE_Physics += CALLBACK_FixedUpdate;

        yield return null;
    }

    public IEnumerator LateInitialize()
    {
        yield break;
    }

    public void AddList(BaseCharacter character)
    {
        if (!_characterList.Contains(character))
        {
            _characterList.Add(character);
        }
    }

    public void RemoveList(BaseCharacter character)
    {
        _characterList.Remove(character);
    }

    public virtual void CALLBACK_Update(float deltaTime)
    {
        for(int i = _characterList.Count-1; i >= 0; --i)
        {
            _characterList[i].Tick(deltaTime);
        }
    }

    public virtual void CALLBACK_FixedUpdate(float deltaTime)
    {
        for (int i = _characterList.Count - 1; i >= 0; --i)
        {
            _characterList[i].FixedTick(deltaTime);
        }
    }

    public void SetActive(bool active)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator Initialize(IModuleHub hub)
    {
        throw new System.NotImplementedException();
    }
}
