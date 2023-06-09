using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixNameSpace
{
    [Serializable]
    public class MixAdjustInfo
    {
        public string adid;
        public string network;
        public string campaign;
        public string adgroup;
        public string createive;

        public MixAdjustInfo(string adid, string network, string campaign, string adgroup, string createive)
        {
            this.adid = adid;
            this.network = network;
            this.campaign = campaign;
            this.adgroup = adgroup;
            this.createive = createive;
        }
    }
}