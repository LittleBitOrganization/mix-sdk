using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixNameSpace
{
    public class MixLogInfo : MonoBehaviour
    {
        public string gameName;
        public string pn;
        public string appVersion;
        public string deviceid;
        public string platform;
        public string idfa = "";
        public string uid;
        public string sessionId;

        public string idfv;
        public string android_id;

        public string attrid;

        public MixLogInfo(string gameName)
        {
            this.gameName = gameName;
            this.pn = Application.identifier;
            this.deviceid = SystemInfo.deviceUniqueIdentifier;
            this.appVersion = Application.version;
#if UNITY_EDITOR
            this.appVersion = "test";
            this.idfa = "test1";
            this.idfv = "test2";
            this.android_id = "test3";
            this.platform = "ios";
#elif UNITY_ANDROID
            this.platform = "android";
            this.idfa = GetGaid();
            this.idfv = "";
            this.android_id = GetAndroidID();
#elif UNITY_IOS
            this.platform = "ios";
            this.idfa = UnityEngine.iOS.Device.advertisingIdentifier;
            this.idfv = UnityEngine.iOS.Device.vendorIdentifier;  
#else
            this.appVersion = "";
            this.idfa = "";
            this.idfv = "";
            this.android_id = "";
            this.platform = "unknow";
#endif
            this.attrid = "";
            this.uid = "";
            var randomInt = UnityEngine.Random.Range(0, 100);

            this.sessionId = this.deviceid + "_" + MixCommon.NowTimestamp() + "_" + randomInt;
        }

        public Dictionary<string, object> toDict()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            FilterAndAdd(data, "gameName", gameName);
            FilterAndAdd(data, "deviceId", deviceid);
            FilterAndAdd(data, "package", pn);
            FilterAndAdd(data, "appVersion", appVersion);
            FilterAndAdd(data, "platform", platform);
            FilterAndAdd(data, "idfa", idfa);
            FilterAndAdd(data, "uid", uid);
            FilterAndAdd(data, "idfv", idfv);
            FilterAndAdd(data, "androidId", android_id);
            FilterAndAdd(data, "attrid", attrid);
            FilterAndAdd(data, "sessionId", sessionId);
            return data;
        }
        private void FilterAndAdd(Dictionary<string, object> data, string key, string value)
        {
            if (value != null && value.Length > 0)
            {
                data.Add(key, value);
            }
        }

        private string GetAndroidID()
        {
            try
            {
                AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
                AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
                string android_id = secure.CallStatic<string>("getString", contentResolver, "android_id");
                if (android_id == null) android_id = "";
                return android_id;
            }
            catch (Exception e)
            {
                //ignore
                return "";
            }
        }

        private string GetGaid()
        {
            string advertisingID = "";
            try
            {
                AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaClass client = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
                AndroidJavaObject adInfo = client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);

                advertisingID = adInfo.Call<string>("getId").ToString();
            }
            catch (Exception)
            {
            }
            return advertisingID;
        }
    }
}