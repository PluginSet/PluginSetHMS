using System.IO;
using HmsPlugin;
using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEditor;

namespace PluginSet.HMS.Editor
{
    [BuildTools]
    public static class BuildHMSTools
    {
        [OnSyncEditorSetting]
        public static void OnSyncEditorSetting(BuildProcessorContext context)
        {
            if (!context.BuildTarget.Equals(BuildTarget.Android))
                return;
            
            
            var buildParams = context.BuildChannels.Get<BuildHMSParams>();
            HMSEditorUtils.SetHMSPlugin(buildParams.Enable, buildParams.Enable);
            
            if (!buildParams.Enable)
                return;
            
            HMSEditorUtils.SetAGConnectConfigFile(buildParams.AGConnectServiceJson);
            
            context.Symbols.Add("ENABLE_HMS");

            if (HMSMainEditorSettings.Instance.Settings.GetBool(AccountToggleEditor.AccountKitEnabled))
            {
                context.Symbols.Add("ENABLE_HMS_LOGIN");
                Global.CopyDependenciesFileInLib("com.pluginset.huawei", "HuaweiAccountDependencies.xml");
            }

            if (HMSMainEditorSettings.Instance.Settings.GetBool(IAPToggleEditor.IAPKitEnabled))
            {
                context.Symbols.Add("ENABLE_HMS_PURCHASE");
                Global.CopyDependenciesFileInLib("com.pluginset.huawei", "HuaweiIAPDependencies.xml");
            }
            
            if (HMSMainEditorSettings.Instance.Settings.GetBool(AdsToggleEditor.AdsKitEnabled))
            {
                context.Symbols.Add("ENABLE_HMS_ADS");
                Global.CopyDependenciesFileInLib("com.pluginset.huawei", "HuaweiAdsDependencies.xml");
            }
            
            if (HMSMainEditorSettings.Instance.Settings.GetBool(AnalyticsToggleEditor.AnalyticsKitEnabled))
            {
                context.Symbols.Add("ENABLE_HMS_ANALYTICS");
                Global.CopyDependenciesFileInLib("com.pluginset.huawei", "HuaweiAnalyticsDependencies.xml");
            }
            
            context.AddLinkAssembly("PluginSet.HMS");
            context.AddLinkAssembly("HuaweiMobileServices");
        }

        [AndroidProjectModify]
        public static void OnAndroidProjectModify(BuildProcessorContext context, AndroidProjectManager projectManager)
        {
            var buildParams = context.BuildChannels.Get<BuildHMSParams>();
            if (!buildParams.Enable)
                return;
            
            if (!string.IsNullOrEmpty(buildParams.AGConnectServiceJson) && File.Exists(buildParams.AGConnectServiceJson))
                File.Copy(buildParams.AGConnectServiceJson, Path.Combine(projectManager.LauncherPath, "agconnect-services.json"), true);
            
            Global.AppendProguardInLib(projectManager.Proguard, "com.pluginset.huawei");
            
            var repo = projectManager.ProjectGradle.ROOT.GetOrCreateNode("allprojects/buildscript/repositories/maven");
            repo.AppendContentNode("url 'https://developer.huawei.com/repo/'");
            
            var deps = projectManager.ProjectGradle.ROOT.GetOrCreateNode("allprojects/buildscript/dependencies");
            deps.AppendContentNode("classpath 'com.huawei.agconnect:agcp:1.6.0.300'");
            
            var root = projectManager.LibraryGradle.ROOT;
            const string applyPlugin = "apply plugin: 'com.huawei.agconnect'";
            root.RemoveContentNode(applyPlugin);
            root.InsertChildNode(new GradleContentNode(applyPlugin, root), 1);
            
            var doc = projectManager.LauncherManifest;
            doc.SetMetaData("com.huawei.hms.client.appid", $"appid={buildParams.AppId}");
            // doc.SetMetaData("com.huawei.hms.client.channel.androidMarket", "false");
        }
    }
}


