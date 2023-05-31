using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppLovinMax;

namespace MixNameSpace
{
    public class MixAppOpenAd
    {
        string adtype = "appopen";
        private string adUnitId = null;
        int retryAttempt;
        private System.Action<string, string> callback = null;
        private bool isInit = false;
        private bool showIfReady = false;
        private bool hasShow = false;
        public void InitializeAppOpenAds(string aid)
        {
            this.adUnitId = aid;
            this.retryAttempt = 0;
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += OnAdClickedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnAdDisplayedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAdDisplayFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAdHiddenEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAdLoadedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAdLoadFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            this.LoadAppOpen();
            this.isInit = true;
        }

        private void LoadAppOpen()
        {
            MaxSdk.LoadAppOpenAd(this.adUnitId);
        }

        private void OnAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            MixMaxManager.LogAdShow(adtype, adUnitId, adInfo);
            if (null != this.callback)
                UnityMainThreadDispatcher.Instance().Enqueue(DelayRunActionNext(this.callback, "show", adUnitId));
        }

        private void OnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) {
            LoadAppOpen();
            if (null != this.callback)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(DelayRunActionNext(this.callback, "failed", adUnitId));
                this.callback = null;
            }
        }

        private void OnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (null != this.callback)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(DelayRunActionNext(this.callback, "close", adUnitId));
                this.callback = null;
            }
            LoadAppOpen();
        }

        static private IEnumerator DelayRunActionNext(System.Action<string, string> action, string message, string adUnitId)
        {
            yield return null;
            action(message, adUnitId);
        } 

        private void OnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            MixMaxManager.LogAdLoad(adtype, adUnitId);
            // Reset retry attempt
            retryAttempt = 0;

            if (this.showIfReady)
            {
                // 立刻播放
                if (!this.hasShow)
                {
                    this.hasShow = true;
                    MaxSdk.ShowAppOpenAd(adUnitId);
                }
            }
        }

        private void OnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            MixMaxManager.LogAdLoadFail(adtype, adUnitId, errorInfo);
            retryAttempt++;
            double retryDelay = System.Math.Pow(2, System.Math.Min(6, retryAttempt));

            MixCommon.DelayRunActionDelay(LoadAppOpen, retryDelay);
        }

        public void ShowAppOpenWithTimeout(float timeout, System.Action<string, string> callback)
        {
            UnityEngine.Debug.LogFormat("MIXSDK[C#]-> [appopen] Show with timeout:{0};", timeout);
             this.callback = callback;
            if (!this.isInit)
            {
                this.showIfReady = true;
                this.hasShow = false;
                UnityEngine.Debug.LogFormat("MIXSDK[C#]-> [appopen] no init and wait");
                UnityMainThreadDispatcher.Instance().Enqueue(DelayRunShow(timeout));
            }
            else
            {
                if (MaxSdk.IsAppOpenAdReady(this.adUnitId))
                {
                    // 如果有，立刻播放
                    this.showIfReady = false;
                    this.hasShow = false;
                    MaxSdk.ShowAppOpenAd(this.adUnitId);
                }
                else
                {
                    // 如果没有
                    this.showIfReady = true;
                    this.hasShow = false;
                    //设置最大超时回调
                    UnityEngine.Debug.LogFormat("MIXSDK[C#]-> [appopen] load and wait");
                    this.LoadAppOpen();
                    UnityMainThreadDispatcher.Instance().Enqueue(DelayRunShow(timeout));
                }
            }
        }
        private IEnumerator DelayRunShow(float timeout)
        {
            yield return new WaitForSeconds(timeout);
            if (this.showIfReady && !this.hasShow) 
            {
                // 超时
                this.showIfReady = false;
                UnityEngine.Debug.LogFormat("MIXSDK[C#]-> [appopen] load and timeout");
                if (null != this.callback)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(DelayRunActionNext(this.callback, "timeout", adUnitId));
                    this.callback = null;
                } 
            }
        }
    }
}
