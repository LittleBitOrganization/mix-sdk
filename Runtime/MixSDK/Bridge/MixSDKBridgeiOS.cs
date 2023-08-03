using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixNameSpace
{
#if UNITY_IOS && !UNITY_EDITOR
    public class MixSDKBridgeiOS : MixSDKBridgeBase
    {
        private readonly string applePayment = MixSDKProto.Type.ApplePayment.Sdk;

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
            Interaction.Api(applePayment, MixSDKProto.Type.ApplePayment.Init, callback, itemInfoJson);
        }

        public override string GetPaymentMethod()
        {
            return MixSDKProto.Type.ApplePayment.PaymentMethod;
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
            Interaction.Api(applePayment, MixSDKProto.Type.ApplePayment.Payment, callback, paymentDataJson);
        }

        public override void OnOrderConsume(Dictionary<string, object> dict, Action<Dictionary<string, object>> callback)
        {
            callback(new Dictionary<string, object>() {
                { MixSDKProto.Key.Code, MixSDKProto.Code.Succ },
                { MixSDKProto.Key.Msg, MixSDKProto.Key.Success },
            });
        }

        public override Dictionary<string, object> GetAllNonConsumable()
        {
            string reciptString = Interaction.Api(applePayment, MixSDKProto.Type.ApplePayment.GetReceipt, null);
            // Debug.LogFormat("MIXSDK[C#]-> MixSDKBridgeiOS-GetAllNonConsumable reciptString:{0};", reciptString);
            if (MixSDKProto.Key.Failed.Equals(reciptString)) 
            {
                Debug.LogErrorFormat("MIXSDK[C#]-> get all non consumable failed");
                return new Dictionary<string, object>(0);
            }

            try {
                AppleReceiptParser appleReceiptParser = new AppleReceiptParser();
                AppleReceipt appleReceipt = appleReceiptParser.Parse(Convert.FromBase64String(reciptString));
                var receiptCreationDate = appleReceipt.receiptCreationDate;
                AppleInAppPurchaseReceipt[] inAppPurchaseReceipts = appleReceipt.inAppPurchaseReceipts;
                // Debug.LogFormat("MIXSDK[C#]-> inAppPurchaseReceipts Length:{0};", inAppPurchaseReceipts.Length);
                 Dictionary<string, object> dict = new Dictionary<string, object>();
                foreach (AppleInAppPurchaseReceipt r in inAppPurchaseReceipts)
                {
                    // Debug.LogFormat("MIXSDK[C#]-> productId:{0}; originalPurchaseDate:{1}; purchaseDate:{2}; subscriptionExpirationDate:{3};", r.productID, r.originalPurchaseDate, r.purchaseDate, r.subscriptionExpirationDate);
                    ItemInfo itemInfo = MixIap.instance.GetItemInfoWithProductId(r.productID);
                    if (null == itemInfo || ProductType.NonConsumable != MixIap.instance.ConvertProductType(itemInfo.type))
                    {
                        continue;
                    }
                    dict.Add(itemInfo.itemId, itemInfo.itemId);
                    return dict;
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("MIXSDK[C#]-> apple receipt parser error:{0}", e.ToString());
            }
            return new Dictionary<string, object>(0);
        }

        public override List<SubscriptionInfo> GetAllSubscriptionInfo()
        {
            string reciptString = Interaction.Api(applePayment, MixSDKProto.Type.ApplePayment.GetReceipt, null);
            // Debug.LogFormat("MIXSDK[C#]-> MixSDKBridgeiOS-GetAllSubscriptionInfo reciptString:{0};", reciptString);
            if (MixSDKProto.Key.Failed.Equals(reciptString))
            {
                Debug.LogErrorFormat("MIXSDK[C#]-> get all subscription failed");
                return new List<SubscriptionInfo>(0);
            }

            try {
                AppleReceiptParser appleReceiptParser = new AppleReceiptParser();
                AppleReceipt appleReceipt = appleReceiptParser.Parse(Convert.FromBase64String(reciptString));
                var receiptCreationDate = appleReceipt.receiptCreationDate;
                AppleInAppPurchaseReceipt[] inAppPurchaseReceipts = appleReceipt.inAppPurchaseReceipts;
                // Debug.LogFormat("MIXSDK[C#]-> inAppPurchaseReceipts Length:{0}; UtcNow:{1}; UtcNow.Second:{2};", inAppPurchaseReceipts.Length, DateTime.UtcNow, (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
                List<SubscriptionInfo> subscriptionInfoList = new List<SubscriptionInfo>(inAppPurchaseReceipts.Length);
                Dictionary<string, object> purchaseRecordsDict = new Dictionary<string, object>();
                // Google specifies times in milliseconds since 1970.
                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                foreach (AppleInAppPurchaseReceipt r in inAppPurchaseReceipts)
                {
                    // Debug.LogFormat("MIXSDK[C#]-> productId:{0}; originalPurchaseDate:{1}; purchaseDate:{2}; subscriptionExpirationDate:{3}; transactionID:{4};", r.productID, r.originalPurchaseDate, r.purchaseDate, r.subscriptionExpirationDate, r.transactionID);
                    purchaseRecordsDict.Add(r.transactionID, new Dictionary<string, object>() {
                        {"productId", r.productID},
                        {"originalPurchaseDate", r.originalPurchaseDate},
                        {"purchaseDate", r.purchaseDate},
                        {"subscriptionExpirationDate", r.subscriptionExpirationDate},
                    });
                    ItemInfo itemInfo = MixIap.instance.GetItemInfoWithProductId(r.productID);
                    if (null == itemInfo || ProductType.Subscription != MixIap.instance.ConvertProductType(itemInfo.type))
                    {
                        continue;
                    }
                    SubsInfo subsInfo = new SubsInfo(r, null);
                    if (Result.True == subsInfo.isExpired() || Result.False == subsInfo.isSubscribed()) continue;
                    // Debug.LogFormat("MIXSDK[C#]-> subsInfo productId:{0}; purchaseDate:{1}; expireDate:{2};", subsInfo.getProductId(), subsInfo.getPurchaseDate(), subsInfo.getExpireDate());


                    // filter records
                    bool isExist = subscriptionInfoList.Exists(sub => sub.itemId.Equals(itemInfo.itemId));
                    if (isExist) 
                    {
                        SubscriptionInfo info = subscriptionInfoList.Find(sub => sub.itemId.Equals(itemInfo.itemId));
                        // Debug.LogFormat("MIXSDK[C#]-> info itemId:{0}; info.expireTime:{1}; subsInfo.getExpireDate:{2};", info.itemId, info.expireTime, subsInfo.getExpireDate().Second);
                        if (info.expireTime > subsInfo.getExpireDate().Second) {
                            continue;
                        } else {
                            subscriptionInfoList.Remove(info);
                        }
                    }

                    SubscriptionInfo subscriptionInfo = new SubscriptionInfo();
                    subscriptionInfo.originalTransactionId = r.transactionID;
                    subscriptionInfo.itemId = itemInfo.itemId;
                    subscriptionInfo.isSubscribed = subsInfo.isSubscribed() == Result.True;
                    subscriptionInfo.purchaseTime = (long)(subsInfo.getPurchaseDate() - epoch).TotalSeconds;
                    subscriptionInfo.expireTime = (long)(subsInfo.getExpireDate() - epoch).TotalSeconds;
                    subscriptionInfo.leftTime = (long)subsInfo.getRemainingTime().TotalSeconds;
                    subscriptionInfo.isInFree = subsInfo.isFreeTrial() == Result.True;
                    subscriptionInfo.isInIntroductory = subsInfo.isIntroductoryPricePeriod() == Result.True;
                    subscriptionInfo.isCancel = subsInfo.isCancelled() == Result.True;
                    subscriptionInfoList.Add(subscriptionInfo);
                }

                string purchaseRecordsJson = Json.Serialize(purchaseRecordsDict);
                // Debug.LogFormat("MIXSDK[C#]-> purchaseRecordsJson:{0};", purchaseRecordsJson);
                Interaction.Api(applePayment, MixSDKProto.Type.ApplePayment.AddRecords, null, purchaseRecordsJson);
                return subscriptionInfoList;
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("MIXSDK[C#]-> apple receipt parser error:{0}", e.ToString());
            }
            return new List<SubscriptionInfo>(0);
        }

        public override void OnAppleRestorePurchase(Action<bool> callback)
        {
            string ret = Interaction.Api(applePayment, MixSDKProto.Type.ApplePayment.RestorePurchase, (retDict) => {
                int code = (int) retDict[MixSDKProto.Key.Code];
                string msg = retDict[MixSDKProto.Key.Msg] as string;
                if (MixSDKProto.Code.Succ != code) {
                    Debug.LogErrorFormat("MIXSDK[C#]-> apple restore purchase failed msg:{0};", msg);
                    callback(false);
                    return;
                }
                callback(true);
            });
            if (MixSDKProto.Key.Failed.Equals(ret)) {
                Debug.LogErrorFormat("MIXSDK[C#]-> apple restore purchase error");
                callback(false);
            }
        }

        public override void OnRefershReceipt(Action<bool> callback)
        {
            string ret = Interaction.Api(applePayment, MixSDKProto.Type.ApplePayment.RefreshReceipt, (retDict) => {
                int code = (int) retDict[MixSDKProto.Key.Code];
                string msg = retDict[MixSDKProto.Key.Msg] as string;
                if (MixSDKProto.Code.Succ != code) {
                    Debug.LogErrorFormat("MIXSDK[C#]-> apple refersh receipt failed msg:{0};", msg);
                    callback(false);
                    return;
                }
                callback(true);
            });
            if (MixSDKProto.Key.Failed.Equals(ret)) {
                Debug.LogErrorFormat("MIXSDK[C#]-> apple refersh receipt error");
                callback(false);
            }
        }

        public override void OnRequestCheckPay(Dictionary<string, object> data, Action<Dictionary<string, object>> success, Action<string> fail) {
            Dictionary<string, object> appleCheck = new Dictionary<string, object>() {
                { "transationId", data["transationId"] as string },
                { "gameOrderId", data["gameOrderId"] + "" },
                { "productId", data["productId"] as string },
                { "reciptString", data["reciptString"] as string },
            };
            Dictionary<string, object> read = new Dictionary<string, object>() {
                { "gameId", MixMain.mixLogInfo.gameName },
                { "system", MixMain.mixLogInfo.platform },
                { "pn", MixMain.mixLogInfo.pn},
                { "deviceId", MixMain.mixLogInfo.deviceid },
                { "itemType", data["itemType"] as string },
                { "appleCheck", appleCheck },
            };
            base.OnRequestCheckPay(read, success, fail);
        }
    }
    // 订阅商品信息
    public class SubsInfo : SubsInfoBase
    {
        private SubsInfo() { }
        /// <summary>
        /// Unpack Apple receipt subscription data.
        /// </summary>
        /// <param name="r">The Apple receipt from <typeparamref name="CrossPlatformValidator"/></param>
        /// <param name="intro_json">From <typeparamref name="IAppleExtensions.GetIntroductoryPriceDictionary"/>. Keys:
        /// <c>introductoryPriceLocale</c>, <c>introductoryPrice</c>, <c>introductoryPriceNumberOfPeriods</c>, <c>numberOfUnits</c>,
        /// <c>unit</c>, which can be fetched from Apple's remote service.</param>
        /// <exception cref="InvalidProductTypeException">Error found involving an invalid product type.</exception>
        /// <see cref="UnityEngine.Purchasing.Security.CrossPlatformValidator"/>
        public SubsInfo(AppleInAppPurchaseReceipt receipt, string intro_json)
        {
            var productType = (AppleStoreProductType) Enum.Parse(typeof(AppleStoreProductType), receipt.productType.ToString());

            if (productType == AppleStoreProductType.Consumable || productType == AppleStoreProductType.NonConsumable)
            {
                throw new Exception("Invalid Product Type");
            }

            if (!string.IsNullOrEmpty(intro_json))
            {
                var intro_wrapper = Json.Deserialize(intro_json) as Dictionary<string, object>;
                var nunit = -1;
                var unit = SubscriptionPeriodUnit.NotAvailable;

                string introductoryPrice = null;
                var validIntroductoryPriceKey = intro_wrapper.TryGetValue("introductoryPrice", out var introductoryPriceObject);
                if (validIntroductoryPriceKey)
                {
                    introductoryPrice = introductoryPriceObject as string;
                }

                string introductoryPriceLocale = null;
                var validIntroductoryPriceLocaleKey = intro_wrapper.TryGetValue("introductoryPriceLocale", out var introductoryPriceLocaleObject);
                if (validIntroductoryPriceLocaleKey)
                {
                    introductoryPriceLocale = introductoryPriceLocaleObject as string;
                }

                if (string.IsNullOrEmpty(introductoryPrice) || string.IsNullOrEmpty(introductoryPriceLocale))
                {
                    this.introductory_price = "not available";
                }
                else
                {
                    this.introductory_price = introductoryPrice + introductoryPriceLocale;
                    if (string.IsNullOrEmpty(this.introductory_price))
                    {
                        this.introductory_price = "not available";
                    }
                    else
                    {
                        try
                        {
                            string introductoryPriceNumberOfPeriods = null;
                            var validIntroductoryPriceNumberOfPeriodsKey = intro_wrapper.TryGetValue("introductoryPriceNumberOfPeriods", out var introductoryPriceNumberOfPeriodsObject);
                            if (validIntroductoryPriceNumberOfPeriodsKey)
                            {
                                introductoryPriceNumberOfPeriods = introductoryPriceNumberOfPeriodsObject as string;
                            }

                            string numberOfUnits = null;
                            var validNumberOfUnitsKey = intro_wrapper.TryGetValue("numberOfUnits", out var numberOfUnitsObject);
                            if (validNumberOfUnitsKey)
                            {
                                numberOfUnits = numberOfUnitsObject as string;
                            }

                            string unit_ = null;
                            var validUnitKey = intro_wrapper.TryGetValue("unit", out var unitObject);
                            if (validUnitKey)
                            {
                                unit_ = unitObject as string;
                            }

                            this.introductory_price_cycles = Convert.ToInt64(introductoryPriceNumberOfPeriods);
                            nunit = Convert.ToInt32(numberOfUnits);
                            unit = (SubscriptionPeriodUnit)Convert.ToInt32(unit_);
                        }
                        catch (Exception e)
                        {
                            Debug.unityLogger.Log("Unable to parse introductory period cycles and duration, this product does not have configuration of introductory price period", e);
                            unit = SubscriptionPeriodUnit.NotAvailable;
                        }
                    }
                }
                
                DateTime now = DateTime.Now;
                switch (unit)
                {
                    case SubscriptionPeriodUnit.Day:
                        this.introductory_price_period = TimeSpan.FromTicks(TimeSpan.FromDays(1).Ticks * nunit);
                        break;
                    case SubscriptionPeriodUnit.Month:
                        TimeSpan month_span = now.AddMonths(1) - now;
                        this.introductory_price_period = TimeSpan.FromTicks(month_span.Ticks * nunit);
                        break;
                    case SubscriptionPeriodUnit.Week:
                        this.introductory_price_period = TimeSpan.FromTicks(TimeSpan.FromDays(7).Ticks * nunit);
                        break;
                    case SubscriptionPeriodUnit.Year:
                        TimeSpan year_span = now.AddYears(1) - now;
                        this.introductory_price_period = TimeSpan.FromTicks(year_span.Ticks * nunit);
                        break;
                    case SubscriptionPeriodUnit.NotAvailable:
                        this.introductory_price_period = TimeSpan.Zero;
                        this.introductory_price_cycles = 0;
                        break;
                }
            }
            else
            {
                this.introductory_price = "not available";
                this.introductory_price_period = TimeSpan.Zero;
                this.introductory_price_cycles = 0;
            }

            DateTime current_date = DateTime.UtcNow;
            this.purchaseDate = receipt.purchaseDate;
            this.productId = receipt.productID;

            this.subscriptionExpireDate = receipt.subscriptionExpirationDate;
            this.subscriptionCancelDate = receipt.cancellationDate;

            // if the product is non-renewing subscription, apple store will not return expiration date for this product
            if (productType == AppleStoreProductType.NonRenewingSubscription)
            {
                this.is_subscribed = Result.Unsupported;
                this.is_expired = Result.Unsupported;
                this.is_cancelled = Result.Unsupported;
                this.is_free_trial = Result.Unsupported;
                this.is_auto_renewing = Result.Unsupported;
                this.is_introductory_price_period = Result.Unsupported;
            }
            else
            {
                this.is_cancelled = (receipt.cancellationDate.Ticks > 0) && (receipt.cancellationDate.Ticks < current_date.Ticks) ? Result.True : Result.False;
                this.is_subscribed = receipt.subscriptionExpirationDate.Ticks >= current_date.Ticks ? Result.True : Result.False;
                this.is_expired = (receipt.subscriptionExpirationDate.Ticks > 0 && receipt.subscriptionExpirationDate.Ticks < current_date.Ticks) ? Result.True : Result.False;
                this.is_free_trial = (receipt.isFreeTrial == 1) ? Result.True : Result.False;
                this.is_auto_renewing = ((productType == AppleStoreProductType.AutoRenewingSubscription) && this.is_cancelled == Result.False
                        && this.is_expired == Result.False) ? Result.True : Result.False;
                this.is_introductory_price_period = receipt.isIntroductoryPricePeriod == 1 ? Result.True : Result.False;
            }

            if (this.is_subscribed == Result.True)
            {
                this.remainedTime = receipt.subscriptionExpirationDate.Subtract(current_date);
            }
            else
            {
                this.remainedTime = TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// An Apple receipt as defined here:
    /// https://developer.apple.com/library/ios/releasenotes/General/ValidateAppStoreReceipt/Chapters/ReceiptFields.html#//apple_ref/doc/uid/TP40010573-CH106-SW1
    /// </summary>
    public class AppleReceipt
    {
        /// <summary>
        /// The app bundle ID
        /// </summary>
        public string bundleID { get; internal set; }

        /// <summary>
        /// The app version number
        /// </summary>
        public string appVersion { get; internal set; }

        /// <summary>
        /// The expiration date of the receipt
        /// </summary>
        public DateTime expirationDate { get; internal set; }

        /// <summary>
        /// An opaque value used, with other data, to compute the SHA-1 hash during validation.
        /// </summary>
        public byte[] opaque { get; internal set; }

        /// <summary>
        /// A SHA-1 hash, used to validate the receipt.
        /// </summary>
        public byte[] hash { get; internal set; }

        /// <summary>
        /// The version of the app that was originally purchased.
        /// </summary>
        public string originalApplicationVersion { get; internal set; }

        /// <summary>
        /// The date the receipt was created
        /// </summary>
        public DateTime receiptCreationDate { get; internal set; }

        /// <summary>
        /// The receipts of the In-App purchases.
        /// </summary>
        public AppleInAppPurchaseReceipt[] inAppPurchaseReceipts;
    }

    /// <summary>
    /// The details of an individual purchase.
    /// </summary>
    public class AppleInAppPurchaseReceipt : IPurchaseReceipt
    {
        /// <summary>
        /// The number of items purchased.
        /// </summary>
        public int quantity { get; internal set; }

        /// <summary>
        /// The product ID
        /// </summary>
        public string productID { get; internal set; }

        /// <summary>
        /// The ID of the transaction.
        /// </summary>
        public string transactionID { get; internal set; }

        /// <summary>
        /// For a transaction that restores a previous transaction, the transaction ID of the original transaction. Otherwise, identical to the transactionID.
        /// </summary>
        public string originalTransactionIdentifier { get; internal set; }

        /// <summary>
        /// The date of purchase.
        /// </summary>
        public DateTime purchaseDate { get; internal set; }

        /// <summary>
        /// For a transaction that restores a previous transaction, the date of the original transaction.
        /// </summary>
        public DateTime originalPurchaseDate { get; internal set; }

        /// <summary>
        /// The expiration date for the subscription, expressed as the number of milliseconds since January 1, 1970, 00:00:00 GMT.
        /// </summary>
        public DateTime subscriptionExpirationDate { get; internal set; }

        /// <summary>
        /// For a transaction that was canceled by Apple customer support, the time and date of the cancellation.
        /// For an auto-renewable subscription plan that was upgraded, the time and date of the upgrade transaction.
        /// </summary>
        public DateTime cancellationDate { get; internal set; }

        /// <summary>
        /// For a subscription, whether or not it is in the free trial period.
        /// </summary>
        public int isFreeTrial { get; internal set; }

        /// <summary>
        /// The type of product.
        /// </summary>
        public int productType { get; internal set; }

        /// <summary>
        /// For an auto-renewable subscription, whether or not it is in the introductory price period.
        /// </summary>
        public int isIntroductoryPricePeriod { get; internal set; }
    }

    /// <summary>
    /// Represents a parsed purchase receipt from a store.
    /// </summary>
    public interface IPurchaseReceipt
    {
        /// <summary>
        /// The ID of the transaction.
        /// </summary>
        string transactionID { get; }

        /// <summary>
        /// The ID of the product purchased.
        /// </summary>
        string productID { get; }

        /// <summary>
        /// The date fof the purchase.
        /// </summary>
        DateTime purchaseDate { get; }
    }

    /// <summary>
    /// Used internally to parse Apple receipts. Corresponds to Apple SKProductPeriodUnit.
    /// </summary>
    /// <see cref="https://developer.apple.com/documentation/storekit/skproductperiodunit?language=objc"/>
    public enum SubscriptionPeriodUnit
    {
        /// <summary>
        /// An interval lasting one day.
        /// </summary>
        Day = 0,
        /// <summary>
        /// An interval lasting one month.
        /// </summary>
        Month = 1,
        /// <summary>
        /// An interval lasting one week.
        /// </summary>
        Week = 2,
        /// <summary>
        /// An interval lasting one year.
        /// </summary>
        Year = 3,
        /// <summary>
        /// Default value when no value is available.
        /// </summary>
        NotAvailable = 4,
    };

    enum AppleStoreProductType
    {
        NonConsumable = 0,
        Consumable = 1,
        NonRenewingSubscription = 2,
        AutoRenewingSubscription = 3,
    };
#endif
}
