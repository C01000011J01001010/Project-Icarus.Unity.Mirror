using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core.Director
{
    internal class BaseDirector<Director> : Singleton<Director>, IDirector
        where Director : MonoBehaviour
    {
    }
}
