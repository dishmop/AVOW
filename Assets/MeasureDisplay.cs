using UnityEngine;
using System.Collections;

public class MeasureDisplay : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		GetComponent<FractionCalc>().value = AVOWSim.singleton.xMax;
	
	}
}
