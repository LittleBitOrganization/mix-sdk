using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixNameSpace
{
    public class MixMain : MonoBehaviour
    {
        static public MixMain instance;
        static public MixLogInfo mixLogInfo;
        static public MixSDKConfig mixSDKConfig;
        private Action<string> initCallback;
        static public string SDK_VERSION = "mix-newpay-v38";
        public Action<MixAdjustInfo> adjustInfoCallback;
        public event Action OnMaxInit;

        private void Awake()
        {
            if (instance == null)
            {
                Debug.LogError("MixMain instance == null");
            }
            else
            {
                Debug.LogError($"MixMain instance: {instance.GetHashCode()}" );

                DontDestroyOnLoad(gameObject);
                instance = this;
                
                Debug.LogError($"MixMain instance after init: {instance.GetHashCode()}" );
            }
        }

        public void Init(MixSDKConfig config, Action<string> initCallback)
        {
            string channelName = MixSDKBridgeFactory.instance.GetWalleChannelName();
            if (!string.IsNullOrWhiteSpace(channelName)) config.debug = true;

            this.initCallback = initCallback;
            mixSDKConfig = config;
            PlatformInfo pinfo = mixSDKConfig.GetPlatformInfo();
            MixSDKBridgeFactory.instance.OnInitBridge();
            MixAdjustManager.instance.start(this.gameObject.name, pinfo.adjustAppkey, config.debug, NextMax);
        }

        public void NextMax(int status)
        {
            mixLogInfo = new MixLogInfo(mixSDKConfig.mixInput.appName);
            MixData.instance.Init(mixSDKConfig.debug);
            //start max
            PlatformInfo pinfo = mixSDKConfig.GetPlatformInfo();
            MixMaxManager.instance.Init(pinfo.maxKey, pinfo.rid, pinfo.iid, pinfo.bid, pinfo.aid, mixSDKConfig.pos, mixSDKConfig.debug, NexIap);
        }
        public void NexIap(string s)
        {
            OnMaxInit?.Invoke();

            MixIap.instance.Init(mixSDKConfig, SDKInitFinish);
        }

        public void IapInit(Action onComplete)
        {
            MixIap.instance.Init(mixSDKConfig, (s)=>
            {
                SDKInitFinish(s);
                onComplete?.Invoke();
            });
        }

        
        public void SDKInitFinish(string ispFinishResult)
        {
            Debug.Log("Mix SDKInitFinish and game_start and restore the adjust");
            MixAdjustManager.instance.ReadOldInfo();
            PlatformInfo pinfo = mixSDKConfig.GetPlatformInfo();
            MixThirdUpload.instance.Init(pinfo);
            MixData.instance.Log("game_start", new Dictionary<string, string>()
            {
                {"version", SDK_VERSION},
                {"debug", mixSDKConfig.debug.ToString()},
                {"secretKey", MixSDKConfig.secretKey}
            });
            // heartbeat上报
            MixSDKHeartbeat.instance.StartToSend();
            this.initCallback("ok");
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            MixData.instance.UpdateSend();
        }

        public void AdjustInfoFinish()
        {
            if (null != MixAdjustManager.instance.info) {
                Debug.LogWarningFormat("the adjust info is not null");
                MixFyberManager.SetNetwork(MixAdjustManager.instance.info.network);
                MixFyberManager.SetLogCallback((eventName, dict) => {
                    string network = dict["network"];
                    string name = dict["name"];
                    string url = dict["url"];
                    string screen_type = dict["screen_type"];
                    Debug.LogFormat("h5 webview eventName:{0}; network:{1}; name:{2}; url:{3}; screen_type:{4};", eventName, network, name, url, screen_type);
                    MixData.instance.Log(eventName, dict);
                });
            } 
            else
            {
                Debug.LogWarningFormat("the adjust info is null, can not init h5 webview");
            }
            if(mixLogInfo != null)
            {
                mixLogInfo.attrid = MixAdjustManager.instance.info.adid;
            }
            if (adjustInfoCallback != null)
            {
                adjustInfoCallback(MixAdjustManager.instance.info);
            }
            //upload attr_on
        }
    }
}