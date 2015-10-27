using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class AVOWMenuButton : MonoBehaviour {

	public int levelNum;

	// Use this for initialization
	public void OnClick(){
		AVOWGameModes.singleton.TriggerStartLevel(levelNum);
		AVOWUI.singleton.PlayPing();
//		Debug.Log("startLevel, buttonName: " + gameObject.GetComponent<Text>().text  + ", levelNum = " + levelNum.ToString());
		Analytics.CustomEvent("startLevel", new Dictionary<string, object>
		{
			{ "buttonName", gameObject.GetComponent<Text>().text },
			{ "levelNum", levelNum.ToString()},
		});
	}
	

}
