using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

[ExecuteInEditMode]
[RequireComponent(typeof(BluetoothState))]
public class iBeaconReceiver : MonoBehaviour {
#if iBeaconDummy
	[System.Serializable]
	public class DummyRegion {
		public string regionName;
		public string UUID;
	}
	public DummyRegion[] regions;
	public string NSLocationUsageDescription;
	public string _NSLocationUsageDescription;
#endif

	public delegate void BeaconRangeChanged(Beacon[] beacons);

	public static event BeaconRangeChanged BeaconRangeChangedEvent;

	[Obsolete("BluetoothStateChangedEvent is deprecated, please use BluetoothState.BluetoothStateChangedEvent instead.")]
	public static event BluetoothState.BluetoothStateChanged BluetoothStateChangedEvent {
		add {
			BluetoothState.BluetoothStateChangedEvent += value;
		}
		remove {
			BluetoothState.BluetoothStateChangedEvent -= value;
		}
	}

#if !iBeaconDummy
	public static iBeaconRegion[] regions {
		get {
			return m_instance._regions;
		}
		set {
			m_instance._regions = value;
			initialized = false;
		}
	}
#endif

	[SerializeField]
	private iBeaconRegion[] _regions;

	[Header("Android")]

	[SerializeField]
	[FormerlySerializedAs("_androidDetectionTimespan")]
	[Range(0, 20)]
	private float _detectionTimespan;
	public static float androidDetectionTimespan {
		get {
			return m_instance._detectionTimespan;
		}
		set {
			m_instance._detectionTimespan = value;
#if UNITY_ANDROID
			initialized = false;
#endif
		}
	}

	[SerializeField]
	[Range(0, 10)]
	private float _scanPeriod;
	public static float androidScanPeriod {
		get {
			return m_instance._scanPeriod;
		}
		set {
			m_instance._scanPeriod = value;
#if UNITY_ANDROID
			initialized = false;
#endif
		}
	}

	[SerializeField]
	[Range(0, 300)]
	private float _betweenScanPeriod;
	public static float androidBetweenScanPeriod {
		get {
			return m_instance._betweenScanPeriod;
		}
		set {
			m_instance._betweenScanPeriod = value;
#if UNITY_ANDROID
			initialized = false;
#endif
		}
	}

#if UNITY_ANDROID
	private static AndroidJavaObject plugin;
#endif

	private static iBeaconReceiver m_instance;

	private static bool initialized = false;

	private static bool receiving = false;

	public const string MessageiBeaconOnly = "Bluetooth is deactivated through Control Center. Only iBeacons will work.";

	private void Awake() {
#if iBeaconDummy
		Debug.LogError("iBeaconDummy is still on! Please remove it from the Scripting Define Symbols.");
#endif
		if (m_instance != null && m_instance != this) {
#if UNITY_EDITOR
			DestroyImmediate(this);
#else
			Destroy(this);
#endif
			return;
		}
#if UNITY_EDITOR
		if (!gameObject.name.Equals(BluetoothState.NAME)) {
			var obj = GameObject.Find(BluetoothState.NAME);
			if (obj == null) {
				gameObject.name = BluetoothState.NAME;
			} else {
	#if !iBeaconDummy
				var component = obj.AddComponent<iBeaconReceiver>();
				component._regions = _regions;
				DestroyImmediate(this);
				return;
	#endif
			}
		}
#endif
		m_instance = this;
		initialized = false;
		receiving = false;
	}

	private void OnDestroy() {
		if (m_instance == this) {
			m_instance = null;
		}
	}

