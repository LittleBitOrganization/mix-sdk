using System;
using System.Collections;
using System.Collections.Generic;
using MixNameSpace;
using UnityEngine;

namespace MixNameSpace
{
#if UNITY_EDITOR && !UNITY_ANDROID && !UNITY_IOS
    public class MixSDKBridgeEditor : MixSDKBridgeBase
    {
        public override void OnLog(string eventName, Dictionary<string, object> dict)
        {
            
        }

        public override void OnPause()
        {
            
        }

        public override void OnResume()
        {
           
        }

        public override void OnInit(List<ItemInfo> itemInfoList, Action<Dictionary<string, object>> callback)
        {
            
        }
        public override string GetPaymentMethod()
        {
            return "None";
        }

        public override Dictionary<string, object> GetAllNonConsumable()
        {
            return new Dictionary<string, object>();
        }
       
        public override List<SubscriptionInfo> GetAllSubscriptionInfo()
        {
            return new List<SubscriptionInfo>(0);
        }

        public override void OnPayment(string itemId, string eventId, string cpOrderId, long gameOrderId, Action<Dictionary<string, object>> callback)
        {
            
        }
    }
    // 订阅商品信息
    public class SubsInfo : SubsInfoBase
    {
        private SubsInfo() { }
        public SubsInfo(string receipt)
        {

        }
    }
#endif
}