using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(LipsyncController))]
public class LipsyncControllerEditor : Editor
{
	private bool isEditing;
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Toggle Advanced Options"))
		{
			isEditing = !isEditing;
		}
		if (isEditing)
			base.OnInspectorGUI();
	}
}
