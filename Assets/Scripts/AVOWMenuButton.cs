using UnityEngine;
using System.Collections;

public class AVOWMenuButton : MonoBehaviour {

	public int levelNum;

	// Use this for initialization
	public void OnClick(){
		AVOWGameModes.singleton.StartLevel(levelNum);
	}
}
