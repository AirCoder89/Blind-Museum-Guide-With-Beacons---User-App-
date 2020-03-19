using UnityEngine;
using System.Collections;
using System.Text;

[System.Serializable]
public class iBeaconRegion {
	public string regionName {
		get {
			return _regionName;
		}
	}

	public Beacon beacon {
		get {
			return _beacon;
		}
	}

	[SerializeField]
	private string _regionName;
	[SerializeField]
	private Beacon _beacon;

	public iBeaconRegion(string regionName, Beacon beacon) {
		_regionName = regionName;
		_beacon = beacon;
	}

	public static string regionsToString(iBeaconRegion[] regions) {
		var sb = new StringBuilder("[");
		foreach (var r in regions) {
			sb.Append(JsonUtility.ToJson(r));
			sb.Append(',');
		}
		sb.Remove(sb.Length - 1, 1);
		sb.Append(']');
		return sb.ToString();
	}
}
