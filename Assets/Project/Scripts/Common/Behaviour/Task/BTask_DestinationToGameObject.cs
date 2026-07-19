using System.Collections;
using UnityEngine;

public class BTask_DestinationToGameObject : BaseRunnableBehaviour
{
    // 목적지를 저장하기 위한 키
    private string KEY_Destination;

    // 목표 게임오브젝트 키
    private string KEY_TargetGameObject;

    public BTask_DestinationToGameObject(string KEY_Destination, string KEY_TargetGameObject)
    {
        this.KEY_Destination = KEY_Destination;
        this.KEY_TargetGameObject = KEY_TargetGameObject;
    }

    public override IEnumerator OnBehaviorStarted()
    {
        if(behaviourController.PropertyDict.TryGetValue(KEY_TargetGameObject, out object value))
        {
            behaviourController.PropertyDict[KEY_Destination] = (value as GameObject).transform.position;

            isSucceeded = true; 
        }
        else
        {
            isSucceeded = false;
        }

        yield return null;
    }
}