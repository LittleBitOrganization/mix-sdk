using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MixNameSpace
{
    [AttributeUsage(AttributeTargets.All,AllowMultiple=true,Inherited=false)]
    public class MixDoNotRename : Attribute
    {
        
    }
}