using UnityEngine;
using System.Collections;

public class BabyBlueCube : MonoBehaviour {

	float rotSpeed;
	float rotSpeed2;


	// Use this for initialization
	void Start () {
	
		Vector3 faceDir = new Vector3(1, 1, 1);
		transform.rotation = Quaternion.Inverse(Quaternion.LookRotation(faceDir));
	
	}
	
	public void SetRotSpeed(float speed){	
		rotSpeed = speed;
	}
	public void SetRotSpeed2(float speed){	
		rotSpeed2 = speed;
	}
	
			
	void Update(){
		transform.Rotate (new Vector3(1, 1, 1), rotSpeed, Space.Self);
	}

}
