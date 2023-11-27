#if ENABLE_HMS_ANALYTICS
using System.Collections;
using System.Collections.Generic;
using PluginSet.Core;
using UnityEngine;

namespace PluginSetHMS.Runtime
{
    public partial class PluginSetHMS: IAnalytics, IPageAnalytics, IUserSet
    {
        private static readonly Dictionary<string, object> EmptyParams = new Dictionary<string, object>();

        private string _channel;
        private string _userId;
        private Dictionary<string, object> _userProperties;
        private string _pageName;
        private string _pageClassOverride;

        private bool _isInited = false;
        private Coroutine _initCoroutine;

        [HMSStartExecutable]
        private void StartAnalytics()
        {
            AddEventListener(PluginConstants.NOTIFY_CHANNEL_CHANGED, UpdateChannel);
            UpdateChannel();
            if (!_isInited && _initCoroutine == null)
                CheckInitedAnalytics();
        }
        
        [HMSDisposeExecutable]
        private void DisposeAnalytics()
        {
            RemoveEventListener(PluginConstants.NOTIFY_CHANNEL_CHANGED, UpdateChannel);
        }

        private void UpdateChannel()
        {
            var channelName = _managerInstance.GetChannelName();
            var channelId = _managerInstance.GetChannelId();
            if (string.IsNullOrEmpty(channelName))
                channelName = "Unknown";
            
            _channel = channelId >= 0 ? $"{channelName}_{channelId}" : channelName;

            if (_isInited)
            {
                var singleton = HMSAnalyticsKitManager.Instance;
                singleton.activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    singleton.hiAnalyticsInstance.SetChannel(_channel);
                }));
            }
        }
        
        private void CheckInitedAnalytics()
        {
            if (_isInited)
                return;
            
            _initCoroutine = StartCoroutine(InitAnalytics());
        }

        private IEnumerator InitAnalytics()
        {
            var singleton = HMSAnalyticsKitManager.Instance;
            yield return new WaitUntil(() => singleton.hiAnalyticsInstance != null);
            
            _isInited = true;
            _initCoroutine = null;
            
            singleton.activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                singleton.hiAnalyticsInstance.SetChannel(_channel);
                
                if (string.IsNullOrEmpty(_userId))
                    singleton.hiAnalyticsInstance.SetUserId(_userId);
                if (_userProperties != null)
                {
                    foreach (var kv in _userProperties)
                    {
                        singleton.hiAnalyticsInstance.SetUserProfile(kv.Key, kv.Value.ToString());
                    }
                }
            }));
        }
        
        public void CustomEvent(string customEventName, Dictionary<string, object> eventData = null)
        {
            HMSAnalyticsKitManager.Instance.SendEventWithBundle(customEventName, eventData ?? EmptyParams);
        }

        public void SetUserInfo(bool isNewUser, string userId, Dictionary<string, object> pairs = null)
        {
            _userId = userId;
            _userProperties = pairs;

            if (_isInited)
            {
                var singleton = HMSAnalyticsKitManager.Instance;
                singleton.activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    singleton.hiAnalyticsInstance.SetChannel(_channel);
                    
                    if (string.IsNullOrEmpty(_userId))
                        singleton.hiAnalyticsInstance.SetUserId(_userId);
                    if (_userProperties != null)
                    {
                        foreach (var kv in _userProperties)
                        {
                            singleton.hiAnalyticsInstance.SetUserProfile(kv.Key, kv.Value.ToString());
                        }
                    }
                }));
            }
        }

        public void ClearUserInfo()
        {
            _userId = null;
            _userProperties = null;
            
            if (_isInited)
            {
                var singleton = HMSAnalyticsKitManager.Instance;
                singleton.activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    singleton.hiAnalyticsInstance.ClearCachedData();
                }));
            }
        }

        public void FlushUserInfo()
        {
        }

        public void PageStart(string pageName, string pageClassOverride)
        {
            _pageName = pageName;
            _pageClassOverride = pageClassOverride;

            if (_isInited)
            {
                var singleton = HMSAnalyticsKitManager.Instance;
                singleton.activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    singleton.hiAnalyticsInstance.PageStart(_pageName, _pageClassOverride);
                }));
            }
        }

        public void PageEnd(string pageName)
        {
            _pageName = null;
            _pageClassOverride = null;

            if (_isInited)
            {
                var singleton = HMSAnalyticsKitManager.Instance;
                singleton.activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    singleton.hiAnalyticsInstance.PageEnd(_pageName);
                }));
            }
        }
    }
}
#endif