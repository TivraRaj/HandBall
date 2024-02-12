#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TomoClub.Core;

namespace TomoClub.EditorScripts
{
	[CustomEditor(typeof(LocalPlayer))]
	public class LocalTomoPlayerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			#region LocalTomoPlayer Custom GUILayout
			GUILayout.Space(5);
			GUI.skin.label.fontSize = 17;
			GUI.skin.label.alignment = TextAnchor.UpperCenter;
			GUILayout.Label("LOCAL TOMO PLAYER");
			GUILayout.Space(2);
			GUI.skin.label.fontSize = 13;
			GUI.skin.label.alignment = TextAnchor.UpperCenter;
			GUILayout.Label("This client's local player data");
			GUILayout.Space(10);
			DrawDefaultInspector();
			#endregion
		}

	}
} 
#endif
