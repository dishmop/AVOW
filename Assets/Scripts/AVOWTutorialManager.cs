using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWTutorialManager : MonoBehaviour {
	public static AVOWTutorialManager singleton = null;
	
	
	public GameObject greySphere;
	
	public GameObject backStory;
	
	
	List<GameObject>	spheres = new List<GameObject>();
	
	List<GameObject>			parentSpheres = new List<GameObject>();
	Vector3[]	frustOuts = new Vector3[4];
	Vector3[]	frustNorms = new Vector3[4];
	
	float		spheresPerSec = 10;
	float 		sphereTime = 0;
	
	float 		timeForParentsToMeet = 15;
	
	float 		worldOfSpheres2Time = 0;
	float		worldOfSpheres2Duration = 10;
	
	enum State{
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
		
		kStop,
		kNumStates
	}
	State state = State.kOff;
	
	Vector3[]		cameraStartPositions = new Vector3[(int)State.kNumStates];
	bool triggered = false;
	
	State waitNextState = State.kOff;
	
	SpringValue		lightIntensity = new SpringValue(0, SpringValue.Mode.kLinear, 0.1f);
	SpringValue 	cubeBrightness = new SpringValue(0, SpringValue.Mode.kAsymptotic, 20);
	Color			reflectionColor;
	
	public void StartTutorial(){
		state = State.kTheWorldOfSpheres0;
		//state = State.kStartup;
		
	}
	public void Trigger(){
		triggered = true;
	}
	
	public void StopTutorial(){
		state = State.kStop;
	}

	// Use this for initialization
	void Start () {
		string name = backStory.transform.FindChild("Intro").FindChild("CursorBlueCube").renderer.materials[0].name;
		reflectionColor = backStory.transform.FindChild("Intro").FindChild("CursorBlueCube").renderer.materials[0].GetColor ("_ReflectColor");
		
		// Set up camera posiitons
		cameraStartPositions[(int)State.kStartup] = new Vector3(0, 2.3f, 104.2f);
		cameraStartPositions[(int)State.kTheWorldOfSpheres0] = new Vector3(0, 0, 200);
		
	
	}
	
	
	void WaitForTextToFinish(State nextState){
		AVOWTutorialText.singleton.AddTrigger();
		waitNextState = nextState;
		state = State.kWaitForText;
	}
	
	// Update is called once per frame
	void Update () {
		switch(state){
			case State.kWaitForText:{
				if (triggered){
					triggered = false;
					state = waitNextState;
				}
				break;
			}
			
			case State.kStartup:{
				SetCameraPos();
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
				SetCameraPos();
				SetupFrustrumVectors();
			
				BackStoryCamera.singleton.state = BackStoryCamera.State.kStill;
				BackStoryCamera.singleton.transform.position = new Vector3(0, 0, 200);
				Random.seed = 1;
				CreateRandomSphere();
				AVOWTutorialText.singleton.AddPause(10);	
				
				AVOWTutorialText.singleton.AddText("The spheres floated in the nothingness.");
				WaitForTextToFinish(State.kTheWorldOfSpheres1);
			
				break;
			}
			case State.kTheWorldOfSpheres1:{
				worldOfSpheres2Time = Time.time + worldOfSpheres2Duration;
				AVOWTutorialText.singleton.AddPause(4);	
				AVOWTutorialText.singleton.AddText("For eons, everyhing remained the same");
			
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
				//AVOWTutorialText.singleton.AddText("Two spheres fell in love");
				state = State.kInLove2;
				
				break;
			}	
			case State.kInLove2:{
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
	
	
	void ParentsCourtship(){
	
		foreach (GameObject go in parentSpheres){
			go.GetComponent<AVOWGreySphere>().StartCourtship();
		}
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
		
		
		parentSpheres.Add(newSphere0);
		
		// Make the left hand sphere
		Vector3 spawnPos1 =  camera.transform.position + spawnDir1 * dist + goOutsideViewDir1 * scale1;
		GameObject newSphere1 = GameObject.Instantiate(greySphere, spawnPos1, Quaternion.identity) as GameObject;
		newSphere1.transform.localScale = new Vector3(scale1, scale1, scale1);
		newSphere1.transform.parent = backStory.transform.FindChild("WorldOfSpheres");
		newSphere1.GetComponent<AVOWGreySphere>().beatColor = new Color(1f, 0.1f, 0f);
		parentSpheres.Add(newSphere1);

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

		int edge = Random.Range(0, 4);
		float dist = Random.Range (camera.nearClipPlane, camera.farClipPlane);
		int tangIndex = (edge + 1) % 4;
		Vector3 spawnDir = frustOuts[edge] + Random.Range (-1f, 1f) * new Vector3(frustOuts[tangIndex].x, frustOuts[tangIndex].y, 0);
		Vector3 goOutsideViewDir = -frustNorms[edge];
		
		float scale = Random.Range (0.2f, 2.0f);
		GameObject newSphere = GameObject.Instantiate(greySphere, camera.transform.position + spawnDir * dist + goOutsideViewDir * scale, Quaternion.identity) as GameObject;
		newSphere.transform.localScale = new Vector3(scale, scale, scale);
		newSphere.transform.parent = backStory.transform.FindChild("WorldOfSpheres");
		newSphere.GetComponent<AVOWGreySphere>().vel = Random.Range (0.3f, 0.5f) * frustNorms[edge] + Random.Range (-0.5f, 0.5f) * new Vector3(0, 0, 1) + Random.Range (-0.5f, 0.5f) * frustNorms[tangIndex];
		spheres.Add(newSphere);
		
		


	}
	


	
	void SetCameraPos(){
		BackStoryCamera.singleton.transform.position = cameraStartPositions[(int)state];
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
