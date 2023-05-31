//
//  MixAdHelper.m
//  UnityFramework
//
//  Created by qus on 2022/10/5.
//

#import <Foundation/Foundation.h>
#import <FBAudienceNetwork/FBAudienceNetwork.h>


void MixFBAdSettingsBridgeSetDataProcessingOptions(){
    [FBAdSettings setDataProcessingOptions: @[@"LDU"] country: 0 state: 0];
    NSLog(@"set facebook LDU");
}
void MixSetAdvertiserTrackingEnabled(){
    [FBAdSettings setAdvertiserTrackingEnabled:YES];
    NSLog(@"set facebook ATE");
}

