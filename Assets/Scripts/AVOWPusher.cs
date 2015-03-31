using UnityEngine;
using System.Collections;

public class AVOWPusher : MonoBehaviour {

	public bool disableMovement = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (!disableMovement) transform.position = new Vector3(AVOWGraph.singleton.xMax, 0 ,0f);
		//transform.localScale = new Vector3(AVOWSim.singleton.xMax, AVOWSim.singleton.yMax,1);
		
	}
}
