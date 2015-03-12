using UnityEngine;
using System.Collections;

public class ProjectorScript : MonoBehaviour {


	public float dist = 1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		UpdatePos();
		
		// Set upt he FOV
		
		gameObject.GetComponent<Projector>().fieldOfView =  14.25f *  transform.parent.localScale.x / 0.2f;
	
	}
	
	void UpdatePos(){
		Vector3 ballPos = transform.parent.position;
		Vector3 lightPos = ballPos + new Vector3(0, 0, -1f);
		Vector3 thisLocalDir = lightPos - ballPos;
		thisLocalDir.Normalize();
		
		Vector3 thisPos = ballPos + dist * thisLocalDir;
		transform.position = thisPos;
		
		Quaternion thisRot = Quaternion.LookRotation(-thisLocalDir, new Vector3 (0, 1, 0));
		transform.rotation = thisRot;
	}
}
