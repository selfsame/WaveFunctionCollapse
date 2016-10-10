using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TileData", menuName = "WaveFunctionCollapse/TileData", order = 1)]
public class TileData : ScriptableObject {
    
    public enum Symmetry {X, T, I, L, N};

    public GameObject prefab;
    public Symmetry symmetry;

}

[CreateAssetMenu(fileName = "NeighborData", menuName = "WaveFunctionCollapse/NeighborData", order = 1)]
public class NeighborData : ScriptableObject {
	public enum Direction {north, east, south, west};
    
	public TileData left;
	public Direction leftD;
	public TileData right;
	public Direction rightD;
}

// [CustomPropertyDrawer (typeof (NeighborData))]
// public class NeighborDrawer : PropertyDrawer {
	
// 	// Draw the property inside the given rect
// 	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
// 		// Using BeginProperty / EndProperty on the parent property means that
// 		// prefab override logic works on the entire property.
// 		EditorGUI.BeginProperty (position, label, property);
		
// 		// Draw label
// 		position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);
		
// 		// Don't make child fields be indented
// 		var indent = EditorGUI.indentLevel;
// 		EditorGUI.indentLevel = 0;
		
// 		// Calculate rects
// 		var leftRect = new Rect (position.x, position.y, 40, position.height);
// 		var leftDRect = new Rect (position.x+42, position.y, 10, position.height);
// 		var rightRect = new Rect (position.x+52, position.y, 40, position.height);
// 		var rightDRect = new Rect (position.x+94, position.y, 6, position.height);
		
// 		// Draw fields - passs GUIContent.none to each so they are drawn without labels
// 		// EditorGUI.PropertyField (leftRect, property.FindPropertyRelative ("left"), GUIContent.none);
// 		// EditorGUI.PropertyField (leftDRect, property.FindPropertyRelative ("leftD"), GUIContent.none);
// 		// EditorGUI.PropertyField (rightRect, property.FindPropertyRelative ("right"), GUIContent.none);
// 		// EditorGUI.PropertyField (rightDRect, property.FindPropertyRelative ("rightD"), GUIContent.none);
		
// 		// Set indent back to what it was
// 		EditorGUI.indentLevel = indent;
		
// 		EditorGUI.EndProperty ();

// 	}
// }