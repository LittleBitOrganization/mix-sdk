using System;
using System.Collections;
using System.Collections.Generic;
using com.adjust.sdk;
using UnityEngine;

namespace MixNameSpace
{
    public class MixThirdAdjust : MixThirdUploader
    {
        //static public MixThirdAdjust instance = newInstance();
        //static private MixThirdAdjust newInstance()
        //{
        //    Debug.Log("MixThirdAdjust try to newInstance and add");
        //    MixThirdAdjust one = new MixThirdAdjust();
        //    MixThirdUpload.instance.AddMixThirdUploader(one);
        //    return one;
        //}

        private PlatformInfo pinfo;

        public void Init(PlatformInfo pinfo)
        {
            this.pinfo = pinfo;
            Debug.Log("MixThirdAdjust Init");
        }

        public void UploadAddToCart(float price, string curr, string itemId)
        {
            try
            {
                if (pinfo.adjustAddToCart != null && pinfo.adjustAddToCart.Length > 1)
                {
                    AdjustEvent adjustEvent = new AdjustEvent(pinfo.adjustAddToCart);
                    Adjust.trackEvent(adjustEvent);
                    Debug.Log("MixThirdAdjust send adjust UploadAddToCart " + pinfo.adjustAddToCart + " success");
                }
            }
            catch (Exception e)
            {
                Debug.Log("MixThirdAdjust send adjust UploadAddToCart " + pinfo.adjustAddToCart + " fail");
                Debug.LogError(e);
            }
        }

        public void UploadInitCheckout(float price, string curr, string itemId)
        {
            try
            {
                if (pinfo.adjustInitCheckout != null && pinfo.adjustInitCheckout.Length > 1)
                {
                    AdjustEvent adjustEvent = new AdjustEvent(pinfo.adjustInitCheckout);
                    Adjust.trackEvent(adjustEvent);
                    Debug.Log("MixThirdAdjust send adjust UploadInitCheckout " + pinfo.adjustInitCheckout + " success");
                }
            }
            catch (Exception e)
            {
                Debug.Log("MixThirdAdjust send adjust UploadInitCheckout " + pinfo.adjustInitCheckout + " fail");
                Debug.LogError(e);
            }
        }

        public void UploadPurchase(float price, string curr, string itemId)
        {
            try
            {
                if (pinfo.adjustPurchase != null && pinfo.adjustPurchase.Length > 1)
                {
                    AdjustEvent adjustEvent = new AdjustEvent(pinfo.adjustPurchase);
                    adjustEvent.setRevenue(price, curr);
                    Adjust.trackEvent(adjustEvent);
                    Debug.Log("MixThirdAdjust send adjust UploadPurchase " + pinfo.adjustPurchase + " fail");
                }
            }
            catch (Exception e)
            {
                Debug.Log("MixThirdAdjust send adjust UploadPurchase " + pinfo.adjustPurchase + " fail");
                Debug.LogError(e);
            }
        }
        public void IncRevenue(float price)
        {

        }
    }

    public class MixAdjustManager
    {
        static public MixAdjustManager instance = new MixAdjustManager();
        static public string SAVE_ADJUST_KEY = "MIX_SDK_ADJUSTINFO";

        public MixAdjustInfo info;

        public void start(string name, string adjustAppkey, bool debug, Action<int> nextOne)
        {
#if UNITY_EDITOR
            nextOne(0);
            return;
#endif
            Debug.Log("ready to init MixAdjustManager");
            AdjustEnvironment environment = AdjustEnvironment.Production;
            AdjustLogLevel level = AdjustLogLevel.Info;
            if (debug)
            {
                environment = AdjustEnvironment.Sandbox;
                level = AdjustLogLevel.Verbose;
            }
            AdjustConfig adjustConfig = new AdjustConfig(adjustAppkey, environment);
            adjustConfig.setLogLevel(level);
            adjustConfig.setExternalDeviceId(SystemInfo.deviceUniqueIdentifier);
            adjustConfig.setAttributionChangedDelegate(AttributionChangedDelegate, name);

#if UNITY_IOS
            Adjust.requestTrackingAuthorizationWithCompletionHandler((status) =>
            {
                switch (status)
                {
                    case 0:
                        // ATTrackingManagerAuthorizationStatusNotDetermined case
                        break;
                    case 1:
                        // ATTrackingManagerAuthorizationStatusRestricted case
                        break;
                    case 2:
                        // ATTrackingManagerAuthorizationStatusDenied case
                        break;
                    case 3:
                        // ATTrackingManagerAuthorizationStatusAuthorized case
                        break;
                }
                Debug.Log("get status from requestTrackingAuthorizationWithCompletionHandler in MixAdjustManager and start adjust");
                Adjust.start(adjustConfig);
                nextOne(status);
            }, name);
#else
            Adjust.start(adjustConfig);
            nextOne(0);
#endif
        }

        public void AttributionChangedDelegate(AdjustAttribution adjustAttribution)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                this.info = new MixAdjustInfo(
                    Adjust.getAdid(),
                    adjustAttribution.network,
                    adjustAttribution.campaign,
                    adjustAttribution.adgroup,
                    adjustAttribution.creative
                );

                SaveInfo();
                MixData.instance.Log("attr_on", new Dictionary<string, string>()
                {
                    {"attr_name", "adjust"},
                    {"attr_version", "adjust-unity"},
                    {"attr_id", Adjust.getAdid()},
                    {"network", adjustAttribution.network},
                    {"campaign", adjustAttribution.campaign},
                    {"adgroup", adjustAttribution.adgroup},
                    {"creative", adjustAttribution.creative},
                    {"fbInstallReferrer", string.IsNullOrEmpty(adjustAttribution.fbInstallReferrer) ? "" : adjustAttribution.fbInstallReferrer},
                });
                MixMain.instance.AdjustInfoFinish();
            });
        }

        private void SaveInfo()
        {
            string j = JsonUtility.ToJson(this.info);
            PlayerPrefs.SetString(SAVE_ADJUST_KEY, j);
            PlayerPrefs.Save();
        }

        public void ReadOldInfo()
        {
            var str = PlayerPrefs.GetString(SAVE_ADJUST_KEY);
            if (str != null && str.Length > 0)
            {
                this.info =  JsonUtility.FromJson<MixAdjustInfo>(str);
                MixMain.instance.AdjustInfoFinish();
            }
        }
    }
}