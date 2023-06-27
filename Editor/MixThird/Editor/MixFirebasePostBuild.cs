using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;

#if UNITY_ANDROID
using UnityEditor.Android;
public class MixFirebasePostBuild : IPostGenerateGradleAndroidProject
{
    private static bool usingFirebase = true;
    private static string jsonFile = "./Assets/Plugins/Android/google-services.json";
    private static string jsonFileName = "google-services.json";
    //private static readonly Regex TokenGooglePlugin = new Regex(".*apply plugin:.+?(?=com.google.gms.google-services).*");
    private static readonly Regex TokenApplicationPlugin = new Regex(".*apply plugin: \'com.android.application\'.*");
    private static readonly Regex TokenBuildScriptDependencies = new Regex(".*classpath \'com.android.tools.build:gradle.*");

    private const string BuildScriptMatcher = "buildscript";
    private const string GoogleServiceDependencyClassPath = "classpath 'com.google.gms:google-services:4.3.3'";
    private const string GoogleServiceApplyPlugin = "apply plugin: 'com.google.gms.google-services'";

    public int callbackOrder
    {
        get { return 1002; }
    }

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        try
        {
            if (usingFirebase)
            {
                //check google service json
                if (!File.Exists(jsonFile))
                {
                    Debug.Log("MixPushPostBuild find exception that no " + jsonFile + " finded");
                }
                else
                {
#if UNITY_2019_3_OR_NEWER
                    var targetJsonFile = Path.Combine(path, "../launcher/", jsonFileName);
#else
                    var targetJsonFile = Path.Combine(path, jsonFileName);
#endif
                    Debug.Log("MixPushPostBuild copy from " + jsonFile + " to " + targetJsonFile);
                    File.Copy(jsonFile, targetJsonFile);
                }

                Debug.Log("MixPushPostBuild begin to add push");
#if UNITY_2019_3_OR_NEWER
                // On Unity 2019.3+, the path returned is the path to the unityLibrary's module.
                // The AppLovin Quality Service buildscript closure related lines need to be added to the root build.gradle file.
                var buildScriptGradleFile = Path.Combine(path, "../build.gradle");
                var applyPluginGradleFile = Path.Combine(path, "../launcher/build.gradle");
                UpdateFile(buildScriptGradleFile, true, false);
                UpdateFile(applyPluginGradleFile, false, true);
#else
                var gradleFile = Path.Combine(path, "build.gradle");
                UpdateFile(gradleFile, true, true);
#endif
            }
            else
            {
                Debug.Log("MixPushPostBuild skip to add push");
            }

        }
        catch (Exception exception)
        {
            Debug.Log("MixPushPostBuild catch unknow exception" + exception.Message);
            throw exception;
        }
    }
    private bool UpdateFile(string addFile, bool addBuildScriptLines, bool addPlugin)
    {
        Debug.Log("MixPushPostBuild GenerateUpdatedBuildFileLines " + addFile + " addPlugin " + addPlugin + " addBuildScriptLines " + addBuildScriptLines);
        var lines = File.ReadAllLines(addFile).ToList();
        var outputLines = GenerateUpdatedBuildFileLines(lines, addBuildScriptLines, addPlugin);

        // outputLines will be null if we couldn't add the build script lines.
        if (outputLines == null) return false;

        try
        {
            File.WriteAllText(addFile, string.Join("\n", outputLines.ToArray()) + "\n");
        }
        catch (Exception exception)
        {
            Debug.Log("Failed to install Google srvice plugin. Root Gradle file write failed.");
            Console.WriteLine(exception);
            return false;
        }

        return true;
    }
    private static List<string> GenerateUpdatedBuildFileLines(List<string> lines, bool addBuildScriptLines, bool addPlugin)
    {
        // A sample of the template file.
        // ...
        // allprojects {
        //     repositories {**ARTIFACTORYREPOSITORY**
        //         google()
        //         jcenter()
        //         flatDir {
        //             dirs 'libs'
        //         }
        //     }
        // }
        //
        // apply plugin: 'com.android.application'
        //     **APPLY_PLUGINS**
        //
        // dependencies {
        //     implementation fileTree(dir: 'libs', include: ['*.jar'])
        //     **DEPS**}
        // ...
        var outputLines = new List<string>();

        var buildScriptClosureDepth = 0;
        var insideBuildScriptClosure = false;
        var buildScriptMatched = false;
        var qualityServiceRepositoryAdded = false;
        var qualityServiceDependencyClassPathAdded = false;
        var qualityServicePluginAdded = false;
        foreach (var line in lines)
        {
            // Add the line to the output lines.
            outputLines.Add(line);

            if (addBuildScriptLines)
            {

                if (!buildScriptMatched && line.Contains(BuildScriptMatcher))
                {
                    buildScriptMatched = true;
                    insideBuildScriptClosure = true;
                }

                // Match the parenthesis to track if we are still inside the buildscript closure.
                if (insideBuildScriptClosure)
                {
                    if (line.Contains("{"))
                    {
                        buildScriptClosureDepth++;
                    }

                    if (line.Contains("}"))
                    {
                        buildScriptClosureDepth--;
                    }

                    if (buildScriptClosureDepth == 0)
                    {
                        insideBuildScriptClosure = false;

                        // There may be multiple buildscript closures and we need to keep looking until we added both the repository and classpath.
                        buildScriptMatched = qualityServiceRepositoryAdded && qualityServiceDependencyClassPathAdded;
                    }
                }

                if (insideBuildScriptClosure)
                {
                    //// Add the build script dependency repositories.
                    //if (!qualityServiceRepositoryAdded && TokenBuildScriptRepositories.IsMatch(line))
                    //{
                    //    outputLines.Add(GetFormattedBuildScriptLine(QualityServiceMavenRepo));
                    //    qualityServiceRepositoryAdded = true;
                    //}
                    // Add the build script dependencies.
                    if (!qualityServiceDependencyClassPathAdded && TokenBuildScriptDependencies.IsMatch(line))
                    {
                        outputLines.Add(GetFormattedBuildScriptLine(GoogleServiceDependencyClassPath));
                        qualityServiceDependencyClassPathAdded = true;
                    }
                }
            }

            if (addPlugin)
            {
                // Add the plugin.
                if (!qualityServicePluginAdded && TokenApplicationPlugin.IsMatch(line))
                {
                    outputLines.Add(GoogleServiceApplyPlugin);
                    qualityServicePluginAdded = true;
                }
            }
        }

        return outputLines;
    }

    private static string GetFormattedBuildScriptLine(string buildScriptLine)
    {
#if UNITY_2019_3_OR_NEWER
        return "            "
#else
        return "        "
#endif
                   + buildScriptLine;
    }
}
#endif

