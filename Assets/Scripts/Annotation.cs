using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;

public class Annotation : MonoBehaviour {

	public List<GameObject> componentGOs = new List<GameObject>();
	
	public enum State{
		kDisabled,
		kLeftTop,
		kRightBottom
	}
	
	float lineWidth = 10;
	
	State voltState = State.kLeftTop;
	
	VectorLine voltArrow;

	// Use this for initialization
	void Start () {
		Vector3[] dummyPoints3 = new Vector3[2];
		voltArrow = new VectorLine("Voltage annotation", dummyPoints3, Explanation.singleton.arrowMaterial, lineWidth);
		voltArrow.endCap = "rounded_2Xarrow";
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (componentGOs.Count() == 0) return;
		
		float arrowOffset = CalcOffsetFromSize(0.5f * lineWidth);
		float boxOffset = arrowOffset * 2;
		
		float voltMin = 100;
		float voltMax = -100;
		float hLeft = 100;
		float hRight = -100;
		foreach (GameObject go in componentGOs){
			AVOWComponent component = go.GetComponent<AVOWComponent>();

			float thisVoltMin = Mathf.Min (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
			float thisVoltMax = Mathf.Max (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
			voltMin = Mathf.Min (voltMin, thisVoltMin);
			voltMax = Mathf.Max (voltMax, thisVoltMax);
			
			hLeft = Mathf.Min (component.h0, hLeft);
			hRight = Mathf.Max (component.h0, hRight);
		}
		
		switch (voltState){
			case State.kLeftTop:{
			
				voltArrow.points3[0] = new Vector3(voltMin - boxOffset, voltMax - arrowOffset, transform.position.z);
				voltArrow.points3[1] = new Vector3(voltMin- boxOffset, voltMin + arrowOffset, transform.position.z);
				voltArrow.Draw3D();			
			//	Debug.DrawLine(new Vector3(voltArrow.points2[0].x-0.1f, voltArrow.points2[0].x, -0.1f) , new Vector3(voltArrow.points2[1].x-0.1f, voltArrow.points2[1].y, -0.1f), Color.red);
			
				break;
			}
		}
	
	}
	
	float CalcOffsetFromSize(float size){
		Vector3 pt1 = Camera.main.ScreenToWorldPoint( new Vector3(0, 0, 10));
		Vector3 pt2 = Camera.main.ScreenToWorldPoint( new Vector3(size, 0, 10));
		return (pt2 - pt1).x;
		
	
	}
}
