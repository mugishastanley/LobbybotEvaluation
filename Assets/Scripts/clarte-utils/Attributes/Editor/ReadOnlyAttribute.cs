using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace CLARTE.Attributes.Editor
{
	/// <summary>
	/// ReadOnly attribute: make a field in the inspector read only, i.e. disabled/greyed out
	/// Source: https://forum.unity.com/threads/read-only-fields.68976/
	/// </summary>
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = true;
		}
	}

	/// <summary>
	/// ReadOnlyOnPlay attribute: make a field in the inspector read only when playing, i.e. disabled/greyed out
	/// Source: https://forum.unity.com/threads/read-only-fields.68976/
	/// </summary>
	//[CustomPropertyDrawer(typeof(ReadOnlyOnPlayAttribute))]
	public class ReadOnlyOnPlayDrawer : ReadOnlyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (Application.isPlaying)
			{
				base.OnGUI(position, property, label);
			}
			else
			{
				EditorGUI.PropertyField(position, property, label, true);
			}
		}
	}
}