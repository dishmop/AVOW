using UnityEngine;
using System.Collections;

public class AVOWGreenBackground : MonoBehaviour {

	public void MakeBig(){
		transform.position = new Vector3(-250, -250, 2f);
		transform.localScale = new Vector3(500, 500, 1);
	}
	
	public void MakeSmall(){
		transform.position = new Vector3(0, 0, 2f);
		transform.localScale = new Vector3(5, 1, 1);
	}

	// Use this for initialization
	void Start () {
		MakeSmall();
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
}
