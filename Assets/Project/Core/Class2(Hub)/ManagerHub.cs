
using System.Collections;

namespace CoreEngine.Hub
{
    internal sealed class ManagerHub : BaseModuleHub<IManager>
    {
        protected override bool moduleEnabled => true;

        public override IEnumerator Initialize()
        {
            Utility.LogFunctionCallStart(this);
            return base.Initialize();
        }

        public override IEnumerator LateInitialize()
        {
            Utility.LogFunctionCallStart(this);
            return base.LateInitialize();
        }
    }
}
