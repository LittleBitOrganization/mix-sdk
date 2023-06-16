using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixNameSpace
{
    public class MixCallbackData
    {
        public long gameOrderId;
        public string cpOrderId;
        public string itemId;
        public ProductType itemType;
    }

    public class SubscriptionInfo
    {
        public string originalTransactionId;
        public string itemId;
        public bool isSubscribed;
        public long purchaseTime;
        public long expireTime;
        public long leftTime;
        public bool isInFree;
        public bool isInIntroductory;
        public bool isCancel;
    }

    public enum ProductType
    {
        Consumable,
        NonConsumable,
        Subscription
    }

    public class MixIap
    {
        static public MixIap instance = new MixIap();
        private Dictionary<string, float> usdPriceMap = new Dictionary<string, float>();
        private Action<string> finishCallback;
        private Action<string> failAction;
        private Action<MixCallbackData> action;
        private bool isInit = false;

        public void Init(MixSDKConfig config, Action<string> finishCallback)
        {
            // 适用于不使用内购的版本
            if (null == config.mixInput.items || 0 == config.mixInput.items.Count) {
                Debug.LogWarningFormat("initialize success, but no product information was found!");
                finishCallback("initialize success");
                return;
            }
            this.finishCallback = finishCallback;
            List<ItemInfo> itemInfoList = config.mixInput.items;

            for (int i = 0, len = itemInfoList.Count; i < len; i++)
            {
                ItemInfo itemInfo = itemInfoList[i];
                float price = 0.0F;
                float.TryParse(itemInfo.usdPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out price);
                usdPriceMap.Add(itemInfo.itemId, price);
            }

            MixSDKBridgeFactory.instance.OnInit(config.mixInput.items, (retDict) => {
                int code = (int) retDict[MixSDKProto.Key.Code];
                string msg = retDict[MixSDKProto.Key.Msg] as string;
                if (MixSDKProto.Code.Succ == code)
                {
                    isInit = true;
                    int status = 3;
                    int.TryParse(retDict[MixSDKProto.Key.Data] as string, out status);
                    switch (status)
                    {
                        // Disconnected[connect fail and would try before buying]
                        case 0: { break; }
                        // OK
                        case 1: { break; }
                        // Unavailable[返回这个一般是国内账号,切换海外账号测试]
                        case 2: { break; }
                        // unknown
                        case 3: { break; }
                    }
                    Debug.LogFormat("MIXSDK[C#]-> init purchasing status:{1}; msg:{0};", msg, status);
                    UnityMainThreadDispatcher.Instance().Enqueue(() => { this.finishCallback(msg); });

                    // 拉取已经支付但未消耗的订单
                    MixSDKBridgeFactory.instance.OnRequestGetUncomsueItems((dict) => {
                        // Debug.LogFormat("MIXSDK[C#]-> request not consume order success:{0};", Json.Serialize(dict));
                        if (null == this.action)
                        {
                            Debug.LogErrorFormat("MIXSDK[C#]-> The action must be not null");
                            return;
                        }
                        List<object> list = dict["unconsumeItems"] as List<object>;
                        for (int i = 0, len = list.Count; i < len; i++)
                        {
                            Dictionary<string, object> data = list[i] as Dictionary<string, object>;
                            MixCallbackData mixCallbackData = new MixCallbackData();
                            mixCallbackData.gameOrderId = (long)data["gameOrderId"];
                            mixCallbackData.itemId = data["itemId"] as string;
                            mixCallbackData.itemType = this.ConvertProductType(this.GetItemInfo(mixCallbackData.itemId).type);
                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                Debug.LogWarning("BEFORE TEST!");
                                PlayerPrefs.SetInt("Test",1);
                                Debug.LogWarning("AFTER TEST!");
                                this.action(mixCallbackData);
                            });
                        }
                    }, (message) => {
                        Debug.LogErrorFormat("MIXSDK[C#]-> request not consume order error:{0};", message);
                    });
                }
                else
                {
                    Debug.LogErrorFormat("MIXSDK[C#]-> init purchasing failed:{0};", msg);
                    UnityMainThreadDispatcher.Instance().Enqueue(() => { this.finishCallback(msg); });
                }
            });
        }

        public SubscriptionInfo GetSubscriptionInfo(string itemId)
        {
            if (!this.isInit) return null;
            return null;
        }

        public List<SubscriptionInfo> GetAllSubscriptionInfo()
        {
            if (!this.isInit) return new List<SubscriptionInfo>(0);
            List<SubscriptionInfo> subscriptionInfoList = MixSDKBridgeFactory.instance.GetAllSubscriptionInfo();
            return subscriptionInfoList;
        }
        public Dictionary<string, string> GetAllNonConsumable()
        {
            if (!this.isInit) return new Dictionary<string, string>(0);
            Dictionary<string, string> ret = new Dictionary<string, string>();
            Dictionary<string, object> dict = MixSDKBridgeFactory.instance.GetAllNonConsumable();
            foreach (var d in dict) {
                ret.Add(d.Key, d.Value.ToString());
            }
            return ret;
        }

        public List<ItemInfo> GetItemInfos() {
            if (!this.isInit) return new List<ItemInfo>(0);
            return MixMain.mixSDKConfig.mixInput.items;
        }

        public ItemInfo GetItemInfo(string itemId)
        {
            if (!this.isInit) return null;
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogErrorFormat("Can not find the itemId:{0};", itemId);
                return null;
            } 
            List <ItemInfo> itemInfoList = this.GetItemInfos();
            foreach (var itemInfo in itemInfoList)
            {
                if (itemId.Equals(itemInfo.itemId))
                {
                    return itemInfo;
                }
            }
            return null;
        }

        public ItemInfo GetItemInfoWithProductId(string productId)
        {
            if (!this.isInit) return null;
            if (string.IsNullOrEmpty(productId))
            {
                Debug.LogErrorFormat("Can not find the productId:{0};", productId);
                return null;
            }
            List<ItemInfo> itemInfoList = this.GetItemInfos();
            foreach (var itemInfo in itemInfoList)
            {
                if (productId.Equals(itemInfo.googlePlayId) || productId.Equals(itemInfo.appleStoreId))
                {
                    return itemInfo;
                }
            }
            return null;
        }

        public ProductType ConvertProductType(int itemType)
        {
            if (!this.isInit) return ProductType.Consumable;
            switch (itemType)
            {
                case 0: { return ProductType.Consumable;    }
                case 1: { return ProductType.NonConsumable; }
                case 2: { return  ProductType.Subscription; }
                default: {
                    Debug.LogErrorFormat("Can not find the itemType:{0};", itemType);
                    return ProductType.Consumable;
                }
            }
        }

        public void SetAction(Action<MixCallbackData> action)
        {
            this.action = action;
        }
        public Action<MixCallbackData> GetAction() {
            return this.action;
        }
        public Action<string> GetPurchaseFailAction()
        {
            return this.failAction;
        }

        public void PurchaseItem(string itemId, Action<string> failAction)
        {
            PurchaseItem(itemId, null, failAction);
        }
        public void PurchaseItem(string itemId, string cpOrderId, Action<string> failAction)
        {
            PurchaseItem(itemId, null, cpOrderId, failAction);
        }

        public void PurchaseItem(string itemId, string envId, string cpOrderId, Action<string> failAction)
        {
            if (!this.isInit) return;
            float price = usdPriceMap[itemId];
            MixThirdUpload.instance.UploadAddToCart(price, "USD", itemId);
            this.failAction = failAction;
            cpOrderId = null == cpOrderId ? System.Guid.NewGuid().ToString("N") : cpOrderId;
            string paymentMethod = MixSDKBridgeFactory.instance.GetPaymentMethod();
            MixSDKBridgeFactory.instance.OnRequestCreateGameOrderId(itemId, paymentMethod, envId, cpOrderId, (dict) => { // success
                Debug.LogFormat("MIXSDK[C#]->create game order success:{0};", dict["gameOrderId"]);
                long gameOrderId = (long) dict["gameOrderId"];
                // 第三方日志上报
                MixThirdUpload.instance.UploadInitCheckout(price, "USD", itemId);
                // 发起支付
                MixSDKBridgeFactory.instance.OnPayment(itemId, envId, cpOrderId, gameOrderId, (retDict) => {
                    int code = (int) retDict[MixSDKProto.Key.Code];
                    string msg = retDict[MixSDKProto.Key.Msg] as string;
                    if (MixSDKProto.Code.Succ != code) // 成功会直接走验单逻辑(不会回调),这里只用于提示错误信息
                    {
                        Debug.LogWarningFormat("MIXSDK[C#]-> PurchaseItem error:{0};", msg);
                        if (null != failAction)  failAction(msg);
                    }
                });
            }, (msg) => {  // failed
                Debug.LogErrorFormat("MIXSDK[C#]-> create game order failed msg:{0};", msg);
                string text = "create game orderId failed:" + msg;
                if (null != failAction) failAction("{\"code\":-5225,\"msg\":\"" + text +"\"}");
            });
        }

        public void FinishPurchase(MixCallbackData e)
        {
            if (!this.isInit) return;
            MixSDKBridgeFactory.instance.OnRequestMixGameOrderIdConsume(e.gameOrderId, (dict) => {
                Debug.LogFormat("MIXSDK[C#]-> consume order success:{0};", Json.Serialize(dict));
            }, (msg) => {
                Debug.LogErrorFormat("MIXSDK[C#]-> consume failed msg:{0}; ", msg);
            });
        }
        // click app store restore
        public void AppleRestore(Action<bool> callback)
        {
            Debug.Log("AppleRestore -----");
            if (null == callback) {
                Debug.LogErrorFormat("MIXSDK[C#]-> the callback must be not null");
                return;
            }
            MixSDKBridgeFactory.instance.OnAppleRestorePurchase(callback);
        }
        public void AppleRefershReceipt(Action<bool> callback) 
        {
            Debug.Log("AppleRefershReceipt -----");
            if (null == callback) {
                Debug.LogErrorFormat("MIXSDK[C#]-> the callback must be not null");
                return;
            }
            MixSDKBridgeFactory.instance.OnRefershReceipt(callback);
        }
    }
}