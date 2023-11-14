#if ENABLE_HMS_ADS
using System;
using System.Collections.Generic;
using HmsPlugin;
using HuaweiConstants;
using HuaweiMobileServices.Ads;
using PluginSet.Core;

namespace PluginSetHMS.Runtime
{
    public partial class PluginSetHMS: IBannerAdPlugin, IRewardAdPlugin, IInterstitialAdPlugin
    {
        [HMSStartExecutable]
        private void StartAdPlugin()
        {
            HMSAdsKitManager.Instance.OnRewardedAdLoaded = OnRewardedAdLoaded;
            HMSAdsKitManager.Instance.OnRewardedAdFailedToLoad = OnRewardedAdFailedToLoad;
            HMSAdsKitManager.Instance.OnRewardAdClosed = OnRewardAdClosed;
            HMSAdsKitManager.Instance.OnRewarded = OnRewarded;
            
            HMSAdsKitManager.Instance.OnInterstitialAdClosed = OnInterstitialAdClosed;
            HMSAdsKitManager.Instance.OnInterstitialAdFailed = OnInterstitialAdFailed;
            HMSAdsKitManager.Instance.OnInterstitialAdLoaded = OnInterstitialAdLoaded;
            
        }

        [HMSDisposeExecutable]
        private void DisposeAdPlugin()
        {
            HMSAdsKitManager.Instance.OnRewardedAdLoaded = null;
            HMSAdsKitManager.Instance.OnRewardedAdFailedToLoad = null;
            HMSAdsKitManager.Instance.OnRewardAdClosed = null;
            HMSAdsKitManager.Instance.OnRewarded = null;
            
            HMSAdsKitManager.Instance.OnInterstitialAdClosed = null;
            HMSAdsKitManager.Instance.OnInterstitialAdFailed = null;
            HMSAdsKitManager.Instance.OnInterstitialAdLoaded = null;
        }
        
        private Action _onRewardAdLoadedSuccess;
        private Action<int> _onRewardAdLoadedFail;
        private Action<bool, int> _onRewardCallback;
        
        private bool _isLoadingRewardAd;
        private bool _isShowingRewardAd;
        
        private Action _onInterstitialAdLoadedSuccess;
        private Action<int> _onInterstitialAdLoadedFail;
        private Action<bool, int> _onInterstitialCallback;
        
        private bool _isLoadingInterstitialAd;
        private bool _isShowingInterstitialAd;

        private void OnRewardedAdLoaded()
        {
            _isLoadingRewardAd = false;
            var callback = _onRewardAdLoadedSuccess;
            _onRewardAdLoadedSuccess = null;
            _onRewardAdLoadedFail = null;
            callback?.Invoke();
        }

        private void OnRewardedAdFailedToLoad(int obj)
        {
            _isLoadingRewardAd = false;
            var callback = _onRewardAdLoadedFail;
            _onRewardAdLoadedSuccess = null;
            _onRewardAdLoadedFail = null;
            callback?.Invoke((int)AdErrorCode.NotLoaded);
        }

        private void OnRewardAdClosed()
        {
            _isShowingRewardAd = false;
            if (_onRewardCallback != null)
                _onRewardCallback.Invoke(false, (int)AdErrorCode.Dismissed);
            _onRewardCallback = null;
        }

        private void OnRewarded(Reward obj)
        {
            if (_onRewardCallback != null)
                _onRewardCallback.Invoke(true, (int)AdErrorCode.Success);
            _onRewardCallback = null;
        }

        private void OnInterstitialAdClosed()
        {
            _isShowingInterstitialAd = false;
            if (_onInterstitialCallback == null) return;
            
            _onInterstitialCallback.Invoke(true, (int)AdErrorCode.Success);
            _onInterstitialCallback = null;
        }

        private void OnInterstitialAdFailed(int obj)
        {
            _isLoadingInterstitialAd = false;
            _isShowingInterstitialAd = false;
            
            if (_onInterstitialCallback != null)
            {
                _onInterstitialCallback.Invoke(false, (int)AdErrorCode.NotLoaded);
                _onInterstitialCallback = null;
                return;
            }
            
            var callback = _onInterstitialAdLoadedFail;
            _onInterstitialAdLoadedSuccess = null;
            _onInterstitialAdLoadedFail = null;
            callback?.Invoke((int)AdErrorCode.NotLoaded);
        }

        private void OnInterstitialAdLoaded()
        {
            _isLoadingInterstitialAd = false;
            var callback = _onInterstitialAdLoadedSuccess;
            _onInterstitialAdLoadedSuccess = null;
            _onInterstitialAdLoadedFail = null;
            callback?.Invoke();
        }
        
#region BannerAd

