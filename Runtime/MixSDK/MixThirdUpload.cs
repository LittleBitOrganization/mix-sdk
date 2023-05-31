using System;
using com.adjust.sdk;
using UnityEngine;

using System.Collections.Generic;
using System.Reflection;

namespace MixNameSpace
{   
    public interface MixThirdUploader
    {
        void Init(PlatformInfo pinfo);
        void UploadAddToCart(float price, string curr, string itemId);
        void UploadInitCheckout(float price, string curr, string itemId);
        void UploadPurchase(float price, string curr, string itemId);
        void IncRevenue(float revenue);
    }

    public class MixThirdUpload
    {
        static public MixThirdUpload instance = new MixThirdUpload();
        private List<MixThirdUploader> uploaders = new List<MixThirdUploader>();

        public void Init(PlatformInfo pinfo)
        {
            uploaders.Add(new MixThirdAdjust());
            //反射MixThirdHelper
            Assembly assembly = Assembly.GetExecutingAssembly(); // 获取当前程序集 
            object obj = assembly.CreateInstance("MixNameSpace.MixThirdHelper");
            if(obj != null)
            {
                uploaders.Add((MixThirdUploader)obj);
            }
            Debug.Log("MixThirdUpload bridgeInit " + uploaders.Count + " uploaders");
            foreach (MixThirdUploader one in uploaders){
                one.Init(pinfo);
            }
        }

        public void IncRevenue(float revenue)
        {
            Debug.Log("MixThirdUpload IncRevenue");
            foreach (MixThirdUploader one in uploaders)
            {
                one.IncRevenue(revenue);
            }
        }

        public void UploadAddToCart(float price, string currency, string itemId)
        {
            Debug.Log("MixThirdUpload UploadAddToCart");
            foreach (MixThirdUploader one in uploaders)
            {
                one.UploadAddToCart(price, currency, itemId);
            }
            
        }

        public void UploadInitCheckout(float price, string currency, string itemId)
        {
            Debug.Log("MixThirdUpload UploadInitCheckout");
            foreach (MixThirdUploader one in uploaders)
            {
                one.UploadInitCheckout(price, currency, itemId);
            }

        }

        public void UploadPurchase(float price, string currency, string itemId)
        {
            Debug.Log("MixThirdUpload UploadPurchase");
            foreach (MixThirdUploader one in uploaders)
            {
                one.UploadPurchase(price, currency, itemId);
            }
            

        }

    }
}