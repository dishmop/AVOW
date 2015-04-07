using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class AVOWUIDisabledTool :  AVOWUITool{
	
	
	// New attempt at encoding the state of the UI
	Vector3 				mouseWorldPos;
	GameObject 				cursorCube;
	
	const int		kLoadSaveVersion = 1;	
	
	
	bool valuesHaveChanged;
	float[] optValues = new float[100];
	
	void ResetOptValues(){
		
		int i = 0;
		
		optValues[i++] = Convert.ToSingle (mouseWorldPos[0]);
		optValues[i++] = Convert.ToSingle (mouseWorldPos[1]);
		optValues[i++] = Convert.ToSingle (mouseWorldPos[2]);
		
		optValues[i++] = Convert.ToSingle (cursorCube != null);
		if (cursorCube != null){
			optValues[i++] = Convert.ToSingle (cursorCube.transform.localPosition[0]);
			optValues[i++] = Convert.ToSingle (cursorCube.transform.localPosition[1]);
			optValues[i++] = Convert.ToSingle (cursorCube.transform.localPosition[2]);
			
			optValues[i++] = Convert.ToSingle (cursorCube.transform.localScale[0]);
			optValues[i++] = Convert.ToSingle (cursorCube.transform.localScale[1]);
			optValues[i++] = Convert.ToSingle (cursorCube.transform.localScale[2]);
			
		}
		
	}
	
	
	void TestIfValuesHaveChanged(){
		int i = 0;
		bool diff = false;
		
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (mouseWorldPos[0]));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (mouseWorldPos[1]));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (mouseWorldPos[2]));
		
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube != null));
		if (cursorCube != null){
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube.transform.localPosition[0]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube.transform.localPosition[1]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube.transform.localPosition[2]));
			
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube.transform.localScale[0]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube.transform.localScale[1]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube.transform.localScale[2]));
		}
		
		if (diff){
			valuesHaveChanged = true;
			ResetOptValues();
		}
		
	}
	
	
	public override void ResetOptFlags(){
		valuesHaveChanged = false;
	}
	
	
	
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
		bw.Write (kLoadSaveVersion);
		bw.Write (valuesHaveChanged);
		if (!valuesHaveChanged) return;
		
		bw.Write (mouseWorldPos);
		
		bw.Write (cursorCube != null);
		if (cursorCube != null){
			bw.Write (cursorCube.transform.localPosition);
			bw.Write (cursorCube.transform.localScale);
		}
		
	}
	
	public override void Deserialise(BinaryReader br){
		int version = br.ReadInt32 ();
		
		switch (version){
			case kLoadSaveVersion:{
				valuesHaveChanged = br.ReadBoolean();
				
				if (!valuesHaveChanged) break;
				
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
			//		cursorCube.transform.localRotation = br.ReadQuaternion ();
					cursorCube.transform.localScale = br.ReadVector3 ();
				}
				break;
			
			}
		}
		

	}
	
	
	public override void GameUpdate () {
		//		Debug.Log(Time.time + ": UICreateTool Update");
		TestIfValuesHaveChanged();
		
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