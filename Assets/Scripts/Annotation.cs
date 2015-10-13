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
	
	float lineWidth = 12;
	
	public State voltState = State.kLeftTop;
	public State ampState = State.kLeftTop;
	public State ohmState = State.kLeftTop;
	public bool showArrows = true;
	
	public float manualVoltage = -1;
	public float manualAmpage = -1;
	public bool hasManualValues = false;
	
	VectorLine voltArrow;
	VectorLine ampArrow;
	
	GameObject ampDisplay;
	GameObject voltDisplay;
	GameObject ohmDisplay;	
	
	public void SetVoltage(float voltage){
		manualVoltage = voltage;
		hasManualValues = true;
	}
	
	public void SetCurrent(float ampage){
		manualAmpage = ampage;
		hasManualValues = true;
	}
	
	public void ClearManualFlag(){
		hasManualValues = false;
	}
	
	// Use this for initialization
	void Start () {
		Vector3[] dummyPoints3 = new Vector3[2];
		voltArrow = new VectorLine("Voltage annotation", dummyPoints3, Explanation.singleton.arrowMaterial, lineWidth);
		voltArrow.endCap = "rounded_2Xarrow";
	
		ampArrow = new VectorLine("Amp annotation", dummyPoints3, Explanation.singleton.arrowMaterial, lineWidth);
		ampArrow.endCap = "rounded_2Xarrow";
		
		voltDisplay = transform.FindChild("VoltDisplay").gameObject;
		ampDisplay = transform.FindChild("AmpDisplay").gameObject;
		ohmDisplay = transform.FindChild("OhmDisplay").gameObject;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float arrowOffset = CalcOffsetFromSize(0.5f * lineWidth);
		
		if (componentGOs.Count() == 0){
			if (hasManualValues){
				voltDisplay.SetActive(voltState != State.kDisabled);
				voltDisplay.GetComponent<RationalDisplay>().value = manualVoltage;

				voltArrow.active = showArrows && (voltState != State.kDisabled);
				Vector3 halfArrowUp = new Vector3(0, 0.5f * manualVoltage - arrowOffset, 0);
				voltArrow.points3[0] = transform.position + halfArrowUp - new Vector3(0.05f + arrowOffset * 2, 0, 0);
				voltArrow.points3[1] = transform.position - halfArrowUp - new Vector3(0.05f + arrowOffset * 2, 0, 0);
				voltArrow.Draw3D();	
				 
				ampArrow.active = showArrows && (ampState != State.kDisabled);
				Vector3 halfArrowRight = new Vector3(0.5f * manualAmpage - arrowOffset, 0, 0);
				ampArrow.points3[0] = transform.position + halfArrowRight + new Vector3(0, 0.05f + arrowOffset * 2, 0);
				ampArrow.points3[1] = transform.position - halfArrowRight + new Vector3(0, 0.05f + arrowOffset * 2, 0);
				ampArrow.Draw3D();		
				
				
				ampDisplay.SetActive(ampState != State.kDisabled);
				ampDisplay.GetComponent<RationalDisplay>().value = manualAmpage;
				ohmDisplay.SetActive(ohmState != State.kDisabled);
			}
			else{
				voltArrow.active = false;
				voltDisplay.SetActive(false);
				ampArrow.active = false;
				ampDisplay.SetActive(false);
				ohmDisplay.SetActive(false);
			}
			return;
			
		}
		
		float boxOffset = arrowOffset * 2;
		
		// Voltage
		float voltMin = 100;
		float voltMax = -100;
		float hLeft = 100;
		float hRight = -100;
		bool hasVoltageSource = false;
		
		foreach (GameObject go in componentGOs){
			AVOWComponent component = go.GetComponent<AVOWComponent>();

			float thisVoltMin = Mathf.Min (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
			float thisVoltMax = Mathf.Max (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
			voltMin = Mathf.Min (voltMin, thisVoltMin);
			voltMax = Mathf.Max (voltMax, thisVoltMax);
			
			hLeft = Mathf.Min (component.h0, hLeft);
			hRight = Mathf.Max (component.h0 + component.hWidth, hRight);
			if (component.type == AVOWComponent.Type.kVoltageSource){
				hasVoltageSource = true;
			}
		}
		if (hasVoltageSource) hRight += 0.35f;
		
		
		voltDisplay.GetComponent<RationalDisplay>().value = voltMax - voltMin;
		
		// Ampage
		float ampMin = 100;
		float ampMax = -100;
		float vTop = 1;
		float vBottom = 0;
		
		foreach (GameObject go in componentGOs){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			
			ampMin = Mathf.Min (ampMin, component.h0);
			ampMax = Mathf.Max (ampMax, component.h0 + component.hWidth);
		}
		
		ampDisplay.GetComponent<RationalDisplay>().value = ampMax - ampMin;
		
		
		
		
		switch (voltState){
			case State.kLeftTop:{
				voltArrow.active = showArrows;
				voltArrow.points3[0] = new Vector3(hLeft - boxOffset, voltMax - arrowOffset, transform.position.z);
				voltArrow.points3[1] = new Vector3(hLeft- boxOffset, voltMin + arrowOffset, transform.position.z);
				voltArrow.Draw3D();	
				
				voltDisplay.SetActive(!AVOWGraph.singleton.HasHalfFinishedComponents());
				voltDisplay.transform.position = 0.5f * (voltArrow.points3[0] + voltArrow.points3[1]) - new Vector3(0.13f, 0, 0);
			
				break;
			}
			case State.kRightBottom:{
				voltArrow.active = showArrows;
				voltArrow.points3[0] = new Vector3(hRight + boxOffset, voltMax - arrowOffset, transform.position.z);
				voltArrow.points3[1] = new Vector3(hRight + boxOffset, voltMin + arrowOffset, transform.position.z);
				voltArrow.Draw3D();			
				
			
				voltDisplay.SetActive(!AVOWGraph.singleton.HasHalfFinishedComponents());
				voltDisplay.transform.position = 0.5f * (voltArrow.points3[0] + voltArrow.points3[1]) + new Vector3(0.05f, 0, 0);
				break;
			}	
			case State.kDisabled:{
				voltArrow.active = false;
				voltDisplay.SetActive(false);
				break;
			}		
			
		}
		
		// If we are positioned right in the middle of the battery current, then move to the right so
		// we are not covered up by the  battery spark
		float batteryOffset = 0;
		if (MathUtils.FP.Feq (0.5f * (ampMax + ampMin), AVOWSim.singleton.cellCurrent * 0.5f)){
			batteryOffset = 0.05f;
		}
	
		switch (ampState){
			case State.kLeftTop:{
				ampArrow.active = showArrows;
				ampArrow.points3[0] = new Vector3(ampMin + arrowOffset, vTop + boxOffset, transform.position.z);
				ampArrow.points3[1] = new Vector3(ampMax - arrowOffset, vTop + boxOffset, transform.position.z);
				ampArrow.Draw3D();		
				
				ampDisplay.SetActive(!AVOWGraph.singleton.HasHalfFinishedComponents());
				ampDisplay.transform.position = 0.5f * (ampArrow.points3[0] + ampArrow.points3[1]) + new Vector3(batteryOffset, 0.075f, 0);
				break;
			}
			case State.kRightBottom:{
				ampArrow.active = showArrows;
				ampArrow.points3[0] = new Vector3(ampMin + arrowOffset, vBottom - boxOffset + 0.01f, transform.position.z);
				ampArrow.points3[1] = new Vector3(ampMax - arrowOffset, vBottom - boxOffset + 0.01f	, transform.position.z);
				ampArrow.Draw3D();			
				
				ampDisplay.SetActive(!AVOWGraph.singleton.HasHalfFinishedComponents());
				ampDisplay.transform.position = 0.5f * (ampArrow.points3[0] + ampArrow.points3[1]) + new Vector3(batteryOffset, -0.1f, 0);
				break;
			}	
			case State.kDisabled:{
				ampArrow.active = false;
				ampDisplay.SetActive(false);
				break;
			}		
				
		}
		
		float resistance = (voltMax - voltMin) / (ampMax - ampMin);
		if (ohmState != State.kDisabled && !MathUtils.FP.Feq(ampMax - ampMin, 0) && !hasVoltageSource){
			ohmDisplay.SetActive(!AVOWGraph.singleton.HasHalfFinishedComponents());
			ohmDisplay.GetComponent<RationalDisplay>().value =resistance;
			ohmDisplay.transform.position = new Vector3(0.5f * (ampMax + ampMin) + 0.05f, 0.5f * (voltMax + voltMin), transform.position.z);
		}
		else{
			ohmDisplay.SetActive(false);
		}
		
	}
	
	void OnDestroy(){
		VectorLine.Destroy(ref voltArrow);
		VectorLine.Destroy(ref ampArrow);
	}
	
	float CalcOffsetFromSize(float size){
		Vector3 pt1 = Camera.main.ScreenToWorldPoint( new Vector3(0, 0, 10));
		Vector3 pt2 = Camera.main.ScreenToWorldPoint( new Vector3(size, 0, 10));
		return (pt2 - pt1).x;
		
	
	}
}
