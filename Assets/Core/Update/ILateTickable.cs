using Core.Manager;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Update
{
    public interface ILateTickable 
    { 
        LateTickGroup LateTickGroup { get; }
        void LateTick(float dt); 
    }
}
