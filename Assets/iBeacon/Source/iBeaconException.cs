using System;

public class iBeaconException : Exception {
	public iBeaconException() {
	}


	public iBeaconException(string message) : base(message) {
	}


	public iBeaconException(string message, Exception innerException) : base(message, innerException) {
	}
}
