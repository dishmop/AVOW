using UnityEngine;
using System.Collections;

public class BackStoryCamera : MonoBehaviour {
	public static BackStoryCamera singleton = null;
	
	public GameObject cube;
	
	public float moveSpeed = 0.1f;
	
	
	public enum State{
		kStill,
		kOrbitCube
	}
	public State state = State.kStill;
	
	// Use this for initialization
	void Start () {
		//LookAtCube();
		
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
