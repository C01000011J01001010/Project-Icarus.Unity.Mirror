using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Update
{
    public interface ILateTickable { void LateTick(float dt); }
}
