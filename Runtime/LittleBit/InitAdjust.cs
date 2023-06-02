using System;
using MixNameSpace;

namespace LittleBit
{
    public class InitAdjust : InitCommand<int>
    {
        private readonly MixSDKConfig _mixSDKConfig;

        public override bool IsInit { get; protected set; } = false;

        public InitAdjust(MixSDKConfig mixSDKConfig)
        {
            _mixSDKConfig = mixSDKConfig;
        }
        public override void Init()
        {
            MixAdjustManager.instance.start("Adjust", _mixSDKConfig.GetPlatformInfo().adjustAppkey, _mixSDKConfig.debug, CompleteInit);
        }


    }
}