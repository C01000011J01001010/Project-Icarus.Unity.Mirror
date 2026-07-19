using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core.Director
{
    public class BaseDirector<Director> : Singleton<Director>, IDirector
        where Director : BaseDirector<Director>
    {
    }
}
