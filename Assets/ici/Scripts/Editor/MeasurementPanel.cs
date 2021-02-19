// Measure distance between 2 GameObjects pivot points

using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

namespace Utils
{
	public class MeasurementPanel : EditorWindow
	{
		[MenuItem("Window/Measurement")]
		public static void ShowWindow()
		{
			//Show existing window instance. If one doesn't exist, make one.
			EditorWindow sizeWnd = EditorWindow.GetWindow(typeof(MeasurementPanel));
			sizeWnd.title = "Measurement";
		}

		void OnGUI()
		{
			GameObject[] selectedGameObjects = Selection.gameObjects;

			if (selectedGameObjects.Length != 2)
				return;

			Vector3 p1 = selectedGameObjects[0].transform.position;
			Vector3 p2 = selectedGameObjects[1].transform.position;

			Vector3 e = p2 - p1;
			float d = e.magnitude;

			string caption = "Distance between " + selectedGameObjects[0].name + " and " + selectedGameObjects[1].name  + ": " + d;
			EditorGUILayout.LabelField(caption);

			string decomposition = "(x: " + e.x + " | y: " + e.y + " | z: " + e.z + ")";
			EditorGUILayout.LabelField(decomposition);
		}

		void OnInspectorUpdate()
		{
			Repaint();
		}
	}
}
