using UnityEngine;
using System;

public class SpringValue{

	public enum Mode{
		kAsymptotic,
		kLinear
	};
	
	float desValue = 0f;
	float currentValue = 0f;
	float linSpeed = 25f;//50f;
	float asSpeed = 4f;
	
	
	
	Mode mode =  Mode.kLinear;
	
	
	public SpringValue(float val){
		desValue = val;
		currentValue = val;
	}

	public SpringValue(float val, Mode setMode ){
		mode = setMode;
		desValue = val;
		currentValue = val;
	}
	
	public SpringValue(float val, Mode setMode, float speed ){
		mode = setMode;
		desValue = val;
		currentValue = val;
		SetSpeed (speed);

	}	
	
	
	public void SetSpeed(float speed){
		if (mode == Mode.kAsymptotic){
			asSpeed = speed;
		}
		else{
			linSpeed = speed;
		}
	}
	
	public void Set(float newVal){
		desValue = newVal;
	}
	
	public void Force(float newVal){
		desValue = newVal;
		currentValue = newVal;
	}
	
	public float GetValue(){
		return currentValue;
	}

	public float GetDesValue(){
		return desValue;
	}
	
	public bool IsAtTarget(){
		return MathUtils.FP.Feq(GetValue(), GetDesValue());
	}
	
	// Update is called once per frame
	public void Update () {
		float deltatTime = Time.deltaTime;
		switch (mode){
			case Mode.kAsymptotic:
				currentValue = Mathf.Lerp(currentValue, desValue, asSpeed * deltatTime);
				break;
			case Mode.kLinear:
				if (!MathUtils.FP.Feq (currentValue, desValue, linSpeed * Time.fixedDeltaTime)){
					if (currentValue > desValue)
						currentValue -= linSpeed * deltatTime;
					else
						currentValue += linSpeed * deltatTime;
				}
				else{
					currentValue = desValue; 
				}
				break;
		}
	
	}
	
}
