using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixNameSpace
{
    public class MixSDKBridgeFactory
    {
        private MixSDKBridgeFactory() { }
        static public IMixSDKBridge instance = Singleton.bridge;
        static private class Singleton {
#if UNITY_ANDROID
            static public readonly IMixSDKBridge bridge = new MixSDKBridgeAndroid();
#elif UNITY_IOS
            static public readonly IMixSDKBridge bridge = new MixSDKBridgeiOS();
#elif  UNITY_EDITOR
            static public readonly IMixSDKBridge bridge = new MixSDKBridgeEditor();
#else
            static public readonly IMixSDKBridge bridge = new MixSDKBridgeEditor();
#endif
        }
    }
}