using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_ANDROID
using UnityEditor.Android;
using UnityEngine;
using System.IO;

public class MixAndroidPostBuild : IPostGenerateGradleAndroidProject
{
    public int callbackOrder
    {
        get { return 1001; }
    }

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        try
        {
#if UNITY_2019_3_OR_NEWER
            // On Unity 2019.3+, the path returned is the path to the unityLibrary's module.
            // The AppLovin Quality Service buildscript closure related lines need to be added to the root build.gradle file.
            var rootPropertiesFilePath = Path.Combine(path, "../gradle.properties");
#else

            var rootPropertiesFilePath = Path.Combine(path, "gradle.properties");
#endif
            Debug.Log("MixAndroidPostBuild BUILD path" + rootPropertiesFilePath);
            
            var writer = new StreamWriter(rootPropertiesFilePath, true);
            writer.WriteLine("");
            writer.WriteLine("android.enableDexingArtifactTransform=false");
            writer.WriteLine("android.useAndroidX=true");
            writer.Close();
        }
        catch(Exception exception)
        {
            Debug.Log("MixAndroidPostBuild catch unknow exception" + exception.Message);
        }
    }
}
#endif