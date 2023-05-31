using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AppLovinMax;

namespace MixNameSpace
{
    public class MixMaxManager
    {
        static public MixMaxManager instance = new MixMaxManager();

        public MixBannerAd mixBannerAd = new MixBannerAd();
        public MixRewardedAd mixRewardedAd = new MixRewardedAd();
        public MixInterstitialAd mixInterstitialAd = new MixInterstitialAd();
        public MixAppOpenAd mixAppOpenAd = new MixAppOpenAd();

        public void Init(string maxKey, string rid, string iid, string bid, string oid, MaxSdkBase.BannerPosition pos,
            bool debug,
            Action<string> finishCallback)
        {
            Debug.Log("ready init the MixMaxManager");
            //AdSettings.SetDataProcessingOptions(new string[] { "LDU" }, 1, 1000);
            MixAdSettings.SetDataProcessingOptions();

            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => {
                Debug.Log("MixMaxManager OnSdkInitializedEvent");
#if UNITY_IOS
                if (MaxSdkUtils.CompareVersions(UnityEngine.iOS.Device.systemVersion, "14.5") != MaxSdkUtils.VersionComparisonResult.Lesser)
                {
                    // Note that App transparency tracking authorization can be checked via `sdkConfiguration.AppTrackingStatus` for Unity Editor and iOS targets
                    // 1. Set Meta ATE flag here, THEN
                    //AdSettings.SetAdvertiserTrackingEnabled(true);
                    MixAdSettings.SetAdvertiserTrackingEnabled();
                }
#endif
                if (debug)
                {
                    MaxSdk.ShowMediationDebugger();
                }
                // 2. Load ads
                if (oid != null && oid.Length > 0)
                {
                    mixAppOpenAd.InitializeAppOpenAds(oid);
                }
                if (rid != null && rid.Length > 0)
                {
                    mixRewardedAd.InitializeRewardedAds(rid);
                }

                if(iid != null && iid.Length > 0)
                {
                    mixInterstitialAd.InitializeInterstitialAds(iid);
                }

                if (bid != null && bid.Length > 0)
                {
                    mixBannerAd.InitializeBannerAds(bid, pos);
                }

                finishCallback("finish");
            };
            MaxSdk.SetSdkKey(maxKey);
            //set fb
            if (debug)
            {
                MaxSdk.SetVerboseLogging(true);
            }
            //MaxSdk.SetUserId("USER_ID");
            MaxSdk.InitializeSdk();
            
        }

        static public void LogAdLoad(string adtype, string unitId)
        {
            MixData.instance.Log("ad_load", new Dictionary<string, string>()
            {
                {"mediationType", "max"},
                {"unitId", unitId},
                {"adtype", adtype}
            });
        }
        static public void LogAdLoadFail(string adtype, string unitId, MaxSdkBase.ErrorInfo info)
        {
            int code = (int)info.Code;
            MixData.instance.Log("ad_load_fail", new Dictionary<string, string>()
            {
                {"mediationType", "max"},
                {"unitId", unitId},
                {"adtype", adtype},
                {"code", code.ToString()},
                {"message", info.Message}
            });
        }
        static public void LogAdShow(string adtype, string unitId, MaxSdkBase.AdInfo adInfo)
        {
            MixData.instance.Log("ad_show", new Dictionary<string, string>()
            {
                {"mediationType", "max"},
                {"unitId", unitId},
                {"adtype", adtype},

                {"network_firm_id", adInfo.NetworkName},
                {"adsource_id", adInfo.AdUnitIdentifier},
                {"network_placement_id", adInfo.Placement},
                {"ecpm", adInfo.Revenue.ToString()},
            });
        }
    }
}