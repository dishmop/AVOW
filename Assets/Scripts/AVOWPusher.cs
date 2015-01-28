using UnityEngine;
using System.Collections;

public class AVOWPusher : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(AVOWSim.singleton.xMax, 0 ,0.1f);
		//transform.localScale = new Vector3(AVOWSim.singleton.xMax, AVOWSim.singleton.yMax,1);
		
	}
}
