using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixNameSpace
{
    public class MixFyberManager : MonoBehaviour
    {
        protected static MixFyberManager Singleton = null;
        static public  MixFyberManager instance
        {
            get
            {
                if (null == Singleton)
                {
                    Singleton = GameObject.FindObjectOfType<MixFyberManager>();
                    if (null == Singleton) {
                        GameObject go = new GameObject("MixFyberManager");
                        Singleton = go.AddComponent<MixFyberManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return Singleton;
            }
        }
        void Awake() { if (null == Singleton) Singleton = this; else Destroy(gameObject); }

        // Start is called before the first frame update
        void Start()
        {
            if (isInitSkip) return;
            this.bridgeFyber_setLogCallback();
            if (!string.IsNullOrWhiteSpace(MixFyberManager.initConfig) 
            || !string.IsNullOrWhiteSpace(MixFyberManager.host)) 
            {
                this.bridgeFyber_initWebView();
            } 
            else 
            {
                this.bridgeFyber_init();
            }
        }

        // Update is called once per frame
        void Update() { }
        private System.Action<string, Dictionary<string, string>> logCallback = null;
        private bool isInitFinish = false;
        static private bool isInitSkip = false;
        static private string network = null; // organic
        static private string initConfig = null;
        static private string host = null;
        private System.Action<string> successCallback = null;
        private System.Action<string> failCallback = null;
        private System.Action<string> closeCallback = null;
        private System.Action<bool, string> fullScreenCallback = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        static private readonly string className = "com.fyber.inneractive.sdk.uni.Fyber";
        static private readonly string bridgeClassName = "com.fyber.inneractive.sdk.uni.FyberBridge";
        static private readonly string bridgeInterfaceName = "com.fyber.inneractive.sdk.uni.IUniListenter";
        static private readonly string initInterfaceName = "com.fyber.inneractive.sdk.uni.FyberListener";
        static private readonly string logInterfaceName = "com.fyber.inneractive.sdk.uni.LogCallback";
        static internal void CallStatic(string className, string methodName, params object[] args)
        {
            try {
                Debug.LogFormat("call fyber {0};", methodName);
                using(AndroidJavaClass jc = new AndroidJavaClass(className)) 
                {
                    jc.CallStatic(methodName, args);
                }
            } catch (System.Exception e) {
                Debug.LogException(e);
            }
        }
        static internal T CallStatic<T>(string className, string methodName, params object[] args)
        {
            try {
                Debug.LogFormat("call fyber {0};", methodName);
                using(AndroidJavaClass jc = new AndroidJavaClass(className)) 
                {
                    return jc.CallStatic<T>(methodName, args);
                }
            } catch (System.Exception e) {
                Debug.LogException(e);
                return (T) default;
            }
        }

        static public void InitSkip() { 
            MixFyberManager.isInitSkip = true;
            MixFyberManager.instance.isInitFinish = true;
        }

        private void bridgeFyber_init() 
        {
            Debug.LogFormat("ready to init fyber");
            if (null != network) 
            {
                AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
                string methodName = "init";
                CallStatic(className, methodName, currentActivity, network, new FyberListener());
            } 
            else 
            {
                Debug.LogWarningFormat("can not find network");
            }
        }

        private void bridgeFyber_initWebView() 
        {
            Debug.LogFormat("ready to init fyber with config");
            if (null != network) 
            {
                if (!string.IsNullOrWhiteSpace(MixFyberManager.initConfig) 
                && !string.IsNullOrWhiteSpace(MixFyberManager.host))
                {
                    string methodName = "bridgeInitWebView";
                CallStatic<string>(bridgeClassName, methodName, network, MixFyberManager.initConfig, MixFyberManager.host, new FyberListener());
                }
                else
                {
                    Debug.LogWarningFormat("can not find initConfig or host");
                }
            }
            else 
            {
                Debug.LogWarningFormat("can not find network");
            }
        }

        private void bridgeFyber_dispatchTouchEvent(float posX, float posY) {
            string methodName = "dispatchTouchEvent";
            CallStatic(className, methodName, posX, posY);
        }

        private void bridgeFyber_refresh_web_show_more() 
        {
            string methodName = "refresh_web_show_more";
            CallStatic(className, methodName);
        }

        private void bridgeFyber_refresh_web_show_reward() 
        {
            string methodName = "refresh_from_reward";
            CallStatic(className, methodName);
        }

        private void bridgeFyber_refresh_web_show_inter() 
        {
            string methodName = "refresh_from_inter";
            CallStatic(className, methodName);
        }

        private void bridgeFyber_setLogCallback() 
        {
            string methodName = "setLogCallback";
            CallStatic(className, methodName, new LogCallback());
        }

        private void bridgeFyber_OpenTaskWebView(double amount)
        { 
            string methodName = "bridgeJumpWebView";
            CallStatic<string>(bridgeClassName, methodName, "", 0, amount, new FyberTaskListener());
        }

        private void bridgeFyber_OpenHalfWebView(int posX, int posY, int width, int height)
        {
            string methodName = "bridgeJumpCustomWebView";
            System.Text.StringBuilder json = new System.Text.StringBuilder();
            json.AppendFormat("{{\"url\":\"{0}\",\"posX\":{1},\"posY\":{2},\"width\":{3},\"height\":{4}}}", "http", posX, posY, width, height);
            Debug.LogFormat("open half webview: {0};", json.ToString());
            CallStatic<string>(bridgeClassName, methodName, json.ToString(), new FyberHalfListener());
        }

        private void bridgeFyber_CloseHalfWebView() 
        {
            string methodName = "bridgeCloseCustomWebView";
            CallStatic<string>(bridgeClassName, methodName);
        }

        static public void OpenTaskWebView(double amount, System.Action<string> successCallback, System.Action<string> failCallback) 
        {
            if (null == MixFyberManager.instance || !MixFyberManager.instance.isInitFinish) 
            {
                Debug.LogErrorFormat("Please initialize it first fyber");
                return;
            }
            MixFyberManager.instance.successCallback = successCallback;
            MixFyberManager.instance.failCallback = failCallback;
            MixFyberManager.instance.bridgeFyber_OpenTaskWebView(amount);
        }

        static public void OpenHalfWebView(int posX, int posY, int width, int height, System.Action<string> closeCallback, System.Action<bool, string> fullScreenCallback)
        { 
            if (null == MixFyberManager.instance || !MixFyberManager.instance.isInitFinish) 
            {
                Debug.LogErrorFormat("Please initialize it first fyber");
                return;
            }
            MixFyberManager.instance.closeCallback = closeCallback;
            MixFyberManager.instance.fullScreenCallback = fullScreenCallback;
            MixFyberManager.instance.bridgeFyber_OpenHalfWebView(posX, posY, width, height);
        }

        static public void CloseHalfWebView() { 
            if (null == MixFyberManager.instance || !MixFyberManager.instance.isInitFinish) 
            {
                Debug.LogErrorFormat("Please initialize it first fyber");
                return;
            }
            MixFyberManager.instance.bridgeFyber_CloseHalfWebView();
        }

        static public void refresh_web_show_more()
        {
            if (null == MixFyberManager.instance) 
            {
                Debug.LogErrorFormat("Please initialize it first fyber");
                return;
            }
            MixFyberManager.instance.bridgeFyber_refresh_web_show_more();
        }

        static public void refresh_web_show_reward() 
        {
            if (null == MixFyberManager.instance) 
            {
                Debug.LogErrorFormat("Please initialize it first fyber");
                return;
            }
            MixFyberManager.instance.bridgeFyber_refresh_web_show_reward();
        }
        static public void refresh_web_show_inter() 
        {
            if (null == MixFyberManager.instance) 
            {
                Debug.LogErrorFormat("Please initialize it first fyber");
                return;
            }
            MixFyberManager.instance.bridgeFyber_refresh_web_show_inter();
        }

        static public int[] GetScreenSizeOfReal()
        {
            int screenWidth = CallStatic<int>(bridgeClassName, "bridgeGetScreenWidthOfReal");
            int screenHeight = CallStatic<int>(bridgeClassName, "bridgeGetScreenHeightOfReal");
            if (-1 == screenWidth) screenWidth = Screen.width;
            if (-1 == screenHeight) screenHeight = Screen.height;
            return new int[] {screenWidth, screenHeight}; 
        }

        static public int[] GetAdapterSizeOfReal() {
            int adapterWidth = CallStatic<int>(bridgeClassName, "bridgeGetAdapterWidthOfReal");
            int adapterHeight = CallStatic<int>(bridgeClassName, "bridgeGetAdapterHeightOfReal");
            if (-1 == adapterWidth) adapterWidth = 375 * 2;
            if (-1 == adapterHeight) adapterHeight = 667 * 2;
            return new int[] { adapterWidth, adapterHeight }; 
        }

        static public void SetInitConfig(string json) {
            MixFyberManager.initConfig = json;
        }

        static public void SetNetwork(string network)
        {
            MixFyberManager.network = network;
        }

        static public void SetHost(string host)
        {
            MixFyberManager.host = host;
        }

        // TODO 需要设置打日志回调
        static public void SetLogCallback(System.Action<string, Dictionary<string, string>> logCallback)
        {   
            if (null == MixFyberManager.instance)
            {
                Debug.LogErrorFormat("the fyber instance is null");
                return;
            }
            MixFyberManager.instance.logCallback = logCallback;
        }

        public class FyberTaskListener : AndroidJavaProxy
        {
            public FyberTaskListener() : base(bridgeInterfaceName) { }
            void onPagePreloadFinish() { }
            void onPageStarted(string url) { }
            void onPageFinished(string url) { }
            void onPageShowing(string url) { }
            void onPageClose(string url, bool isSucc) { 
                Debug.LogFormat("Task Listener onPageClose ----- url: {0}; isSucc: {1};", url, isSucc);
                if (isSucc) {
                    if (null != Singleton.successCallback) Singleton.successCallback("success");
                } else {
                    if (null != Singleton.failCallback) Singleton.failCallback("failed");
                }
            }
            void onPageFullScreen(string url) { }
            void onPageExitFullScreen(string url) { }
            void onReceivedError(int code, string errorMsg) { }
            void onReceivedHttpError(int code, string errorMsg) { }
            bool shouldOverrideUrlLoading(string url, string scheme) { return true; }
            bool shouldOverrideKeyEvent(int keyCode) { return false; }
        }

        public class FyberHalfListener : AndroidJavaProxy
        {
            public FyberHalfListener() : base(bridgeInterfaceName) { }
            void onPagePreloadFinish() { }
            void onPageStarted(string url) { }
            void onPageFinished(string url) { }
            void onPageShowing(string url) { }
            void onPageClose(string url, bool isSucc) 
            {
                Debug.LogFormat("Half Listener onPageClose ----- url: {0}; isSucc: {1};", url, isSucc);
                if (null != Singleton.closeCallback) Singleton.closeCallback("success");
            }
            void onPageFullScreen(string url) 
            {
                Debug.LogFormat("Half Listener Enter Full ----- url: {0};", url);
                if (null != Singleton.fullScreenCallback) Singleton.fullScreenCallback(true, "success");
            }
            void onPageExitFullScreen(string url) 
            {
                Debug.LogFormat("Half Listener Exit Full ----- url: {0};", url);
                if (null != Singleton.fullScreenCallback) Singleton.fullScreenCallback(false, "success");
            }
            void onReceivedError(int code, string errorMsg) { }
            void onReceivedHttpError(int code, string errorMsg) { }
            bool shouldOverrideUrlLoading(string url, string scheme) { return true; }
            bool shouldOverrideKeyEvent(int keyCode) { return false; }
        }

        public class FyberListener : AndroidJavaProxy 
        {
            public FyberListener() : base(initInterfaceName) { }
            void initSuccess(string msg)
            {
                Debug.LogFormat("fyber init success: {0};", null == msg ? "" : msg);
                MixFyberManager.instance.isInitFinish = true;
            }
            void initFailed(string errorMsg) {
                Debug.LogWarningFormat("fyber init failed: {0};", null == errorMsg ? "" : errorMsg);
            }
        }
        
        static private object GetStaticVariable(string className, string variableName) 
        {
            try 
            {   
                if (!string.IsNullOrWhiteSpace(className) && !string.IsNullOrWhiteSpace(variableName)) 
                {
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    if (null != assembly) 
                    {
                        System.Type type = assembly.GetType(className);
                        if (null != type) 
                        {
                            System.Reflection.FieldInfo fieldInfo = type.GetField(variableName);
                            if (null != fieldInfo) 
                            {
                                return fieldInfo.GetValue(null);
                            }
                        }
                    }
                }
            } 
            catch(System.Exception e)
            {
                Debug.LogWarningFormat("try get static variable {0}", e.ToString());
            }
            return null;
        }

        static private object InvokeNemberMethod(object instance, string methodName, params object[] args)
        {
            try 
            {
                if (null != instance && !string.IsNullOrWhiteSpace(methodName))
                {
                    System.Type type = instance.GetType();
                    if (null != type)
                    {
                        System.Reflection.MethodInfo methodInfo = type.GetMethod(methodName);
                        if (null != methodInfo) 
                        {
                            return methodInfo.Invoke(instance, args);
                        }
                    }
                }
            }
            catch(System.Exception e)
            {
                Debug.LogWarningFormat("try invoke nember method {0}", e.ToString());
            }
            return null;
        }

        static private object InvokeStaticMethod(string className, string methodName, params object[] args)
        {
            try 
            {
                if (!string.IsNullOrWhiteSpace(className) && !string.IsNullOrWhiteSpace(methodName)) 
                {
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    if (null != assembly)
                    {
                        System.Type type = assembly.GetType(className);
                        if (null != type)
                        {
                            System.Reflection.MethodInfo methodInfo = type.GetMethod(methodName);
                            if (null != methodInfo)
                            {
                                return methodInfo.Invoke(null, args);
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarningFormat("try invoke static method {0}", e.ToString());
            }
            return null;
        }

        public class LogCallback : AndroidJavaProxy 
        {
            public LogCallback() : base(logInterfaceName) { }
            void log(string eventName, AndroidJavaObject map)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    try {
                        Debug.LogFormat("fyber log callback eventName: {0};", eventName);
                        Dictionary<string, string> paramsDict = new Dictionary<string, string>();
                        if (map.Call<bool>("containsKey", "network"))       paramsDict.Add("network",       map.Call<string>("get", "network"));
                        if (map.Call<bool>("containsKey", "name"))          paramsDict.Add("name",          map.Call<string>("get", "name"));
                        if (map.Call<bool>("containsKey", "url"))           paramsDict.Add("url",           map.Call<string>("get", "url"));
                        if (map.Call<bool>("containsKey", "screen_type"))   paramsDict.Add("screen_type",   map.Call<string>("get", "screen_type"));
                        if (map.Call<bool>("containsKey", "seconds"))       paramsDict.Add("seconds",       map.Call<string>("get", "seconds"));
                        if (map.Call<bool>("containsKey", "sdk_version"))   paramsDict.Add("sdk_version",   map.Call<string>("get", "sdk_version"));
                        if (map.Call<bool>("containsKey", "actionid"))      paramsDict.Add("actionid",      map.Call<string>("get", "actionid"));
                        if (map.Call<bool>("containsKey", "actiontype"))    paramsDict.Add("actiontype",    map.Call<string>("get", "actiontype"));
                        if (map.Call<bool>("containsKey", "errorMsg"))      paramsDict.Add("errorMsg",      map.Call<string>("get", "errorMsg"));
                        if (map.Call<bool>("containsKey", "response"))      paramsDict.Add("response",      map.Call<string>("get", "response"));
                        if (map.Call<bool>("containsKey", "config"))        paramsDict.Add("config",        map.Call<string>("get", "config"));
                        if (map.Call<bool>("containsKey", "status"))        paramsDict.Add("status",        map.Call<string>("get", "status"));

                        object inst = null;
                        string logMethodName = "Log";
                        if (null == inst) 
                        {
                            inst = GetStaticVariable("MixNameSpace.MixData", "instance");
                            logMethodName = "Log";
                        }
                        if (null == inst)
                        {
                            inst = GetStaticVariable("DataUploadNS.DataUpload", "instance");
                            logMethodName = "LogWithReturn";
                        }

                        System.Action action = ()=> {
                            Debug.LogFormat("try to initiative upload log");
                            if (null != MixFyberManager.instance && null != MixFyberManager.instance.logCallback)
                            {
                                MixFyberManager.instance.logCallback(eventName, paramsDict);
                            }
                        };

                        if (null != inst) 
                        {
                            try {
                                Debug.LogFormat("try to invoke upload log");
                                object ret = InvokeNemberMethod(inst, logMethodName, eventName, paramsDict);
                                if (null != ret) {
                                    bool b;
                                    if (bool.TryParse(ret.ToString(), out b))
                                    {
                                        if (!b) 
                                        {
                                            action();
                                        }
                                    } 
                                    else 
                                    {
                                        action();
                                    }
                                } else {
                                    action();
                                }
                            } catch (System.Exception ex) {
                                Debug.LogFormat(ex.Message);
                                action();
                            }
                        } 
                        else 
                        {
                            action();
                        }
                    } catch (System.Exception e) {
                        Debug.LogWarningFormat("fyber log callback error: {0};", e.ToString());
                    }
                });
            }
        }
    #else
        private void bridgeFyber_init() { Debug.LogFormat("ready to init fyber - {0}", "editor"); }
        private void bridgeFyber_initWebView() { Debug.LogFormat("ready to init fyber 2 - {0}", "editor"); }
        private void bridgeFyber_dispatchTouchEvent(float posX, float posY) { }
        private void bridgeFyber_refresh_web_show_more() { }
        private void bridgeFyber_refresh_web_show_reward() { }
        private void bridgeFyber_refresh_web_show_inter() { }
        private void bridgeFyber_setLogCallback() { }
        private void bridgeFyber_OpenTaskWebView(double amount) { }
        private void bridgeFyber_OpenHalfWebView(int posX, int posY, int width, int height) { }
        private void bridgeFyber_CloseHalfWebView() { }
        static public int[] GetScreenSizeOfReal() { return new int[] { Screen.width, Screen.height }; }
        static public int[] GetAdapterSizeOfReal() { return new int[] { 375 * 2, 667 * 2 }; }
        static public void SetInitConfig(string json) { }
        static public void SetNetwork(string network) { }
        static public void SetHost(string host) { }
        static public void SetLogCallback(System.Action<string, Dictionary<string, string>> logCallback) { }
        static public void OpenTaskWebView(double amount, System.Action<string> successCallback, System.Action<string> failCallback) { }
        static public void OpenHalfWebView(int posX, int posY, int width, int height, System.Action<string> closeCallback, System.Action<bool, string> fullScreenCallback) { }
        static public void CloseHalfWebView() { }
        static public void refresh_web_show_more() { }
        static public void refresh_web_show_reward() { }
        static public void refresh_web_show_inter() { }
        static public void InitSkip() {  }
    #endif
    }
}