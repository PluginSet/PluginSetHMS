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
            
            // if (buildParams.EnablePurchase)
            //     context.Symbols.Add("ENABLE_HMS_PURCHASE");
            
            context.AddLinkAssembly("PluginSet.HMS");
            Global.CopyDependenciesInLib("com.pluginset.huawei");
        }

        [AndroidProjectModify]
        public static void OnAndroidProjectModify(BuildProcessorContext context, AndroidProjectManager projectManager)
        {
            var buildParams = context.BuildChannels.Get<BuildHMSParams>();
            if (!buildParams.Enable)
                return;
            
            if (!string.IsNullOrEmpty(buildParams.AGConnectServiceJson) && File.Exists(buildParams.AGConnectServiceJson))
                File.Copy(buildParams.AGConnectServiceJson, Path.Combine(projectManager.LauncherPath, "agconnect-services.json"), true);
            
#if false
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
#endif
        }
    }
}


