//
//  CSBean.h
//  Demo
//
//  Created by Bepic on 2022/10/20.
//

#import <Foundation/Foundation.h>

#if defined (__cplusplus)
extern "C" {
#endif
    // 定义一个结构体(用于Unity 和 iOS 之间的数据交互)
    typedef struct Parameter {  // 注意:结构体属性按必须此顺序排列
        const char * _Nonnull symbol;
        int code;
        const char * _Nonnull msg;
        const char * _Nullable data;
    } Params;
    // 定义 C# 回调函数
    typedef void (*CSFunction)(Params * _Nonnull params);
#if defined (__cplusplus)
}
#endif

NS_ASSUME_NONNULL_BEGIN

// 封装 Bean
@interface CSBean : NSObject
    @property (nonatomic, copy) NSString * symbol;
    @property (nonatomic) CSFunction csFunc;
@end

NS_ASSUME_NONNULL_END
