using UnityEngine;
using System.Collections;

public class AVOWUIButton : MonoBehaviour {

	public AVOWUI.ToolMode mode;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.FindChild("SelectionFrame").gameObject.SetActive((mode == AVOWUI.singleton.GetUIMode()));
	}
}