        public bool IsEnableShowBanner => true;
        
        private string _bannerAdId;

        private static UnityBannerAdPositionCode.UnityBannerAdPositionCodeType ConvertBannerPosition(BannerPosition position)
        {
            switch (position)
            {
                case BannerPosition.TopLeft:
                    return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_TOP_LEFT;
                case BannerPosition.TopCenter:
                    return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_TOP;
                case BannerPosition.TopRight:
                    return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_TOP_RIGHT;
                case BannerPosition.BottomLeft:
                    return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_BOTTOM_LEFT;
                case BannerPosition.BottomCenter:
                    return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_BOTTOM;
                case BannerPosition.BottomRight:
                    return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_BOTTOM_RIGHT;
                case BannerPosition.Centered:
                case BannerPosition.CenterLeft:
                case BannerPosition.CenterRight:
                    return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_CUSTOM;
                default:
                    return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_BOTTOM;
            }
        }
            
        public void ShowBannerAd(string adId, BannerPosition position = BannerPosition.BottomCenter, Dictionary<string, object> extensions = null)
        {
            HideAllBanners();
            _bannerAdId = adId;
            if (string.IsNullOrEmpty(adId))
                HMSAdsKitManager.Instance.LoadBannerAd(ConvertBannerPosition(position));
            else
                HMSAdsKitManager.Instance.LoadBannerAd(adId, ConvertBannerPosition(position));
            HMSAdsKitManager.Instance.ShowBannerAd();
        }

        public void HideBannerAd(string adId)
        {
            if (_bannerAdId != adId) return;
            HideAllBanners();
        }

        public void HideAllBanners()
        {
            HMSAdsKitManager.Instance.HideBannerAd();
            _bannerAdId = null;
        }
#endregion
        
#region RewardAd

        public bool IsEnableShowRewardAd => true;

        public bool IsReadyToShowRewardAd => HMSAdsKitManager.Instance.IsRewardedAdLoaded;
        
        public void LoadRewardAd(Action success = null, Action<int> fail = null)
        {
            if (IsReadyToShowRewardAd)
            {
                success?.Invoke();
                return;
            }
            
            if (_isLoadingRewardAd)
            {
                fail?.Invoke((int)AdErrorCode.IsLoading);
                return;
            }
            
            if (_isShowingRewardAd)
            {
                fail?.Invoke((int)AdErrorCode.IsShowing);
                return;
            }
            
            
            _onRewardAdLoadedSuccess = success;
            _onRewardAdLoadedFail = fail;
            _isLoadingRewardAd = true;
            HMSAdsKitManager.Instance.LoadRewardedAd();
        }

        public void ShowRewardAd(Action<bool, int> dismiss = null)
        {
            if (!IsReadyToShowRewardAd)
            {
                dismiss?.Invoke(false, (int)AdErrorCode.NotLoaded);
                return;
            }
            
            if (_isShowingRewardAd)
            {
                dismiss?.Invoke(false, (int)AdErrorCode.IsShowing);
                return;
            }
            
            _isShowingRewardAd = true;
            _onRewardCallback = dismiss;
            HMSAdsKitManager.Instance.ShowRewardedAd();
        }
        
#endregion

#region InterstitialAd

        public bool IsEnableShowInterstitialAd => true;
        public bool IsReadyToShowInterstitialAd => HMSAdsKitManager.Instance.IsInterstitialAdLoaded;

        public void LoadInterstitialAd(Action success = null, Action<int> fail = null)
        {
            if (IsReadyToShowInterstitialAd)
            {
                success?.Invoke();
                return;
            }
            
            if (_isLoadingInterstitialAd)
            {
                fail?.Invoke((int)AdErrorCode.IsLoading);
                return;
            }
            
            if (_isShowingInterstitialAd)
            {
                fail?.Invoke((int)AdErrorCode.IsShowing);
                return;
            }
            
            _onInterstitialAdLoadedSuccess = success;
            _onInterstitialAdLoadedFail = fail;
            _isLoadingInterstitialAd = true;
            HMSAdsKitManager.Instance.LoadInterstitialAd();
        }

        public void ShowInterstitialAd(Action<bool, int> dismiss = null)
        {
            if (!IsReadyToShowInterstitialAd)
            {
                dismiss?.Invoke(false, (int)AdErrorCode.NotLoaded);
                return;
            }
            
            if (_isShowingInterstitialAd)
            {
                dismiss?.Invoke(false, (int)AdErrorCode.IsShowing);
                return;
            }
            
            _isShowingInterstitialAd = true;
            _onInterstitialCallback = dismiss;
            HMSAdsKitManager.Instance.ShowInterstitialAd();
        }
#endregion
    }
}
#endif