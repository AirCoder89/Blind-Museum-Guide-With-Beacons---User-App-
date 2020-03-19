using UnityEngine;
using System;
using System.Collections;

[AttributeUsage(AttributeTargets.Field)]
public class BeaconPropertyAttribute : PropertyAttribute {
	public string Field;
	public BeaconType[] Types;
	public bool Exclude;

	public BeaconPropertyAttribute(string field, params BeaconType[] types)
		: this(false, field, types) {
	}

	public BeaconPropertyAttribute(bool exclude, string field, params BeaconType[] types) {
		Field = field;
		Types = types;
		Exclude = exclude;
	}
}
