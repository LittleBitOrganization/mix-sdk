using System;
using MixNameSpace;

namespace LittleBit
{
    public class InitIap : InitCommand<string>
    {
        private readonly MixSDKConfig _mixSDKConfig;
        public override bool IsInit { get; protected set; }

        public InitIap(MixSDKConfig mixSDKConfig)
        {
            _mixSDKConfig = mixSDKConfig;
        }
        public override void Init()
        {
            MixIap.instance.Init(_mixSDKConfig, CompleteInit);
        }

  
    }
}