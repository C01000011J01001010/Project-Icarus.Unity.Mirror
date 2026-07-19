using UnityEngine;

/// <summary>
/// Animatior 컴포넌트를 제어하기 위한 컴포넌트
/// </summary>
[RequireComponent(typeof(Animator))]
public abstract class BaseAnim : MonoBehaviour
{
    private Animator _animator;
    public Animator animator => _animator ?? (_animator = GetComponent<Animator>());

    protected abstract void GetAnimPrarmHash();

    protected void SetParam(int ParamHash, bool value) => animator.SetBool(ParamHash, value);
    protected void SetParam(int ParamHash, float value) => animator.SetFloat(ParamHash, value);
    protected void SetParam(int ParamHash, int value) => animator.SetInteger(ParamHash, value);
    protected void SetParam(int ParamHash) => animator.SetTrigger(ParamHash);
}
