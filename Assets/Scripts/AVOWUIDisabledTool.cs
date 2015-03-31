using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class AVOWUIDisabledTool :  AVOWUITool{
	
	
	// New attempt at encoding the state of the UI
	Vector3 				mouseWorldPos;
	GameObject 				cursorCube;
	
	const int		kLoadSaveVersion = 1;	
	
	public override void Startup(){
		cursorCube = InstantiateCursorCube();
		cursorCube.transform.parent = AVOWUI.singleton.transform;
		
		// Remove the metal material from the cube that we have
		RemoveMetal(cursorCube);
		uiZPos = AVOWUI.singleton.transform.position.z;
		
	}
	
	public override void OnDestroy(){
		GameObject.Destroy(cursorCube);
	}
	
	public override GameObject GetCursorCube(){
		return cursorCube;
	}
	
	public override void Serialise(BinaryWriter bw){
		bw.Write (mouseWorldPos);
		
		bw.Write (cursorCube != null);
		if (cursorCube != null){
			bw.Write (cursorCube.transform.localPosition);
			bw.Write (cursorCube.transform.localRotation);
			bw.Write (cursorCube.transform.localScale);
		}
		
	}
	
	public override void Deserialise(BinaryReader br){
		mouseWorldPos = br.ReadVector3();
		
		bool hasCursorCube = br.ReadBoolean();
		if (hasCursorCube && cursorCube == null){
			cursorCube = InstantiateCursorCube();
		}
		else if (!hasCursorCube && cursorCube != null){
			GameObject.Destroy(cursorCube);
			cursorCube = null;
		}
		
		if (cursorCube != null){
			cursorCube.transform.localPosition = br.ReadVector3 ();
			cursorCube.transform.localRotation = br.ReadQuaternion ();
			cursorCube.transform.localScale = br.ReadVector3 ();
		}
	}
	
	


	
	public override void RenderUpdate () {
		// Calc the mouse posiiton on world space
		Vector3 screenCentre = new Vector3(Screen.width * 0.75f, Screen.height * 0.5f, 0);
		Vector3 inputScreenPos = Vector3.Lerp(screenCentre, Input.mousePosition, AVOWConfig.singleton.cubeToCursor.GetValue());
		
		Vector3 mousePos = inputScreenPos;
		mousePos.z = 0;
		mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
		// Set the cursor cubes position
		mouseWorldPos.z = uiZPos;
			
		cursorCube.transform.position = mouseWorldPos;
		
	}
	
	protected override GameObject InstantiateCursorCube(){
		return AVOWUI.singleton.InstantiateGreyCursorCube();
	}
	
	
	
}