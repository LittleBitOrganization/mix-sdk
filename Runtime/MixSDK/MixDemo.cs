using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppLovinMax;

namespace MixNameSpace
{
    public class MixDemo : MonoBehaviour
    {
        private int count;
        private List<string> logs = new List<string>() { };
        int maxLogsCount = 30;

        //int startPaymentIndex = 0;
        int buttonWith = 300;
        int buttonHeight = 80;
        string myGameContentVersion = "1.0";

        static public bool onFront = true;

        int screenWidth;
        int screenHeight;
        private Vector2 btnScrollPosition;
        private Vector2 logScrollPosition;
        private int totalScrollHeight = 1000;

        void Start()
        {
            screenWidth = UnityEngine.Screen.width;
            screenHeight = UnityEngine.Screen.height;
#if !UNITY_EDITOR
    		buttonWith = screenWidth / 2 ;
    		buttonHeight = screenHeight / 13;
#endif
            btnScrollPosition = Vector2.zero;
            logScrollPosition = Vector2.zero;

            //the sdkOrderIds should save in the local or cloud
            HashSet<string> hasSendedSdkOrders = new HashSet<string>();
            this.KeepScreenOn();

            MixMain.instance.adjustInfoCallback = (r) =>
            {
                logs.Add("get Attribution  " +  Json.Serialize(new Dictionary<string, object>() {
                    {"network",r.network},
                    {"campaign",r.campaign},
                    {"adgroup",r.adgroup},
                    {"createive",r.createive},
                }) );
                logs.Add("try show app open ad");
                MixMaxManager.instance.mixAppOpenAd.ShowAppOpenWithTimeout(5.0F, (flag, adUnitId) => {
                    switch (flag)
                    {
                        case "show": {
                            logs.Add("appopen ad show " + adUnitId);
                            break;
                        }
                        case "close": {
                            logs.Add("appopen ad close " + adUnitId);
                            break;
                        }
                        case "timeout": {
                            logs.Add("appopen ad timeout " + adUnitId);
                            break;
                        }
                    }
                });
            };
            MixIap.instance.SetAction((e) =>
            {
                logs.Add("find " + e.itemId + " buy " + e.itemType);
                if (e.itemType == ProductType.Consumable)
                {
                    //如果客户端发货,需要消耗
                    MixIap.instance.FinishPurchase(e);
                }
                else if(e.itemType == ProductType.NonConsumable)
                {
                    //发货即可
                    MixIap.instance.GetAllNonConsumable();
                }
                else if (e.itemType == ProductType.Subscription)
                {
                    //发货即可
                    MixIap.instance.GetSubscriptionInfo(e.itemId);
                }

                //send item
                //MixIap.instance.FinishPurchase(e.purchasedProduct.definition.id);
            });
        }

