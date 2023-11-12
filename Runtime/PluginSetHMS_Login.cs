#if ENABLE_HMS_LOGIN
using System;
using HmsPlugin;
using HuaweiMobileServices.Id;
using HuaweiMobileServices.Utils;
using PluginSet.Core;

namespace PluginSetHMS.Runtime
{
    public partial class PluginSetHMS: ILoginPlugin
    {
        public bool IsEnableLogin { get; }
        public bool IsLoggedIn { get; private set; }
        
        private Action<Result> _loginCallback;

        [HMSStartExecutable]
        private void StartLoginPlugin()
        {
            HMSAccountKitManager.Instance.OnSignInSuccess = OnLoginSuccess;
            HMSAccountKitManager.Instance.OnSignInFailed = OnLoginFailure;
        }

        [HMSDisposeExecutable]
        private void DisposeLoginPlugin()
        {
            HMSAccountKitManager.Instance.OnSignInSuccess = null;
            HMSAccountKitManager.Instance.OnSignInFailed = null;
        }
        
        public void Login(Action<Result> callback = null, string json = null)
        {
            _loginCallback = callback;
            HMSAccountKitManager.Instance.SignIn();
        }

        public void Logout(Action<Result> callback = null, string json = null)
        {
            HMSAccountKitManager.Instance.SignOut();
            IsLoggedIn = false;
            callback?.Invoke(new Result()
            {
                Success = true,
                PluginName = Name,
            });
        }

        public string GetUserLoginData()
        {
            return string.Empty;
        }
        
        private void OnLoginSuccess(AuthAccount authHuaweiId)
        {
            IsLoggedIn = true;
            if (_loginCallback == null)
                return;

            var result = new Result()
            {
                Success = true,
                PluginName = Name,
            };
            OnLoginResult(in result);
        }

        private void OnLoginFailure(HMSException error)
        {
            IsLoggedIn = false;
            if (_loginCallback == null)
                return;
            
            var result = new Result()
            {
                Success = false,
                PluginName = Name,
                Code = error.ErrorCode,
                Error = error.WrappedExceptionMessage,
            };
            OnLoginResult(in result);
        }
        
        private void OnLoginResult(in Result result)
        {
            _loginCallback?.Invoke(result);
            _loginCallback = null;
        }
    }
}
#endif