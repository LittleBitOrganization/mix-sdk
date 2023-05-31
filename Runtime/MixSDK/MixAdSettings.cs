using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MixNameSpace
{
    public static class MixAdSettings
    {

        public static void SetDataProcessingOptions()
        {
#if UNITY_EDITOR

#elif UNITY_ANDROID
            //TODO
#elif UNITY_IOS
            MixFBAdSettingsBridgeSetDataProcessingOptions();
#endif
        }

        public static void SetAdvertiserTrackingEnabled()
        {

#if UNITY_EDITOR

#elif UNITY_IOS
            MixSetAdvertiserTrackingEnabled();
#endif
        }


#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void MixFBAdSettingsBridgeSetDataProcessingOptions();

        [DllImport("__Internal")]
        private static extern void MixSetAdvertiserTrackingEnabled();
#endif
    }
}
