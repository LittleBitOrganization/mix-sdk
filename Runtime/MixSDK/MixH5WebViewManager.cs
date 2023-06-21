using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixNameSpace
{
    public class MixH5WebViewManager : MonoBehaviour
    {
        protected static MixH5WebViewManager Singleton = null;
        static public  MixH5WebViewManager instance
        {
            get
            {
                if (null == Singleton)
                {
                    Singleton = GameObject.FindObjectOfType<MixH5WebViewManager>();
                    if (null == Singleton) {
                        GameObject go = new GameObject("MixH5WebViewManager");
                        Singleton = go.AddComponent<MixH5WebViewManager>();
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
            this.bridgeH5WebView_setLogCallback();
            if (null == MixH5WebViewManager.initConfig) {
                this.bridgeH5WebView_init();
            } else {
                this.bridgeH5WebView_initWebView();
            }
        }

        // Update is called once per frame
        void Update() { }
        private System.Action<string, Dictionary<string, string>> logCallback = null;
        private bool isInitFinish = false;
        static private bool isInitSkip = false;
        static private string network = null; // organic
        static private string initConfig = null;
        private System.Action<string> successCallback = null;
        private System.Action<string> failCallback = null;
        private System.Action<string> closeCallback = null;
        private System.Action<bool, string> fullScreenCallback = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        static private readonly string className = "com.mix.h5.webview.MixH5WebView";
        static private readonly string bridgeClassName = "com.mix.h5.webview.MixWebViewBridge";
        static internal void CallStatic(string className, string methodName, params object[] args)
        {
            try {
                Debug.LogFormat("call h5 webview {0};", methodName);
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
                Debug.LogFormat("call h5 webview {0};", methodName);
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
            MixH5WebViewManager.isInitSkip = true;
            MixH5WebViewManager.instance.isInitFinish = true;
        }

        private void bridgeH5WebView_init() 
        {
            Debug.LogFormat("ready to init h5 webview");
            if (null != network) 
            {
                AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
                string methodName = "init";
                CallStatic(className, methodName, currentActivity, network, new MixH5WebViewListener());
            } 
            else 
            {
                Debug.LogErrorFormat("can not find network");
            }
        }

        private void bridgeH5WebView_initWebView() 
        {
            Debug.LogFormat("ready to init h5 webview with config");
            if (null != network) 
            {
                string methodName = "bridgeInitWebView";
                CallStatic<string>(bridgeClassName, methodName, network, MixH5WebViewManager.initConfig, new MixH5WebViewListener());
            }
            else 
            {
                Debug.LogErrorFormat("can not find network");
            }
        }

        private void bridgeH5WebView_dispatchTouchEvent(float posX, float posY) {
            string methodName = "dispatchTouchEvent";
            CallStatic(className, methodName, posX, posY);
        }

        private void bridgeH5WebView_refresh_web_show_more() 
        {
            string methodName = "refresh_web_show_more";
            CallStatic(className, methodName);
        }

        private void bridgeH5WebView_refresh_web_show_reward() 
        {
            string methodName = "refresh_from_reward";
            CallStatic(className, methodName);
        }

        private void bridgeH5WebView_refresh_web_show_inter() 
        {
            string methodName = "refresh_from_inter";
            CallStatic(className, methodName);
        }

        private void bridgeH5WebView_setLogCallback() 
        {
            string methodName = "setLogCallback";
            CallStatic(className, methodName, new LogCallback());
        }

        private void bridgeH5WebView_OpenTaskWebView(double amount)
        { 
            string methodName = "bridgeJumpWebView";
            CallStatic<string>(bridgeClassName, methodName, "", 0, amount, new MixTaskWebViewListener());
        }

        private void bridgeH5WebView_OpenHalfWebView(int posX, int posY, int width, int height)
        {
            string methodName = "bridgeJumpCustomWebView";
            System.Text.StringBuilder json = new System.Text.StringBuilder();
            json.AppendFormat("{{\"url\":\"{0}\",\"posX\":{1},\"posY\":{2},\"width\":{3},\"height\":{4}}}", "http", posX, posY, width, height);
            Debug.LogFormat("open half webview: {0};", json.ToString());
            CallStatic<string>(bridgeClassName, methodName, json.ToString(), new MixHalfWebViewListener());
        }

        private void bridgeH5WebView_CloseHalfWebView() 
        {
            string methodName = "bridgeCloseCustomWebView";
            CallStatic<string>(bridgeClassName, methodName);
        }

        static public void OpenTaskWebView(double amount, System.Action<string> successCallback, System.Action<string> failCallback) 
        {
            if (null == MixH5WebViewManager.instance || !MixH5WebViewManager.instance.isInitFinish) 
            {
                Debug.LogErrorFormat("Please initialize it first h5 webview");
                return;
            }
            MixH5WebViewManager.instance.successCallback = successCallback;
            MixH5WebViewManager.instance.failCallback = failCallback;
            MixH5WebViewManager.instance.bridgeH5WebView_OpenTaskWebView(amount);
        }

        static public void OpenHalfWebView(int posX, int posY, int width, int height, System.Action<string> closeCallback, System.Action<bool, string> fullScreenCallback)
        { 
            if (null == MixH5WebViewManager.instance || !MixH5WebViewManager.instance.isInitFinish) 
            {
                Debug.LogErrorFormat("Please initialize it first h5 webview");
                return;
            }
            MixH5WebViewManager.instance.closeCallback = closeCallback;
            MixH5WebViewManager.instance.fullScreenCallback = fullScreenCallback;
            MixH5WebViewManager.instance.bridgeH5WebView_OpenHalfWebView(posX, posY, width, height);
        }

        static public void CloseHalfWebView() { 
            if (null == MixH5WebViewManager.instance || !MixH5WebViewManager.instance.isInitFinish) 
            {
                Debug.LogErrorFormat("Please initialize it first h5 webview");
                return;
            }
            MixH5WebViewManager.instance.bridgeH5WebView_CloseHalfWebView();
        }

        static public void refresh_web_show_more()
        {
            if (null == MixH5WebViewManager.instance) 
            {
                Debug.LogErrorFormat("Please initialize it first h5 webview");
                return;
            }
            MixH5WebViewManager.instance.bridgeH5WebView_refresh_web_show_more();
        }

        static public void refresh_web_show_reward() 
        {
            if (null == MixH5WebViewManager.instance) 
            {
                Debug.LogErrorFormat("Please initialize it first h5 webview");
                return;
            }
            MixH5WebViewManager.instance.bridgeH5WebView_refresh_web_show_reward();
        }
        static public void refresh_web_show_inter() 
        {
            if (null == MixH5WebViewManager.instance) 
            {
                Debug.LogErrorFormat("Please initialize it first h5 webview");
                return;
            }
            MixH5WebViewManager.instance.bridgeH5WebView_refresh_web_show_inter();
        }

        static public int[] GetScreenSizeOfReal()
        {
            int screenWidth = CallStatic<int>(bridgeClassName, "bridgeGetScreenWidthOfReal");
            int screenHeight = CallStatic<int>(bridgeClassName, "bridgeGetScreenHeightOfReal");
            if (-1 == screenWidth) screenWidth = Screen.width;
            if (-1 == screenHeight) screenHeight = Screen.height;
            return new int[] {screenWidth, screenHeight}; 
        }

        static public void SetInitConfig(string json) {
            MixH5WebViewManager.initConfig = json;
        }

        static public void SetNetwork(string network)
        {
            MixH5WebViewManager.network = network;
        }

        // TODO 需要设置打日志回调
        static public void SetLogCallback(System.Action<string, Dictionary<string, string>> logCallback)
        {   
            if (null == MixH5WebViewManager.instance)
            {
                Debug.LogErrorFormat("The h5 webview instance is null");
                return;
            }
            MixH5WebViewManager.instance.logCallback = logCallback;
        }

        public class MixTaskWebViewListener : AndroidJavaProxy
        {
            public MixTaskWebViewListener() : base("com.mix.h5.webview.IWebViewListenter") { }
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

        public class MixHalfWebViewListener : AndroidJavaProxy
        {
            public MixHalfWebViewListener() : base("com.mix.h5.webview.IWebViewListenter") { }
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

        public class MixH5WebViewListener : AndroidJavaProxy 
        {
            public MixH5WebViewListener() : base("com.mix.h5.webview.MixH5WebViewListener") { }
            void initSuccess(string msg)
            {
                Debug.LogFormat("h5 init success: {0};", null == msg ? "" : msg);
                MixH5WebViewManager.instance.isInitFinish = true;
            }
            void initFailed(string errorMsg) {
                Debug.LogWarningFormat("h5 init failed: {0};", null == errorMsg ? "" : errorMsg);
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
            public LogCallback() : base("com.mix.h5.webview.LogCallback") { }
            void log(string eventName, AndroidJavaObject map)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    try {
                        string network = map.Call<string>("get", "network");
                        string name = map.Call<string>("get", "name");
                        string url = map.Call<string>("get", "url");
                        string screen_type = map.Call<string>("get", "screen_type");
                        string seconds = map.Call<bool>("containsKey", "seconds") ? map.Call<string>("get", "seconds") : "-1";
                        Debug.LogFormat("h5 log callback eventName: {0}; network {1}; name {2}; url {3}; screen_type {4}; seconds: {5};", eventName, network, name, url, screen_type, seconds);
                        Dictionary<string, string> paramsDict = new Dictionary<string, string>();
                        paramsDict.Add("network", network);
                        paramsDict.Add("name", name);
                        paramsDict.Add("url", url);
                        paramsDict.Add("screen_type", screen_type);
                        if (!"-1".Equals(seconds)) {
                            paramsDict.Add("seconds", seconds.ToString());
                        }
                        object inst = null;
                        if (null == inst) 
                        {
                            inst = GetStaticVariable("MixNameSpace.MixData", "instance");
                        }
                        if (null == inst) 
                        {
                            inst = GetStaticVariable("DataUploadNS.DataUpload", "instance");
                        }

                        System.Action action = ()=> {
                            Debug.LogFormat("try to initiative upload log");
                            if (null != MixH5WebViewManager.instance && null != MixH5WebViewManager.instance.logCallback)
                            {
                                MixH5WebViewManager.instance.logCallback(eventName, paramsDict);
                            }
                        };

                        if (null != inst) 
                        {
                            try {
                                Debug.LogFormat("try to invoke upload log");
                                object ret = InvokeNemberMethod(inst, "Log", eventName, paramsDict);
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
                                action();
                            }
                        } 
                        else 
                        {
                            action();
                        }
                    } catch (System.Exception e) {
                        Debug.LogWarningFormat("h5 log callback error: {0};", e.ToString());
                    }
                });
            }
        }
    #else
        private void bridgeH5WebView_init() { Debug.LogFormat("ready to init h5 webview - {0}", "editor"); }
        private void bridgeH5WebView_initWebView() { Debug.LogFormat("ready to init h5 webview2 - {0}", "editor"); }
        private void bridgeH5WebView_dispatchTouchEvent(float posX, float posY) { }
        private void bridgeH5WebView_refresh_web_show_more() { }
        private void bridgeH5WebView_refresh_web_show_reward() { }
        private void bridgeH5WebView_refresh_web_show_inter() { }
        private void bridgeH5WebView_setLogCallback() { }
        private void bridgeH5WebView_OpenTaskWebView(double amount) { }
        private void bridgeH5WebView_OpenHalfWebView(int posX, int posY, int width, int height) { }
        private void bridgeH5WebView_CloseHalfWebView() { }
        static public int[] GetScreenSizeOfReal() { return new int[] {Screen.width, Screen.height}; }
        static public void SetInitConfig(string json) { }
        static public void SetNetwork(string network) { }
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