using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppLovinMax;

namespace MixNameSpace
{
    [Serializable]
    public class PlatformInfo
    {
        [SerializeField]
        public string adjustAppkey;
        [SerializeField]
        public string maxKey;
        [SerializeField]
        public string rid;
        [SerializeField]
        public string iid;
        [SerializeField]
        public string bid;
        [SerializeField]
        public string aid;
        [SerializeField]
        public string adjustGameStart;
        [SerializeField]
        public string adjustAddToCart;
        [SerializeField]
        public string adjustInitCheckout;
        [SerializeField]
        public string adjustPurchase;
    }
    [Serializable]
    public class ItemInfo
    {
        [SerializeField]
        public string itemId;
        [SerializeField]
        public int type;
        [SerializeField]
        public string googlePlayId;
        [SerializeField]
        public string appleStoreId;
        [SerializeField]
        public string usdPrice;
        [SerializeField]
        public string formattedPrice;

        public ItemInfo(string itemId, int type, string googlePlayId, string appleStoreId, string usdPrice, string formattedPrice)
        {
            this.itemId = itemId;
            this.type = type;
            this.googlePlayId = googlePlayId;
            this.appleStoreId = appleStoreId;
            this.usdPrice = usdPrice;
            this.formattedPrice = formattedPrice;
        }
    }
    [Serializable]
    public class MixInput
    {
        [SerializeField]
        public string appName;
        [SerializeField]
        public PlatformInfo androidInfo;
        [SerializeField]
        public PlatformInfo iosInfo;
        [SerializeField]
        public List<ItemInfo> items;

        public MixInput(string appName, PlatformInfo androidInfo, PlatformInfo iosInfo, List<ItemInfo> items)
        {
            this.appName = appName;
            this.androidInfo = androidInfo;
            this.iosInfo = iosInfo;
            this.items = items;
        }
    }
    public class MixSDKConfig
    {
        public MixInput mixInput;
        public bool usingServer;
        public MaxSdkBase.BannerPosition pos;
        public bool debug;
        public static string secretKey = "";

        public MixSDKConfig(MixInput mixInput, bool usingServer, MaxSdkBase.BannerPosition pos, bool debug)
        {
            this.mixInput = mixInput;
            this.usingServer = usingServer;
            this.pos = pos;
            this.debug = debug;
        }
        public PlatformInfo GetPlatformInfo()
        {
#if UNITY_IOS
            return mixInput.iosInfo;
#else
            return mixInput.androidInfo;
#endif
        }

        static public MixSDKConfig parse(string data, bool usingServer, MaxSdkBase.BannerPosition pos, bool debug)
        {
            MixSDKConfig.secretKey = data;
            string s =  MixCommon.AesDecryptorBase64(data, "MixSDK");
            Debug.Log(s);
            var input = JsonUtility.FromJson<MixInput>(s);
            Debug.Log(input.appName);
            return new MixSDKConfig(input, usingServer, pos, debug);
        }
    }
}
