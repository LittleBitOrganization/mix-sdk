using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace MixNameSpace
{
    public class MixThirdShare : MonoBehaviour
    {
        protected static MixThirdShare Singleton = null;
        static public  MixThirdShare instance
        {
            get
            {
                if (null == Singleton)
                {
                    Singleton = GameObject.FindObjectOfType<MixThirdShare>();
                    if (null == Singleton) {
                        GameObject go = new GameObject("MixShare");
                        Singleton = go.AddComponent<MixThirdShare>();
                        DontDestroyOnLoad(go);
                    }
                }
                return Singleton;
            }
        }
        void Awake() { if (null == Singleton) Singleton = this; else Destroy(gameObject); }

        private MixThirdHelper helper = null;
        private Action<string> shareFacebookAsyncSuccessAction;
        private Action<int, string> shareFacebookAsyncFailAction;
        private volatile bool isInited = false;
        internal bool OnInit(MixThirdHelper helper)
        {
            this.isInited = true;
            this.helper = helper;
            return true;
        }

        public void ShareFacebook(string url, byte[] image, Action<string> success, Action<int, string> fail)
        {
            if (!this.isInited) return;
            this.shareFacebookAsyncSuccessAction = success;
            this.shareFacebookAsyncFailAction = fail;
            if (null != url) {
                helper.ShareLinkByFacebook(url);
            } else if (null != image) {
                helper.SharePhotoByFacebook(image);
            }
        }
        [MixDoNotRename]
        public void shareFacebookAsyncSuccess(string s)
        {
            if (null != this.shareFacebookAsyncSuccessAction) this.shareFacebookAsyncSuccessAction(s);
        }
        [MixDoNotRename]
        public void shareFacebookAsyncFail(string s)
        {
            Dictionary<string, object> dict = Json.Deserialize(s) as Dictionary<string, object>;
            int code = Convert.ToInt32((long) dict["code"]);
            string errorMsg = dict["msg"] as string;
            if (null != this.shareFacebookAsyncFailAction) this.shareFacebookAsyncFailAction(code, errorMsg);
        }
    }
}