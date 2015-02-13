using UnityEngine;
using System.Collections;

public class FractionCalc : MonoBehaviour {

	public float value = 0f;
	public Color color;
	

	float 	lastValue = -1f;
	int		integer = 0;
	int		numerator = 0;
	int		denominator = 0;
	bool 	isNeg = false;


	// Use this for initialization
	void Start () {
		Transform recentreT = transform.FindChild ("Recentre").transform;
		recentreT.gameObject.SetActive(false);
		
	
	}
	
	// Update is called once per frame
	void Update () {
	
		Transform recentreT = transform.FindChild ("Recentre").transform;
		
		recentreT.gameObject.SetActive(true);
		
		// Ensure the text always points upwards
		recentreT.rotation = Quaternion.identity;
		
		recentreT.FindChild("Integer").GetComponent<TextMesh>().color = color;
		recentreT.FindChild("Numerator").GetComponent<TextMesh>().color = color;
		recentreT.FindChild("Denominator").GetComponent<TextMesh>().color = color;
		recentreT.FindChild("Seperator").GetComponent<TextMesh>().color = color;
		if (!MathUtils.FP.Feq(lastValue, value, MathUtils.FP.fracEpsilon)){
			lastValue = value;
			RecalcFraction();
			int intToDisplay = integer * (isNeg ? -1 : 1);
			recentreT.FindChild("Integer").GetComponent<TextMesh>().text = intToDisplay.ToString();
			if (!MathUtils.FP.Feq(numerator, 0, MathUtils.FP.fracEpsilon)){
				recentreT.FindChild("Numerator").GetComponent<TextMesh>().text = numerator.ToString();
				recentreT.FindChild("Denominator").GetComponent<TextMesh>().text = denominator.ToString();
				recentreT.FindChild("Seperator").GetComponent<TextMesh>().text = "_";
				
				// If the integer is zero then don't show the integer
				if (MathUtils.FP.Feq(integer, 0, MathUtils.FP.fracEpsilon)){
					if (isNeg){
						recentreT.FindChild("Integer").GetComponent<TextMesh>().text = "-";
					}
					else{
						recentreT.FindChild("Integer").GetComponent<TextMesh>().text = "";
					}
				}
					
			}
			else{
				recentreT.FindChild("Numerator").GetComponent<TextMesh>().text = "";
				recentreT.FindChild("Denominator").GetComponent<TextMesh>().text = "";
				recentreT.FindChild("Seperator").GetComponent<TextMesh>().text = "";
			}
		}
		// Recentre
		Bounds integerBounds = recentreT.FindChild("Integer").GetComponent<TextMesh>().renderer.bounds;
		Bounds numeratorBounds = recentreT.FindChild("Numerator").GetComponent<TextMesh>().renderer.bounds;
		Bounds denominatorBounds = recentreT.FindChild("Denominator").GetComponent<TextMesh>().renderer.bounds;
		
		float minX = Mathf.Min (Mathf.Min (integerBounds.min.x, numeratorBounds.min.x), denominatorBounds.min.x);
		float maxX= Mathf.Max (Mathf.Max (integerBounds.max.x, numeratorBounds.max.x), denominatorBounds.max.x);
		float midX = 0.5f * (minX + maxX);
		
		Vector3 offset = new Vector3(transform.position.x - midX, 0, 0);
		
		// Need to do this to cope with Amater effect scaking the number
		offset.x *= 1f/recentreT.lossyScale.x;
		offset.y *= 1f/recentreT.lossyScale.y;
		
		Vector3 newPos = recentreT.position + offset;
		recentreT.position  = newPos;
		
		
		
	}
	

	
	void RecalcFraction(){
		MathUtils.FP.CalcFraction(value, out integer, out numerator, out denominator, out isNeg);
	
	}
}
