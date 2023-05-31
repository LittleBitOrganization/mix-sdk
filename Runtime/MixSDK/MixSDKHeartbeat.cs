using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixNameSpace
{
    public class MixSDKHeartbeat 
    {
        static public MixSDKHeartbeat instance = new MixSDKHeartbeat();
        private int nextInterval = 10;
        private int interval = 10;
        private int maxInterval = 300;
        static public readonly string HEARTBEAT = "heartbeat";

        public void StartToSend()
        {
            nextInterval = interval / 2;
            SendHeartbeat();
        }

        void SendHeartbeat()
        {
            Dictionary<string, string> param = new Dictionary<string, string>()
            {
                {"passTime", ((int)Time.time).ToString() }
            };
            MixData.instance.Log(HEARTBEAT, param);
            if (nextInterval >= maxInterval)
            {
                nextInterval = maxInterval;
            }
            else
            {
                nextInterval *= 2;
            }
            MixCommon.DelayRunActionDelay(()=> { this.SendHeartbeat();}, nextInterval);
        }
    }
}