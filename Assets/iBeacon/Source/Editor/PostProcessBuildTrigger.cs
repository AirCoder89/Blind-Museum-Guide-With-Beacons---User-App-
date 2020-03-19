#if UNITY_EDITOR && UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class PostProcessBuildTrigger {
	[PostProcessBuild]
	private static void OnPostProcessBuild(BuildTarget target, string path) {
		string plistPath = path + "/Info.plist";
		PlistDocument plist = new PlistDocument();

		plist.ReadFromFile(plistPath);
		PlistElementDict rootDict = plist.root;

		rootDict.SetString("NSBluetoothPeripheralUsageDescription", BluetoothState.BluetoothPeriphealUsageDescription);
		plist.WriteToFile(plistPath);
	}
}
#endif
