using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MixNameSpace
{
    public class MixDataOne
    {
        private string eventName;
        private Dictionary<string, string> paramsMap;
        public MixDataOne(string eventName, Dictionary<string, string> paramsMap)
        {
            this.eventName = eventName;
            this.paramsMap = paramsMap;
        }

        public Dictionary<string, object> getDict()
        {
            Dictionary<string, object> data = MixMain.mixLogInfo.toDict();
            var valueStr = "{}";
            if (paramsMap != null)
            {
                Dictionary<string, string> filterMap = new Dictionary<string, string>();
                foreach (var pair in paramsMap)
                {
                    if (pair.Key != null && pair.Value != null)
                    {
                        filterMap.Add(pair.Key, pair.Value);
                    }
                }
                valueStr = Json.Serialize(filterMap);
            }
            FilterAndAdd(data, "value", valueStr);
            FilterAndAdd(data, "ev", eventName);
            FilterAndAdd(data, "clientTs", MixCommon.NowTimestamp().ToString());
            FilterAndAdd(data, "logIndex", MixData.GetIndex().ToString());
            return data;
        }

        private void FilterAndAdd(Dictionary<string, object> data, string key, string value)
        {
            if (value != null && value.Length > 0)
            {
                data.Add(key, value);
            }
        }




    }
    public class MixData
    {
        static public MixData instance = new MixData();
        static public System.Object lockObj = new System.Object();
        static private int logIndex = 0;
        static private int maxBatchSize = 100;
        private bool debug = false;

        private ConcurrentQueue<MixDataOne> queues = new ConcurrentQueue<MixDataOne>();
        private int maxQueueSize = 1000;
        private float COOL_DOWN = 5;
        private float sendCoolDown = 3;
        private string postHost = "https://bepicdata.log-global.aliyuncs.com";
        private bool sending = false;

        static public int GetIndex()
        {
            lock (lockObj)
            {
                var now = logIndex;
                logIndex++;
                return now;
            }
        }
        public void Init(bool debug)
        {
            this.debug = debug;
        }
        public bool Log(string eventName, Dictionary<string, string> paramsMap)
        {
            if (queues.Count < maxQueueSize)
            {
                Debug.Log("MixData add log to queue " + eventName);
                queues.Enqueue(new MixDataOne(eventName, paramsMap));
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateSend()
        {
            if (sendCoolDown < 0)
            {
                //recheck analytics
                if (CanSend())
                {
                    SendOneBatch();
                }
                sendCoolDown = COOL_DOWN;
            }
            else
            {
                sendCoolDown -= Time.deltaTime;
            }
        }

        private bool CanSend()
        {
            return queues.Count > 0;
        }

        private void SendOneBatch()
        {
            var oneBatch = new List<MixDataOne>();
            while (queues.Count > 0 && oneBatch.Count < maxBatchSize)
            {
                MixDataOne item;
                var has = queues.TryDequeue(out item);
                if (has)
                {
                    oneBatch.Add(item);
                }
            }
            List<Dictionary<string, object>> logs = new List<Dictionary<string, object>>();
            foreach (var one in oneBatch)
            {
                logs.Add(one.getDict());
            }
            Dictionary<string, object> json = new Dictionary<string, object>();
            json.Add("__logs__", logs);
            string bodyStr = Json.Serialize(json);
            sending = true;
            SendDataWithRetry(bodyStr, 0, 3);
        }

        void SendDataWithRetry(string bodyStr,int delay, int retryTime)
        {
            var run = SendData(bodyStr, delay, (s) =>
            {
                Debug.Log("MixData send success " + bodyStr);
                sending = false;
            },
            (f) =>
            {
                if (retryTime == 0)
                {
                    Debug.Log("MixData send fail and all retry");
                    sending = false;
                }
                else
                {
                    Debug.Log("MixData send fail and retry");
                    SendDataWithRetry(bodyStr, 3, retryTime - 1);
                }
            });
            UnityMainThreadDispatcher.Instance().Enqueue(run);
            
        }

        private IEnumerator SendData(string bodyStr, int retryDelay, Action<string> success, Action<string> fail)
        {
            yield return new WaitForSeconds((float)retryDelay);
            byte[] body = Encoding.UTF8.GetBytes(bodyStr);

            //send to aliyun
            var headers = new Dictionary<string, string>();
            headers.Add("x-log-apiversion", "0.6.0");
            headers.Add("x-log-bodyrawsize", bodyStr.Length.ToString());


            UnityWebRequest unityWeb = new UnityWebRequest(postHost+ "/logstores/sdklog/track", "POST");
            unityWeb.uploadHandler = new UploadHandlerRaw(body);
            unityWeb.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            if (headers != null)
            {
                foreach (var entry in headers)
                {
                    unityWeb.SetRequestHeader(entry.Key, entry.Value);
                }
            }
            unityWeb.downloadHandler = new DownloadHandlerBuffer();
            unityWeb.timeout = 5;

            yield return unityWeb.SendWebRequest();
            if (unityWeb.isHttpError || unityWeb.isNetworkError)
            {
                fail(unityWeb.responseCode+" "+unityWeb.error);
            }
            else
            {
                string result = unityWeb.downloadHandler.text;
                success(result);
            }
        }
    }
}
