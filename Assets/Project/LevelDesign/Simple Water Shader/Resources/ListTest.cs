using System;
using UnityEngine;
using CoreEngine;


[RequireComponent(typeof(CullingActiveDynamicActor))]
public class ListTest : MonoBehaviour
{
    [Serializable] 
    struct ToNext
    {
        public Vector3 WolrdPosition;
        public Vector3 WolrdRotation;
    }
    [SerializeField] private ToNext[] pos;

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i< pos.Length; i++)
        {

        }
            
    }


}
