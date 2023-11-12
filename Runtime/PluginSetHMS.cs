#if ENABLE_HMS
using System;
using System.Collections;
using PluginSet.Core;

namespace PluginSetHMS.Runtime
{
    [PluginRegister]
    public partial class PluginSetHMS: PluginBase, IStartPlugin
    {
        [AttributeUsage(AttributeTargets.Method)]
        private class HMSStartExecutableAttribute: ExecutableAttribute
        {
        }
        
        [AttributeUsage(AttributeTargets.Method)]
        private class HMSDisposeExecutableAttribute: ExecutableAttribute
        {
        }
        
        public override string Name => "Huawei";

        public int StartOrder => PluginsStartOrder.SdkDefault;
        public bool IsRunning { get; private set; }
        public IEnumerator StartPlugin()
        {
            ExecuteAll<HMSStartExecutableAttribute>();
            IsRunning = true;
            yield break;
        }

        public void DisposePlugin(bool isAppQuit = false)
        {
            ExecuteAll<HMSDisposeExecutableAttribute>();
            IsRunning = false;
        }
    }
}
#endif