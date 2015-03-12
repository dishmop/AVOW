using UnityEngine;
using System.Collections;

public class BabyBlueCube : MonoBehaviour {

	float rotSpeed;
	float rotSpeed2;
	
	Quaternion initialRot;
	
	bool reInitRot = false;
	public bool rotOnInit = true;


	// Use this for initialization
	void Start () {
		Vector3 faceDir = new Vector3(1, 1, 1);
		initialRot =  Quaternion.Inverse(Quaternion.LookRotation(faceDir));
		if (rotOnInit) transform.rotation = initialRot;
	}
	
	public void ReinitialiseRotation(){
		reInitRot  = true;
		
	}
	

	
	public void SetRotSpeed(float speed){	
		rotSpeed = speed;
	}
	public void SetRotSpeed2(float speed){	
		rotSpeed2 = speed;
	}
	
			
	void Update(){
		transform.Rotate (new Vector3(1, 1, 1), rotSpeed, Space.Self);
		transform.Rotate (new Vector3(0, 0, 1), rotSpeed2, Space.Self);
		
		if (reInitRot){
			
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, 0.01f);
		}
		
	}

}
