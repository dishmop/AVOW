using UnityEngine;
using System.Collections;

public class BackStoryCamera : MonoBehaviour {
	public static BackStoryCamera singleton = null;
	
	public GameObject cube;
	
	public float moveSpeed = 0.1f;
	
	public Vector3 loveLook;
	public Vector3 bePos;
	
	
	float envyRampUp = 0;
	
	
	public enum State{
		kStill,
		kOrbitCube,
		kLoveLook,
		kEnvyFollow,
		kEnvyFollowUp
		
	}
	public State state = State.kStill;
	
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
	
	
	// Update is called once per frame
	void Update () {
	
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
		}
		
		
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
