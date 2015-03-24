using System;
using System.Collections.Generic;
using UnityEngine;

public class AVOWCircuitTarget{

	public float totalCurrent;
	
	// componentDesc[0] - width
	// componentDesc[1] - h0
	// componentDesc[2] - min voltage
	public List<Vector3> componentDesc;
	public List<Vector3> hiddenComponents;
	
	public int lcm;
	public int widthInLCMs;
	public long orderingValue;
	

	// copy constructor
	public AVOWCircuitTarget (AVOWCircuitTarget other){


		totalCurrent = other.totalCurrent;
		componentDesc = new List<Vector3>();
		hiddenComponents = new List<Vector3>();
		foreach (Vector3 val3 in other.componentDesc){
			componentDesc.Add (val3);
		}
		foreach (Vector3 val3 in other.hiddenComponents){
			hiddenComponents.Add (val3);
		}
		lcm = other.lcm;
		widthInLCMs = other.widthInLCMs;
		orderingValue = other.orderingValue;
		
	}
	
	public AVOWCircuitTarget (float totalCurrent, List<Vector3> componentDesc)
	{
		this.totalCurrent = totalCurrent;
		this.componentDesc = componentDesc;
		hiddenComponents = new List<Vector3>();
		CalcStats();
	}
	
	public void HideComponent(int index){
		Vector3 vals = componentDesc[index];
		componentDesc.RemoveAt(index);
		hiddenComponents.Add (vals);
		CalcStats();
	}
	
	public void UnhideAllComponents(){
		foreach (Vector3 val3 in hiddenComponents){
			componentDesc.Add (val3);
		}
		hiddenComponents.Clear();
		CalcStats();
	}
	
	public AVOWCircuitTarget (AVOWGraph graph){
		if (graph.HasHalfFinishedComponents()){
			Debug.LogError ("Trying to make a target from half finished components");
		}
		totalCurrent = graph.GetTotalWidth();
		componentDesc = new List<Vector3>();
		hiddenComponents = new List<Vector3>();
		foreach (GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.type == AVOWComponent.Type.kVoltageSource) continue;
			if (MathUtils.FP.Feq (component.hWidth, 0)) continue;
			componentDesc.Add(new Vector3(component.hWidth, component.h0, Mathf.Min (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage)));
		}
		CalcStats();
		
	}
	
	public override int GetHashCode (){
		return (int)orderingValue;
	}
	
	public long GetSortValue (){
		return orderingValue;
	}
	
	
	public override bool Equals(object other){
		AVOWCircuitTarget otherTarget = (AVOWCircuitTarget)other;
		if (componentDesc.Count != otherTarget.componentDesc.Count){
			return false;
		}
		for (int i = 0; i < componentDesc.Count; ++i){
			if (!MathUtils.FP.Feq (componentDesc[i][0], otherTarget.componentDesc[i][0])){
				return false;
			}
		}
		return true;
	}
	
	public string MakeDebugString(){
		int vulgarNumerator;
		int denominator;
		CalcVulgarFraction(totalCurrent, out vulgarNumerator, out denominator);
		string text = "[" + vulgarNumerator + "/" + denominator + "]";
		foreach (Vector3 vals in componentDesc){
			CalcVulgarFraction(vals[0], out vulgarNumerator, out denominator);
			
			text += " " + vulgarNumerator + "/" + denominator;
		}
		return text;
	}
	
	
	public void CalcStats(){
		componentDesc.Sort((obj1, obj2) => obj2[0].CompareTo(obj1[0]));
		
		int denominator;
		int vulgarNumerator;
		
		lcm = 1;
		float width = 0;
		if (componentDesc == null){
			Debug.LogError("(componentDesc == null)");
		}
		orderingValue = 0;
		int powerOfTen = 1;
		foreach (Vector3 val3 in componentDesc){
			float val = val3[0];
			CalcVulgarFraction(val, out vulgarNumerator, out denominator);
			width += val;
			lcm = MathUtils.FP.lcm(denominator, lcm);
			orderingValue += denominator * powerOfTen;
			powerOfTen *= 10;
			orderingValue += vulgarNumerator * powerOfTen;
			powerOfTen *= 10;
		}
		foreach (Vector3 val3 in hiddenComponents){
			float val = val3[0];
			CalcVulgarFraction(val, out vulgarNumerator, out denominator);
			width += val;
			lcm = MathUtils.FP.lcm(denominator, lcm);
		}
		widthInLCMs = Mathf.RoundToInt(width * lcm);
	}
	
	
	
	
	
	void CalcVulgarFraction(float val, out int vulgarNumerator, out int denominator){
		int numerator;
		int integer;
		bool isNeg;
		MathUtils.FP.CalcFraction(val, out integer, out numerator, out denominator, out isNeg);
		vulgarNumerator = (isNeg ? -1 : 1) * numerator + integer * denominator;
		
	}
	
}

