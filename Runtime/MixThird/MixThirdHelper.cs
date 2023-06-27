using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace MixNameSpace {
    [MixDoNotRename]
    public class MixThirdHelper : MixThirdUploader
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void bridgeUploadAddToCart(float price, string curr, string itemId);
        [DllImport("__Internal")]
        private static extern void bridgeInitCheckout(float price, string curr, string itemId);
        [DllImport("__Internal")]
        private static extern void bridgeUploadPurchase(float price, string curr, string itemId);
        [DllImport("__Internal")]
        private static extern void bridgeIncRevenue(float revenue);
        [DllImport("__Internal")]
        private static extern void bridgeShareLinkByFacebook(string url);
        [DllImport("__Internal")]
        private static extern void bridgeSharePhotoByFacebook(byte[] bytes);
        public void Init(PlatformInfo pinfo)
        {
            Debug.Log("MixThirdHelper Init");
            // init share
            MixThirdShare.instance.OnInit(this);
        }

        public void UploadAddToCart(float price, string curr, string itemId)
        {
            Debug.Log("MixThirdHelper UploadAddToCart");
            bridgeUploadAddToCart(price, curr, itemId);
        }

        public void UploadInitCheckout(float price, string curr, string itemId)
        {
            Debug.Log("MixThirdHelper UploadInitCheckout");
            bridgeInitCheckout(price, curr, itemId);
        }

        public void UploadPurchase(float price, string curr, string itemId)
        {
            Debug.Log("MixThirdHelper UploadPurchase");
            bridgeUploadPurchase(price, curr, itemId);
        }
        public void IncRevenue(float revenue)
        {
            Debug.Log("MixThirdHelper IncRevenue");
            bridgeIncRevenue(revenue);
        }

        internal void ShareLinkByFacebook(string url)
        {
            Debug.Log("MixThirdHelper ShareLinkByFacebook");
            bridgeShareLinkByFacebook(url);
        }

        internal void SharePhotoByFacebook(byte[] bytes)
        {
            Debug.Log("MixThirdHelper SharePhotoByFacebook");
            string imageString = System.Convert.ToBase64String(bytes);
            byte[] imageBytes = System.Text.Encoding.UTF8.GetBytes(imageString);
            bridgeSharePhotoByFacebook(imageBytes);
        }
# elif UNITY_ANDROID

        string className = "com.mix.sdk.MixHelper";

        private string CallStaticWithString(string methodName, params object[] args)
        {
            AndroidJavaClass jc = new AndroidJavaClass(className);
            return jc.CallStatic<string>(methodName, args);
        }
        private bool CallStaticWithBool(string methodName, params object[] args)
        {
            AndroidJavaClass jc = new AndroidJavaClass(className);
            return jc.CallStatic<bool>(methodName, args);
        }
        private void CallStatic(string methodName, params object[] args)
        {
            AndroidJavaClass jc = new AndroidJavaClass(className);
            jc.CallStatic(methodName, args);
        }

        public void Init(PlatformInfo pinfo)
        {
            Debug.Log("MixThirdHelper Init");
            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
            CallStatic("init", currentActivity, MixMain.mixSDKConfig.debug);
            // init share
            MixThirdShare.instance.OnInit(this);
        }
        public void UploadAddToCart(float price, string curr, string itemId)
        {
            Debug.Log("MixThirdHelper UploadAddToCart");
            CallStatic("uploadAddToCart", price, curr, itemId);
        }
        public void UploadInitCheckout(float price, string curr, string itemId)
        {
            Debug.Log("MixThirdHelper UploadInitCheckout");
            CallStatic("uploadInitCheckout", price, curr, itemId);
        }
        public void UploadPurchase(float price, string curr, string itemId)
        {
            Debug.Log("MixThirdHelper UploadPurchase");
            CallStatic("uploadPurchase", price, curr, itemId);
        }
        public void IncRevenue(float revenue)
        {
            Debug.Log("MixThirdHelper IncRevenue");
            CallStatic("incRevenue", revenue);
        }
        internal void ShareLinkByFacebook(string url)
        {
            Debug.Log("MixThirdHelper ShareLinkByFacebook");
            CallStatic("shareLinkByFacebook", url);
        }

        internal void SharePhotoByFacebook(byte[] bytes)
        {
            Debug.Log("MixThirdHelper SharePhotoByFacebook");
            CallStatic("sharePhotoByFacebook", bytes);
        }
#else
        internal void ShareLinkByFacebook(string url){}
        internal void SharePhotoByFacebook(byte[] bytes){}
#endif
    }
}