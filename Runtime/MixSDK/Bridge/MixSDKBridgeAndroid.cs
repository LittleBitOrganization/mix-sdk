using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using MixNameSpace;
using UnityEngine;

namespace MixNameSpace
{
#if UNITY_ANDROID && !UNITY_EDITOR
    public class MixSDKBridgeAndroid : MixSDKBridgeBase
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
            string itemInfoJson = Json.ToJsonWithList<ItemInfo>(itemInfoList);
            Interaction.Api(MixSDKProto.Type.GooglePayment.Sdk, MixSDKProto.Type.GooglePayment.Init, callback, itemInfoJson);
        }
        public override string GetPaymentMethod()
        {
            return MixSDKProto.Type.GooglePayment.PaymentMethod;
        }
        public override void OnPayment(string itemId, string eventId, string cpOrderId, long gameOrderId, Action<Dictionary<string, object>> callback)
        {
            Dictionary<string, object> paymentDataDict = new Dictionary<string, object>() {
                    { "itemId", itemId },
                    { "envId", eventId },
                    { "cpOrderId", cpOrderId },
                    { "gameOrderId", gameOrderId },
                };
            string paymentDataJson = Json.Serialize(paymentDataDict);
            Interaction.Api(MixSDKProto.Type.GooglePayment.Sdk, MixSDKProto.Type.GooglePayment.Payment, callback, paymentDataJson);
        }

        public override void OnOrderConsume(Dictionary<string, object> dict, Action<Dictionary<string, object>> callback)
        {
            System.Int64 type = (System.Int64) dict["type"];
            string purchaseToken = dict["purchaseToken"] as string;
            string orderId = dict["orderId"] as string;
            string methodName = MixSDKProto.Type.GooglePayment.ConfirmOrderConsume;
            switch (type)
            {
                case 0: { methodName = MixSDKProto.Type.GooglePayment.ConfirmOrderConsume; break; }
                case 1: { methodName = MixSDKProto.Type.GooglePayment.ConfirmOrderUnConsume; break; }
            }
            // 确认谷歌订单:消耗品
            Interaction.Api(MixSDKProto.Type.GooglePayment.Sdk, methodName, callback,
                Json.Serialize(new Dictionary<string, object> {
                    { "purchaseToken", purchaseToken },
                    { "orderId", orderId },
            }));
        }

        public override Dictionary<string, object> GetAllNonConsumable()
        {
            string dictJson = Interaction.Api(MixSDKProto.Type.GooglePayment.Sdk, MixSDKProto.Type.GooglePayment.AllNonConsumable, null);
            // Debug.LogFormat("MIXSDK[C#]-> MixSDKBridgeAndroid-GetAllNonConsumable dictJson:{0};", dictJson);
            if (MixSDKProto.Key.Failed.Equals(dictJson))
            {
                Debug.LogErrorFormat("MIXSDK[C#]-> get all non consumable failed");
                return new Dictionary<string, object>();
            }
            Dictionary<string, object> dict = Json.Deserialize(dictJson) as Dictionary<string, object>;
            return dict;
        }

        public override List<SubscriptionInfo> GetAllSubscriptionInfo()
        {
            string dictJson = Interaction.Api(MixSDKProto.Type.GooglePayment.Sdk, MixSDKProto.Type.GooglePayment.AllSubscriptionInfo, null);
            // Debug.LogFormat("MIXSDK[C#]-> MixSDKBridgeAndroid-GetAllSubscriptionInfo dataType:{0}; dictJson:{1};", Json.Deserialize(dictJson).GetType().Name, dictJson.Substring(0, dictJson.Length / 2));
            // Debug.LogFormat("MIXSDK[C#]-> MixSDKBridgeAndroid-GetAllSubscriptionInfo dataType:{0}; dictJson:{1};", Json.Deserialize(dictJson).GetType().Name, dictJson.Substring(dictJson.Length / 2));
            if (MixSDKProto.Key.Failed.Equals(dictJson))
            {
                Debug.LogErrorFormat("MIXSDK[C#]-> get all subscription info failed");
                return new List<SubscriptionInfo>(0);
            }

            Dictionary<string, object> dict = Json.Deserialize(dictJson) as Dictionary<string, object>;
            List<SubscriptionInfo> subscriptionInfoList = new List<SubscriptionInfo>(dict.Count);
            foreach (var entry in dict)
            {
                string itemId = entry.Key;
                string subsJson = entry.Value as string;
                Debug.LogFormat("MIXSDK[C#]-> MixSDKBridgeAndroid-GetAllSubscriptionInfo dataType:{0}; itemId: {1}; \ndictJson:{2};", Json.Deserialize(subsJson).GetType().Name, itemId, subsJson);
                Debug.LogFormat("MIXSDK[C#]-> dictJson A: {0};", subsJson.Substring(0, subsJson.Length / 2));
                Debug.LogFormat("MIXSDK[C#]-> dictJson B: {0};", subsJson.Substring(subsJson.Length / 2));
                var receipt_wrapper = Json.Deserialize(subsJson) as Dictionary<string, object>;


                var validSkuDetailsKey_ = receipt_wrapper.TryGetValue("skuDetails", out var skuDetailsObject_);
                string skuDetails_ = null;
                if (validSkuDetailsKey_) skuDetails_ = skuDetailsObject_ as string;


                var original_json_payload_wrapper = Json.Deserialize(receipt_wrapper["purchaseJson"] as string) as Dictionary<string, object>;
                var validIsAutoRenewingKey = original_json_payload_wrapper.TryGetValue("autoRenewing", out var autoRenewingObject);
                var isAutoRenewing_ = false;
                if (validIsAutoRenewingKey) isAutoRenewing_ = (bool)autoRenewingObject;


                string orderId = null;
                var validOrderIdKey = original_json_payload_wrapper.TryGetValue("orderId", out var orderIdObject);
                if (validOrderIdKey) orderId = orderIdObject as string;


                // Google specifies times in milliseconds since 1970.
                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var validPurchaseTimeKey = original_json_payload_wrapper.TryGetValue("purchaseTime", out var purchaseTimeObject);
                long purchaseTime = 0;
                if (validPurchaseTimeKey) purchaseTime = (long)purchaseTimeObject;
                var purchaseDate_ = epoch.AddMilliseconds(purchaseTime);


                var validDeveloperPayloadKey = original_json_payload_wrapper.TryGetValue("developerPayload", out var developerPayloadObject);
                var isFreeTrial_ = false;
                var hasIntroductoryPrice_ = false;
                var purchaseHistorySupported_ = false;
                string updateMetadata_ = null;
                if (validDeveloperPayloadKey)
                {
                    var developerPayloadJSON = (string)developerPayloadObject;
                    var developerPayload_wrapper = (Dictionary<string, object>) Json.Deserialize(developerPayloadJSON);
                    var validIsFreeTrialKey =
                        developerPayload_wrapper.TryGetValue("is_free_trial", out var isFreeTrialObject);
                    if (validIsFreeTrialKey) isFreeTrial_ = (bool) isFreeTrialObject;

                    var validHasIntroductoryPriceKey =
                        developerPayload_wrapper.TryGetValue("has_introductory_price_trial",
                            out var hasIntroductoryPriceObject);

                    if (validHasIntroductoryPriceKey) hasIntroductoryPrice_ = (bool)hasIntroductoryPriceObject;

                    var validIsUpdatedKey = developerPayload_wrapper.TryGetValue("is_updated", out var isUpdatedObject);

                    var isUpdated = false;

                    if (validIsUpdatedKey) isUpdated = (bool)isUpdatedObject;

                    if (isUpdated)
                    {
                        var isValidUpdateMetaKey = developerPayload_wrapper.TryGetValue("update_subscription_metadata",
                            out var updateMetadataObject);

                        if (isValidUpdateMetaKey) updateMetadata_ = (string)updateMetadataObject;
                    }
                }

                string skuDetails = skuDetails_;
                bool isAutoRenewing = isAutoRenewing_;
                DateTime purchaseDate = purchaseDate_;
                bool isFreeTrial = isFreeTrial_;
                bool hasIntroductoryPriceTrial = hasIntroductoryPrice_;
                bool purchaseHistorySupported = purchaseHistorySupported_;
                string updateMetadata = updateMetadata_;
                SubsInfo subsInfo = new SubsInfo(skuDetails, isAutoRenewing, purchaseDate, isFreeTrial, hasIntroductoryPriceTrial, purchaseHistorySupported, updateMetadata);

                SubscriptionInfo subscriptionInfo = new SubscriptionInfo();
                subscriptionInfo.originalTransactionId = orderId;
                subscriptionInfo.itemId = itemId;
                subscriptionInfo.isSubscribed = subsInfo.isSubscribed() == Result.True;
                subscriptionInfo.purchaseTime = (long)(subsInfo.getPurchaseDate() - epoch).TotalSeconds;
                subscriptionInfo.expireTime = (long)(subsInfo.getExpireDate() - epoch).TotalSeconds;
                subscriptionInfo.leftTime = (long)subsInfo.getRemainingTime().TotalSeconds;
                subscriptionInfo.isInFree = subsInfo.isFreeTrial() == Result.True;
                subscriptionInfo.isInIntroductory = subsInfo.isIntroductoryPricePeriod() == Result.True;
                subscriptionInfo.isCancel = subsInfo.isCancelled() == Result.True;
                subscriptionInfoList.Add(subscriptionInfo);
            }
            return subscriptionInfoList;
        }

        public override void OnRequestCheckPay(Dictionary<string, object> data, Action<Dictionary<string, object>> success, Action<string> fail)
        {
            Dictionary<string, object> googleCheck = new Dictionary<string, object>() {
                { "originalJson", data["originalJson"] as string },
                { "signature", data["signature"] as string },
            };
            Dictionary<string, object> read = new Dictionary<string, object>() {
                { "gameId", MixMain.mixLogInfo.gameName },
                { "system", MixMain.mixLogInfo.platform },
                { "pn", MixMain.mixLogInfo.pn},
                { "deviceId", MixMain.mixLogInfo.deviceid },
                { "itemType", data["itemType"] as string },
                { "googleCheck", googleCheck },
            };
            base.OnRequestCheckPay(read, success, fail);
        }

        public override string GetWalleChannelName() 
        {
            try 
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) 
                {
                    using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) 
                    {
                        using (AndroidJavaObject applicationContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext")) 
                        {
                            string className = "com.mix.walle.WalleChannelReader";
                            string methodName = "getChannel";
                            using (AndroidJavaClass walleChannelReader = new AndroidJavaClass(className))
                            {
                                string channelName = walleChannelReader.CallStatic<string>(methodName, applicationContext);
                                Debug.Log($"find channel name: {channelName}");
                                return channelName;
                            }
                        }
                    }
                }
            } 
            catch (System.Exception e) { Debug.LogException(e); }
            Debug.Log("not find channel name");
            return null;
        }
    }
    // 订阅商品信息
    public class SubsInfo : SubsInfoBase
    {
        private SubsInfo() { }
        /// <summary>
        /// Especially crucial values relating to Google subscription products.
        /// Note this is intended to be called internally.
        /// </summary>
        /// <param name="skuDetails">The raw JSON from <c>SkuDetail.getOriginalJson</c></param>
        /// <param name="isAutoRenewing">Whether this subscription is expected to auto-renew</param>
        /// <param name="purchaseDate">A date this subscription was billed</param>
        /// <param name="isFreeTrial">Indicates whether this Product is a free trial</param>
        /// <param name="hasIntroductoryPriceTrial">Indicates whether this Product may be owned with an introductory price period.</param>
        /// <param name="purchaseHistorySupported">Unsupported</param>
        /// <param name="updateMetadata">Unsupported. Mechanism previously propagated subscription upgrade information to new subscription. </param>
        /// <exception cref="InvalidProductTypeException">For non-subscription product types. </exception>
        public SubsInfo(string skuDetails, bool isAutoRenewing, DateTime purchaseDate, bool isFreeTrial, bool hasIntroductoryPriceTrial, bool purchaseHistorySupported, string updateMetadata)
        {
            var skuDetails_wrapper = (Dictionary<string, object>) Json.Deserialize(skuDetails);
            var validTypeKey = skuDetails_wrapper.TryGetValue("type", out var typeObject);

            if (!validTypeKey || (string) typeObject == "inapp")
            {
                throw new Exception("Invalid Product Type");
            }

            var validProductIdKey = skuDetails_wrapper.TryGetValue("productId", out var productIdObject);
            productId = null;
            if (validProductIdKey) productId = productIdObject as string;

            this.purchaseDate = purchaseDate;
            this.is_subscribed = Result.True;
            this.is_auto_renewing = isAutoRenewing ? Result.True : Result.False;
            this.is_expired = Result.False;
            this.is_cancelled = isAutoRenewing ? Result.False : Result.True;
            this.is_free_trial = Result.False;

            string sub_period = null;
            if (skuDetails_wrapper.ContainsKey("subscriptionPeriod"))
            {
                sub_period = (string)skuDetails_wrapper["subscriptionPeriod"];
            }
            string free_trial_period = null;
            if (skuDetails_wrapper.ContainsKey("freeTrialPeriod"))
            {
                free_trial_period = (string)skuDetails_wrapper["freeTrialPeriod"];
            }
            string introductory_price = null;
            if (skuDetails_wrapper.ContainsKey("introductoryPrice"))
            {
                introductory_price = (string)skuDetails_wrapper["introductoryPrice"];
            }
            string introductory_price_period_string = null;
            if (skuDetails_wrapper.ContainsKey("introductoryPricePeriod"))
            {
                introductory_price_period_string = (string)skuDetails_wrapper["introductoryPricePeriod"];
            }
            long introductory_price_cycles = 0;
            if (skuDetails_wrapper.ContainsKey("introductoryPriceCycles"))
            {
                introductory_price_cycles = (long)skuDetails_wrapper["introductoryPriceCycles"];
            }

            // for test
            free_trial_period_string = free_trial_period;

            this.subscriptionPeriod = computePeriodTimeSpan(parsePeriodTimeSpanUnits(sub_period));

            this.freeTrialPeriod = TimeSpan.Zero;
            if (isFreeTrial)
            {
                this.freeTrialPeriod = parseTimeSpan(free_trial_period);
            }

            this.introductory_price = introductory_price;
            this.introductory_price_cycles = introductory_price_cycles;
            this.introductory_price_period = TimeSpan.Zero;
            this.is_introductory_price_period = Result.False;
            TimeSpan total_introductory_duration = TimeSpan.Zero;

            if (hasIntroductoryPriceTrial)
            {
                if (introductory_price_period_string != null && introductory_price_period_string.Equals(sub_period))
                {
                    this.introductory_price_period = this.subscriptionPeriod;
                }
                else
                {
                    this.introductory_price_period = parseTimeSpan(introductory_price_period_string);
                }
                // compute the total introductory duration according to the introductory price period and period cycles
                total_introductory_duration = accumulateIntroductoryDuration(parsePeriodTimeSpanUnits(introductory_price_period_string), this.introductory_price_cycles);
            }

            // if this subscription is updated from other subscription, the remaining time will be applied to this subscription
            TimeSpan extra_time = TimeSpan.FromSeconds(updateMetadata == null ? 0.0 : computeExtraTime(updateMetadata, this.subscriptionPeriod.TotalSeconds));

            TimeSpan time_since_purchased = DateTime.UtcNow.Subtract(purchaseDate);


            // this subscription is still in the extra time (the time left by the previous subscription when updated to the current one)
            if (time_since_purchased <= extra_time)
            {
                // this subscription is in the remaining credits from the previous updated one
                this.subscriptionExpireDate = purchaseDate.Add(extra_time);
            }
            else if (time_since_purchased <= this.freeTrialPeriod.Add(extra_time))
            {
                // this subscription is in the free trial period
                // this product will be valid until free trial ends, the beginning of next billing date
                this.is_free_trial = Result.True;
                this.subscriptionExpireDate = purchaseDate.Add(this.freeTrialPeriod.Add(extra_time));
            }
            else if (time_since_purchased < this.freeTrialPeriod.Add(extra_time).Add(total_introductory_duration))
            {
                // this subscription is in the introductory price period
                this.is_introductory_price_period = Result.True;
                DateTime introductory_price_begin_date = this.purchaseDate.Add(this.freeTrialPeriod.Add(extra_time));
                this.subscriptionExpireDate = nextBillingDate(introductory_price_begin_date, parsePeriodTimeSpanUnits(introductory_price_period_string));
            }
            else
            {
                // no matter sub is cancelled or not, the expire date will be next billing date
                DateTime billing_begin_date = this.purchaseDate.Add(this.freeTrialPeriod.Add(extra_time).Add(total_introductory_duration));
                this.subscriptionExpireDate = nextBillingDate(billing_begin_date, parsePeriodTimeSpanUnits(sub_period));
            }

            this.remainedTime = this.subscriptionExpireDate.Subtract(DateTime.UtcNow);
            this.sku_details = skuDetails;
        }
    }
#endif
}