using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace MixNameSpace
{   
    public abstract class MixSDKBridgeBase : IMixSDKBridge
    {
        public abstract void OnLog(string eventName, Dictionary<string, object> dict);
        public abstract void OnInit(List<ItemInfo> itemInfoList, Action<Dictionary<string, object>> callback);
        public abstract void OnPause();
        public abstract void OnResume();
        public abstract string GetPaymentMethod();
        public abstract void OnPayment(string itemId, string eventId, string cpOrderId, long gameOrderId, Action<Dictionary<string, object>> callback);
        public abstract void OnOrderConsume(Dictionary<string, object> dict, Action<Dictionary<string, object>> callback);
        public abstract Dictionary<string, object> GetAllNonConsumable();
        public abstract List<SubscriptionInfo> GetAllSubscriptionInfo();
        public virtual void OnAppleRestorePurchase(Action<bool> callback) { callback(false); }
        public virtual void OnRefershReceipt(Action<bool> callback) { callback(false); }
        // 处理各个平台相同的功能
        public bool OnInitBridge()
        {
            //GameObject go = new GameObject("MixSDKBridge");
            //UnityEngine.Object.DontDestroyOnLoad(go);
            //go.AddComponent<MixSDKBridgeCallback>();
            return true;
        }

        public void OnRequestCreateGameOrderId(string itemId, string paymentMethod, string envId, string cpOrderId, Action<Dictionary<string, object>> success, Action<string> fail)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                cpOrderId = null == cpOrderId ? System.Guid.NewGuid().ToString("N") : cpOrderId;
                envId = null == envId ? "purchase" : envId;
                Dictionary<string, object> read = new Dictionary<string, object>();
                read.Add("system", MixMain.mixLogInfo.platform);
                read.Add("deviceId", MixMain.mixLogInfo.deviceid);
                read.Add("pn", MixMain.mixLogInfo.pn);
                read.Add("gameId", MixMain.mixLogInfo.gameName);
                read.Add("createRead", new Dictionary<string, object>() {
                    { "itemId", itemId },
                    { "paymentMethod", paymentMethod },
                    { "cpOrderId", cpOrderId },
                    { "extraInfo", new Dictionary<string, object>() {
                        { "envId", envId },
                    } },
                });
                Debug.LogFormat("MIXSDK[C#]-> request Create Game OrderId read:{0};", Json.Serialize(read));
                MixSDKBridgeCallback.instance.StartCoroutine(MixServer.SendPost(MixUrlPath.CREATE_GAME_ORDER_PATH, read, success, fail));
            });
        }
        
        public void OnRequestGetUncomsueItems(Action<Dictionary<string, object>> success, Action<string> fail)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Dictionary<string, object> read = new Dictionary<string, object>();
                read.Add("gameId", MixMain.mixLogInfo.gameName);
                read.Add("system", MixMain.mixLogInfo.platform);
                read.Add("pn", MixMain.mixLogInfo.pn);
                read.Add("deviceId", MixMain.mixLogInfo.deviceid);
                MixSDKBridgeCallback.instance.StartCoroutine(MixServer.SendPost(MixUrlPath.GET_UNCONSUME_ITEM_PATH, read, success, fail));
            });
        }

        public virtual void OnRequestCheckPay(Dictionary<string, object> read, Action<Dictionary<string, object>> success, Action<string> fail)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                MixSDKBridgeCallback.instance.StartCoroutine(MixServer.SendPost(MixUrlPath.CHECK_PAY_PATH, read, success, fail));
            });
        }

        public void OnRequestMixGameOrderIdConsume(long gameOrderId, Action<Dictionary<string, object>> success, Action<string> fail)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Dictionary<string, object> read = new Dictionary<string, object>();
                read.Add("gameOrderId", gameOrderId);
                MixSDKBridgeCallback.instance.StartCoroutine(MixServer.SendPost(MixUrlPath.GAME_ORDER_ID_CONSUME_PATH, read, success, fail));
            });
        }

        public virtual string GetWalleChannelName() 
        {
            return null;
        }
    }

    // 订阅商品信息
    public class SubsInfoBase
    {
        protected Result is_subscribed;
        protected Result is_expired;
        protected Result is_cancelled;
        protected Result is_free_trial;
        protected Result is_auto_renewing;
        protected Result is_introductory_price_period;
        protected string productId;
        protected DateTime purchaseDate;
        protected DateTime subscriptionExpireDate;
        protected DateTime subscriptionCancelDate;
        protected TimeSpan remainedTime;
        protected string introductory_price;
        protected TimeSpan introductory_price_period;
        protected long introductory_price_cycles;

        protected TimeSpan freeTrialPeriod;
        protected TimeSpan subscriptionPeriod;

        // for test
        protected string free_trial_period_string;
        protected string sku_details;

        protected SubsInfoBase() { }

        /// <summary>
        /// Store specific product identifier.
        /// </summary>
        /// <returns>The product identifier from the store receipt.</returns>
        public string getProductId() { return this.productId; }

        /// <summary>
        /// A date this subscription was billed.
        /// Note the store-specific behavior.
        /// </summary>
        /// <returns>
        /// For Apple, the purchase date is the date when the subscription was either purchased or renewed.
        /// For Google, the purchase date is the date when the subscription was originally purchased.
        /// </returns>
        public DateTime getPurchaseDate() { return this.purchaseDate; }

        /// <summary>
        /// Indicates whether this auto-renewable subscription Product is currently subscribed or not.
        /// Note the store-specific behavior.
        /// Note also that the receipt may update and change this subscription expiration status if the user sends
        /// their iOS app to the background and then returns it to the foreground. It is therefore recommended to remember
        /// subscription expiration state at app-launch, and ignore the fact that a subscription may expire later during
        /// this app launch runtime session.
        /// </summary>
        /// <returns>
        /// <typeparamref name="Result.True"/> Subscription status if the store receipt's expiration date is
        /// after the device's current time.
        /// <typeparamref name="Result.False"/> otherwise.
        /// Non-renewable subscriptions in the Apple store return a <typeparamref name="Result.Unsupported"/> value.
        /// </returns>
        /// <seealso cref="isExpired"/>
        /// <seealso cref="DateTime.UtcNow"/>
        public Result isSubscribed() { return this.is_subscribed; }

        /// <summary>
        /// Indicates whether this auto-renewable subscription Product is currently unsubscribed or not.
        /// Note the store-specific behavior.
        /// Note also that the receipt may update and change this subscription expiration status if the user sends
        /// their iOS app to the background and then returns it to the foreground. It is therefore recommended to remember
        /// subscription expiration state at app-launch, and ignore the fact that a subscription may expire later during
        /// this app launch runtime session.
        /// </summary>
        /// <returns>
        /// <typeparamref name="Result.True"/> Subscription status if the store receipt's expiration date is
        /// before the device's current time.
        /// <typeparamref name="Result.False"/> otherwise.
        /// Non-renewable subscriptions in the Apple store return a <typeparamref name="Result.Unsupported"/> value.
        /// </returns>
        /// <seealso cref="isSubscribed"/>
        /// <seealso cref="DateTime.UtcNow"/>
        public Result isExpired() { return this.is_expired; }

        /// <summary>
        /// Indicates whether this Product has been cancelled.
        /// A cancelled subscription means the Product is currently subscribed, and will not renew on the next billing date.
        /// </summary>
        /// <returns>
        /// <typeparamref name="Result.True"/> Cancellation status if the store receipt's indicates this subscription is cancelled.
        /// <typeparamref name="Result.False"/> otherwise.
        /// Non-renewable subscriptions in the Apple store return a <typeparamref name="Result.Unsupported"/> value.
        /// </returns>
        public Result isCancelled() { return this.is_cancelled; }

        /// <summary>
        /// Indicates whether this Product is a free trial.
        /// Note the store-specific behavior.
        /// </summary>
        /// <returns>
        /// <typeparamref name="Result.True"/> This subscription is a free trial according to the store receipt.
        /// <typeparamref name="Result.False"/> This subscription is not a free trial according to the store receipt.
        /// Non-renewable subscriptions in the Apple store
        /// and Google subscriptions queried on devices with version lower than 6 of the Android in-app billing API return a <typeparamref name="Result.Unsupported"/> value.
        /// </returns>
        public Result isFreeTrial() { return this.is_free_trial; }

        /// <summary>
        /// Indicates whether this Product is expected to auto-renew. The product must be auto-renewable, not canceled, and not expired.
        /// </summary>
        /// <returns>
        /// <typeparamref name="Result.True"/> The store receipt's indicates this subscription is auto-renewing.
        /// <typeparamref name="Result.False"/> The store receipt's indicates this subscription is not auto-renewing.
        /// Non-renewable subscriptions in the Apple store return a <typeparamref name="Result.Unsupported"/> value.
        /// </returns>
        public Result isAutoRenewing() { return this.is_auto_renewing; }

        /// <summary>
        /// Indicates how much time remains until the next billing date.
        /// Note the store-specific behavior.
        /// Note also that the receipt may update and change this subscription expiration status if the user sends
        /// their iOS app to the background and then returns it to the foreground.
        /// </summary>
        /// <returns>
        /// A time duration from now until subscription billing occurs.
        /// Google subscriptions queried on devices with version lower than 6 of the Android in-app billing API return <typeparamref name="TimeSpan.MaxValue"/>.
        /// </returns>
        /// <seealso cref="DateTime.UtcNow"/>
        public TimeSpan getRemainingTime() { return this.remainedTime; }

        /// <summary>
        /// Indicates whether this Product is currently owned within an introductory price period.
        /// Note the store-specific behavior.
        /// </summary>
        /// <returns>
        /// <typeparamref name="Result.True"/> The store receipt's indicates this subscription is within its introductory price period.
        /// <typeparamref name="Result.False"/> The store receipt's indicates this subscription is not within its introductory price period.
        /// <typeparamref name="Result.False"/> If the product is not configured to have an introductory period.
        /// Non-renewable subscriptions in the Apple store return a <typeparamref name="Result.Unsupported"/> value.
        /// Google subscriptions queried on devices with version lower than 6 of the Android in-app billing API return a <typeparamref name="Result.Unsupported"/> value.
        /// </returns>
        public Result isIntroductoryPricePeriod() { return this.is_introductory_price_period; }

        /// <summary>
        /// Indicates how much time remains for the introductory price period.
        /// Note the store-specific behavior.
        /// </summary>
        /// <returns>
        /// Duration remaining in this product's introductory price period.
        /// Subscription products with no introductory price period return <typeparamref name="TimeSpan.Zero"/>.
        /// Products in the Apple store return <typeparamref name="TimeSpan.Zero"/> if the application does
        /// not support iOS version 11.2+, macOS 10.13.2+, or tvOS 11.2+.
        /// <typeparamref name="TimeSpan.Zero"/> returned also for products which do not have an introductory period configured.
        /// </returns>
        public TimeSpan getIntroductoryPricePeriod() { return this.introductory_price_period; }

        /// <summary>
        /// For subscriptions with an introductory price, get this price.
        /// Note the store-specific behavior.
        /// </summary>
        /// <returns>
        /// For subscriptions with a introductory price, a localized price string.
        /// For Google store the price may not include the currency symbol (e.g. $) and the currency code is available in <typeparamref name="ProductMetadata.isoCurrencyCode"/>.
        /// For all other product configurations, the string <c>"not available"</c>.
        /// </returns>
        /// <seealso cref="ProductMetadata.isoCurrencyCode"/>
        public string getIntroductoryPrice() { return string.IsNullOrEmpty(this.introductory_price) ? "not available" : this.introductory_price; }

        /// <summary>
        /// Indicates the number of introductory price billing periods that can be applied to this subscription Product.
        /// Note the store-specific behavior.
        /// </summary>
        /// <returns>
        /// Products in the Apple store return <c>0</c> if the application does not support iOS version 11.2+, macOS 10.13.2+, or tvOS 11.2+.
        /// <c>0</c> returned also for products which do not have an introductory period configured.
        /// </returns>
        /// <seealso cref="intro"/>
        public long getIntroductoryPricePeriodCycles() { return this.introductory_price_cycles; }

        /// <summary>
        /// When this auto-renewable receipt expires.
        /// </summary>
        /// <returns>
        /// An absolute date when this receipt will expire.
        /// </returns>
        public DateTime getExpireDate() { return this.subscriptionExpireDate; }

        /// <summary>
        /// When this auto-renewable receipt was canceled.
        /// Note the store-specific behavior.
        /// </summary>
        /// <returns>
        /// For Apple store, the date when this receipt was canceled.
        /// For other stores this will be <c>null</c>.
        /// </returns>
        public DateTime getCancelDate() { return this.subscriptionCancelDate; }

        /// <summary>
        /// The period duration of the free trial for this subscription, if enabled.
        /// Note the store-specific behavior.
        /// </summary>
        /// <returns>
        /// For Google Play store if the product is configured with a free trial, this will be the period duration.
        /// For Apple store this will be <c> null </c>.
        /// </returns>
        public TimeSpan getFreeTrialPeriod() { return this.freeTrialPeriod; }

        /// <summary>
        /// The duration of this subscription.
        /// Note the store-specific behavior.
        /// </summary>
        /// <returns>
        /// A duration this subscription is valid for.
        /// <typeparamref name="TimeSpan.Zero"/> returned for Apple products.
        /// </returns>
        public TimeSpan getSubscriptionPeriod() { return this.subscriptionPeriod; }

        /// <summary>
        /// The string representation of the period in ISO8601 format this subscription is free for.
        /// Note the store-specific behavior.
        /// </summary>
        /// <returns>
        /// For Google Play store on configured subscription this will be the period which the can own this product for free, unless
        /// the user is ineligible for this free trial.
        /// For Apple store this will be <c> null </c>.
        /// </returns>
        public string getFreeTrialPeriodString() { return this.free_trial_period_string; }

        /// <summary>
        /// The raw JSON SkuDetails from the underlying Google API.
        /// Note the store-specific behavior.
        /// Note this is not supported.
        /// </summary>
        /// <returns>
        /// For Google store the <c> SkuDetails#getOriginalJson </c> results.
        /// For Apple this returns <c>null</c>.
        /// </returns>
        public string getSkuDetails() { return this.sku_details; }

        /// <summary>
        /// A JSON including a collection of data involving free-trial and introductory prices.
        /// Note the store-specific behavior.
        /// Used internally for subscription updating on Google store.
        /// </summary>
        /// <returns>
        /// A JSON with keys: <c>productId</c>, <c>is_free_trial</c>, <c>is_introductory_price_period</c>, <c>remaining_time_in_seconds</c>.
        /// </returns>
        /// <seealso cref="SubscriptionManager.UpdateSubscription"/>
        public string getSubscriptionInfoJsonString()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("productId", this.productId);
            dict.Add("is_free_trial", this.is_free_trial);
            dict.Add("is_introductory_price_period", this.is_introductory_price_period == Result.True);
            dict.Add("remaining_time_in_seconds", this.remainedTime.TotalSeconds);
            return Json.Serialize(dict);
        }

        protected DateTime nextBillingDate(DateTime billing_begin_date, TimeSpanUnits units)
        {

            if (units.days == 0.0 && units.months == 0 && units.years == 0) return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            DateTime next_billing_date = billing_begin_date;
            // find the next billing date that after the current date
            while (DateTime.Compare(next_billing_date, DateTime.UtcNow) <= 0)
            {

                next_billing_date = next_billing_date.AddDays(units.days).AddMonths(units.months).AddYears(units.years);
            }
            return next_billing_date;
        }

        protected TimeSpan accumulateIntroductoryDuration(TimeSpanUnits units, long cycles)
        {
            TimeSpan result = TimeSpan.Zero;
            for (long i = 0; i < cycles; i++)
            {
                result = result.Add(computePeriodTimeSpan(units));
            }
            return result;
        }

        protected TimeSpan computePeriodTimeSpan(TimeSpanUnits units)
        {
            DateTime now = DateTime.Now;
            return now.AddDays(units.days).AddMonths(units.months).AddYears(units.years).Subtract(now);
        }


        protected double computeExtraTime(string metadata, double new_sku_period_in_seconds)
        {
            var wrapper = (Dictionary<string, object>)Json.Deserialize(metadata);
            // var wrapper = (Dictionary<string, object>) MiniJson.JsonDecode(metadata);
            long old_sku_remaining_seconds = (long)wrapper["old_sku_remaining_seconds"];
            long old_sku_price_in_micros = (long)wrapper["old_sku_price_in_micros"];

            double old_sku_period_in_seconds = (parseTimeSpan((string)wrapper["old_sku_period_string"])).TotalSeconds;
            long new_sku_price_in_micros = (long)wrapper["new_sku_price_in_micros"];
            double result = ((((double)old_sku_remaining_seconds / (double)old_sku_period_in_seconds) * (double)old_sku_price_in_micros) / (double)new_sku_price_in_micros) * new_sku_period_in_seconds;
            return result;
        }

        protected TimeSpan parseTimeSpan(string period_string)
        {
            TimeSpan result = TimeSpan.Zero;
            try
            {
                result = XmlConvert.ToTimeSpan(period_string);
            }
            catch (Exception)
            {
                if (period_string == null || period_string.Length == 0)
                {
                    result = TimeSpan.Zero;
                }
                else
                {
                    // .Net "P1W" is not supported and throws a FormatException
                    // not sure if only weekly billing contains "W"
                    // need more testing
                    result = new TimeSpan(7, 0, 0, 0);
                }
            }
            return result;
        }

        protected TimeSpanUnits parsePeriodTimeSpanUnits(string time_span)
        {
            switch (time_span)
            {
                case "P1W":
                    // weekly subscription
                    return new TimeSpanUnits(7.0, 0, 0);
                case "P1M":
                    // monthly subscription
                    return new TimeSpanUnits(0.0, 1, 0);
                case "P3M":
                    // 3 months subscription
                    return new TimeSpanUnits(0.0, 3, 0);
                case "P6M":
                    // 6 months subscription
                    return new TimeSpanUnits(0.0, 6, 0);
                case "P1Y":
                    // yearly subscription
                    return new TimeSpanUnits(0.0, 0, 1);
                default:
                    // seasonal subscription or duration in days
                    return new TimeSpanUnits((double)parseTimeSpan(time_span).Days, 0, 0);
            }
        }

    }

    /// <summary>
    /// A period of time expressed in either days, months, or years. Conveys a subscription's duration definition.
    /// Note this reflects the types of subscription durations settable on a subscription on supported app stores.
    /// </summary>
    public class TimeSpanUnits
    {
        /// <summary>
        /// Discrete duration in days, if less than a month, otherwise zero.
        /// </summary>
        public double days;
        /// <summary>
        /// Discrete duration in months, if less than a year, otherwise zero.
        /// </summary>
        public int months;
        /// <summary>
        /// Discrete duration in years, otherwise zero.
        /// </summary>
        public int years;

        /// <summary>
        /// Construct a subscription duration.
        /// </summary>
        /// <param name="d">Discrete duration in days, if less than a month, otherwise zero.</param>
        /// <param name="m">Discrete duration in months, if less than a year, otherwise zero.</param>
        /// <param name="y">Discrete duration in years, otherwise zero.</param>
        public TimeSpanUnits(double d, int m, int y)
        {
            this.days = d;
            this.months = m;
            this.years = y;
        }
    }

    /// <summary>
    /// For representing boolean values which may also be not available.
    /// </summary>
    public enum Result
    {
        /// <summary>
        /// Corresponds to boolean <c> true </c>.
        /// </summary>
        True,
        /// <summary>
        /// Corresponds to boolean <c> false </c>.
        /// </summary>
        False,
        /// <summary>
        /// Corresponds to no value, such as for situations where no result is available.
        /// </summary>
        Unsupported,
    };
}
