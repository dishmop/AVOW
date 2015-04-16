using UnityEngine;
using System.Collections;

public class AVOWPusher : MonoBehaviour {

	public static AVOWPusher singleton = null;


	public bool disableMovement = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	public void RenderUpdate() {
		if (!disableMovement){
			transform.position = new Vector3(AVOWGraph.singleton.xMax, 0 ,0f);
		}
		//transform.localScale = new Vector3(AVOWSim.singleton.xMax, AVOWSim.singleton.yMax,1);
		
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
