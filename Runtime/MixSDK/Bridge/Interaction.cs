using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MixNameSpace
{
    public class Interaction
    {
        private static Dictionary<string, System.Action<Dictionary<string, object>>> callbackDict = new Dictionary<string, System.Action<Dictionary<string, object>>>();
        public static string Api(string sdkType, string methodName, System.Action<Dictionary<string, object>> callback, params object[] args) {
            string ret = null;
            string symbol = System.Guid.NewGuid().ToString("N");
            if (null != callback) callbackDict.Add(symbol, callback);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add(MixSDKProto.Key.Symbol, symbol);
            for (int i = 0, len = args.Length; i < len; i++) {
                object o = args[i];
                dict.Add(MixSDKProto.Key.Arg + i, o);
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            ret = JavaApi(sdkType, methodName, Json.Serialize(dict), new BaseInterface());
#elif UNITY_IOS && !UNITY_EDITOR
            ret = OCApi(sdkType, methodName, Json.Serialize(dict), OCCallBackFunc);
#elif UNITY_WEBGL && !UNITY_EDITOR
#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
#elif UNITY_EDITOR
            ret = Json.Serialize(new Dictionary<string, object>() {
                { MixSDKProto.Key.Code, MixSDKProto.Code.Error_NotSupport },
                { MixSDKProto.Key.Msg, "Can not Call By Editor -----" },
            });
#endif
            return (null == ret) ? MixSDKProto.Key.Failed : ret;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static readonly string PackagePath = "com.mix.interaction";
        public class BaseInterface : AndroidJavaProxy {
            public BaseInterface() : base(PackagePath + ".IBaseInterface"){}
            void JavaCallBackFunc(AndroidJavaObject obj)
            {
                string symbol = obj.Get<string>(MixSDKProto.Key.Symbol);
                if (callbackDict.ContainsKey(symbol))
                {
                    int code = obj.Get<int>(MixSDKProto.Key.Code);
                    string msg = obj.Get<string>(MixSDKProto.Key.Msg);
                    string data = obj.Get<string>(MixSDKProto.Key.Data);
                    // Debug.LogFormat("[Java --Callback--> C#] symbol:{0}; code:{1}; msg:{2}; data:{3};", symbol, code, msg, data);
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict.Add(MixSDKProto.Key.Code, code);
                    dict.Add(MixSDKProto.Key.Msg, msg);
                    dict.Add(MixSDKProto.Key.Data, data);
                    callbackDict[symbol](dict);
                    callbackDict.Remove(symbol);
                }
            }
        }
        /// <summary>
        /// C# 交互 Android(Java) 统一接口(提供 Lua)
        /// </summary>
        /// <param name="sdkType">sdk 类型</param>
        /// <param name="methodName">接口名称</param>
        /// <param name="jsonParams">Json字符串参数</param>
        /// <param name="inter">回调接口</param>
        /// <returns>同步返回字符串</returns>
        private static string JavaApi(string sdkType, string methodName, string jsonParams, BaseInterface inter) {
            // Debug.LogFormat("[C# --Call--> Java] jsonParams:{0};", jsonParams);
            string ret = null;
            using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(PackagePath + ".Interaction")) {
                ret = androidJavaClass.CallStatic<string>("Api", sdkType, methodName, jsonParams, inter);
            }
            return ret;
        }

#elif UNITY_IOS && !UNITY_EDITOR
        // 定义一个OC的结构体类型(注意:结构体属性的排序需要和OC对应(会影响回调))
        [StructLayout(LayoutKind.Sequential)]
        struct Parameter {
            public string symbol;
            public int code;
            public string msg;
            public string data;
        }
        delegate void CSFunction(System.IntPtr L);
        [AOT.MonoPInvokeCallback(typeof(CSFunction))]
        static void OCCallBackFunc(System.IntPtr L) {
            Parameter p = (Parameter) Marshal.PtrToStructure(L, typeof(Parameter));
            if (callbackDict.ContainsKey(p.symbol))
            {
                Debug.LogFormat("[OC --Callback--> C#] symbol:{0}; code:{1}; msg:{2}; data:{3};", p.symbol, p.code, p.msg, p.data);
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add(MixSDKProto.Key.Code, p.code);
                dict.Add(MixSDKProto.Key.Msg, p.msg);
                dict.Add(MixSDKProto.Key.Data, p.data);
                callbackDict[p.symbol](dict);
                callbackDict.Remove(p.symbol);
            }
            else
            {
                Debug.LogErrorFormat("[OC --Callback--> Unknown]");
            }
        }
        /// <summary>
        /// C# 交互 iOS(OC) 统一接口
        /// </summary>
        /// <param name="sdkType">sdk 类型</param>
        /// <param name="methodName">接口名称</param>
        /// <param name="jsonParams">Json字符串参数</param>
        /// <param name="csFun">C#映射到OC的回调函数</param>
        /// <returns>同步返回字符串</returns>
        [DllImport("__Internal")]
        private static extern string OCApi(string sdkType, string methodName, string jsonParams, CSFunction csFun);
#endif
    }
}