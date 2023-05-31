using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppLovinMax;

namespace MixNameSpace
{
    public class MixInterstitialAd
    {
        string adtype = "interstitial";
        string adUnitId;
        int retryAttempt;
        Action onInterstitialHiddenEventAction;
        public event Action<string, MaxSdkBase.AdInfo> onInterAdRevenuePaidEvent;
        public event Action<string, MaxSdkBase.ErrorInfo, MaxSdkBase.AdInfo> onAdDisplayFailedEvent;
        public event Action<string, MaxSdkBase.AdInfo> onAdHiddenEvent;
        public event Action<string, MaxSdkBase.AdInfo> onAdClickedEvent;
        public void InitializeInterstitialAds(string adUnitId)
        {
            this.adUnitId = adUnitId;
            this.retryAttempt = 0;
            // Attach callback
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += InterstitialOnOnAdRevenuePaidEvent;

            // Load the first interstitial
            LoadInterstitial();
        }

        private void LoadInterstitial()
        {
            MaxSdk.LoadInterstitial(adUnitId);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            MixMaxManager.LogAdLoad(adtype, adUnitId);
            // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'

            // Reset retry attempt
            retryAttempt = 0;
        }

        private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Interstitial ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)
            MixMaxManager.LogAdLoadFail(adtype, adUnitId, errorInfo);
            retryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));

            MixCommon.DelayRunActionDelay(LoadInterstitial, retryDelay);
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.LogFormat("OnInterstitialDisplayedEvent -----");
            MixMaxManager.LogAdShow(adtype, adUnitId, adInfo);
            MixH5WebViewManager.refresh_web_show_inter();
        }

        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            onAdDisplayFailedEvent?.Invoke(adUnitId, errorInfo, adInfo);
            // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
            LoadInterstitial();
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            onAdClickedEvent?.Invoke(adUnitId, adInfo);
        }

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            onAdHiddenEvent?.Invoke(this.adUnitId, adInfo);
            // Interstitial ad is hidden. Pre-load the next ad.
            if (this.onInterstitialHiddenEventAction != null)
            {
                MixCommon.DelayRunActionNext(onInterstitialHiddenEventAction);
            }
            LoadInterstitial();
        }

        private void InterstitialOnOnAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            onInterAdRevenuePaidEvent?.Invoke(arg1, arg2);
        }
        
        public bool IsInterstitialReady()
        {
            return MaxSdk.IsInterstitialReady(adUnitId);
        }
        public void ShowInterstitial(Action a1)
        {
            this.onInterstitialHiddenEventAction = a1;
            MaxSdk.ShowInterstitial(adUnitId);
        }
    }
}