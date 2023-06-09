using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace MixNameSpace
{
    public class MixCommon
    {
        static public void DebugLog(string log)
        {
            if (MixMain.mixSDKConfig.debug)
            {
                Debug.Log(log);
            }
        }
        static public int ToTimestamp(DateTime value)
        {
            //TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            //return (int)span.TotalSeconds;
            DateTime zeroDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            double x = value.ToUniversalTime().Subtract(zeroDateTime).TotalMilliseconds;
            return (int)x;
        }

        static public int NowTimestamp()
        {
            return (int)DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        static public DateTime FromTimestamp(int timestamp)
        {
            //create a new DateTime value based on the Unix Epoch
            DateTime converted = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            //add the timestamp to the value
            DateTime newDateTime = converted.AddSeconds(timestamp);

            //return the value in string format
            return newDateTime;
        }

        static public void DelayRunActionNext(Action action)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(DelayRun(action));
        }
        static private IEnumerator DelayRun(Action action)
        {
            yield return null;
            action();
        }
        static public void DelayRunActionDelay(Action action, double retryDelay)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(DelayRun(action, retryDelay));
        }
        static private IEnumerator DelayRun(Action action, double retryDelay)
        {
            yield return new WaitForSeconds((float)retryDelay);
            action();
        }
        private static RijndaelManaged GetRijndaelManaged(string key)
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            return rDel;
        }
        public static string AesEncryptorBase64(string EncryptStr, string Key)
        {
            try
            {
                string key16 = Key;
                while (key16.Length < 16)
                {
                    key16 += '0';
                }
                byte[] toEncryptArray = Encoding.UTF8.GetBytes(EncryptStr);

                ICryptoTransform cTransform = GetRijndaelManaged(key16).CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception ex)
            {
                Debug.Log("aes encryptor error");
                Debug.LogException(ex);
                return null;
            }
        }
        public static string AesDecryptorBase64(string DecryptStr, string Key)
        {
            try
            {
                string key16 = Key;
                while(key16.Length < 16)
                {
                    key16 += '0';
                }
                byte[] toEncryptArray = Convert.FromBase64String(DecryptStr);
                ICryptoTransform cTransform = GetRijndaelManaged(key16).CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Encoding.UTF8.GetString(resultArray);//  UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                Debug.Log("aes decryptor error");
                Debug.LogException(ex);
                return null;
            }
        }

        public static string FromProductType(ProductType productType)
        {
            if (productType == ProductType.Consumable)
            {
                return "consume";
            }
            else if (productType == ProductType.NonConsumable)
            {
                return "unconsume";
            }
            else if (productType == ProductType.Subscription)
            {
                return "subscription";
            }
            else return "";
        }
    }
}
