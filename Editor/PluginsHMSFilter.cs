using System;
using System.Collections.Generic;
using HmsPlugin;
using PluginSet.Core.Editor;
using PluginSet.HMS.Editor;
using UnityEditor;
using UnityEngine;

namespace PluginSetUAPM.Editor
{
    [InitializeOnLoad]
    public static class PluginsHMSFilter
    {
        static PluginsHMSFilter()
        {
            var fileter = PluginFilter.IsBuildParamsEnable<BuildHMSParams>();
            PluginFilter.RegisterFilter("com.pluginset.huawei/Plugins", fileter);
            PluginFilter.RegisterFilter("com.pluginset.huawei/Plugins/Android", new Dictionary<string, Func<string, BuildProcessorContext, bool>>()
            {
                {"app-debug.aar", fileter},
                {"BookInfo.java", IsHmsEnable(CloudDBToggleEditor.CloudDBEnabled)},
                {"ObjectTypeInfoHelper.java", IsHmsEnable(CloudDBToggleEditor.CloudDBEnabled)},
                {"HMSUnityPushKit.plugin", IsHmsEnable(PushToggleEditor.PushKitEnabled)},
                {"HMSUnityModelingKit.plugin", IsHmsEnable(Modeling3dKitToggleEditor.Modeling3dkitEnabled)},
            });
        }

        public static Func<string, BuildProcessorContext, bool> IsHmsEnable(string key)
        {
            return delegate(string s, BuildProcessorContext context)
            {
                if (PluginFilter.IsBuildParamsEnable<BuildHMSParams>()(s, context))
                    return true;

                return !HMSMainEditorSettings.Instance.Settings.GetBool(key);
            };
        }
    }
}
