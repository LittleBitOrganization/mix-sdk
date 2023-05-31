//
//  JsonUtil.h
//  Unity-iPhone
//
//  Created by imac on 2022/9/29.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface JsonUtil : NSObject

/**
 将 Json 字符串转换成字典数据

 @param jsonString Json 字符串
 @return 转换后的字典数据
 */
+ (NSDictionary *) jsonStringToDictionary:(NSString *)jsonString;

/**
 将字典数据转化为 JSON 字符串

 @param dict 字典数据
 @return 转换后的 JSON 字符串
 */
+ (NSString *) dictionaryToJsonString:(NSDictionary *) dict;

@end

NS_ASSUME_NONNULL_END
