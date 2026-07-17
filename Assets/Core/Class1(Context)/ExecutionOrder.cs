using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    internal enum ExecutionOrder
    {
        ProjectContext = -100,
        SceneContext = -99,
        SceneTester = -88,
    }
}
