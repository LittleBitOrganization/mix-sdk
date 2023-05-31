using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MixNameSpace
{
    public class MixServer : MonoBehaviour
    {
        static string GAME_HOST = "https://d36o8p6n45qxca.cloudfront.net";
        static public IEnumerator SendPost(string path, Dictionary<string, object> read, Action<Dictionary<string, object>> success, Action<string> fail)
        {
            string bodyStr = Json.Serialize(read);

            byte[] body = Encoding.UTF8.GetBytes(bodyStr);

            UnityWebRequest unityWeb = new UnityWebRequest(GAME_HOST + path, "POST");
            unityWeb.uploadHandler = new UploadHandlerRaw(body);
            unityWeb.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            unityWeb.downloadHandler = new DownloadHandlerBuffer();
            unityWeb.timeout = 15;
            MixCommon.DebugLog("MixServer ready to send " + path + " " + bodyStr);
            yield return unityWeb.SendWebRequest();
            if (unityWeb.isHttpError || unityWeb.isNetworkError)
            {
                var m = unityWeb.responseCode + " " + unityWeb.error;
                MixCommon.DebugLog("MixServer send fail " + path + " " + bodyStr + " " + m);
                fail(m);
            }
            else
            {
                string result = unityWeb.downloadHandler.text;
                MixCommon.DebugLog("MixServer get response from " + path + " " + bodyStr + " " + result);
                var r = (Dictionary<string, object>)Json.Deserialize(result);
                var state = (Dictionary<string, object>)r["state"];
                var code = (long)state["code"];
                if (code == 0)
                {
                    var data = (Dictionary<string, object>)r["data"];
                    success(data);
                }
                else
                {
                    var msg = (string)state["msg"];
                    fail(code + " " + msg);
                }

            }
        }
    }
}