using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Dungeonizer))] 	
public class DungeonizerEditor : Editor {
	
	public override void OnInspectorGUI () {
		//Called whenever the inspector is drawn for this object.,
		Dungeonizer realscript = (Dungeonizer)target;
		if(realscript.minRoomSize > realscript.maxRoomSize){
			EditorGUILayout.HelpBox("Please make sure your minumum room size isn't bigger than maximum room size.", MessageType.Error);
		}
		EditorGUILayout.HelpBox("Follow us, Lets Make Dungeons Together: https://www.twitch.tv/dungeonizer", MessageType.Info);
		

		DrawDefaultInspector();
		//This draws the default screen.  You don't need this if you want
		//to start from scratch, but I use this when I'm just adding a button or
		//some small addition and don't feel like recreating the whole inspector.
		
		
 
		if(GUILayout.Button("Create Now")) {
			//add everthing the button would do.
			realscript.ClearOldDungeon(true);
			realscript.Generate();
			
		}
	}
}