using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEngine;

namespace PluginSet.HMS.Editor
{
    [BuildChannelsParams("HMS", "HMS Core SDK配置")]
    [VisibleCaseBoolValue("SupportAndroid", true)]
    public class BuildHMSParams : ScriptableObject
    {
        [Tooltip("是否启用HMS")]
        public bool Enable;

        [Tooltip("HMS提供的APPID")]
        public string AppId;
        
        [Tooltip("HMS提供的AGConnectService.json文件")]
        [BrowserFile("agconnect-services.json文件路径", "json")]
        public string AGConnectServiceJson;
    }
}
