using UnityEngine;
using System.Collections;

public class BackStoryCamera : MonoBehaviour {
	public static BackStoryCamera singleton = null;
	
	public GameObject cube;
	
	public float moveSpeed = 0.1f;
	
	public Vector3 loveLook;
	public Vector3 bePos;
	public float mouseMul = 10;
	
	
	Vector3 	mousePos = Vector3.zero;
	
	float ctrlSpeed = 0.00f;
	
	float ctrlDist;
	
	float envyRampUp = 0;
	float initialDist;
	Vector3 lastMousePos;
	Vector3 initialPos;
	Vector3 cubeLookPos;
	Vector3 cubeInitialPos;
	
	public float ctrlLerpVal=  0;
	
	Vector3 	vel = Vector3.zero;
	
	
	
	
	public enum State{
		kStill,
		kOrbitCube,
		kLoveLook,
		kEnvyFollow,
		kEnvyFollowUp,
		kControl0,
		kControl0a,
		kControl1
		
	}
	public State state = State.kStill;
	State oldState = State.kStill;
	
	// Use this for initialization
	void Start () {
		//LookAtCube();
		
	}
	
	public void SetLoveLook(Vector3 loveLookPos){
		loveLook = loveLookPos;
		state = State.kLoveLook;
	}
	
	public void SetEnvyFollow(Vector3 envyLookPos, Vector3 envyBePos){
		loveLook = envyLookPos;
		bePos = envyBePos;
		state = State.kEnvyFollow;
	}
	
	
	public void SetEnvyFollowUp(Vector3 envyLookPos, Vector3 envyBePos){
		loveLook = envyLookPos;
		bePos = envyBePos;
		state = State.kEnvyFollowUp;
	}	
	
	public void StartOrbit(){
		state = State.kOrbitCube;
	}
	
	public void StartControl(){
		state = State.kControl0;
		
	}
	
	
	// Update is called once per frame
	void Update () {
		UpdateMousePos();
	
		if (oldState != state){
			oldState = state;
			envyRampUp =0;
		}
		
		switch (state){
			case State.kOrbitCube:{
				Vector3 moveDir = Vector3.Cross(cube.transform.position - transform.position, new Vector3(0, 1, 0));
				moveDir.Normalize();
				transform.position += moveDir * moveSpeed;
				LookAtCube();
				break;
			}
			case State.kLoveLook:{
				Quaternion currRot = transform.rotation; 	
				Quaternion desRot = Quaternion.LookRotation( loveLook - transform.position);
				transform.rotation = Quaternion.Lerp(currRot, desRot, 0.02f);
				break;
			}
			case State.kEnvyFollow:{
				float envyLerp = Mathf.Lerp(0, 0.1f, envyRampUp);
				Quaternion currRot = transform.rotation; 	
				// Get the current up direction
				Vector3 thisUp = transform.TransformDirection(new Vector3 (0, 1, 0));
				Quaternion desRot = Quaternion.LookRotation( loveLook - transform.position, thisUp);
				transform.rotation = Quaternion.Lerp(currRot, desRot, envyLerp);
				transform.position = Vector3.Lerp (transform.position, bePos, envyLerp);
				envyRampUp = Mathf.Min (envyRampUp + 0.01f, 1f);
				break;
			}
			case State.kEnvyFollowUp:{
				float envyLerp = Mathf.Lerp(0, 0.1f, envyRampUp);
				Quaternion currRot = transform.rotation; 	
				// Get the current up direction
				Quaternion desRot = Quaternion.LookRotation( loveLook - transform.position);
				transform.rotation = Quaternion.Lerp(currRot, desRot, envyLerp);
				transform.position = Vector3.Lerp (transform.position, bePos, envyLerp);
				envyRampUp = Mathf.Min (envyRampUp + 0.01f, 1f);
				break;
			}	
			
			case State.kControl0:{
				initialDist = (cube.transform.position - transform.position).magnitude;
				initialPos = transform.position;
				cubeLookPos = cube.transform.position;
				cubeInitialPos = cube.transform.position;
				state = State.kControl0a;

				
				break;
			}
			case State.kControl0a:{
				Vector3 hereToTarget = cube.transform.position - transform.position;
				Vector3 moveDir = Vector3.Cross(hereToTarget, new Vector3(0, 1, 0));
				moveDir.Normalize();
				transform.position += moveDir * moveSpeed;
				float dist = hereToTarget.magnitude - initialDist;
				if ( dist < 10){
					ctrlSpeed += 0.0001f;
				}
				else{
					ctrlSpeed -= 0.0001f;
				}
				if (dist < 19.9f){
					hereToTarget.Normalize();
					hereToTarget *= ctrlSpeed;
					transform.position -= hereToTarget;
					cubeLookPos.y += ctrlSpeed * 0.30f;
				}
				else{
					state = State.kControl1;
					lastMousePos = mousePos;
					ctrlDist = (transform.position - cube.transform.position).magnitude;
				}
				Vector3 pos = transform.position;
				pos.y = initialPos.y;
				transform.position = pos;
				
				transform.rotation = Quaternion.LookRotation(cubeLookPos - transform.position);
				AVOWBackStoryCutscene.singleton.backStory.transform.FindChild("Music").GetComponent<AudioSource>().volume = 0.3f * (1 - (dist / 20));
			

				break;
			}
			case State.kControl1:{	
				Vector3 lastCubePos = cube.transform.position;
				
				Vector3 mouseDelta = mousePos - lastMousePos;
				lastMousePos = mousePos;
				
				mouseDelta = transform.rotation * mouseDelta;
				
				if (mouseDelta.y > 0) vel = Vector3.zero;
				
				vel += new Vector3(0, Mathf.Lerp (-2, 0, ctrlLerpVal), 0) * Time.fixedDeltaTime;
				
				cube.transform.position += vel * Time.fixedDeltaTime;
				cube.transform.position += Mathf.Lerp (0.0025f, 0.025f, ctrlLerpVal) * mouseDelta;
				if (cube.transform.position.y < cubeInitialPos.y){
					Vector3 testVec = cube.transform.position;
					testVec.y = cubeInitialPos.y;
					cube.transform.position = testVec;
				}
				
				Vector3 horzDiff = cubeInitialPos - cube.transform.position;
				horzDiff.y = 0;
				float dist = horzDiff.magnitude;
				horzDiff.Normalize();
				if (dist > 10){
					cube.transform.position += horzDiff * (dist - 10);
				}
				
				Vector3 fromCubeToHere = transform.position - cube.transform.position;
				float currentDist = fromCubeToHere.magnitude;
				fromCubeToHere.Normalize();
				
				cube.transform.position  += fromCubeToHere * (currentDist - ctrlDist);
				
				float distMoved = (lastCubePos - cube.transform.position).magnitude;
				ctrlLerpVal = Mathf.Min (ctrlLerpVal + distMoved / 100, 1);
//				Debug.Log ("ctrlLerpVal = " + ctrlLerpVal + " distMoved / 1000 = " + distMoved / 100000);
				
				
				//LookAtCube();
				break;
			}
		}
		
		
	}
	
	Vector2 GetMouseDelta(){
		return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
	}
	
	void UpdateMousePos(){
		Vector2 delta = GetMouseDelta() * mouseMul;
		mousePos += new Vector3(delta.x, delta.y, 0);
	}
	
	
	void LookAtCube(){
		transform.rotation = Quaternion.LookRotation(cube.transform.position - transform.position);
	}
	
	
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
		
	}
	
	
	void OnDestroy(){
		
		singleton = null;
	}	
}
