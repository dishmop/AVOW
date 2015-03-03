using UnityEngine;
using System.Collections;

public class BabyBlueCube : MonoBehaviour {

	public float rotSpeed = 10;


	// Use this for initialization
	void Start () {
	
		Vector3 faceDir = new Vector3(1, 1, 1);
		transform.rotation = Quaternion.Inverse(Quaternion.LookRotation(faceDir));
	
	}
	
	void Update(){
		transform.Rotate (new Vector3(1, 1, 1), rotSpeed, Space.Self);
	}

}
