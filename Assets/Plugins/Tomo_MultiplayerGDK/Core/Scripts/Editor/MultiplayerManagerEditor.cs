#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TomoClub.Core;

namespace TomoClub.EditorScripts
{
	[CustomEditor(typeof(MultiplayerManager))]
	public class MultiplayerManagerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			#region Multiplayer Manager Custom GUILayout
			GUILayout.Space(5);
			GUI.skin.label.fontSize = 17;
			GUI.skin.label.alignment = TextAnchor.UpperCenter;
			GUILayout.Label("MULTIPLAYER MANAGER");
			GUILayout.Space(2);
			GUI.skin.label.fontSize = 13;
			GUI.skin.label.alignment = TextAnchor.UpperCenter;
			GUILayout.Label("Manages the multiplayer experience for the game.");
			GUILayout.Space(10);
			DrawDefaultInspector();
			#endregion
		}

	}
} 
#endif
