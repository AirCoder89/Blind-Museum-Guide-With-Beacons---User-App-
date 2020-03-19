using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Text;
using UnityEditor;
#endif

public enum BluetoothLowEnergyState {
	UNKNOWN,
	RESETTING,
	UNSUPPORTED,
	UNAUTHORIZED,
	POWERED_OFF,
	POWERED_ON,
	TURNING_OFF,
	TURNING_ON,
	IBEACON_ONLY = 0xFF,
}

[ExecuteInEditMode]
public class BluetoothState : MonoBehaviour {
	public delegate void BluetoothStateChanged(BluetoothLowEnergyState state);

	public static event BluetoothStateChanged BluetoothStateChangedEvent;

	private static bool initialized = false;

	public const string NAME = "IBeacon";

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void InitBluetoothState(bool shouldLog);

	[DllImport("__Internal")]
	private static extern void EnableIOSBluetooth();

	[DllImport("__Internal")]
	private static extern int GetIOSBluetoothState();
#endif

#if UNITY_ANDROID
	private static AndroidJavaObject plugin;

	private static AndroidJavaObject GetPlugin() {
		if (plugin == null) {
			plugin = new AndroidJavaObject("com.kaasa.ibeacon.BluetoothState");
		}
		return plugin;
	}

	private static bool hasSupport = false;
#endif

#if UNITY_EDITOR
	private static string bluetoothPeripheralUsageDescription = "";

	[SerializeField]
	private string _bluetoothPeripheralUsageDescription = "";

	public static string BluetoothPeriphealUsageDescription {
		get {
			return bluetoothPeripheralUsageDescription;
		}
		set {
			m_instance._bluetoothPeripheralUsageDescription = value;
			bluetoothPeripheralUsageDescription = value;
		}
	}

	private void UpdateDescriptions() {
		if (!bluetoothPeripheralUsageDescription.Equals(_bluetoothPeripheralUsageDescription)) {
			bluetoothPeripheralUsageDescription = _bluetoothPeripheralUsageDescription;
		}
	}

	private void Update() {
		name = NAME;
	#if UNITY_IOS
		UpdateDescriptions();
		CheckDescriptions();
	#endif
	}
#endif

	private static BluetoothState m_instance;

	private void Awake() {
		if (m_instance != null && m_instance != this) {
#if UNITY_EDITOR
			DestroyImmediate(this);
#else
			Destroy(this);
#endif
			return;
		}
		m_instance = this;
		name = NAME;
		initialized = false;
#if UNITY_EDITOR
	#if UNITY_ANDROID
		CheckManifest();
	#endif
	#if UNITY_IOS
		if (string.IsNullOrEmpty(bluetoothPeripheralUsageDescription)) {
			bluetoothPeripheralUsageDescription = _bluetoothPeripheralUsageDescription;
		} else {
			_bluetoothPeripheralUsageDescription = bluetoothPeripheralUsageDescription;
		}
		CheckDescriptions();
	#endif
#endif
	}

	private void OnDestroy() {
		if (m_instance == this) {
			m_instance = null;
		}
	}

#if UNITY_EDITOR
	public const string MANIFEST_PATH = "/Plugins/Android";
	public const string MANIFEST_FILE = "/AndroidManifest.xml";
	public const string ANDROID_NAMESPACE = "http://schemas.android.com/apk/res/android";

	private static void CopyManifest() {
		Directory.CreateDirectory(Application.dataPath + MANIFEST_PATH);
		File.Copy(Application.dataPath + "/iBeacon/Plugins/Android/AndroidManifest.xml", Application.dataPath + MANIFEST_PATH + MANIFEST_FILE, true);
		AssetDatabase.Refresh();
	}

	public static void CheckManifest() {
		XDocument manifest;
		try {
			manifest = XDocument.Load(Application.dataPath + MANIFEST_PATH + MANIFEST_FILE);
		} catch (XmlException) {
			CopyManifest();
			return;
		} catch (IOException) {
			CopyManifest();
			return;
		}

		XNamespace android = ANDROID_NAMESPACE;
		const string permissionElement = "uses-permission-sdk-23";
		const string permissionName = "android.permission.ACCESS_COARSE_LOCATION";

		if (manifest.Root.Element(permissionElement) == null) {
			manifest.Root.Add(new XComment("iBeacon"));
			manifest.Root.Add(new XElement(permissionElement, new XAttribute(android + "name", permissionName)));
		}

		using (var writer = new StreamWriter(Application.dataPath + MANIFEST_PATH + MANIFEST_FILE, false, Encoding.UTF8)) {
			manifest.Save(writer);
		}
	}

	public static void CheckDescriptions() {
		if (string.IsNullOrEmpty(PlayerSettings.iOS.locationUsageDescription)) {
			Debug.LogError("Location Usage Description is not set in the Player Settings");
		}
		if (string.IsNullOrEmpty(bluetoothPeripheralUsageDescription)) {
			Debug.LogError("Bluetooth Peripheral Usage Description is not set in the Bluetooth State component");
		}
	}
#endif

	public static void Init() {
		Init(true);
	}

	public static void Init(bool shouldLog) {
		if (initialized) {
			return;
		}
		if (m_instance == null) {
			m_instance = FindObjectOfType<BluetoothState>();
			if (m_instance == null) {
				var obj = GameObject.Find(NAME);
				if (obj == null) {
					obj = new GameObject(NAME);
				}
				m_instance = obj.AddComponent<BluetoothState>();
			}
		}
#if !UNITY_EDITOR
	#if UNITY_IOS
		InitBluetoothState(shouldLog);
	#elif UNITY_ANDROID
		GetPlugin().Call("Init", shouldLog);
	#endif
#endif
		initialized = true;
		if (BluetoothStateChangedEvent != null) {
			BluetoothStateChangedEvent(GetBluetoothLEStatus());
		}
	}

	public static BluetoothLowEnergyState GetBluetoothLEStatus() {
		Init();
#if UNITY_EDITOR
		return BluetoothLowEnergyState.UNSUPPORTED;
#elif UNITY_ANDROID
		if (!GetPlugin().Call<bool>("IsBLEFeatured")) {
			return BluetoothLowEnergyState.UNSUPPORTED;
		}
		hasSupport = true;
		if (!GetPlugin().Call<bool>("IsBluetoothAvailable")) {
			return BluetoothLowEnergyState.UNKNOWN;
		}
		if (!GetPlugin().Call<bool>("IsBluetoothTurnedOn")) {
			return BluetoothLowEnergyState.POWERED_OFF;
		}
		return BluetoothLowEnergyState.POWERED_ON;
#elif UNITY_IOS
		return (BluetoothLowEnergyState)GetIOSBluetoothState();
#else
		return BluetoothLowEnergyState.UNSUPPORTED;
#endif
	}

	private void ReportBluetoothStateChange(string newstate) {
		if (BluetoothStateChangedEvent != null) {
#if UNITY_ANDROID
			if (!hasSupport) {
				return;
			}
#endif
			BluetoothStateChangedEvent((BluetoothLowEnergyState)int.Parse(newstate));
		}
	}

	public static void EnableBluetooth() {
		Init();
		if (GetBluetoothLEStatus() == BluetoothLowEnergyState.UNSUPPORTED) {
			throw new iBeaconException("This device does not support Bluetooth Low Energy");
		}
#if !UNITY_EDITOR
	#if UNITY_ANDROID
		GetPlugin().Call("EnableBluetooth");
	#elif UNITY_IOS
		EnableIOSBluetooth();
	#endif
#endif
	}
}
