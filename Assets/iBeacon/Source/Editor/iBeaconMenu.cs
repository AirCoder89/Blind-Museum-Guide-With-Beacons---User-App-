using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System;

public class iBeaconMenu : MonoBehaviour {
	private static void Add(Type component, string undo) {
		var obj = GameObject.Find(BluetoothState.NAME);
		if (obj == null) {
			obj = new GameObject(BluetoothState.NAME, component);
			Undo.RegisterCreatedObjectUndo(obj, undo);
		} else if (obj.GetComponent(component) == null) {
			obj.AddComponent(component);
			Undo.RegisterCreatedObjectUndo(obj, undo);
		}
		Selection.activeGameObject = obj;
	}

	[MenuItem("GameObject/iBeacon/Receiver", false, 0)]
	private static void AddReceiver() {
		Add(typeof(iBeaconReceiver), "Create Beacon Receiver");
	}

	[MenuItem("GameObject/iBeacon/Server", false, 1)]
	private static void AddServer() {
		Add(typeof(iBeaconServer), "Create Beacon Server");
	}

	private static void SetDialogs(bool skip) {
		BluetoothState.CheckManifest();

		var manifest = XDocument.Load(Application.dataPath + BluetoothState.MANIFEST_PATH + BluetoothState.MANIFEST_FILE);
		XNamespace android = BluetoothState.ANDROID_NAMESPACE;
		const string metadataElement = "meta-data";
		const string metadataName = "unityplayer.SkipPermissionsDialog";

		var metadata = manifest.Descendants(metadataElement).Where(e => (string)e.Attribute(android + "name") == metadataName).Select(e => e).FirstOrDefault();
		if (metadata == null) {
			var application = manifest.Root.Element("application");
			application.Add(new XComment("iBeacon"));
			application.Add(new XElement(metadataElement, new XAttribute(android + "name", metadataName), new XAttribute(android + "value", skip ? "true" : "false")));
		} else {
			metadata.SetAttributeValue(android + "value", skip ? "true" : "false");
		}

		using (var writer = new StreamWriter(Application.dataPath + BluetoothState.MANIFEST_PATH + BluetoothState.MANIFEST_FILE, false, Encoding.UTF8)) {
			manifest.Save(writer);
		}
	}

	[MenuItem("iBeacon/Android permissions/Ask on app start", false, 0)]
	private static void DialogsNotSkip() {
		SetDialogs(false);
	}

	[MenuItem("iBeacon/Android permissions/Ask at runtime", false, 1)]
	private static void DialogsSkip() {
		SetDialogs(true);
	}
}
