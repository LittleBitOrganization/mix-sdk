using System;
using MixNameSpace;

namespace LittleBit
{
    public class InitMax : InitCommand<string>
    {
        private readonly MixSDKConfig _mixSDKConfig;
        public override bool IsInit { get; protected set; } = false;

        public InitMax(MixSDKConfig mixSDKConfig)
        {
            _mixSDKConfig = mixSDKConfig;
        }
    
        public override void Init()
        {
            MixMain.mixLogInfo = new MixLogInfo(_mixSDKConfig.mixInput.appName);
            MixData.instance.Init(_mixSDKConfig.debug);
            PlatformInfo platformInfo = _mixSDKConfig.GetPlatformInfo();
            MixMaxManager.instance.Init(
                platformInfo.maxKey, 
                platformInfo.rid, 
                platformInfo.iid, 
                platformInfo.bid, 
                platformInfo.aid, 
                _mixSDKConfig.pos, 
                _mixSDKConfig.debug, CompleteInit); 
        }
    }
}