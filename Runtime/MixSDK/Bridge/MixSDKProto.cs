using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixNameSpace
{
    public class MixSDKProto
    {
        public static class Code
        {
            public static readonly int Succ                                     = 200;                                      // 成功
            public static readonly int Fail                                     = 400;                                      // 失败
            public static readonly int Error_NotSupport                         = 403;                                      // 拒绝访问
            public static readonly int Error_NotFound                           = 404;                                      // 未找到对应接口
        }
        public static class Key
        {
            public static readonly string Symbol                                = "symbol";                                 // 回调事件标识
            public static readonly string Arg                                   = "arg";                                    // 参数
            public static readonly string Code                                  = "code";                                   // 状态码
            public static readonly string Msg                                   = "msg";                                    // 信息
            public static readonly string Data                                  = "data";                                   // 数据
            public static readonly string Success                               = "success";                                // 成功(同步)
            public static readonly string Failed                                = "failed";                                 // 失败(同步)
        }
        public static class Type {
            public static class GooglePayment {
                public static readonly string Sdk                               = "com.mix.purchase.GooglePayment";         // SDK 类型
                public static readonly string PaymentMethod                     = "GOOGLE";                                 // 支付方式
                public static readonly string Init                              = "onInit";                                 // 初始化
                public static readonly string Payment                           = "onPayment";                              // 发起支付
                public static readonly string AllNonConsumable                  = "getAllNonConsumable";                    // 获取购买的一次性商品(非消耗品)信息
                public static readonly string AllSubscriptionInfo               = "getAllSubscriptionInfo";                 // 获取所有购买的订阅商品信息
                public static readonly string ConfirmOrderConsume               = "onConfirmOrderConsume";                  // 确认谷歌订单:一次性商品(消耗品)
                public static readonly string ConfirmOrderUnConsume             = "onConfirmOrderUnConsume";                // 确认谷歌订单:一次性商品(非消耗品)
            }
            public static class ApplePayment {
                public static readonly string Sdk                               = "ApplePayment";                           // SDK 类型
                public static readonly string PaymentMethod                     = "APPSTORE";                               // 支付方式
                public static readonly string Init                              = "onInit";                                 // 初始化
                public static readonly string Payment                           = "onPayment";                              // 发起支付
                public static readonly string RestorePurchase                   = "onRestorePurchase";                      // 恢复购买
                public static readonly string RefreshReceipt                    = "onRefershReceipt";                       // 刷新收据
                public static readonly string GetReceipt                        = "getReceipt";                             // 获取购买收据
                public static readonly string AddRecords                        = "addRecords";                             // 添加购买记录(订阅商品续期订单)
            }
            public static class Helper {
                public static readonly string Sdk                               = "com.mix.helper.Helper";                  // SDK 类型
                public static readonly string Log                               = "onLog";                                  // 日志
            }
        }
    }
}
