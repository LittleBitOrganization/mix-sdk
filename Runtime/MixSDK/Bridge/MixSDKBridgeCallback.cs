using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

namespace MixNameSpace
{
    public class MixSDKBridgeCallback : MonoBehaviour
    {
        static public MixSDKBridgeCallback instance = null;
        public void Awake()
        {
            instance = this;
        }

        private void OnApplicationFocus(bool focusStatus) 
        {
            if (focusStatus) {
                MixSDKBridgeFactory.instance.OnResume();
            }
            else 
            {
                MixSDKBridgeFactory.instance.OnPause();
            }
        }
        [MixDoNotRename]
        // 查单
        public void CheckOrder(string jsonParams)
        {
            Debug.LogFormat("MIXSDK[C#]-> CheckOrder data:{0};", jsonParams);
            Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(jsonParams);
            Action<string> failAction = MixIap.instance.GetPurchaseFailAction();
            System.Int64 type = (System.Int64) data["itemType"];
            ProductType productType_ = MixIap.instance.ConvertProductType((int)type);
            string itemType = MixCommon.FromProductType(productType_);
            data.Remove("itemType");
            data.Add("itemType", itemType);
            data.Add("type", type);
            MixSDKBridgeFactory.instance.OnRequestCheckPay(data, (dict) => {
                Debug.LogFormat("MIXSDK[C#]-> check game order success {0};", Json.Serialize(data));
                string productId = data["productId"] as string;
                ItemInfo itemInfo = MixIap.instance.GetItemInfoWithProductId(productId);
                ProductType productType = MixIap.instance.ConvertProductType(itemInfo.type);
                if (null == itemInfo) {
                    Debug.LogErrorFormat("MIXSDK[C#]-> Can not found the item:{0}:", productId);
                    if (null != failAction) failAction("Can not found the item:" + productId);
                    return;
                }

                if (productType == ProductType.Consumable || productType == ProductType.NonConsumable) {
                    float price = 0.0F;
                    float.TryParse(itemInfo.usdPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out price);
                    MixThirdUpload.instance.UploadPurchase(price, "USD", itemInfo.itemId);
                }

                if (productType == ProductType.Subscription) { // 订阅查单成功直接回调CP
                    MixCallbackData mixCallbackData = new MixCallbackData();
                    mixCallbackData.gameOrderId = (long)dict["gameOrderId"];
                    mixCallbackData.itemId = itemInfo.itemId;
                    mixCallbackData.itemType = productType;
                    Action<MixCallbackData> action = MixIap.instance.GetAction();
                    if (null == action)
                    {
                        Debug.LogErrorFormat("MIXSDK[C#]-> The Purchase Action must be not null");
                        return;
                    }
                    action(mixCallbackData);
                } else {
                    MixSDKBridgeFactory.instance.OnOrderConsume(data, (d) => {
                        int code = (int)d[MixSDKProto.Key.Code];
                        if (MixSDKProto.Code.Succ == code)
                        {
                            MixCallbackData mixCallbackData = new MixCallbackData();
                            mixCallbackData.gameOrderId = (long)dict["gameOrderId"];
                            mixCallbackData.itemId = itemInfo.itemId;
                            mixCallbackData.itemType = productType;
                            Action<MixCallbackData> action = MixIap.instance.GetAction();
                            if (null == action)
                            {
                                Debug.LogErrorFormat("MIXSDK[C#]-> The Purchase Action must be not null");
                                return;
                            }
                            action(mixCallbackData);
                        }
                        else
                        {
                            string msg = d[MixSDKProto.Key.Msg] as string;
                            Debug.LogFormat("MIXSDK[C#]-> ConfirmOrderConsume failed msg:{0};", msg);
                            if (null != failAction) failAction(msg);

                        }
                    });
                }
            }, (msg) => {
                Debug.LogErrorFormat("MIXSDK[C#]-> check game order failed msg:{0};", msg);
                // TODO 上传查单失败日志
                if (null != failAction) failAction(msg);
            });
        }
    }
}
