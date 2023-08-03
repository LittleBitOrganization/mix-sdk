using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppLovinMax;

namespace MixNameSpace
{
    public class MixRewardedAd
    {
        string adtype = "reward";
        string adUnitId;
        int retryAttempt;
        Action onRewardedAdReceivedRewardEventAction;
        Action onRewardedAdHiddenEventAction;
        public event Action<string, MaxSdkBase.AdInfo> onRewardedAdRevenuePaidEvent;
        public event Action<string, MaxSdkBase.AdInfo> onAdLoadedEvent;
        public event Action<string, MaxSdkBase.ErrorInfo> onAdLoadFailedEvent;
        public event Action<string, MaxSdkBase.AdInfo> onAdReceivedRewardEvent;
        public event Action<string, MaxSdkBase.AdInfo> onAdClickedEvent;
        public event Action<string, MaxSdkBase.AdInfo> onAdHiddenEvent;
        public event Action<string, MaxSdkBase.ErrorInfo, MaxSdkBase.AdInfo> onAdDisplayFailedEvent;

        public void InitializeRewardedAds(string adUnitId)
        {
            this.adUnitId = adUnitId;
            this.retryAttempt = 0;
            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

            // Load the first rewarded ad
            LoadRewardedAd();
        }

        private void LoadRewardedAd()
        {
            MaxSdk.LoadRewardedAd(adUnitId);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.
            onAdLoadedEvent?.Invoke(adUnitId, adInfo);
            // Reset retry attempt
            retryAttempt = 0;
            MixMaxManager.LogAdLoad(adtype, adUnitId);
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            onAdLoadFailedEvent?.Invoke(adUnitId, errorInfo);
            // Rewarded ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).
            MixMaxManager.LogAdLoadFail(adtype, adUnitId, errorInfo);
            retryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));
            MixCommon.DelayRunActionDelay(LoadRewardedAd, retryDelay);
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            MixMaxManager.LogAdShow(adtype, adUnitId, adInfo);
            MixThirdUpload.instance.IncRevenue((float)adInfo.Revenue);
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            onAdDisplayFailedEvent?.Invoke(adUnitId, errorInfo, adInfo);
            // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
            LoadRewardedAd();
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            onAdClickedEvent?.Invoke(adUnitId, adInfo);
        }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            onAdHiddenEvent?.Invoke(adUnitId, adInfo);
            if (this.onRewardedAdHiddenEventAction != null)
            {
                MixCommon.DelayRunActionNext(onRewardedAdHiddenEventAction);
            }
            // Rewarded ad is hidden. Pre-load the next ad
            LoadRewardedAd();
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            onAdReceivedRewardEvent?.Invoke(adUnitId, adInfo);
            if (this.onRewardedAdReceivedRewardEventAction != null)
            {
                MixCommon.DelayRunActionNext(onRewardedAdReceivedRewardEventAction);
            }
            MixFyberManager.refresh_web_show_reward();
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            onRewardedAdRevenuePaidEvent?.Invoke(adUnitId, adInfo);
            // Ad revenue paid. Use this callback to track user revenue.
        }

        public bool IsRewardedAdReady()
        {
            return MaxSdk.IsRewardedAdReady(adUnitId);
        }
        public void ShowRewardedAd(Action OnRewardedAction, Action AdHiddenEventAction)
        {
            this.onRewardedAdReceivedRewardEventAction = OnRewardedAction;
            this.onRewardedAdHiddenEventAction = AdHiddenEventAction;
            MaxSdk.ShowRewardedAd(adUnitId);
        }
    }
}