#if UNITY_IOS || UNITY_IPHONE
using UnityEngine.Networking;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class MixFirebasePostBuild
{
    private static string plistFile = "./Assets/Plugins/iOS/GoogleService-Info.plist";
    private static string plistFileName = "GoogleService-Info.plist";

    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            Debug.Log("MixPushPostBuild path " + buildPath);
            
            if (!File.Exists(plistFile))
            {
                Debug.LogError("MixFirebasePostBuild find exception that no " + plistFile + " finded");
            }
            else
            {
                var targetJsonFile = Path.Combine(buildPath, plistFileName);
                Debug.Log("MixPushPostBuild copy from " + plistFile + " to " + targetJsonFile);
                File.Copy(plistFile, targetJsonFile);

                string projectPath = buildPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
                PBXProject project = new PBXProject();
                project.ReadFromFile(projectPath);
                string UnityMainTargetName = "Unity-iPhone";
#if UNITY_2019_3_OR_NEWER
                var unityMainTargetGuid = project.GetUnityMainTargetGuid();
                var unityFrameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
#else
                    
                var unityMainTargetGuid = project.TargetGuidByName(UnityMainTargetName);
                var unityFrameworkTargetGuid = project.TargetGuidByName(UnityMainTargetName);
#endif
                // Add plist file
                var jsonFileGuid = project.AddFile(targetJsonFile, plistFileName, PBXSourceTree.Source);
                project.AddFileToBuild(unityMainTargetGuid, jsonFileGuid);
                project.WriteToFile(projectPath);

                var capManager = new ProjectCapabilityManager(projectPath, "Unity-iPhone/Unity-iPhone.entitlements", UnityMainTargetName);
                //capManager.AddBackgroundModes(BackgroundModesOptions.BackgroundFetch);
                capManager.AddPushNotifications(true);
                capManager.WriteToFile();

            }
        }
    }

}
#endif