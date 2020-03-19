using UnityEngine;
using UnityEditor;
using System.Text;

[CustomPropertyDrawer(typeof(BeaconPropertyAttribute))]
public class BeaconPropertyDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		BeaconPropertyAttribute beaconAttribute = (BeaconPropertyAttribute)attribute;
		BeaconType type;
		bool enabled = GetBeaconPropertyAttributeResult(beaconAttribute, property, out type);

		if (type == BeaconType.EddystoneUID && label.text.Equals("UUID")) {
			label.text = "Namespace";
		} else if (type == BeaconType.EddystoneURL && label.text.Equals("UUID")) {
			label.text = "URL";
		}

		bool wasEnabled = GUI.enabled;
		GUI.enabled = enabled;
		if (enabled) {
			EditorGUI.PropertyField(position, property, label, true);
		}

		GUI.enabled = wasEnabled;
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		BeaconPropertyAttribute beaconAttribute = (BeaconPropertyAttribute)attribute;
		BeaconType type;
		bool enabled = GetBeaconPropertyAttributeResult(beaconAttribute, property, out type);

		if (enabled) {
			return EditorGUI.GetPropertyHeight(property, label);
		} else {
			return -EditorGUIUtility.standardVerticalSpacing;
		}
	}

	private bool GetBeaconPropertyAttributeResult(BeaconPropertyAttribute beaconAttribute, SerializedProperty property, out BeaconType type) {
		var sb = new StringBuilder(property.propertyPath);
		var i = property.propertyPath.LastIndexOf('.') + 1;
		sb.Remove(i, property.propertyPath.Length - i);
		sb.Append("_type");
		type = (BeaconType)property.serializedObject.FindProperty(sb.ToString()).enumValueIndex;
		return ArrayUtility.Contains<BeaconType>(beaconAttribute.Types, type) ^ beaconAttribute.Exclude;
	}
}
