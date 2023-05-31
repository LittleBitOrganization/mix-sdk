#if UNITY_IOS
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class MixiOSPostBuild {
	/**
	 * 编译iOS时修改 Info.plist 文件
	 */
	[UnityEditor.Callbacks.PostProcessBuild]
	public static void ChangeXcodePlist(UnityEditor.BuildTarget buildTarget, string path) {
		if (BuildTarget.iOS == buildTarget) {
			string plistPath = path + "/Info.plist";
			PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;
            Debug.Log(">> Automation, plist ... <<");
            // example of changing a value:
            // rootDict.SetString("CFBundleVersion", "6.6.6");

            // example of adding a boolean key...
            // < key > ITSAppUsesNonExemptEncryption </ key > < false />
            // rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            File.WriteAllText(plistPath, plist.WriteToString());
        }
	}
	[PostProcessBuildAttribute(1)]
	public static void OnPostProcessBuild(BuildTarget target, string path) {
		if (BuildTarget.iOS == target) {
			PBXProject project = new PBXProject();
            string sPath = PBXProject.GetPBXProjectPath(path);
            project.ReadFromFile(sPath);

            string unityFrameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
            string unityMainTargetGuid = project.GetUnityMainTargetGuid();
            
            ModifyFrameworksSettings(project, unityFrameworkTargetGuid);
            ModifyUnityiPhoneSettings(project, unityMainTargetGuid);

            // modify frameworks and settings as desired
            File.WriteAllText(sPath, project.WriteToString());
		}
    }
    private static void ModifyFrameworksSettings(PBXProject project, string g) {
    	// add hella frameworks
    	Debug.Log(">> Automation, Frameworks... <<");
    	// project.AddFrameworkToProject(g, "blah.framework", false);
        // project.AddFrameworkToProject(g, "libz.tbd", false);

    	// go insane with build settings
    	Debug.Log(">> Automation, Settings... <<");

    	// project.AddBuildProperty(g, "LIBRARY_SEARCH_PATHS", "../blahblah/lib");
        // project.AddBuildProperty(g, "OTHER_LDFLAGS", "-lsblah -lbz2");

    	// note that, due to some Apple shoddyness, you usually need to turn this off
        // to allow the project to ARCHIVE correctly (ie, when sending to testflight):
        project.AddBuildProperty(g, "ENABLE_BITCODE", "NO");
        project.AddBuildProperty(g, "Always_Embed_Swift_Standard_Libraries", "YES");
    }
    private static void ModifyUnityiPhoneSettings(PBXProject project, string g)
    {
        // add hella frameworks
        Debug.Log(">> Automation, Frameworks... <<");
        // project.AddFrameworkToProject(g, "blah.framework", false);
        // project.AddFrameworkToProject(g, "libz.tbd", false);

        // go insane with build settings
        Debug.Log(">> Automation, Settings... <<");

        // project.AddBuildProperty(g, "LIBRARY_SEARCH_PATHS", "../blahblah/lib");
        // project.AddBuildProperty(g, "OTHER_LDFLAGS", "-lsblah -lbz2");

        // note that, due to some Apple shoddyness, you usually need to turn this off
        // to allow the project to ARCHIVE correctly (ie, when sending to testflight):
        project.AddBuildProperty(g, "ENABLE_BITCODE", "NO");
        project.AddBuildProperty(g, "Always_Embed_Swift_Standard_Libraries", "YES");
    }
}
#endif