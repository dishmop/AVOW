using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AVOWBackStoryCutscene : MonoBehaviour {
	public static AVOWBackStoryCutscene singleton = null;
	
	
	public GameObject greySphere;
	
	public GameObject backStory;
	
	public GameObject babyCubePrefab;
	public GameObject babyCubePrefabMid;
	public GameObject babyCubePrefabAfter;
	public GameObject backStoryQuit;
	
	
	public float danceFollow3Dist = 10;
	
	public GameObject prisonSphere1;

	
	GameObject babyCube = null;
	
	GameObject[] squareSpheres;
	
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
	
	float 		inLove5Time = 0;
	
	float 		inLove11Time = 0;
	
	int			maxSphereCount = 100;
	
	float 		envy2Time = 0;
	
	float 		 zoomInTime;
	float electrify5Time;
	
	// Dance backup stuff
	float backDesDistToOther;
	float backDesSpeed;
	float backAlignCoef;
	float backHomeCoef;
	float backSpeedMod;
	float backSpiralCoef;
	
	bool outro3Flag;
	
	float love11Drag = 1;
	float electTime;
	
	
	int largeSphereCount = -1;
	int idealLargeSphereCount = 300;
	Vector3[] sphereTargets;
	
	
	public enum State{
		kDebugJumpToDance,
		kDebugJumpToDance2,
		kDebugResetDance,
		kDebugJumpToOutro,
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
		kEnvy0,
		kEnvy1,
		kEnvy2,
		kEnvy3,
		kEnvy3a,
		kEnvy4,
		kEnvy5,
		kZoomIn0,
		kZoomIn1,
		kZoomIn2,
		kElectrify0,
		kElectrify0A,
		kElectrify1,
		kElectrify2,
		kElectrify3,
		kElectrify4,
		kElectrify5,
		kElectrify6,
		kLand0,
		kLand1,
		kOutro0,
		kOutro1,
		kOutro2,
		kOutro3,
		kOutro4,
		kOutro5,
		kOutro6,
		kStop,
		kNumStates
	}
	
	public State state = State.kOff;
	
	enum SteeringState{
		kAvoidCentre,
		kSpheresFromUs,
		kMakeSphere
	}
	
	SteeringState steeringState = SteeringState.kAvoidCentre;
	
	public float steerSphereRadius = 15;
	public Vector3 steerSphereCentre = new Vector3(0, 0, 210);
		
	
	Vector3[]		cameraStartPositions = new Vector3[(int)State.kNumStates];
	bool triggered = false;
	
	State waitNextState = State.kOff;
	
	SpringValue		lightIntensityIntro = new SpringValue(0, SpringValue.Mode.kLinear, 0.1f);
	SpringValue		lightIntensityOutro = new SpringValue(0, SpringValue.Mode.kLinear, 0.1f);
	SpringValue 	cubeBrightness = new SpringValue(0, SpringValue.Mode.kAsymptotic, 20);
	Color			reflectionColor;
	Color			rustColor;
	
	bool 			triggerOutroLighting = false;
	
	public void StartBackStory(){
	
		
		largeSphereCount = GenerateSpherePoints(idealLargeSphereCount, steerSphereRadius);
		
		//state = State.kDebugJumpToDance;
		//state = State.kDebugJumpToOutro;
		//state = State.kIntro2;
		//state = State.kTheWorldOfSpheres0;
		state = State.kStartup;
		
	}
	
	public void TriggerZoomIn(){
		state = State.kZoomIn0;
	}
	
	public void Trigger(){
		triggered = true;
	}
	
	public void StopBackStory(){
		state = State.kStop;
	}
	
	public bool IsRunning(){
		return state != State.kOff;
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
		AVOWConfig.singleton.flockDesSpeed = 2;
		AVOWConfig.singleton.flockAlignCoef = 0.2f;
		AVOWConfig.singleton.flockHomeCoef = 1f;
		AVOWConfig.singleton.flockSpeedMod = 1f;
		AVOWConfig.singleton.flockSpiralCoef = 0;
	}
	
	void RestoreFlockConfigOld(){
		
		
		AVOWConfig.singleton.flockDesDistToOther = 4f;
		AVOWConfig.singleton.flockDesSpeed = 4;
		AVOWConfig.singleton.flockAlignCoef = 0.2f;
		AVOWConfig.singleton.flockHomeCoef = 3f;
		AVOWConfig.singleton.flockSpeedMod = 2.2f;
		AVOWConfig.singleton.flockSpiralCoef = 0;
	}

	// Use this for initialization
	public void Initialise () {
//		string name = backStory.transform.FindChild("Intro").FindChild("CursorBlueCube").GetComponent<Renderer>().materials[0].name;
//		reflectionColor = backStory.transform.FindChild("Intro").FindChild("CursorBlueCube").GetComponent<Renderer>().materials[0].GetColor ("_ReflectColor");
//		rustColor = backStory.transform.FindChild("Intro").FindChild("CursorBlueCube").GetComponent<Renderer>().materials[1].GetColor ("_TintColor");
		
		// Set up camera posiitons
		cameraStartPositions[(int)State.kStartup] = new Vector3(0, 2.3f, 104.2f);
		cameraStartPositions[(int)State.kTheWorldOfSpheres0] = new Vector3(0, 0, 200);
		cameraStartPositions[(int)State.kOutro0] = new Vector3(0, 2.3f, 104.2f);
		
	
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
	
	void DestroySpheres(){
		foreach (GameObject go in spheres){
			GameObject.Destroy(go);
		}
		spheres.Clear();
		
		foreach (GameObject go in parentSpheres){
			GameObject.Destroy(go);
		}
		parentSpheres.Clear();
		
		if (babyCube != null){
			if (babyCube.GetComponent<DanceThreesome>() != null){
				babyCube.GetComponent<DanceThreesome>().isActive = false;
			}
			
		}
	}
	
	// Update is called once per frame
	public void RenderUpdate () {
		backStoryQuit.SetActive(state != State.kOff);
		if (steeringState == SteeringState.kAvoidCentre){
			SteerSpheresAwayFromCentre();
		}
		else if (steeringState == SteeringState.kSpheresFromUs){
			SteerSpheresFromUs();
		}
		else if (steeringState == SteeringState.kMakeSphere){
			SteerSpheresToSphere();
		}
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
			case State.kDebugJumpToOutro:{
				DebugJumpToOutro();
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
				triggered = false;
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

				//Debug.Log("Cube Intro animation");
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

				lightIntensityIntro.Set (0);
				lightIntensityIntro.SetSpeed(0.25f);
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
				UnityEngine.Random.seed = 3;
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
				AVOWTutorialText.singleton.AddPause(6);
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
				if (Time.fixedTime > danceTime + 7f){
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
						AVOWConfig.singleton.flockSpiralCoef = -2f;
					else
						AVOWConfig.singleton.flockSpiralCoef = 2f;
				
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
				BackStoryCamera.singleton.GetComponent<Camera>().fieldOfView = Mathf.Min (BackStoryCamera.singleton.GetComponent<Camera>().fieldOfView + zoomSpeed, 40);

				float linSpeed = 0.1f;
				AVOWConfig.singleton.flockHomeCoef = Mathf.Max (AVOWConfig.singleton.flockHomeCoef - linSpeed * 1f, 15f);
				if (zoomSpeed < 0 ){
					state = State.kInLove9;
					babyCube.GetComponent<BabyBlueParent>().DestroyLightening();
				
				}
				
				break;
			}
			case State.kInLove9:{
				CreateRandomSpheres();
				float linSpeed = 0.1f;
				AVOWConfig.singleton.flockHomeCoef = Mathf.Max (AVOWConfig.singleton.flockHomeCoef - linSpeed * 1f, 15f);
				babyCube.GetComponent<BabyBlueParent>().rotSpeed = Mathf.Max (babyCube.GetComponent<BabyBlueParent>().rotSpeed - 0.01f, 0);
				if (AVOWConfig.singleton.flockHomeCoef == 15 && babyCube.GetComponent<BabyBlueParent>().rotSpeed == 0){
					AVOWTutorialText.singleton.AddPause(3);
					AVOWTutorialText.singleton.AddText("I was alive.");
					AVOWTutorialText.singleton.AddPause(1);
					maxSphereCount = 200;
					WaitForTextToFinish(State.kInLove10);
				}
				
				break;
			}
			case State.kInLove10:{
				RestoreFlockConfig();
				StartDanceThreesome();
				inLove11Time = Time.fixedTime + 20f;
				break;
			}
			case State.kInLove11:{
				CreateRandomSpheres();
				
				float timeToReach = 5;
				love11Drag = Mathf.Max(love11Drag - 1/(timeToReach*60), 0f);
				babyCube.GetComponent<DanceThreesome>().SetDrag(love11Drag);
				parentSpheres[0].GetComponent<DanceThreesome>().SetDrag(love11Drag);
				parentSpheres[1].GetComponent<DanceThreesome>().SetDrag(love11Drag);
				
				babyCube.GetComponent<BabyBlueParent>().rotSpeed = Mathf.Lerp (3, 0, love11Drag);
				babyCube.GetComponent<BabyBlueParent>().rotSpeed2 = Mathf.Lerp (2, 0, love11Drag);
				UpdateDanceFollow();
				if (Time.fixedTime > inLove11Time){
					state = State.kEnvy0;
				}
				break;
			}
			case State.kEnvy0:{
				AVOWTutorialText.singleton.AddText("But the other spheres were afraid of us.");
				AVOWTutorialText.singleton.AddPause(6);
				UpdateDanceFollow2();

				state = State.kEnvy1;
				
				break;
			}
			case State.kEnvy1:{
				spheresPerSec = 200;
				maxSphereCount = largeSphereCount;
				CreateRandomSpheres(false);
				
				UpdateDanceFollow2();
				
				if (spheres.Count == largeSphereCount){
					steeringState = SteeringState.kMakeSphere;
					foreach (GameObject go in spheres){
						AVOWGreySphere sphere = go.GetComponent<AVOWGreySphere>();
						sphere.beatColor = new Color(0, 0.5f, 0);
						sphere.SetRegularBeats();
						sphere.ActivateSilentBeat();
						sphere.bpm = 15;
					}
					state = State.kEnvy2;
					envy2Time = Time.fixedTime + 6f;
				}
				else{
					steeringState = SteeringState.kSpheresFromUs;
				}
			
				break;
			}
			case State.kEnvy2:{				
				UpdateDanceFollow2();
				if (Time.fixedTime > envy2Time){
					babyCube.GetComponent<BabyBlueParent>().SetToActive();
					babyCube.GetComponent<BabyBlueParent>().updateSize = false;
					state = State.kEnvy3;
				
				}

				break;
			}	
			case State.kEnvy3:{		
				UpdateDanceFollow3();		
				float lerpVal = babyCube.GetComponent<BabyBlueParent>().lerpVal * 3;
				AVOWConfig.singleton.flockDesSpeed = Mathf.Max (AVOWConfig.singleton.flockDesSpeed - 0.01f, 0f);
				AVOWConfig.singleton.flockSpeedMod = Mathf.Max (AVOWConfig.singleton.flockSpeedMod - 0.01f, 0f);
				babyCube.GetComponent<DanceThreesome>().SetDrag(lerpVal	);
				parentSpheres[0].GetComponent<DanceThreesome>().SetDrag( lerpVal	);
				parentSpheres[1].GetComponent<DanceThreesome>().SetDrag( lerpVal	);
				
				parentSpheres[0].GetComponent<DanceThreesome>().ForceVelocity(parentSpheres[0].GetComponent<DanceThreesome>().GetVelocity() * 0.98f);
				parentSpheres[1].GetComponent<DanceThreesome>().ForceVelocity(parentSpheres[1].GetComponent<DanceThreesome>().GetVelocity() * 0.98f);
				babyCube.GetComponent<DanceThreesome>().ForceVelocity(babyCube.GetComponent<DanceThreesome>().GetVelocity() * 0.98f);
				babyCube.GetComponent<BabyBlueParent>().rotSpeed = Mathf.Max (babyCube.GetComponent<BabyBlueParent>().rotSpeed - 0.01f, -0.1f);
				babyCube.GetComponent<BabyBlueParent>().rotSpeed2 = Mathf.Max (babyCube.GetComponent<BabyBlueParent>().rotSpeed2 - 0.01f, -0.1f);
				
				parentSpheres[2].transform.position = Vector3.Lerp (parentSpheres[2].transform.position , steerSphereCentre, 0.01f);
				BackStoryCamera.singleton.GetComponent<Camera>().fieldOfView = Mathf.Min (BackStoryCamera.singleton.GetComponent<Camera>().fieldOfView + zoomSpeed, 45);
				if ((babyCube.transform.position - BackStoryCamera.singleton.transform.position).magnitude > danceFollow3Dist * 0.8){
					state = State.kEnvy3a;
					AVOWTutorialText.singleton.AddText("The spheres that made me were cast out.");
					envy2Time = Time.fixedTime + 7f;
				
				}
			
				break;
			}	
			case State.kEnvy3a:{				
				UpdateDanceFollow3();
				if (Time.fixedTime > envy2Time){
					state = State.kEnvy4;
				}
				break;
			}
			case State.kEnvy4:{				
				UpdateDanceFollow3();
				
				// Choose two spheres
				int numSpheres = 10;
				int numToIgnore = 40;
				GameObject[] nearSpheres = GetNearestSpheresToCamera(numSpheres, numToIgnore);
				
				// NOw select a random two
				GameObject bullet1 = nearSpheres[UnityEngine.Random.Range(0, numSpheres/2)];
				GameObject bullet2 = nearSpheres[UnityEngine.Random.Range(numSpheres/2, numSpheres)];
				
				bullet1.GetComponent<AVOWGreySphere>().ShootAt(parentSpheres[0]);
				bullet2.GetComponent<AVOWGreySphere>().ShootAt(parentSpheres[1]);
				state = State.kEnvy5;
				break;
			}	
			case State.kEnvy5:{		
				UpdateDanceFollow3();

				break;
			}	
			case State.kZoomIn0:{		
				UpdateDanceFollow3();
				zoomInTime = Time.fixedTime + 0f;
				state = State.kZoomIn1;
				break;
			}
			case State.kZoomIn1:{		
				UpdateDanceFollow3();
				if (Time.fixedTime > zoomInTime){
					state = State.kZoomIn2;
					squareSpheres = FindSquareSpheres();
					foreach (GameObject go in squareSpheres){
						go.GetComponent<AVOWGreySphere>().ReadyToElectrify();
					}
					
				}
			
			
			
				break;
			}
			case State.kZoomIn2:{		
				UpdateDanceFollow5();
				prisonSphere1.SetActive(true);
				Color thisCol = prisonSphere1.GetComponent<Renderer>().material.GetColor ("_Color0");
				thisCol.a = Mathf.Min (thisCol.a + 0.002f, 1f);
				prisonSphere1.GetComponent<Renderer>().material.SetColor ("_Color0", thisCol);
				prisonSphere1.GetComponent<Renderer>().material.SetFloat ("_MidPoint", BackStoryCamera.singleton.transform.position.y);
				if (thisCol.a == 1f){
					state = State.kElectrify0;
				}
			    break;			
			}	
			case State.kElectrify0:{
				UpdateDanceFollow5();
				babyCube.GetComponent<BabyBlueParent>().Electrify(squareSpheres);
			
				state = State.kElectrify0A;
				
				electTime = Time.fixedTime + 0.5f;
				break;
			}	
			case State.kElectrify0A:{
				UpdateDanceFollow5();
				if (Time.fixedTime > electTime){
					state = State.kElectrify1;
				}
				break;
			}					
			case State.kElectrify1:{
				UpdateDanceFollow6();
				danceFollow3Dist = 1;
//				GameObject newCubeParent = GameObject.Instantiate(babyCubePrefabMid) as GameObject;
//				GameObject newCube = newCubeParent.transform.FindChild("BabyBlueCube").gameObject;
//				GameObject oldCube = babyCube.transform.FindChild("BabyBlueCube").gameObject;
//				newCube.transform.parent = oldCube.transform.parent;
//				newCube.transform.position = oldCube.transform.position;
//				newCube.transform.localScale = oldCube.transform.localScale;
//				newCube.transform.rotation = oldCube.transform.rotation;
//				newCube.GetComponent<BabyBlueCube>().ReinitialiseRotation();
//				GameObject.Destroy(oldCube);
//				GameObject.Destroy(newCubeParent);
				
				state = State.kElectrify2;
				//babyCube.transform.FindChild("BabyBlueCube").renderer.materials
				break;
			}	
			case State.kElectrify2:{
				UpdateDanceFollow6();
				if ((BackStoryCamera.singleton.transform.position - babyCube.transform.position).magnitude < 6f){
					state = State.kElectrify3;
				}
				break;
			}
			case State.kElectrify3:{
				UpdateDanceFollow6();
				float intensity = babyCube.transform.FindChild("BabyBlueCube").GetComponent<Renderer>().materials[0].GetFloat("_Intensity");
				babyCube.transform.FindChild("BabyBlueCube").GetComponent<Renderer>().materials[0].SetFloat("_Intensity", intensity + 0.04f);
				if (intensity > 10f){
					GameObject newCubeParent = GameObject.Instantiate(babyCubePrefabAfter) as GameObject;
					GameObject newCube = newCubeParent.transform.FindChild("BabyBlueCube").gameObject;
					GameObject oldCube = babyCube.transform.FindChild("BabyBlueCube").gameObject;
					newCube.transform.parent = oldCube.transform.parent;
					newCube.transform.position = oldCube.transform.position;
					newCube.transform.localScale =oldCube.transform.localScale;
					newCube.transform.rotation =oldCube.transform.rotation;
					newCube.GetComponent<BabyBlueCube>().ReinitialiseRotation();
					newCube.GetComponent<Renderer>().materials[1].SetFloat("_Intensity", 10);
				
					GameObject.Destroy(oldCube);
					GameObject.Destroy(newCubeParent);
					babyCube.GetComponent<BabyBlueParent>().StopSquareLightening();
//					babyCube.GetComponent<BabyBlueParent>().rotSpeed = 3;
//					babyCube.GetComponent<BabyBlueParent>().rotSpeed2 = 1;
					state = State.kElectrify4;
				}
				break;
			}	
			case State.kElectrify4:{
				UpdateDanceFollow6();
				float intensity = babyCube.transform.FindChild("BabyBlueCube").GetComponent<Renderer>().materials[1].GetFloat("_Intensity");
				babyCube.transform.FindChild("BabyBlueCube").GetComponent<Renderer>().materials[1].SetFloat("_Intensity", intensity - 0.05f);
				if (intensity <= 0.5f){
					state = State.kElectrify5;
					electrify5Time = Time.fixedTime + 8;
					AVOWTutorialText.singleton.AddText("I was shackled in gold.");
					lightIntensityOutro.Set (1);
				}
				
				
				break;
			}		
			case State.kElectrify5:{
				UpdateDanceFollow6();
				if (Time.fixedTime > electrify5Time){
					state = State.kElectrify6;
				}
				break;
			}
			case State.kElectrify6:{
				UpdateDanceFollow7();
				babyCube.GetComponent<BabyBlueParent>().SetFall();
				Vector3 pos = prisonSphere1.transform.position;
				pos.y = babyCube.transform.position.y* 0.95f;
				prisonSphere1.transform.position = pos;
				float intensity = babyCube.transform.FindChild("BabyBlueCube").GetComponent<Renderer>().materials[1].GetFloat("_Intensity");
				if (intensity > 0.01f){
					babyCube.transform.FindChild("BabyBlueCube").GetComponent<Renderer>().materials[1].SetFloat("_Intensity", intensity - 0.05f);
				}
				
				if (babyCube.transform.position.y - 0.5f * babyCube.transform.localScale.x < -200){
					triggerOutroLighting = true;
				}
				if (babyCube.transform.position.y - 0.5f * babyCube.transform.localScale.x < -250){
					Vector3 newPos = babyCube.transform.position;
					newPos.y = -250 + 0.5f * babyCube.transform.localScale.x ;
					babyCube.GetComponent<BabyBlueParent>().vel = Vector3.zero;
					babyCube.transform.position  = newPos;
					electrify5Time = Time.fixedTime + 7;
					prisonSphere1.SetActive(false);
					
					DestroySpheres();
					state = State.kLand0;
				}
				break;
			}	
			case State.kLand0:{
				danceFollow3Dist = 1.5f;
				UpdateDanceFollow7();
				babyCube.GetComponent<BabyBlueParent>().Land();
		//		Debug.Log ("Ground = " + babyCube.transform.position.y);
				if (Time.fixedTime > electrify5Time){
					state = State.kLand1;
				}
				break;
			}
			case State.kLand1:{
				UpdateDanceFollow7();
				lightIntensityOutro.Set (0);
				if (lightIntensityOutro.IsAtTarget()){
					SetCameraPos(State.kOutro0);
					BackStoryCamera.singleton.StartOrbit();
					lightIntensityIntro.Set (1);
					state = State.kOutro0;
				}
				break;
			}
			case State.kOutro0:{	
				if (lightIntensityIntro.IsAtTarget()){
					AVOWTutorialText.singleton.AddText("I have been here ever since.");
					AVOWTutorialText.singleton.AddPause(3);
					AVOWTutorialText.singleton.AddText("I am Cube - an idea...and an idea cannot move physical things on its own. Maybe you can help me?.");
					AVOWTutorialText.singleton.AddPause(3);
				
					WaitForTextToFinish(State.kOutro5);
				}
				break;
			}
			case State.kOutro1:{
				BackStoryCamera.singleton.StartControl();
				state = State.kOutro2;
				break;
			}
			case State.kOutro2:{
				if (BackStoryCamera.singleton.state == BackStoryCamera.State.kControl1){
					AVOWTutorialText.singleton.AddText("You can try if you want....");
					state = State.kOutro3;
					electrify5Time = Time.fixedTime + 20;
				}
				break;
			}
			case State.kOutro3:{
				if (BackStoryCamera.singleton.ctrlLerpVal > 0.05f){
					AVOWTutorialText.singleton.AddText("I think I felt some movement there. Do it again.");
					state = State.kOutro4;
				}
				if (Time.fixedTime > electrify5Time && !outro3Flag){
					outro3Flag = true;
					AVOWTutorialText.singleton.AddText("Moving your mouse might nudge me.");
				}
				
				break;
			}
			case State.kOutro4:{
				if (BackStoryCamera.singleton.ctrlLerpVal > 0.7f){
					BackStoryCamera.singleton.ctrlLerpVal = 1;
					AVOWTutorialText.singleton.AddText("That feels nice. However, these walls are all around me. Even if you can move me, I don't know how I can get out of here.");
					AVOWTutorialText.singleton.AddPause(4);
					WaitForTextToFinish(State.kOutro5);
				}
				
				break;
			}		
			case State.kOutro5:{
				lightIntensityIntro.Set (0);
				AVOWBackStoryCutscene.singleton.backStory.transform.FindChild("Music").GetComponent<AudioSource>().volume = 0.3f * lightIntensityIntro.GetValue();
				
				if (lightIntensityIntro.IsAtTarget()){
					state = State.kOutro6;		
					
				}
			
				break;
			}	
			case State.kOutro6:{
				AVOWGameModes.singleton.GoToMain();
				state = State.kOff;		
				
				break;
			}
			case State.kStop:{
				AVOWTutorialText.singleton.ClearText();
				DestroySpheres();
				GameObject.Destroy(babyCube);
				babyCube = null;
				state = State.kOff;		
				break;
			}
		}	
	
		ManageObjects();
	
	}
	
	void  DebugJumpToOutro(){
		SetCameraPos(State.kOutro0);
		BackStoryCamera.singleton.StartOrbit();
		lightIntensityIntro.Set (1);
		state = State.kOutro0;
	}
	
	GameObject[] FindSquareSpheres(){
		Vector3[] corners = new Vector3[8];
		GameObject[] squareSpheres = new GameObject[8];
		float[] dist = new float[8];
		
		corners[0] = steerSphereCentre + steerSphereRadius * new Vector3(1, 1, 1);
		corners[1] = steerSphereCentre + steerSphereRadius * new Vector3(1, 1, -1);
		corners[2] = steerSphereCentre + steerSphereRadius * new Vector3(1, -1, 1);
		corners[3] = steerSphereCentre + steerSphereRadius * new Vector3(1, -1, -1);
		corners[4] = steerSphereCentre + steerSphereRadius * new Vector3(-1, 1, 1);
		corners[5] = steerSphereCentre + steerSphereRadius * new Vector3(-1, 1, -1);
		corners[6] = steerSphereCentre + steerSphereRadius * new Vector3(-1, -1, 1);
		corners[7] = steerSphereCentre + steerSphereRadius * new Vector3(-1, -1, -1);
		
		for (int i = 0; i < 8; ++i){
			dist[i] = 500;
		}
		
		foreach(GameObject go in spheres){
			Vector3 spherePos = go.transform.position;
			for (int i = 0; i < 8; ++i){
				float thisDist = (spherePos - corners[i]).sqrMagnitude;
				if (thisDist < dist[i]){
					dist[i] = thisDist;
					squareSpheres[i] = go;
				}
			}
		
		}
		return squareSpheres;
		
		
	}
	
	GameObject[] GetNearestSpheresToCamera(int numSpheres, int numToIgnore){
		
		Vector3 camPos = BackStoryCamera.singleton.transform.position;
		
		foreach (GameObject go in spheres){
			float dist = (camPos - go.transform.position).sqrMagnitude;
			go.GetComponent<AVOWGreySphere>().distSqToCam = dist;
		}

		GameObject[] tempSpheres = new GameObject[spheres.Count];
		spheres.CopyTo(tempSpheres);
		
		Array.Sort(tempSpheres, (obj1, obj2) => obj1.GetComponent<AVOWGreySphere>().distSqToCam.CompareTo(obj2.GetComponent<AVOWGreySphere>().distSqToCam));
		
		GameObject[] results = new GameObject[numSpheres];
		Array.Copy(tempSpheres, numToIgnore, results, 0, numSpheres);
		
		
		return results;
		
	}
	
	void StartDanceThreesome(){
	//	Debug.Log ("StartDanceThreesome:");
		foreach (GameObject go in parentSpheres){
		//	Debug.Log (go.name);
		//	Debug.Log (go.transform.position.ToString());
		//	Debug.Log (go.GetComponent<AVOWGreySphere>().vel.ToString());
			//Debug.Log  ("");
			
		}
		//Debug.Log (babyCube.name);
	//	Debug.Log (babyCube.transform.position.ToString());
	//	Debug.Log (babyCube.GetComponent<DanceThreesome>().GetVelocity().ToString());
	//	Debug.Log  ("");
		
		parentSpheres[0].GetComponent<DanceThreesome>().ForceVelocity(parentSpheres[0].GetComponent<AVOWGreySphere>().vel + new Vector3(0, 0, 1));
		parentSpheres[1].GetComponent<DanceThreesome>().ForceVelocity(parentSpheres[1].GetComponent<AVOWGreySphere>().vel + new Vector3(0, 0, 1));
		babyCube.GetComponent<DanceThreesome>().ForceVelocity(new Vector3(0, 0, 1));
		
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
		massObj.GetComponent<Renderer>().material.SetColor("_RimColour", Color.white);
		parentSpheres.Add(massObj);
		massObj.name = "MassObj";
		danceTime = Time.fixedTime;
		
		parentSpheres[0].GetComponent<AVOWGreySphere>().StartDancing(parentSpheres[2]);
		parentSpheres[1].GetComponent<AVOWGreySphere>().StartDancing(parentSpheres[2]);
		
		
		Debug.Log ("ParentSpheres:");
		foreach (GameObject go in parentSpheres){
			//Debug.Log (go.name);
		//	Debug.Log (go.transform.position.ToString());
		//	Debug.Log (go.GetComponent<AVOWGreySphere>().vel.ToString());
		//	Debug.Log  ("");
			
		}
		
	}
	
	Vector3 CalcSpherePos(float t, float p, float r){
		return r * new Vector3(Mathf.Sin(t)*Mathf.Cos (p), Mathf.Sin (t) * Mathf.Sin (p), Mathf.Cos (t));
		
	}
	
	int GenerateSpherePoints(int N, float r){
		sphereTargets = new Vector3[N+10];
		
		int index= 0;
		float a = 4 * Mathf.PI / N;
		float d = Mathf.Sqrt(a);
		float mt = Mathf.RoundToInt(Mathf.PI/d);
		float dt = Mathf.PI / mt;
		float dp = a / dt;
		for (int m = 0; m < mt; ++m){
			float t = Mathf.PI * (m + 0.5f) / mt;
			float mp = Mathf.RoundToInt(2 * Mathf.PI * Mathf.Sin(t)/dp);	// This is ambiguous
			for (int n = 0; n < mp; ++n){
				float p = 2 * Mathf.PI * n / mp;
				sphereTargets[index++] = CalcSpherePos(t, p, r);
			}
		}
		
		return index;
		
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
	
	
	void UpdateDanceFollow(){
		float speed = 0.7f;
		parentSpheres[2].transform.position = danceOffset + new Vector3(2 * Mathf.Sin (speed*(Time.fixedTime - danceTime)), Mathf.Sin (2 * speed*(Time.fixedTime - danceTime)), 210);
		
		float danceFollowDist = 8;
		
		// Get the camrera following the,
		Vector3 lookPos = babyCube.transform.position;
		Vector3 bePos = babyCube.transform.position  - babyCube.GetComponent<DanceThreesome>().GetVelocity().normalized * danceFollowDist;
		BackStoryCamera.singleton.GetComponent<BackStoryCamera>().SetEnvyFollow(lookPos, bePos);
	}
	
	
	void UpdateDanceFollow2(){
		float speed = 0.7f;
		parentSpheres[2].transform.position = danceOffset + new Vector3(2 * Mathf.Sin (speed*(Time.fixedTime - danceTime)), Mathf.Sin (2 * speed*(Time.fixedTime - danceTime)), 210 + Mathf.Sin (3 * speed*(Time.fixedTime - danceTime)));
		
		float danceFollowDist = 8;
		
		// Get the camrera following the,
		Vector3 lookPos = babyCube.transform.position;
		Vector3 bePos = babyCube.transform.position  - babyCube.GetComponent<DanceThreesome>().GetVelocity().normalized * danceFollowDist;
		BackStoryCamera.singleton.GetComponent<BackStoryCamera>().SetEnvyFollow(lookPos, bePos);
	}
	
	void UpdateDanceFollow3(){
		float danceFollowDist = 8;
		
		// Get the camrera following the,
		Vector3 lookPos = babyCube.transform.position;
		Vector3 bePos2 = babyCube.transform.position  - babyCube.GetComponent<DanceThreesome>().GetVelocity().normalized * danceFollowDist;
		
		
		// Get the camrera following the,
		Vector3 fromLookToHere = BackStoryCamera.singleton.transform.position - lookPos;
		float dist = fromLookToHere.magnitude;
		
		Vector3 distVel = 0.025f * fromLookToHere.normalized * (danceFollow3Dist - dist);
		
		Vector3 bePos3 = BackStoryCamera.singleton.transform.position  + BackStoryCamera.singleton.transform.TransformDirection(new Vector3(0.3f, 0.1f, 0)) + distVel;
		
		float lerpVel = babyCube.GetComponent<DanceThreesome>().GetVelocity().magnitude;
		
		BackStoryCamera.singleton.GetComponent<BackStoryCamera>().SetEnvyFollow(lookPos, Vector3.Lerp (bePos3, bePos2, lerpVel));
	}
	
	
	void UpdateDanceFollow4(){
		
		// Get the camrera following the,
		Vector3 lookPos = babyCube.transform.position;
		Vector3 fromLookToHere = BackStoryCamera.singleton.transform.position - lookPos;
		float dist = fromLookToHere.magnitude;
		
		
		Vector3 distVel = 0.12f * fromLookToHere.normalized * (danceFollow3Dist - dist);
		
		Vector3 bePos3 = BackStoryCamera.singleton.transform.position  + BackStoryCamera.singleton.transform.TransformDirection(new Vector3(0.3f, 0.1f, 0)) + distVel;
		
		BackStoryCamera.singleton.GetComponent<BackStoryCamera>().SetEnvyFollow(lookPos, bePos3);
	}
	
	
	void UpdateDanceFollow5(){
		// Get the camrera following the,
		Vector3 lookPos = babyCube.transform.position;
		Vector3 fromLookToHere = BackStoryCamera.singleton.transform.position - lookPos;
		float dist = fromLookToHere.magnitude;
		
		
		Vector3 distVel = 0.02f * fromLookToHere.normalized * (danceFollow3Dist - dist);
		
		Vector3 bePos3 = BackStoryCamera.singleton.transform.position  + BackStoryCamera.singleton.transform.TransformDirection(new Vector3(0.3f, 0.1f, 0)) + distVel;
		
		BackStoryCamera.singleton.GetComponent<BackStoryCamera>().SetEnvyFollow(lookPos, bePos3);
	}
	
	
	
	void UpdateDanceFollow6(){
		// Get the camrera following the,
		Vector3 lookPos = babyCube.transform.position;
		Vector3 fromLookToHere = BackStoryCamera.singleton.transform.position - lookPos;
		float dist = fromLookToHere.magnitude;
		
		
		Vector3 distVel = 0.12f * fromLookToHere.normalized * (danceFollow3Dist - dist);
		
		Vector3 bePos3 = BackStoryCamera.singleton.transform.position  + BackStoryCamera.singleton.transform.TransformDirection(new Vector3(0.03f, 0.01f, 0)) + distVel;
		
		BackStoryCamera.singleton.GetComponent<BackStoryCamera>().SetEnvyFollow(lookPos, bePos3);
	}

	void UpdateDanceFollow7(){
		float revPerSecond = 0.02f;
		
		// Get the camrera following the,
		Vector3 lookPos = babyCube.transform.position;
		
		Vector3 target = babyCube.transform.position + new Vector3(danceFollow3Dist * Mathf.Sin (revPerSecond * Time.fixedTime * 2 * Mathf.PI), 1, danceFollow3Dist * Mathf.Cos (revPerSecond * Time.fixedTime * 2 * Mathf.PI));
		
		Vector3 hereToTarget = target - BackStoryCamera.singleton.transform.position;
		Vector3 distVel =  0.12f * hereToTarget;
		Vector3 bePos3 = BackStoryCamera.singleton.transform.position + distVel;//  + BackStoryCamera.singleton.transform.TransformDirection(new Vector3(0.03f, 0.0f, 0)) + distVel;
		
		BackStoryCamera.singleton.GetComponent<BackStoryCamera>().SetEnvyFollowUp(lookPos, bePos3);
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
		BackStoryCamera.singleton.GetComponent<Camera>().fieldOfView = 40;
		
		
		
		// set up heart
		parentSpheres[0].GetComponent<AVOWGreySphere>().SetRegularBeats();
		parentSpheres[1].GetComponent<AVOWGreySphere>().SetRegularBeats();
		parentSpheres[0].GetComponent<AVOWGreySphere>().ActivateSilentBeat();
		parentSpheres[1].GetComponent<AVOWGreySphere>().ActivateSilentBeat();
		
		
		CreateCube();
		
		babyCube.GetComponent<BabyBlueParent>().sizeMul = 1;
		babyCube.GetComponent<BabyBlueParent>().CreateLightening();
		babyCube.GetComponent<BabyBlueParent>().DestroyLightening();
		babyCube.GetComponent<BabyBlueParent>().rotSpeed = 0;
		babyCube.GetComponent<BabyBlueParent>().Update();
		StartDanceThreesome();
		RestoreFlockConfig();
		foreach (GameObject go in parentSpheres){
			go.GetComponent<AVOWGreySphere>().SetCube(babyCube);
		}
		
		inLove11Time = Time.fixedTime + 10f;
		//state = State.kElectrify1;
		
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
		CreateRandomSpheres(true);
	}
	
	void CreateRandomSpheres(bool doRemoval){
		if (Time.fixedTime > sphereTime){
			if (doRemoval) RemoveDepartedSpheres();
			sphereTime = Time.fixedTime + 1f/spheresPerSec;
			if (spheres.Count < maxSphereCount){
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
			bool isInside = GeometryUtility.TestPlanesAABB(planes, sphereGO.GetComponent<Renderer>().bounds);
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
		
		Vector3[] frustOutsLocal = new Vector3[4];
		Vector3[] frustNormsLocal = new Vector3[4];
		
		for (int i = 0; i < 4; ++i){
			frustOutsLocal[i] = camera.transform.TransformDirection(frustOuts[i]);
			frustNormsLocal[i] = camera.transform.TransformDirection(frustNorms[i]);
			
		}
		
		bool calcVel = true;
		while (calcVel){
			int edge = UnityEngine.Random.Range(0, 4);
			float dist = UnityEngine.Random.Range (camera.nearClipPlane + 3f, 50);
			int tangIndex = (edge + 1) % 4;
			Vector3 spawnDir = frustOutsLocal[edge] + UnityEngine.Random.Range (-1f, 1f) * new Vector3(frustOutsLocal[tangIndex].x, frustOutsLocal[tangIndex].y, 0);
			Vector3 goOutsideViewDir = -frustNormsLocal[edge];
		
			//float scale = Random.Range (0.2f, 2.0f);
			float scale = 1f;
			newSphere.transform.position = camera.transform.position + spawnDir * dist + goOutsideViewDir * scale;
			newSphere.transform.localScale = new Vector3(scale, scale, scale);
			newSphere.GetComponent<AVOWGreySphere>().vel = UnityEngine.Random.Range (0.3f, 0.5f) * frustNormsLocal[edge] + UnityEngine.Random.Range (-0.5f, 0.5f) * new Vector3(0, 0, 1) + UnityEngine.Random.Range (-0.5f, 0.5f) * frustNormsLocal[tangIndex];
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
	
	void SteerSpheresToSphere(){
		SteerSpheresFromUs();
		for (int i = 0; i < spheres.Count; ++i){
			GameObject go = spheres[i];
			if (go.GetComponent<AVOWGreySphere>().disableSphering) continue;
			
			// Get Vector from this sphere to target
			Vector3 hereToTarget = sphereTargets[i]  + steerSphereCentre -  go.transform.position;

			Vector3 accn = 2f * hereToTarget;
			go.GetComponent<AVOWGreySphere>().vel += accn * Time.fixedDeltaTime;
			
			// Dampen the velocity
			go.GetComponent<AVOWGreySphere>().vel *= 0.96f;
			
		}
	}
	
	void SteerSpheresFromUs(){
		for (int i = 0; i < spheres.Count; ++i){
			GameObject go = spheres[i];
			if (go.GetComponent<AVOWGreySphere>().disableSphering) continue;
			
			// Create vectors pointing from spheres to cube and its parents
			Vector3 fromCubeToHere = go.transform.position - babyCube.transform.position;
			Vector3 fromParent0ToHere = go.transform.position - parentSpheres[0].transform.position;
			Vector3 fromParent1ToHere = go.transform.position - parentSpheres[1].transform.position;
			float fromCubeToHereDist = fromCubeToHere.magnitude;
			float fromParent0ToHereDist = fromParent0ToHere.magnitude;
			float fromParent1ToHereDist = fromParent1ToHere.magnitude;
			

			float cubeForce = 1/(fromCubeToHereDist * fromCubeToHereDist);
			float parent0Force = 1/(fromParent0ToHereDist * fromParent0ToHereDist);
			float parent1Force = 1/(fromParent1ToHereDist * fromParent1ToHereDist);

			if (fromCubeToHereDist > 8){
				cubeForce = 0;
			}
			if (fromParent0ToHereDist > 8){
				parent0Force = 0;
			}
			if (fromParent1ToHereDist > 8){
				parent1Force = 0;
			}
			
//			float cubeForce = 1/(fromCubeToHereDist );
//			float parent0Force = 1/(fromParent0ToHereDist);
//			float parent1Force = 1/(fromParent1ToHereDist );
			
			Vector3 accn = fromCubeToHere.normalized * cubeForce + fromParent0ToHere.normalized * parent0Force + fromParent1ToHere.normalized * parent1Force;

			go.GetComponent<AVOWGreySphere>().vel += 250f * accn * Time.fixedDeltaTime;
			
			// Dampen the velocity
			if (go.GetComponent<AVOWGreySphere>().vel.magnitude > 1){
				go.GetComponent<AVOWGreySphere>().vel *= 0.98f;
			}
			
		}
	}
	
	void SteerSpheresToSphereOld(){
		foreach (GameObject go in spheres){
			// Get Vector from centre to sphere
			Vector3 centreToSphere = go.transform.position - steerSphereCentre;
			float currentDist = centreToSphere.magnitude;
			// Size it by the distance we have still to travel
			centreToSphere.Normalize();
			
			Vector3 accn = centreToSphere * (steerSphereRadius - currentDist);
			go.GetComponent<AVOWGreySphere>().vel += accn * Time.fixedDeltaTime;
			
			// Dampen the velocity in the direction of the radious of the sphere
			float dotResult = Vector3.Dot(centreToSphere, go.GetComponent<AVOWGreySphere>().vel);
			Vector3 radialVel = centreToSphere * dotResult;
			go.GetComponent<AVOWGreySphere>().vel -= radialVel * 0.05f;
			//go.GetComponent<AVOWGreySphere>().vel *= 0.99f;
			
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
		cubeBrightness.Force (lightIntensityIntro.GetValue());
		cubeBrightness.Set (0);
	
	}
	
	void BrightenScene(){
		lightIntensityIntro.Set (1);
	}
	
	
	void ManageObjects(){
		lightIntensityIntro.Update ();
		lightIntensityOutro.Update();
		cubeBrightness.Update ();
		backStory.transform.FindChild("Intro").FindChild("CursorBlueCube").GetComponent<Renderer>().materials[2].SetFloat("_Intensity", cubeBrightness.GetValue());
		backStory.transform.FindChild("Intro").FindChild("CursorBlueCube").GetComponent<Renderer>().materials[1].SetColor ("_TintColor", rustColor * lightIntensityIntro.GetValue());
		backStory.transform.FindChild("Intro").FindChild("CursorBlueCube").GetComponent<Renderer>().materials[0].SetColor ("_ReflectColor", reflectionColor * lightIntensityIntro.GetValue());
		
		backStory.transform.FindChild("Intro").FindChild("Floor").FindChild("Point light").GetComponent<Light>().intensity = 3 * lightIntensityIntro.GetValue();
		backStory.transform.FindChild("Outro").FindChild("Floor").FindChild("Point light").GetComponent<Light>().intensity = 3 * lightIntensityOutro.GetValue();
		if (babyCube != null && babyCube.transform.FindChild("BabyBlueCube").GetComponent<Renderer>().materials.Length == 2 && triggerOutroLighting){
			babyCube.transform.FindChild("BabyBlueCube").GetComponent<Renderer>().materials[0].SetColor ("_ReflectColor", reflectionColor * lightIntensityOutro.GetValue());
		}
		
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
	void OnGUI(){
	
//		GUI.Box (new Rect(50, 50, 500, 30), "BackStory: " + state.ToString() + " numSpheres = " + spheres.Count);
	}
	
}
