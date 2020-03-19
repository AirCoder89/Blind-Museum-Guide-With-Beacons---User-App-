using System;
using System.Linq;
using UnityEngine;

public class Telemetry {
	public readonly float voltage;
	public readonly float temperature;
	public readonly int frameCounter;
	public readonly TimeSpan uptime;
	public readonly byte[] encryptedData;
	public readonly byte[] salt;
	public readonly byte[] integrityCheck;
	public readonly bool encrypted;

	public Telemetry(string hex) {
		var raw = Enumerable.Range(0, hex.Length / 2).Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16)).ToArray();

		if (raw[0] != 0x20) {
			throw new Exception("Unknown frame type");
		}

		switch (raw[1]) {
		case 0x00:
			encrypted = false;
			short tmp = (short)(raw[2] * 0x100 + raw[3]);
			voltage = tmp / 1000f;
			tmp = (short)(raw[4] * 0x100 + raw[5]);
			temperature = tmp / 256f;
			frameCounter = raw[6] * 0x1000000 + raw[7] * 0x10000 + raw[8] * 0x100 + raw[9];
			uptime = TimeSpan.FromMilliseconds((raw[10] * 0x1000000 + raw[11] * 0x10000 + raw[12] * 0x100 + raw[13]) * 100);
			break;
		case 0x01:
			encrypted = true;
			encryptedData = new byte[12];
			salt = new byte[2];
			integrityCheck = new byte[2];
			Array.Copy(raw, 2, encryptedData, 0, 12);
			Array.Copy(raw, 14, salt, 0, 2);
			Array.Copy(raw, 16, integrityCheck, 0, 2);
			temperature = -128;
			break;
		default:
			throw new Exception("Unknown TLM version");
		}
	}
}
