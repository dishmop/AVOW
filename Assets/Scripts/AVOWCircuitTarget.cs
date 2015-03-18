using System;
using System.Collections.Generic;
using UnityEngine;

public class AVOWCircuitTarget{

	public float totalCurrent;
	public List<float> individualCurrents;
	
	public int lcm;
	public int totalCurrentInLCMs;
	

	
	public AVOWCircuitTarget (float totalCurrent, List<float> IndividualCurrents)
	{
		this.totalCurrent = totalCurrent;
		this.individualCurrents = IndividualCurrents;
		CalcStats();
	}
	
	
	void CalcStats(){
		int denominator;
		int vulgarNumerator;
		
		lcm = 1;
		float width = 0;
		foreach (float val in individualCurrents){
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

