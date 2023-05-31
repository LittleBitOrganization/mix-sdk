﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppLovinMax;
namespace MixNameSpace
{
    public class MixBannerAd
    {
        string adtype = "banner";
        string bannerAdUnitId; // Retrieve the ID from your account
        MaxSdkBase.BannerPosition pos;

        public void InitializeBannerAds(string bannerAdUnitId, MaxSdkBase.BannerPosition pos)
        {
            this.bannerAdUnitId = bannerAdUnitId;
            this.pos = pos;
            // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
            // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            MaxSdk.CreateBanner(bannerAdUnitId, pos);

            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, Color.white);

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            MixMaxManager.LogAdLoad(adtype, adUnitId);
        }

        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            MixMaxManager.LogAdLoadFail(adtype, adUnitId, errorInfo);
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        public void ShowBanner()
        {
            MaxSdk.ShowBanner(bannerAdUnitId);
        }
        public void HideBanner()
        {
            MaxSdk.HideBanner(bannerAdUnitId);
        }
    }
}