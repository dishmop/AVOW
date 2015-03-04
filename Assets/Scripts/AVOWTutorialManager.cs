﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWTutorialManager : MonoBehaviour {
	public static AVOWTutorialManager singleton = null;
	
	
	public GameObject greySphere;
	
	public GameObject backStory;
	
	public GameObject babyCubePrefab;
	
	GameObject babyCube = null;
	
	List<GameObject>	spheres = new List<GameObject>();
	
	List<GameObject>			parentSpheres = new List<GameObject>();
	Vector3[]	frustOuts = new Vector3[4];
	Vector3[]	frustNorms = new Vector3[4];
	
	float		spheresPerSec = 10;
	float 		sphereTime = 0;
	
	float 		timeForParentsToMeet = 15;
	
	float 		worldOfSpheres2Time = 0;
	float		worldOfSpheres2Duration = 10;
	float 		danceTime = 0;
	Vector3 	danceOffset = Vector3.zero;
	float 		zoomSpeed = 0;
	float 		zoomDist = 0;
	float 		zoomTargetFOV = 2f;
	
	float 		midAvoidProp = 0.25f;
	
	float inLove5Time = 0;
	
	// Dance backup stuff
	float backDesDistToOther;
	float backDesSpeed;
	float backAlignCoef;
	float backHomeCoef;
	float backSpeedMod;
	float backSpiralCoef;
	
	float love11Drag = 1;
	
	public enum State{
		kDebugJumpToDance,
		kDebugJumpToDance2,
		kDebugResetDance,
		kOff,
		kWaitForText,
		kStartup,
		kIntro0,
		kIntro1,
		kIntro2,
		kTheWorldOfSpheres0,
		kTheWorldOfSpheres1,
		kTheWorldOfSpheres2,
		kTheWorldOfSpheres3,
		kInLove0,
		kInLove1,
		kInLove2,
		kInLove3,
		kInLove4,
		kInLove5,
		kInLove6,
		kInLove7,
		kInLove8,
		kInLove9,
		kInLove10,
		kInLove11,
		
		kStop,
		kNumStates
	}
	public State state = State.kOff;
	
	Vector3[]		cameraStartPositions = new Vector3[(int)State.kNumStates];
	bool triggered = false;
	
	State waitNextState = State.kOff;
	
	SpringValue		lightIntensity = new SpringValue(0, SpringValue.Mode.kLinear, 0.1f);
	SpringValue 	cubeBrightness = new SpringValue(0, SpringValue.Mode.kAsymptotic, 20);
	Color			reflectionColor;
	
	public void StartTutorial(){
		state = State.kDebugJumpToDance2;
		state = State.kIntro2;
		//state = State.kTheWorldOfSpheres0;
		state = State.kStartup;
		
	}
	public void Trigger(){
		triggered = true;
	}
	
	public void StopTutorial(){
		state = State.kStop;
	}
	
	void SaveFlockConfig(){
		backDesDistToOther = AVOWConfig.singleton.flockDesDistToOther;
		backDesSpeed = AVOWConfig.singleton.flockDesSpeed;
		backAlignCoef = AVOWConfig.singleton.flockAlignCoef;
		backHomeCoef = AVOWConfig.singleton.flockHomeCoef;
		backSpeedMod = AVOWConfig.singleton.flockSpeedMod;
		backSpiralCoef = AVOWConfig.singleton.flockSpiralCoef;
		
	}
	
	void RestoreFlockConfigReal(){
		AVOWConfig.singleton.flockDesDistToOther = backDesDistToOther;
		AVOWConfig.singleton.flockDesSpeed = backDesSpeed;
		AVOWConfig.singleton.flockAlignCoef = backAlignCoef;
		AVOWConfig.singleton.flockHomeCoef = backHomeCoef;
		AVOWConfig.singleton.flockSpeedMod = backSpeedMod;
		AVOWConfig.singleton.flockSpiralCoef = backSpiralCoef;
	}
	
//	void RestorePostCreateConfig(){
//		AVOWConfig.singleton.flockDesDistToOther = 27f;
//		AVOWConfig.singleton.flockDesSpeed = 0.1f;
//		AVOWConfig.singleton.flockAlignCoef = 0.2f;
//		AVOWConfig.singleton.flockHomeCoef = 20f;
//		AVOWConfig.singleton.flockSpeedMod = 2.2f;
//		AVOWConfig.singleton.flockSpiralCoef = 0;
//	}
//	
	
	void RestoreFlockConfig(){
	
	
		AVOWConfig.singleton.flockDesDistToOther = 4f;
		AVOWConfig.singleton.flockDesSpeed = 4;
		AVOWConfig.singleton.flockAlignCoef = 0.2f;
		AVOWConfig.singleton.flockHomeCoef = 3f;
		AVOWConfig.singleton.flockSpeedMod = 2.2f;
		AVOWConfig.singleton.flockSpiralCoef = 0;
	}

	// Use this for initialization
	void Start () {
		string name = backStory.transform.FindChild("Intro").FindChild("CursorBlueCube").renderer.materials[0].name;
		reflectionColor = backStory.transform.FindChild("Intro").FindChild("CursorBlueCube").renderer.materials[0].GetColor ("_ReflectColor");
		
		// Set up camera posiitons
		cameraStartPositions[(int)State.kStartup] = new Vector3(0, 2.3f, 104.2f);
		cameraStartPositions[(int)State.kTheWorldOfSpheres0] = new Vector3(0, 0, 200);
		
	
	}
	
	void CreateCube(){
		if (babyCube != null) return;
		
		babyCube = GameObject.Instantiate(babyCubePrefab) as GameObject;
		babyCube.transform.parent = transform;
		babyCube.GetComponent<BabyBlueParent>().parent0 = parentSpheres[0];
		babyCube.GetComponent<BabyBlueParent>().parent1 = parentSpheres[1];
		GetComponent<AudioSource>().Play();
	}
	
	void WaitForTextToFinish(State nextState){
		AVOWTutorialText.singleton.AddTrigger();
		waitNextState = nextState;
		state = State.kWaitForText;
	}
	
	void WaitForParentToFinish(State nextState){
		waitNextState = nextState;
		state = State.kWaitForText;
	}	
	
	// Update is called once per frame
	void Update () {
		SteerSpheresAwayFromCentre();
		switch(state){
		
			case State.kDebugJumpToDance:{
				DebugJumpToDance();
				break;
			}
			case State.kDebugJumpToDance2:{
				DebugJumpToDance2();
				break;
			}
			case State.kDebugResetDance:{
				DebugResetDance();
				break;
			}
			case State.kWaitForText:{
				if (triggered){
					triggered = false;
					state = waitNextState;
				}
				break;
			}
			
			case State.kStartup:{
				SetCameraPos(state);
				state = State.kIntro0;
				break;
			}
			case State.kIntro0:{
				AVOWTutorialText.singleton.AddPause(5);
				AVOWTutorialText.singleton.SetSpeed(2);
				AVOWTutorialText.singleton.AddText("Hello?");
				AVOWTutorialText.singleton.AddPause(3);
				AVOWTutorialText.singleton.SetSpeed(4);
				AVOWTutorialText.singleton.AddText("Can you help me?");
				AVOWTutorialText.singleton.SetSpeedDefault();
				WaitForTextToFinish(State.kIntro1);
				break;
			}
			case State.kIntro1:{
				// Wait for text to finish
				BackStoryCamera.singleton.state = BackStoryCamera.State.kOrbitCube;
				BrightenScene();

				Debug.Log("Cube Intro animation");
				AVOWTutorialText.singleton.AddPause(8);
				AVOWTutorialText.singleton.AddText("I'm Cube.");
				AVOWTutorialText.singleton.AddPause(2);	
				AVOWTutorialText.singleton.AddTextNoLine("Maybe, ");
				AVOWTutorialText.singleton.AddPause(2);	
				AVOWTutorialText.singleton.AddText("if I tell you how I came to be here, you would help me get out?");
				AVOWTutorialText.singleton.AddPause(5);	
				AVOWTutorialText.singleton.AddText("In the beginning, there was nothing but spheres.");
				WaitForTextToFinish(State.kIntro2);
				break;
			}
			case State.kIntro2:{

				lightIntensity.Set (0);
				lightIntensity.SetSpeed(0.25f);
				backStory.transform.FindChild("Music").GetComponent<AudioSource>().Play();
				AVOWTutorialText.singleton.AddPause(3);	
				WaitForTextToFinish(State.kTheWorldOfSpheres0);
				break;
			}
			case State.kTheWorldOfSpheres0:{
				SetCameraPos(state);
				SetupFrustrumVectors();
			
				BackStoryCamera.singleton.state = BackStoryCamera.State.kStill;
				BackStoryCamera.singleton.transform.position = new Vector3(0, 0, 200);
				Random.seed = 3;
				CreateRandomSphere();
				AVOWTutorialText.singleton.AddPause(10);	
				
				AVOWTutorialText.singleton.AddTextNoLine("The spheres floated in the nothingness . . .");
				WaitForTextToFinish(State.kTheWorldOfSpheres1);
			
				break;
			}
			case State.kTheWorldOfSpheres1:{
				worldOfSpheres2Time = Time.time + worldOfSpheres2Duration;
				AVOWTutorialText.singleton.AddPause(4);	
				AVOWTutorialText.singleton.AddText(" and for eons, nothing happened");
			
				state = State.kTheWorldOfSpheres2;		
				break;
			}
			case State.kTheWorldOfSpheres2:{
				if (Time.time < worldOfSpheres2Time - worldOfSpheres2Duration / 2){
					CreateRandomSpheres();
				}
				if (Time.time > worldOfSpheres2Time){
					state = State.kTheWorldOfSpheres3;	
				}
				break;
			}
			case State.kTheWorldOfSpheres3:{
				WaitForTextToFinish(State.kInLove0);
				SpawnParents();
				AVOWTutorialText.singleton.AddPause(timeForParentsToMeet - 7);	
				AVOWTutorialText.singleton.AddText("Then, something happened");
			
				break;
			}
			case State.kInLove0:{
				AVOWTutorialText.singleton.AddPause(4);
				WaitForTextToFinish(State.kInLove1);
				break;
			}
			case State.kInLove1:{
				ParentsCourtship();
				AVOWTutorialText.singleton.AddPause(10);
				WaitForTextToFinish(State.kInLove2);
	
				
				break;
			}	
			case State.kInLove2:{

				CreateRandomSpheres();
				WaitForParentToFinish(State.kInLove3);
				parentSpheres[0].GetComponent<AVOWGreySphere>().enableTrigger = true;
				break;
			}	
			case State.kInLove3:{
				StartDancing();
				SaveFlockConfig();
				state = State.kInLove4;
				break;
			}	
			case State.kInLove4:{
				CreateRandomSpheres();
				UpdateDance();
				if (Time.fixedTime > danceTime + 10f){
					parentSpheres[0].GetComponent<AVOWGreySphere>().SetRegularBeats();
					parentSpheres[1].GetComponent<AVOWGreySphere>().SetRegularBeats();
				}
				if (triggered){
					state = State.kInLove5;
					AVOWConfig.singleton.flockAlignCoef = -1f;
					AVOWConfig.singleton.flockSpeedMod = 0;
					triggered = false;
					inLove5Time = Time.fixedTime + 7;
				}
				break;
			}
			case State.kInLove5:{
				UpdateDanceSpiral();
				if (Time.fixedTime > inLove5Time){
					// Test which way we are rotating and set the spiral force to that direction (to ensure we keep going).
					Vector3 fromHereToHome = parentSpheres[2].transform.position - parentSpheres[0].transform.position;
					Vector3 vel = parentSpheres[0].GetComponent<AVOWGreySphere>().vel;
					
					Vector3 crossRes = Vector3.Cross(fromHereToHome, vel);
					if (crossRes.z > 0)
						AVOWConfig.singleton.flockSpiralCoef = -1f;
					else
						AVOWConfig.singleton.flockSpiralCoef = 1f;
				
					foreach (GameObject go in parentSpheres){
						go.GetComponent<AVOWGreySphere>().heartRestartTrigger = true;
					}
	
					float linSpeed = 0.2f;
					AVOWConfig.singleton.flockDesDistToOther = Mathf.Min (AVOWConfig.singleton.flockDesDistToOther + linSpeed * 0.5f, 27f);
					AVOWConfig.singleton.flockDesSpeed = Mathf.Min (AVOWConfig.singleton.flockDesSpeed + linSpeed * 0.25f, 10f);
					AVOWConfig.singleton.flockHomeCoef = Mathf.Min (AVOWConfig.singleton.flockHomeCoef + linSpeed * 1f, 50f);
					if (AVOWConfig.singleton.flockHomeCoef == 50f && AVOWConfig.singleton.flockDesDistToOther == 27f){
						state = State.kInLove6;
					}
				}
				break;
			}	
			case State.kInLove6:{
				UpdateDanceSpiral();
				float speed = 0.02f;
				AVOWConfig.singleton.flockDesSpeed = Mathf.Max (AVOWConfig.singleton.flockDesSpeed - speed, 0.1f);
				//AVOWConfig.singleton.flockDesSpeed = Mathf.Lerp (AVOWConfig.singleton.flockDesSpeed, 0.1f, speed);
				if (AVOWConfig.singleton.flockDesSpeed < 2f){
					CreateCube();
					foreach (GameObject go in parentSpheres){
						go.GetComponent<AVOWGreySphere>().SetCube(babyCube);
					}
					zoomSpeed = 0.0f;
					zoomDist = BackStoryCamera.singleton.GetComponent<Camera>().fieldOfView;
				
					state = State.kInLove7;
				}

				break;
			
			}	
			case State.kInLove7:{
				UpdateDanceSpiral();
				float speed = 0.01f;
				AVOWConfig.singleton.flockDesSpeed = Mathf.Max (AVOWConfig.singleton.flockDesSpeed - speed, 0.1f);
				
				
				float zoomAccn = 0.001f;
				if (BackStoryCamera.singleton.GetComponent<Camera>().fieldOfView  > zoomTargetFOV + 0.5f * (zoomDist - zoomTargetFOV)){
					zoomSpeed += zoomAccn;
				}
				else{
					zoomSpeed -= zoomAccn;
				}
				
				BackStoryCamera.singleton.GetComponent<Camera>().fieldOfView = Mathf.Max (BackStoryCamera.singleton.GetComponent<Camera>().fieldOfView - zoomSpeed, zoomTargetFOV);

				if (AVOWConfig.singleton.flockDesSpeed <= 0.1f && zoomSpeed < 0){
					babyCube.GetComponent<BabyBlueParent>().StartGrowing();
					parentSpheres[0].GetComponent<AVOWGreySphere>().ActivateSilentBeat();
					parentSpheres[1].GetComponent<AVOWGreySphere>().ActivateSilentBeat();
					zoomSpeed = 0;
					state = State.kInLove8;
				}
				
				break;
			}
			case State.kInLove8:{
				if (!babyCube.GetComponent<BabyBlueParent>().IsGrown()) break;
				float zoomAccn = 0.0003f;
				if (BackStoryCamera.singleton.GetComponent<Camera>().fieldOfView  < zoomTargetFOV + 0.5f * (zoomDist - zoomTargetFOV)){
					zoomSpeed += zoomAccn;
				}
				else{
					zoomSpeed -= zoomAccn;
				}
				BackStoryCamera.singleton.GetComponent<Camera>().fieldOfView = Mathf.Min (BackStoryCamera.singleton.GetComponent<Camera>().fieldOfView + zoomSpeed, 35);

				float linSpeed = 0.1f;
				AVOWConfig.singleton.flockHomeCoef = Mathf.Max (AVOWConfig.singleton.flockHomeCoef - linSpeed * 1f, 20f);
				if (zoomSpeed < 0 ){
					state = State.kInLove9;
					babyCube.GetComponent<BabyBlueParent>().DestroyLightening();
				
				}
				
				break;
			}
			case State.kInLove9:{
				float linSpeed = 0.1f;
				AVOWConfig.singleton.flockHomeCoef = Mathf.Max (AVOWConfig.singleton.flockHomeCoef - linSpeed * 1f, 20f);
				babyCube.GetComponent<BabyBlueParent>().rotSpeed = Mathf.Max (babyCube.GetComponent<BabyBlueParent>().rotSpeed - 0.01f, 0);
				if (AVOWConfig.singleton.flockHomeCoef == 20 && babyCube.GetComponent<BabyBlueParent>().rotSpeed == 0){
					AVOWTutorialText.singleton.AddPause(4);
					AVOWTutorialText.singleton.AddTextNoLine("I was alive. . .");
					AVOWTutorialText.singleton.AddPause(6);
					AVOWTutorialText.singleton.AddText (" and everything was perfect");
					WaitForTextToFinish(State.kInLove10);
				}
				
				break;
			}
			case State.kInLove10:{
				RestoreFlockConfig();
				StartDanceThreesome();
				break;
			}
			case State.kInLove11:{
				float timeToReach = 5;
				love11Drag = Mathf.Max(love11Drag - 1/(timeToReach*60), 0f);
				babyCube.GetComponent<DanceThreesome>().SetDrag(love11Drag);
				parentSpheres[0].GetComponent<DanceThreesome>().SetDrag(love11Drag);
				parentSpheres[1].GetComponent<DanceThreesome>().SetDrag(love11Drag);
				
				babyCube.GetComponent<BabyBlueParent>().rotSpeed = Mathf.Lerp (3, 0, love11Drag);
				babyCube.GetComponent<BabyBlueParent>().rotSpeed2 = Mathf.Lerp (2, 0, love11Drag);
				UpdateDance();
				break;
			}
			case State.kStop:{
				AVOWTutorialText.singleton.ClearText();
				state = State.kOff;		
				break;
			}
		}	
	
		ManageObjects();
	
	}
	
	void StartDanceThreesome(){
		Debug.Log ("StartDanceThreesome:");
		foreach (GameObject go in parentSpheres){
			Debug.Log (go.name);
			Debug.Log (go.transform.position.ToString());
			Debug.Log (go.GetComponent<AVOWGreySphere>().vel.ToString());
			Debug.Log  ("");
			
		}
		Debug.Log (babyCube.name);
		Debug.Log (babyCube.transform.position.ToString());
		Debug.Log (babyCube.GetComponent<DanceThreesome>().GetVelocity().ToString());
		Debug.Log  ("");
		
		parentSpheres[0].GetComponent<DanceThreesome>().ForceVelocity(parentSpheres[0].GetComponent<AVOWGreySphere>().vel);
		parentSpheres[1].GetComponent<DanceThreesome>().ForceVelocity(parentSpheres[1].GetComponent<AVOWGreySphere>().vel);
		
		
		babyCube.GetComponent<DanceThreesome>().SetDrag(love11Drag);
		parentSpheres[0].GetComponent<DanceThreesome>().SetDrag(love11Drag);
		parentSpheres[1].GetComponent<DanceThreesome>().SetDrag(love11Drag);
		
		
		danceTime = Time.fixedTime;
		danceOffset = parentSpheres[2].transform.position;
		danceOffset.z = 0;
		danceOffset.y += 2f; 
	
		// Get the normal behaviours turned off (or at least in a kind of dormant hibernation)
		babyCube.GetComponent<BabyBlueParent>().isActive = false;
//		babyCube.GetComponent<BabyBlueParent>().rotSpeed = 3;
//		babyCube.GetComponent<BabyBlueParent>().rotSpeed2 = 2;
		parentSpheres[0].GetComponent<AVOWGreySphere>().StartDanceThreesome();
		parentSpheres[1].GetComponent<AVOWGreySphere>().StartDanceThreesome();

		babyCube.GetComponent<DanceThreesome>().Start(parentSpheres[0], parentSpheres[1], parentSpheres[2]);
		parentSpheres[0].GetComponent<DanceThreesome>().Start(parentSpheres[1], babyCube, parentSpheres[2]);
		parentSpheres[1].GetComponent<DanceThreesome>().Start(parentSpheres[0], babyCube, parentSpheres[2]);
		state = State.kInLove11;
		
	}
	
	void StartDancing(){
		// Make the right hand sphere
		Vector3 spawnPos = new Vector3(0, 0, 210);
		GameObject massObj = GameObject.Instantiate(greySphere, spawnPos, Quaternion.identity) as GameObject;
		massObj.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
		massObj.transform.parent = backStory.transform.FindChild("WorldOfSpheres");
		massObj.renderer.material.SetColor("_RimColour", Color.white);
		parentSpheres.Add(massObj);
		massObj.name = "MassObj";
		danceTime = Time.fixedTime;
		
		parentSpheres[0].GetComponent<AVOWGreySphere>().StartDancing(parentSpheres[2]);
		parentSpheres[1].GetComponent<AVOWGreySphere>().StartDancing(parentSpheres[2]);
		
		
		Debug.Log ("ParentSpheres:");
		foreach (GameObject go in parentSpheres){
			Debug.Log (go.name);
			Debug.Log (go.transform.position.ToString());
			Debug.Log (go.GetComponent<AVOWGreySphere>().vel.ToString());
			Debug.Log  ("");
			
		}
		
	}
//		
//	void UpdateDance(){
//		foreach (GameObject go in parentSpheres){
//			AVOWGreySphere sphere = go.GetComponent<AVOWGreySphere>();
//			sphere.accn = Vector3.zero;
//			foreach (GameObject otherGO in parentSpheres){
//				if (otherGO != go){
//					Vector3 fromHereToThere = otherGO.transform.position - go.transform.position;
//					float distSq = fromHereToThere.sqrMagnitude;
//					Vector3 forceDir = Vector3.Normalize(fromHereToThere);
//					Vector3 force = forceDir * 0.2f/distSq;
//					sphere.accn += force;
//				
//				}
//			}
//		}
//	}
//	

	void UpdateDance(){
		float speed = 0.7f;
		parentSpheres[2].transform.position = danceOffset + new Vector3(2 * Mathf.Sin (speed*(Time.fixedTime - danceTime)), Mathf.Sin (2 * speed*(Time.fixedTime - danceTime)), 210);
		
		
		// Get the camrera following the,
		Vector3 lookPos = 0.5f * (parentSpheres[0].transform.position +  parentSpheres[1].transform.position);
		BackStoryCamera.singleton.GetComponent<BackStoryCamera>().SetLoveLook(lookPos);
		
		
	}
	
	
	void UpdateDanceSpiral(){
		//parentSpheres[2].transform.position = new Vector3(2 * Mathf.Sin (Time.fixedTime - danceTime), Mathf.Sin (2 * (Time.fixedTime - danceTime)), 210);
		
		
		// Get the camrera following the,
		Vector3 lookPos = 0.5f * (parentSpheres[0].transform.position +  parentSpheres[1].transform.position);
		BackStoryCamera.singleton.GetComponent<BackStoryCamera>().SetLoveLook(lookPos);
		
		
	}
	
	void ParentsCourtship(){
	
		foreach (GameObject go in parentSpheres){
			go.GetComponent<AVOWGreySphere>().StartCourtship();
		}
	}
	
	void DebugJumpToDance2(){
		SetCameraPos(State.kTheWorldOfSpheres0);
		SetupFrustrumVectors();
		SaveFlockConfig();
		
		SpawnParents ();
		ParentsCourtship();
		parentSpheres[0].GetComponent<AVOWGreySphere>().FixedUpdate();
		parentSpheres[0].transform.position = new Vector3(-0.7f, -0.1f, 208.9f);
		parentSpheres[0].GetComponent<AVOWGreySphere>().vel = new Vector3(0.2f, -0.3f, 0.0f);
		
		parentSpheres[1].GetComponent<AVOWGreySphere>().FixedUpdate();
		parentSpheres[1].transform.position = new Vector3(3.2f, 2.1f, 208.5f);
		parentSpheres[1].GetComponent<AVOWGreySphere>().vel = new Vector3(-0.2f, 0.3f, 0.0f);
		StartDancing();
		
		CreateCube();
		
		babyCube.GetComponent<BabyBlueParent>().sizeMul = 1;
		babyCube.GetComponent<BabyBlueParent>().CreateLightening();
		babyCube.GetComponent<BabyBlueParent>().DestroyLightening();
		babyCube.GetComponent<BabyBlueParent>().rotSpeed = 0;
		babyCube.GetComponent<BabyBlueParent>().Update();
		StartDanceThreesome();
		RestoreFlockConfig();
		
		//state = State.kInLove4;
		
	}
	
	void DebugJumpToDance(){
		SetCameraPos(State.kTheWorldOfSpheres0);
		SetupFrustrumVectors();
		SaveFlockConfig();
	
		SpawnParents ();
		ParentsCourtship();
		parentSpheres[0].GetComponent<AVOWGreySphere>().FixedUpdate();
		parentSpheres[0].transform.position = new Vector3(-1f, -0.2f, 208.93f);
		parentSpheres[0].GetComponent<AVOWGreySphere>().vel = new Vector3(-1.3f, -0.1f, -0.1f);
		
		parentSpheres[1].GetComponent<AVOWGreySphere>().FixedUpdate();
		parentSpheres[1].transform.position = new Vector3(1.5f, -0.1f, 208.5f);
		parentSpheres[1].GetComponent<AVOWGreySphere>().vel = new Vector3(1.7f, 0.8f, 0.1f);
		StartDancing();
		
//		parentSpheres[1].GetComponent<AVOWGreySphere>().DoLightening();
//		parentSpheres[0].GetComponent<AVOWGreySphere>().DoLightening();
		// debug
		//CreateCube();
		state = State.kInLove4;
	}
	
	
	void DebugResetDance(){
		parentSpheres[0].transform.position = new Vector3(-1.8f, -0.1f, 209.9f);
		parentSpheres[0].GetComponent<AVOWGreySphere>().vel = new Vector3(-2.8f, -0.6f, -0.4f);
		
		parentSpheres[1].transform.position = new Vector3(-2.2f, -0.4f, 210.4f);
		parentSpheres[1].GetComponent<AVOWGreySphere>().vel = new Vector3(3.2f, 1.2f, 0.4f);
		
		
		state = State.kInLove4;
	}
	
	
	void SpawnParents(){
		Camera camera = BackStoryCamera.singleton.GetComponent<Camera>();
		
		Vector3 spawnDir0 = frustOuts[1] + new Vector3(0, 0.1f, 0);
		Vector3 spawnDir1 = frustOuts[3] - new Vector3(0, 0.1f, 0);
		Vector3 goOutsideViewDir0 = -frustNorms[1];
		Vector3 goOutsideViewDir1 = -frustNorms[3];
		float dist = 5f;
		float scale0 = 1;
		float scale1 = 0.8f;
		
		// Make the right hand sphere
		Vector3 spawnPos0 = camera.transform.position + spawnDir0 * dist + goOutsideViewDir0 * scale0;
		GameObject newSphere0 = GameObject.Instantiate(greySphere, spawnPos0, Quaternion.identity) as GameObject;
		newSphere0.transform.localScale = new Vector3(scale0, scale0, scale0);
		newSphere0.transform.parent = backStory.transform.FindChild("WorldOfSpheres");
		newSphere0.GetComponent<AVOWGreySphere>().beatColor = new Color(1f, 0f, 0.1f);
		newSphere0.name = "Parent0";
		// debug
		
		parentSpheres.Add(newSphere0);
		
		// Make the left hand sphere
		Vector3 spawnPos1 =  camera.transform.position + spawnDir1 * dist + goOutsideViewDir1 * scale1;
		GameObject newSphere1 = GameObject.Instantiate(greySphere, spawnPos1, Quaternion.identity) as GameObject;
		newSphere1.transform.localScale = new Vector3(scale1, scale1, scale1);
		newSphere1.transform.parent = backStory.transform.FindChild("WorldOfSpheres");
		newSphere1.GetComponent<AVOWGreySphere>().beatColor = new Color(1f, 0.1f, 0f);
		parentSpheres.Add(newSphere1);
		newSphere1.name = "Parent1";
		
		// Calc the velcoties required
		float xDist = Mathf.Abs(spawnPos0.x - spawnPos1.x);	
		float desSpeed = 0.5f * xDist / timeForParentsToMeet;
		newSphere1.GetComponent<AVOWGreySphere>().vel = desSpeed * frustNorms[3];
		newSphere0.GetComponent<AVOWGreySphere>().vel = desSpeed * frustNorms[1];

		newSphere1.GetComponent<AVOWGreySphere>().SetExpectant(newSphere0, backStory.transform.FindChild("WorldOfSpheres").FindChild("Light").gameObject);
		
	}
	
	void CreateRandomSpheres(){
		if (Time.fixedTime > sphereTime){
			RemoveDepartedSpheres();
			sphereTime = Time.fixedTime + 1f/spheresPerSec;
			if (spheres.Count < 100){
				CreateRandomSphere();
				
			}
		}
	}
	
	void RemoveDepartedSpheres(){
		Camera camera = BackStoryCamera.singleton.GetComponent<Camera>();
		// Get the frustrum planes for the camera
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
	
		List<GameObject> toRemove = new List<GameObject>();
		foreach (GameObject sphereGO in spheres){
			bool isInside = GeometryUtility.TestPlanesAABB(planes, sphereGO.renderer.bounds);
			if (!isInside){
				// Test if we are heading  towards the viewing axis
				Vector3 sphereToViewAxis = camera.transform.position - sphereGO.transform.position;
				sphereToViewAxis.z = 0;
				Vector3 projectedVal = sphereGO.GetComponent<AVOWGreySphere>().vel;
				projectedVal.z = 0;
				float dotResult = Vector3.Dot(sphereToViewAxis, projectedVal);
				// If travelling away from the centre, then remove it
				if (dotResult < 0){
					toRemove.Add (sphereGO);
				}
			}
		}
		foreach(GameObject sphereGO in toRemove){
			GameObject.Destroy(sphereGO);
			spheres.Remove(sphereGO);
		}
		
	}
	
	void SetupFrustrumVectors(){
		Camera camera = BackStoryCamera.singleton.GetComponent<Camera>();
		float aspect = camera.aspect;
		float vHalfFOVRad = Mathf.Deg2Rad * 0.5f * camera.fieldOfView;
		
		frustOuts[0] = new Vector3(0, Mathf.Tan(vHalfFOVRad), 1);
		frustOuts[1] = new Vector3(aspect * Mathf.Tan(vHalfFOVRad), 0, 1);
		frustOuts[2] = new Vector3(0, -Mathf.Tan(vHalfFOVRad), 1);
		frustOuts[3] = new Vector3(-aspect * Mathf.Tan(vHalfFOVRad), 0, 1);
		
		frustNorms[0] = Vector3.Cross(frustOuts[0], new Vector3(-1, 0, 0));
		frustNorms[1] = Vector3.Cross(frustOuts[1], new Vector3(0, 1, 0));
		frustNorms[2] = Vector3.Cross(frustOuts[2], new Vector3(1, 0, 0));
		frustNorms[3] = Vector3.Cross(frustOuts[3], new Vector3(0, -1, 0));
		foreach (Vector3 vec in frustNorms){
			vec.Normalize();
		}
	}
	
	void CreateRandomSphere(){
		// Decide which edge to be on (or rather just off)
		Camera camera = BackStoryCamera.singleton.GetComponent<Camera>();
		GameObject newSphere = GameObject.Instantiate(greySphere) as GameObject;
		newSphere.transform.parent = backStory.transform.FindChild("WorldOfSpheres");
		
		bool calcVel = true;
		while (calcVel){
			int edge = Random.Range(0, 4);
			float dist = Random.Range (camera.nearClipPlane + 3f, camera.farClipPlane);
			int tangIndex = (edge + 1) % 4;
			Vector3 spawnDir = frustOuts[edge] + Random.Range (-1f, 1f) * new Vector3(frustOuts[tangIndex].x, frustOuts[tangIndex].y, 0);
			Vector3 goOutsideViewDir = -frustNorms[edge];
		
			float scale = Random.Range (0.2f, 2.0f);
			newSphere.transform.position = camera.transform.position + spawnDir * dist + goOutsideViewDir * scale;
			newSphere.transform.localScale = new Vector3(scale, scale, scale);
			newSphere.GetComponent<AVOWGreySphere>().vel = Random.Range (0.3f, 0.5f) * frustNorms[edge] + Random.Range (-0.5f, 0.5f) * new Vector3(0, 0, 1) + Random.Range (-0.5f, 0.5f) * frustNorms[tangIndex];
			calcVel = IsHeadingForCentre(newSphere);
		}
		
		spheres.Add(newSphere);
	}
	
	bool IsHeadingForCentre(GameObject sphere){
		
		// Create two points in space
		Vector3 pos0 = sphere.transform.position;
		Vector3 pos1 = pos0 + sphere.GetComponent<AVOWGreySphere>().vel;
		
		// Convert to screenspace pos
		Vector3 screenPos0 = Camera.main.WorldToScreenPoint(pos0);
		Vector3 screenPos1 = Camera.main.WorldToScreenPoint(pos1);
		float distFromCentre = DistFromLineToPoint2D(screenPos0, screenPos1, new Vector3(Camera.main.pixelWidth * 0.5f, Camera.main.pixelHeight * 0.5f, 0f));
	//	return false;
		return (distFromCentre < Camera.main.pixelHeight * midAvoidProp);
	}
	
	void SteerSpheresAwayFromCentre(){
		foreach (GameObject go in spheres){
			if (IsHeadingForCentre(go)){
				// Get vector from line of site of camera towards sphere
				
				// First transform sphere into camera coods
				Vector3 fromSightToSphere = Camera.main.transform.InverseTransformPoint(go.transform.position);
				
				fromSightToSphere.z = 0.0025f;
				
				fromSightToSphere.Normalize();
				Vector3 localPushDir =  Camera.main.transform.InverseTransformDirection(fromSightToSphere);
				go.GetComponent<AVOWGreySphere>().vel += localPushDir * 0.02f;
				
				
			}
		}
	}
	
	// Line defined by two points, p1 and p2, and point is q0
	float DistFromLineToPoint2D(Vector3 p1, Vector3 p2, Vector3 q0){
		float numerator = Mathf.Abs ((p2.y - p1.y) * q0.x - (p2.x - p1.x) * q0.y + p2.x * p1.y - p2.y * p1.x);
		float denominator = Mathf.Sqrt((p2.y - p1.y) * (p2.y - p1.y) + (p2.x - p1.x) * (p2.x - p1.x));
		return numerator / denominator;
	
	}
	


	
	void SetCameraPos(State thisState){
		BackStoryCamera.singleton.transform.position = cameraStartPositions[(int)thisState];
		BackStoryCamera.singleton.transform.rotation = Quaternion.identity;
		
	}
	
	public void TriggerLight(){
		cubeBrightness.Force (lightIntensity.GetValue());
		cubeBrightness.Set (0);
	
	}
	
	void BrightenScene(){
		lightIntensity.Set (1);
	}
	
	
	void ManageObjects(){
		lightIntensity.Update ();
		cubeBrightness.Update ();
		backStory.transform.FindChild("Intro").FindChild("CursorBlueCube").renderer.materials[1].SetFloat("_Intensity", cubeBrightness.GetValue());
		backStory.transform.FindChild("Intro").FindChild("CursorBlueCube").renderer.materials[0].SetColor ("_ReflectColor", reflectionColor * lightIntensity.GetValue());
		
		backStory.transform.FindChild("Intro").FindChild("Point light").GetComponent<Light>().intensity = 2 * lightIntensity.GetValue();
		
		
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
}
