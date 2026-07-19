using System.Collections;
using UnityEngine;

public class BTask_Wait : BaseRunnableBehaviour
{
    private float waitSeconds;

    public BTask_Wait(float time)
    {
        waitSeconds = time;
    }

    public override IEnumerator OnBehaviorStarted()
    {
        yield return new WaitForSeconds(waitSeconds);
        isSucceeded = true;
    }
}