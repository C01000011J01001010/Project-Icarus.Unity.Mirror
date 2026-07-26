using UnityEngine;
using FishNet;
using FishNet.Object;
namespace CoreEngine
{
    public class BaseMono : MonoBehaviour
    {
        private void Awake()
        {
            if(GetComponents(GetType()).Length > 1)
            {
                Destroy(this);
            }
        }
    }

    public class BaseMonoNetworked : NetworkBehaviour
    {
        private void Awake()
        {
            if (GetComponents(GetType()).Length > 1)
            {
                Destroy(this);
            }
        }
    }
}
