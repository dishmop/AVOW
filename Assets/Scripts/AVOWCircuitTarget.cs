using System;
using System.Collections.Generic;
using UnityEngine;

public class AVOWCircuitTarget{

	public float totalCurrent;
	
	// componentDesc[0] - width
	// componentDesc[1] - h0
	// componentDesc[2] - min voltage
	public List<Vector3> componentDesc;
	
	

	
	public int lcm;
	public int totalCurrentInLCMs;
	

	// copy constructor
	public AVOWCircuitTarget (AVOWCircuitTarget other){


		totalCurrent = other.totalCurrent;
		componentDesc = new List<Vector3>();
		foreach (Vector3 val3 in other.componentDesc){
			componentDesc.Add (val3);
		}
		lcm = other.lcm;
		totalCurrentInLCMs = other.totalCurrentInLCMs;
		
	}
	
	public AVOWCircuitTarget (float totalCurrent, List<Vector3> componentDesc)
	{
		this.totalCurrent = totalCurrent;
		this.componentDesc = componentDesc;
		CalcStats();
	}
	
	public AVOWCircuitTarget (AVOWGraph graph){
		if (graph.HasHalfFinishedComponents()){
			Debug.LogError ("Trying to make a target from half finished components");
		}
		totalCurrent = graph.GetTotalWidth();
		componentDesc = new List<Vector3>();
		foreach (GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.type == AVOWComponent.Type.kVoltageSource) continue;
			if (MathUtils.FP.Feq (component.hWidth, 0)) continue;
			componentDesc.Add(new Vector3(component.hWidth, component.h0, Mathf.Min (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage)));
		}
		componentDesc.Sort((obj1, obj2) => obj2[0].CompareTo(obj1[0]));
		CalcStats();
		
	}
	
	
	public void CalcStats(){
		int denominator;
		int vulgarNumerator;
		
		lcm = 1;
		float width = 0;
		if (componentDesc == null){
			Debug.LogError("(componentDesc == null)");
		}
		foreach (Vector3 val3 in componentDesc){
			float val = val3[0];
			CalcVulgarFraction(val, out vulgarNumerator, out denominator);
			width += val;
			lcm = MathUtils.FP.lcm(denominator, lcm);
		}
		totalCurrentInLCMs = Mathf.RoundToInt(width * lcm);
	}
	
	
	void CalcVulgarFraction(float val, out int vulgarNumerator, out int denominator){
		int numerator;
		int integer;
		bool isNeg;
		MathUtils.FP.CalcFraction(val, out integer, out numerator, out denominator, out isNeg);
		vulgarNumerator = (isNeg ? -1 : 1) * numerator + integer * denominator;
		
	}
	
}