        void OnGUI()
        {
            if (!onFront)
            {
                //if it is not on front stop draw the gui
                //Debug.Log("no on front return");
                return;
            }
            while (logs.Count > maxLogsCount)
            {
                logs.RemoveAt(0);
            }

            GUIStyle fontStyle = new GUIStyle();
            fontStyle.normal.background = null;
            fontStyle.normal.textColor = new Color(1, 1, 1);
            fontStyle.fontSize = 30;
            fontStyle.wordWrap = true;

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.alignment = TextAnchor.MiddleCenter;
            btnStyle.fontSize = 28;
            btnStyle.normal.textColor = Color.white;
            btnStyle.wordWrap = true;


            string l = "";
            for (int i = logs.Count - 1; i >= 0; i--)
            {
                l += logs[i] + "\n*************\n";
            }

            logScrollPosition = GUI.BeginScrollView(new Rect(5, 0, Screen.width - 5, Screen.height / 4), logScrollPosition, new Rect(5, 0, Screen.width - 10, 2000), false, false);
            GUI.Label(new Rect(5, 0, Screen.width - 15, Screen.height / 4), l, fontStyle);
            GUI.EndScrollView();

            //string playerItemStr = "player packages:\n";
            //foreach (var pair in playerPackages)
            //{
            //    playerItemStr += pair.Key + " : " + pair.Value + " \n";
            //}

            int y = Screen.height / 4 + 5;

            //GUI.Label(new Rect(buttonWith + 10, y + 10, Screen.width - buttonWith - 20, 150), playerItemStr, fontStyle);

            btnScrollPosition = GUI.BeginScrollView(new Rect(0, y, Screen.width, Screen.height - Screen.height / 4), btnScrollPosition, new Rect(0, y, Screen.width, totalScrollHeight), false, false);

            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "Init", btnStyle))
            {
                //var c = "HGULZdVNm7p+XWQ6LKk/M7Fek5/Ho5GIwjO2j756Ea0EyqheNaQ76qtvK8T1JV1BDmEpz+pfoHXA1I0oa8ow+9qhCwLMdODjjL2lYq29pbVxbYEgLBwcHATv/BENZ4vhmO65jXyZBO/JRq/p/tDl4Ltqh4S0GQKh0kFvWDBsTzmXvlhFylEaUUaX9i0GR5biDnCvmil3wAV17SjMjJinO9VriVNQRRXrLYmrrRfUeSuVGFjMIPy+Ba8sRdBgZ/8THbDtW74XQN/e1G8CiulC8axQdM6x0EIculpPfl/iDMJ5FgTIIky4XZp2MNrUqBGq3S2lykpLebD+IAKeINwfrm543WjfXhMK7KeV0CUS52MCSnxNKjuv/kdLzWQOtdw5fS3zrGSMy3wv8HUmoZ7U7I8R+vmDQY13Lc5fiBAmbSbT/T+okR+Gt0l6/vm3JOK4qS4XhXmYDACn1n4p0V1qZlydmK2T5fMsLs9gPOhtAi1pFks0pTmItS6q6RUdyqhRhEglT1tE6dl3rAckFph3UCIH0w7Blz7BAPHOV7zXWnGKTHlWDJ5EdKRgBPc9fIO47eDguF9YebkxB4y5UdduK4LgtZRVxrSepYhyK8YV/jZiDIYxONT0xzFuygA7yibldmFtUOKAX9HZ8ddjLGz1IymGUbbQh/d16rTAWFkK14B5Bw74wSE8/y9tF3iCxRgCpz34xWKks35A7a0xFClIGqgXhK8JRtVDfperjcT3upTH//pBwc37llyKVkJpI0RMN8RZ/7FBMbAHOpmWOinJnNt1d8jwbfcYGmxTqo3ZFKKDhXEhPxeA+9mnQCB7y6Q/Csp4RnaEklo10jTxnX/qvcN0eYUc3POeEPKchzoHrn1Ir+XA9pkY+uTK/58ZtshWBPiDgarbknYO6ZhR/6kOYHRzB18OCrny10l1v8T/3YS+dk/48besA/2mAyxUaQVCFxTM/YZOQm5sGHaiu+MGS5dt9fxjpy4sp9UsQfmQNWnTk0oAetKwSH0TNNFp33XtV86O7Li4t9G/RBaM3nwqtLpFa7vBXJufJw6kKHhUatMZLeGZGL1AA6ZyPhc+BIGXNmx/6Q2VGhHJqimlCtiPoiBKOktpNe/xAUUDh9jE5ILdH9uygXr3Iyh5E36siXoa05NKAHrSsEh9EzTRad917aWGPhOH+mqrUTT7KWkAmfzCvSUvHQGbRsrtTHM4Xe6vDrj4C07LCv5OJDWoiteG2tDfg5Jv8cjTiM1bAIYwTZhn4wVyMP75i817VQlVDMHjUDf9H/A5j6OK9M8sAyOtBdxBBCU6eKCwbwzulJcWe/QvTLpEYlUH+aZfUGRWnEbotsQDZDxDkKD/ung11I/JUs2barHk5fRVJEBi84dA+xKgUNC1JblMjl3/lhXV1dmnAuD2HB7P6pEAP9ylFYz9deXZpkqjQ1TDGZJ/H4amp4185ABhjV0FQd7n/epbgn5UWEcc2d5pQ+1flPn/UZIuTwMBtg3BwKYMefqDktQlku4nPg329w2nPYaQyyjQ8KuMilBTSMN6ZoIjrp83FQSEhIgPNF4tq7mLB2wEmyd0pdc7UH+QiRZRU9eHpV/cN43Yc1yOCgn5+vlBfsSPZmdCkA==";
                //var c = "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";
                // var c = "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";
                // var c = "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";
                var c = "HGULZdVNm7p+XWQ6LKk/M7Fek5/Ho5GIwjO2j756Ea0EyqheNaQ76qtvK8T1JV1Be/otF8T8Sij548ywAlqG+dqhCwLMdODjjL2lYq29pbVxbYEgLBwcHATv/BENZ4vhmO65jXyZBO/JRq/p/tDl4Ltqh4S0GQKh0kFvWDBsTzmXvlhFylEaUUaX9i0GR5biDnCvmil3wAV17SjMjJinO9VriVNQRRXrLYmrrRfUeSuVGFjMIPy+Ba8sRdBgZ/8THbDtW74XQN/e1G8CiulC8axQdM6x0EIculpPfl/iDMKZmmrQ64T+fRgLP91auv0zFFXhN6E9Ec7JLHHyjwRV8MQPQm2DPuuW5DZ+lIYMjROdsxlgszIW1Q4rxKrPs5+FJzHN7Ki0n59IRg1emfG6Jy0gUR8eAQyuunD4iG7i73jljEtBleR1M3lV0XkZlNunS7O+6twfQ0h5QkIusJHpi6HTnBzvBlQsdup0u8zZwFiDG1JEGEzWgURUwYRSE0nrMz0/wRgnksEPLATyPsEBaF3ayzDxVDVNkrzX004a0lvOGxRzUwHc+WQf1Nhet2fplsNFpv0SAulXe7gAw2Qts5J/azO2lbcqEq6HRMRG7BCHxGOve1pKmaU/vLCOXOhBiOVo8jCDBa+HrM2nCbrbI2mdUtxUrQK+S8FxEs6RJ8sQGtqD3iSZA4KFd6cDqEAvAVrLhdhKaN2EyOp7nCUogMTbiXmPZl6BY0gm8ZZjPMvgp5/IIfJCQkCpp641Fsr29AJpVwZfY6hz6o32WYuhwdo3MVQHIb46m92RQcxayUcf0LLWpNiYi5hug9bscCD/GbGPd9m1FPx8hxsgqRjZBVl7d+OraBo/4eFxt5xzDyaNFtpXJKrLDZYrjzOzs8A0cFUWFJMA7FeVQdtEziJyt8T0RHqLqR1Unng0s/qHpofU0ypzwinGlhZhPiBY1WDQoMTnWHCvW3QqIbzCk40pTRLcOqH6Ab9fAEXENvxN+5MZU09aLYjMG4uNHiWv3Kvg2q0UnXMDDfw9H1+rs0m14cb3WNAGNx+cSVx18KffjM/+/TqZ2nOeQ40LzcFzT3HPahHtBitZzKJ0EAuU0YnobXuVnwyMEmH9jP9/JbBwJITRKzXeLE8ZBpqCO2ef9pFww0pG6tRWvrG9+f21ck4ven4RoMG3Ggn0ooXS441/fhVv4CNeW3MfB68hhCjQiNSSAjSx/MerZDw3xlVICiXja8Lc3LN/r3XWbeExfG2l6A69FIWTSw1GnyhmotK9Cp27IKKu90//WK2D7Mdaa27XqDKdZaNOyoPSW2bQJt0TWbE7ew/nnCaKwU/dpf3NJ+mBtd1Cm2U3djjygJvBKSyxra9uKR5C60f3KSLjMZ0EhX+GBBw0/JLEvjfn7UaGhMPfXBTMdf4l+snhW4WO3x8LzpaWf4aEbzUP3+nO+vkoc6SrQpXZN1mdiAHfsVVcf0g6BqyeG6Uxp74uErg/MpxDnGXsGXmEjtWkOb16CJtTgroChdzi6wa9iHJEoAwTXkJAUFV/k2SOat2QyHIqRllzIT1L4EFEoDaQ5DRolSAs52ATRbBBJFWN+Jtj+KKP+iGYVoIpOuOUv/Vc5UfiUof2Pk0Q/gwOv7mUOrsjISC1g9MuNxRcVKo6HPvqqlMwbClEowUI7x3pZKOBmU7iCQiub+ItGvcAy19c7q8343U+rTbTk0oAetKwSH0TNNFp33XtpYY+E4f6aqtRNPspaQCZ/MK9JS8dAZtGyu1Mczhd7q8OuPgLTssK/k4kNaiK14ba0N+Dkm/xyNOIzVsAhjBNmGfjBXIw/vmLzXtVCVUMweOX8M/ahSIgsP2IAegaKlVD+z/H7WvAXh0ddCpIB2ZKGZb7IFXYfdUkQWj6ZkjGwotFiBWgERF2r02RMrRX1JR/vtcCP3A3N/HVBKoqPmynx4gPNF4tq7mLB2wEmyd0pdf4xZC3TiivDCPhFTpTZj29iA80Xi2ruYsHbASbJ3Sl1/TzhfjE+zmU66/pVPckkyELd0iNJxQEPxHC/WnxgqcNMYRxrFZ1QTyR4kssseB0tqDSrSlvAB6cMocEA0Ocnw5OjzWKwvespF0ZSq9U5nj9qdTVkrcIovxPrCbo0bHT/sc2I3vweK642jBi3qcj+x+XQcAH314QuKL7oZ3ryFzfDlTS7W1djkkNz6ROj9VwT4HEvuZ6pPFKCrjMuaxx1nhh1ml7EC2+FRfJcZ6Ox3J2259V0BDjFwyZgyf5EUjP3gRwDqZeP6tY2Omn1bwyJYw=";
                MixMain.instance.Init(MixSDKConfig.parse(c, true, MaxSdkBase.BannerPosition.BottomCenter, true), (s) =>
                {
                    Debug.Log("finish init");
                    //query all subscription item
                    logs.Add("finish init " + s);
                });
            }

            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "hasReward", btnStyle))
            {
                var result = MixMaxManager.instance.mixRewardedAd.IsRewardedAdReady();
                logs.Add("call hasRewardedVideo " + result);
            }

            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "showRewardVideoAd", btnStyle))
            {
                logs.Add("call showRewardVideoAd ");
                MixMaxManager.instance.mixRewardedAd.ShowRewardedAd(() =>
                {
                    logs.Add("on reward callback ");
                },
                () =>
                {
                    logs.Add("on reward hiden callback ");
                });
            }

            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "hasInterstitial", btnStyle))
            {
                var result = MixMaxManager.instance.mixInterstitialAd.IsInterstitialReady();
                logs.Add("call hasInterstitial " + result + " ");
            }

            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "showInterstitialAd", btnStyle))
            {
                logs.Add("call showInterstitialAd ");
                MixMaxManager.instance.mixInterstitialAd.ShowInterstitial(() => {
                    logs.Add("on showInterstitialAd hidden ");
                });
            }


            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "showBannerAd top", btnStyle))
            {
                logs.Add("call showBannerAd");
                MixMaxManager.instance.mixBannerAd.ShowBanner();
            }

            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "removeBannerAd", btnStyle))
            {
                logs.Add("call dismissBannerAd ");
                MixMaxManager.instance.mixBannerAd.HideBanner();
            }

            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "testLog", btnStyle))
            {
                logs.Add("call testLog ");
                Dictionary<string, string> paramsDic = new Dictionary<string, string>();
                paramsDic.Add("paramA", "valueA");
                paramsDic.Add("paramB", "valueB");
                MixData.instance.Log("unityLogTest", paramsDic);
            }
            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "increvenue", btnStyle))
            {
                MixThirdUpload.instance.IncRevenue((float)0.02);
            }
            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "ios restore", btnStyle))
            {
                MixIap.instance.AppleRestore((t) =>
                {
                    Debug.Log("restore finish");
                });
            }
            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "query sub", btnStyle))
            {
                var infos = MixIap.instance.GetAllSubscriptionInfo();
                logs.Add("get result from subs " + infos.Count);
                foreach (var one in infos)
                {
                    logs.Add("querysub " + one.itemId + " " + one.purchaseTime + " " + one.expireTime);
                    Debug.Log("querysub " + one.isSubscribed + " " + one.purchaseTime + " "+one.expireTime);
                }
            }
            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "query noncomsu", btnStyle))
            {
                var a = MixIap.instance.GetAllNonConsumable();
                logs.Add("get result from allNonConsumable " + a.Count);
                foreach (var one in a)
                {
                    Debug.Log("querynoncomsu " + one.Key + " " + one.Value);
                }
            }
            if ( MixMain.instance != null && MixMain.mixSDKConfig != null)
            {
                foreach (var i in MixMain.mixSDKConfig.mixInput.items)
                {
                    y += buttonHeight + 10;
                    if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "purchase "+i.itemId, btnStyle))
                    {
                        MixIap.instance.PurchaseItem(i.itemId, null, (e) =>
                        {
                            Debug.Log("demo get fail  in PurchaseItem " + e.ToString());
                        });
                    }
                    
                }
            }
            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "share link fb", btnStyle))
            {
                // logs.Add("try share url to facebook");
                // string url = "https://www.facebook.com";
                // MixThirdShare.instance.ShareFacebook(url, null, (result) => {
                //     logs.Add("share fb link success: " + result);
                // }, (code, errorMsg) => {
                //     logs.Add("share fb link failed: code: " + code + "; errorMsg: " + errorMsg);
                // });
            }
            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "share photo fb", btnStyle))
            {
                // logs.Add("try share image to facebook");
                // string imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAARCAIAAAAzPjmrAAAE+UlEQVQ4ESWVyW4dxxWG69TcM+9AUaJImYyswaOMLALbCWDAq+wC5HXyDHmPrLMykG1sIEACBPEg2JRE2qJIXvFOfW9PNaduAjQK6Oq/vjrndP2nAP/+z4HgQAEwA4xBUg+IUkoo1RgQIx4HRAAIQTsZjmOUIUJ3ryQgSlBchYECpjiqMI846nmUE08iChiNQ8D/R5D4NeI8+x8OdvMoQhmB3XychN0GsFsXeQ9SlHCYabAhYgBHTQCB7ZiFioSRII21NHBMGP7s/i7Of9yCiyoSZWAxxHgjBlEXcTHGmGWM1ROEAEnqDqT904f5aRG+nqGYikbo7zdD06vRppb1KnN6moZ3DzgNbJc1Je6TA3MyTb9b2xvFO48UQaWEbXDWAw6xbISD84jk2H02Rs+qQQR70i3ucfKHnBqnGg2yby9mmxcv32rnGnBrCO1NjI1ii8PrWj3Lw+/u0y+PyWLQrcMak++3sHDhgbQnE/lj4xJC3rT9Hw/Rs2yguk0yIXAYFOoB1z2qm77ZbE/L8PlvDw8rooyardS676mQWMqwJPlfW/qX5zBKXMqhYLQS8Lw1H5b4031XlvZc47POCy4uzfBBkpfTPco4j3+Vy0WPzmOFpP3NgX0yCVlw7aK+fNvsjflJOaIP0+1RTuayMkAes9ABfmH5ZSBUo7sVkIx8a7BdkmvPPrmD3i9pD/lPEPQAd0BMOahWT7F/RDqsmim2Zk1gr5icnKbHsOnsXkYo+8/fVok8ffqo2L9TCJZNy18hjwlJMBxwGDM/TRBieOaTeILL+CAiwrDPoErZ2/nix/Pl2Zutauv7KSokrUbZ/akvM2U8TpKkKvdoCV3f+ctvf7Dh30lZVodHhWQfPT4aF2nioGTs8X7l8vI9xjHPdg4xHdV+lMgklX1x9+zW/muxEJ63EKYYHzpZ0fH+dDKJrDxHJB7yfGygenmz5kgXtru4eQlGLy9njz5+LKf7GPmLrk5H+Eqh9+75Xx9P8ny0dUVDRW3gm1/mX71W5d39CqwISqbEkPSy9/Vci0wlCiQBmozL2cJjJhBhDcGWIU7oL7eD/e5icmRvxXT9shtYtxb50ZU//bmbpIyJaGJ4fbm8brUBWnFhHTKEDVky5FKUBQhZW/TD1dZhT1+smgAozal2WBESLS7lno4+wHR+czOI5mKgJK9cgX5G8OqqNiF6EawzGYap4AH5VfBVkUou5zjrBjgL7qEPm1Y9n8ckDe2GGiGidVDWC8aPHxweVJNVE42JTAgHZNvpuV0HiSprsk7em+l8o0zcwwDCCeIytixiDKexB2kjWXq9NP88f52AR8F165pKzhjP6o1VKtCMc5k610tJ39RGG3y1CpXIKrEYttelhyfV9TIdn6/yRJSzFnedNTY2SdEHOwRHB0v6ZVNv7e1cDYuC9yVxtGL5MMS6SyuDlFm7VZ4h50wqswFcr6kz3KBy1tSZ6ms135+8OCIV4PF0OlpZphTdqJznKA1u/uqMbBdFt3G6CX0N2ASiKHLrYIPpTZYkMngJWZrkjSV790Z09UbwV6fj5eHY3myRdjUjam1xs2mRu74zQmWAVouHSTQG+MZrvXFEQRFbIwneOhdCMPDoi6eEyvXWImDBoXdPiuN3Ro7TRbdmtkvQbZ77zsVLorRqKblqFTQK8xynUkVM7L2CGeIVQrFCLtY5NncE3mi0aajW9L/yrp7wvsDabwAAAABJRU5ErkJggg==";
                // byte[] imageBase64Bytes = System.Convert.FromBase64String(imageBase64);
                // MixThirdShare.instance.ShareFacebook(null, imageBase64Bytes, (result) => {
                //     logs.Add("share fb photo success: " + result);
                // }, (code, errorMsg) => {
                //     logs.Add("share fb photo failed: code: " + code + "; errorMsg: " + errorMsg);
                // });
            }
            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "open half webview", btnStyle))
            {
                logs.Add("half webview open");
                float dpi = Screen.dpi;
                int screenWidth = Screen.width;
                int screenHeight = Screen.height;
                int width = 896;
                int height = 1344;
                logs.Add("screen dpi: " + dpi+ "; screenWidth: " + screenWidth + "; screenHeight: " + screenHeight);
                MixFyberManager.OpenHalfWebView(screenWidth / 2, screenHeight / 2, width, height, (msg) => {
                    logs.Add("half webview close " + msg);
                }, (isOpen, msg) => {
                    if (isOpen) {
                        logs.Add("half webview enter full");
                    } else {
                        logs.Add("half webview exit full");
                    }
                });
            }
            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "close half webview", btnStyle))
            {
                MixFyberManager.CloseHalfWebView();
            }
            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "open task webview", btnStyle))
            {
                MixFyberManager.OpenTaskWebView(0.24, (msg) => {
                    logs.Add("task webview success " + msg);
                }, (errorMsg) => {
                    logs.Add("task webview failed " + errorMsg);
                });
            }
            y += buttonHeight + 10;
            if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "refresh web show more", btnStyle))
            {
                logs.Add("refresh web show more");
                MixFyberManager.refresh_web_show_more();
            }

            
            //y += buttonHeight + 10;
            //if (GUI.Button(new Rect(0, y, buttonWith, buttonHeight), "log pay", btnStyle))
            //{
            //    DataUpload.instance.LogPaySuccess("tid", "pid", "2022-01-01 00:00:00", 6.99, "USD", true);
            //}

            GUI.EndScrollView();

            totalScrollHeight = y + 100;

            //		//supp
            //		if (showRward) {
            //			if (currentTs > rewardTs) {
            //				showRward = false;
            //			} else {
            //				GUI.Box (new Rect (0, 0, Screen.width, Screen.height, "showing reward"));
            //				if (GUI.Button (new Rect (100, 100, 100, 100), "clickAd")) );
            //					logs.Add ("click Ad");
            //					Mopub
            //				}
            //			}
            //		}
            //		//show all buy items

        }

        void Update()
        {
            if (Input.touchCount > 0)
            {

                Touch touch = Input.touches[0];
                if (touch.phase == TouchPhase.Moved)
                {
                    if (touch.position.y < Screen.height / 4 * 3)
                    {
                        btnScrollPosition.y += touch.deltaPosition.y;
                    }
                    else
                    {
                        logScrollPosition.x -= touch.deltaPosition.x;
                        logScrollPosition.y += touch.deltaPosition.y;
                    }
                }
            }
        }

        void KeepScreenOn() 
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try {
                int FLAG_KEEP_SCREEN_ON = 128;
                AndroidJavaClass jcPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = jcPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                    //需要在UI线程中调用
                    activity.Call<AndroidJavaObject>("getWindow").Call("addFlags", FLAG_KEEP_SCREEN_ON);
                }));
            } catch (System.Exception e) {
                Debug.LogError(e.Message);
            }
#endif
        }    
    }
}