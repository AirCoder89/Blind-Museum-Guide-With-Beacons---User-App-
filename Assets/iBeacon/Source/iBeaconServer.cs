using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Text;

[RequireComponent(typeof(BluetoothState))]
public class iBeaconServer : MonoBehaviour {
#if iBeaconDummy
	public string m_uuid;
	public string m_identifier;
	public int m_major;
	public int m_minor;
#endif

#if UNITY_ANDROID
	private static AndroidJavaObject plugin;
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern bool InitBeaconServer(string region, bool shouldLog);

	[DllImport("__Internal")]
	private static extern void Transmit(bool transmit);
#endif

    public enum AndroidPowerLevel {
        UltraLow,
        Low,
        Medium,
        High,
    }

    public enum AndroidMode {
        LowPower,
        Balanced,
        LowLatency,
    }

    [SerializeField]
    private iBeaconRegion _region;
    [SerializeField]
    private int _txPower;
    [Header("Android")]
    [SerializeField]
    private AndroidPowerLevel _powerLevel;
    [SerializeField]
    private AndroidMode _mode;

    public static iBeaconRegion region {
        get {
            return m_instance._region;
        }
        set {
            m_instance._region = value;
            initialized = false;
        }
    }

    public static int txPower {
        get {
            return m_instance._txPower;
        }
        set {
            m_instance._txPower = value;
            initialized = false;
        }
    }

    public static AndroidPowerLevel androidPowerLevel {
        get {
            return m_instance._powerLevel;
        }
        set {
            m_instance._powerLevel = value;
            initialized = false;
        }
    }

    public static AndroidMode androidMode {
        get {
            return m_instance._mode;
        }
        set {
            m_instance._mode = value;
            initialized = false;
        }
    }

    private static iBeaconServer m_instance;

    private static bool initialized = false;

    private static bool transmitting = false;

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
                var component = obj.AddComponent<iBeaconServer>();
                component._region = _region;
                component._txPower = _txPower;
                DestroyImmediate(this);
                return;
#endif
            }
        }
#endif
        m_instance = this;
        initialized = false;
        transmitting = false;
    }

    private void OnDestroy() {
        if (m_instance == this) {
            m_instance = null;
        }
    }

#if UNITY_ANDROID
	private static AndroidJavaObject GetPlugin() {
		if (plugin == null) {
			plugin = new AndroidJavaObject("com.kaasa.ibeacon.BeaconServer");
		}
		return plugin;
	}
#endif

    public static bool checkTransmissionSupported() {
        return checkTransmissionSupported(true);
    }

    public static bool checkTransmissionSupported(bool shouldLog) {
#if UNITY_EDITOR
        return false;
#elif UNITY_IOS
		return true;
#elif UNITY_ANDROID
		return GetPlugin().Call<bool>("checkTransmissionSupported", shouldLog);
#endif
    }

    public static void Restart() {
        StopTransmit();
        Transmit();
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
            Transmit();
            return;
        }
        if (m_instance == null) {
            m_instance = FindObjectOfType<iBeaconServer>();
            if (m_instance == null) {
                BluetoothState.Init();
                m_instance = GameObject.Find(BluetoothState.NAME).AddComponent<iBeaconServer>();
            }
        }
        StopTransmit();
        if (BluetoothState.GetBluetoothLEStatus() != BluetoothLowEnergyState.POWERED_ON) {
            BluetoothState.EnableBluetooth();
            if (BluetoothState.GetBluetoothLEStatus() != BluetoothLowEnergyState.POWERED_ON) {
                throw new iBeaconException("Bluetooth is off and could not be enabled.");
            }
        }
        if (!checkTransmissionSupported(shouldLog)) {
            throw new iBeaconException("This device does not support transmitting as a beacon.");
        }
#if !UNITY_EDITOR
		var sb = new StringBuilder(JsonUtility.ToJson(m_instance._region));
		sb.Insert(sb.Length - 1, ",\"_txPower\":");
		sb.Insert(sb.Length - 1, m_instance._txPower);
#if UNITY_IOS
		if (!InitBeaconServer(sb.ToString(), shouldLog)) {
			throw new iBeaconException("Server initialization failed.");
		}
#elif UNITY_ANDROID
		sb.Insert(sb.Length - 1, ",\"_powerLevel\":");
		sb.Insert(sb.Length - 1, (int)m_instance._powerLevel);
		sb.Insert(sb.Length - 1, ",\"_mode\":");
		sb.Insert(sb.Length - 1, (int)m_instance._mode);
		if (!GetPlugin().Call<bool>("Init", sb.ToString(), shouldLog)) {
			throw new iBeaconException("Server initialization failed.");
		}
#endif
#endif
        initialized = true;
        Transmit();
    }

    public static void Transmit() {
        if (!initialized) {
            InternInit(true);
            return;
        }
        if (transmitting) {
            return;
        }
#if !UNITY_EDITOR
#if UNITY_IOS
		Transmit(true);
#elif UNITY_ANDROID
		GetPlugin().Call("transmit", true);
#endif
#endif
        transmitting = true;
    }

    public static void StopTransmit() {
        if (!transmitting) {
            return;
        }
#if !UNITY_EDITOR
#if UNITY_IOS
		Transmit(false);
#elif UNITY_ANDROID
		GetPlugin().Call("transmit", false);
#endif
#endif
        transmitting = false;
    }

}
