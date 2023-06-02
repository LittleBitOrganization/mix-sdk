using System;
using System.Collections.Generic;
using System.Linq;
using MixNameSpace;
using UnityEngine;

namespace LittleBit
{
    public class MixInstaller
    {
        public MixSDKConfig mixSDKConfig { get; private set; }
        private Action<string> _initCallback;
        public event Action<MixAdjustInfo> AdjustInfoCallback;
        public string SDK_VERSION => MixMain.SDK_VERSION;

        private List<IInitCommand> _initCommands;

        public T GetInitCommand<T>() where T : IInitCommand =>
            (T)_initCommands.FirstOrDefault(v => v.GetType() == typeof(T));
    

        public MixInstaller()
        {
       
        }

        public void Init(MixSDKConfig config)
        {
            _initCommands = new List<IInitCommand>();
        
            _initCommands.Add(new InitAdjust(config));
            _initCommands.Add(new InitMax(config));

            var initIap = new InitIap(config);
            initIap.OnInit += SDKInitFinish;
            _initCommands.Add(initIap);
        
            MixMain.instance.adjustInfoCallback = info => AdjustInfoCallback?.Invoke(info); 
            _initCallback = s => Debug.Log("finish init " + s);

            MixMain.mixSDKConfig = config;
            mixSDKConfig = config;
            MixSDKBridgeFactory.instance.OnInitBridge();
        }

        private void SDKInitFinish(string finishResult)
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
            MixSDKHeartbeat.instance.StartToSend();
            _initCallback("ok");
        }
    }
}