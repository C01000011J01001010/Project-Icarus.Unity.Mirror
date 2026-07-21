using UnityEngine;
using FishNet;
using FishNet.Object;
namespace Core
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