	private void OnApplicationQuit() {
		Stop();
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern bool InitReceiver(string regions, bool shouldLog);

	[DllImport("__Internal")]
	private static extern void StartIOSScan();

	[DllImport("__Internal")]
	private static extern void StopIOSScan();
#endif

	public static void Restart() {
		Stop();
		Scan();
	}

	[Obsolete("Init() is deprecated, please remove the usage or use Restart() instead.")]
	public static void Init() {
		InternInit(true);
	}

	[Obsolete("Init(bool) is deprecated, please remove the usage or use Restart() instead.")]
	public static void Init(bool shouldLog) {
		InternInit(shouldLog);
	}

	private static void InternInit(bool shouldLog) {
		if (initialized) {
			Scan();
			return;
		}
		if (m_instance == null) {
			m_instance = FindObjectOfType<iBeaconReceiver>();
			if (m_instance == null) {
				BluetoothState.Init();
				m_instance = GameObject.Find(BluetoothState.NAME).AddComponent<iBeaconReceiver>();
			}
		}
		Stop();
		var state = BluetoothState.GetBluetoothLEStatus();
		if (state == BluetoothLowEnergyState.IBEACON_ONLY) {
			if (regions.Any(region => region.beacon.type == BeaconType.iBeacon)) {
				Debug.LogWarning(MessageiBeaconOnly);
			} else {
				throw new iBeaconException(MessageiBeaconOnly);
			}
		} else if (state != BluetoothLowEnergyState.POWERED_ON) {
			BluetoothState.EnableBluetooth();
			if (BluetoothState.GetBluetoothLEStatus() != BluetoothLowEnergyState.POWERED_ON) {
				throw new iBeaconException("Bluetooth is off and could not be enabled.");
			}
		}
#if !UNITY_EDITOR
	#if UNITY_IOS
		if (!InitReceiver(iBeaconRegion.regionsToString(m_instance._regions), shouldLog)) {
			throw new iBeaconException("Receiver initialization failed.");
		}
	#elif UNITY_ANDROID
		GetPlugin().Call(
			"Init",
			iBeaconRegion.regionsToString(m_instance._regions),
			Mathf.FloorToInt(m_instance._detectionTimespan * 1000),
			Mathf.FloorToInt(m_instance._scanPeriod * 1000),
			Mathf.FloorToInt(m_instance._betweenScanPeriod * 1000),
			shouldLog);
	#endif
#endif
		initialized = true;
		Scan();
	}

	public static void Stop() {
		if (!receiving) {
			return;
		}
#if !UNITY_EDITOR
	#if UNITY_IOS
		StopIOSScan();
	#elif UNITY_ANDROID
		GetPlugin().Call("Stop");
	#endif
#endif
		receiving = false;
	}

	public static void Scan() {
		if (!initialized) {
			InternInit(true);
			return;
		}
		if (receiving) {
			return;
		}
#if !UNITY_EDITOR
	#if UNITY_IOS
		StartIOSScan();
	#elif UNITY_ANDROID
		GetPlugin().Call("Scan");
	#endif
#endif
		receiving = true;
	}

	[Obsolete("EnableBluetooth() is deprecated, please use BluetoothState.EnableBluetooth() instead.")]
	public static void EnableBluetooth() {
		BluetoothState.EnableBluetooth();
	}

#if UNITY_ANDROID
	private static AndroidJavaObject GetPlugin() {
		if (plugin == null) {
			plugin = new AndroidJavaObject("com.kaasa.ibeacon.BeaconService");
		}
		return plugin;
	}
#endif

	private static T[] GetJsonArray<T>(string json) {
		string newJson = "{\"array\":" + json + "}";
		JsonWrapper<T> wrapper = JsonUtility.FromJson<JsonWrapper<T>>(newJson);
		return wrapper.array;
	}

	[Serializable]
	private class JsonWrapper<T> {
		public T[] array = null;
	}

	private void RangeBeacons(string beacons) {
		if (BeaconRangeChangedEvent != null) {
			Beacon[] beaconArray = GetJsonArray<Beacon>(beacons);
			foreach (var b in beaconArray) {
				b.ResetLastSeen();
			}
			BeaconRangeChangedEvent(beaconArray);
		}
	}
}
