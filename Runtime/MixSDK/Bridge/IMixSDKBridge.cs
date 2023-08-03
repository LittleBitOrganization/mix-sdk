using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixNameSpace
{
    public interface IMixSDKBridge 
    {
        void OnLog(string eventName, Dictionary<string, object> dict);
        void OnResume();
        void OnPause();
        void OnInit(List<ItemInfo> itemInfoList, System.Action<Dictionary<string, object>> callback);
        string GetPaymentMethod();
        void OnPayment(string itemId, string eventId, string cpOrderId, long gameOrderId, System.Action<Dictionary<string, object>> callback);
        void OnOrderConsume(Dictionary<string, object> dict, System.Action<Dictionary<string, object>> callback);
        Dictionary<string, object> GetAllNonConsumable();
        List<SubscriptionInfo> GetAllSubscriptionInfo();
        void OnAppleRestorePurchase(Action<bool> callback);
        void OnRefershReceipt(Action<bool> callback);
        bool OnInitBridge();
        // Network Request
        // 请求创建游戏订单
        void OnRequestCreateGameOrderId(string itemId, string paymentMethod, string envId, string cpOrderId, Action<Dictionary<string, object>> success, Action<string> fail);
        // 请求获取未消耗品信息
        void OnRequestGetUncomsueItems(Action<Dictionary<string, object>> success, Action<string> fail);
        // 请求验单
        void OnRequestCheckPay(Dictionary<string, object> read, Action<Dictionary<string, object>> success, Action<string> fail);
        // 请求sdk的消耗品需要主动消耗
        void OnRequestMixGameOrderIdConsume(long gameOrderId, Action<Dictionary<string, object>> success, Action<string> fail);

        // 获取 Walle 渠道名称
        string GetWalleChannelName();
    }
}
