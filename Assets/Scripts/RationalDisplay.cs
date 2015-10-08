using UnityEngine;
using System.Collections;

public class RationalDisplay : MonoBehaviour {

	public float value;
	
	
	int integer;
	int numerator;
	int denominator;
	bool isNeg;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		MathUtils.FP.CalcFraction(value, out integer, out numerator, out denominator, out isNeg);
		if (numerator == 0){
			transform.FindChild("Mixed").gameObject.SetActive(false);
			transform.FindChild("IntegerOnly").gameObject.SetActive(true);
			if (isNeg){
				transform.FindChild("IntegerOnly").FindChild("Integer").GetComponent<TextMesh>().text = "-"+integer.ToString ();
			}
			else{
				transform.FindChild("IntegerOnly").FindChild("Integer").GetComponent<TextMesh>().text = integer.ToString ();
			}
		}
		else{
			transform.FindChild("Mixed").gameObject.SetActive(true);
			transform.FindChild("IntegerOnly").gameObject.SetActive(false);
			
			if (integer == 0){
				if (isNeg){
					transform.FindChild("Mixed").FindChild("Integer").GetComponent<TextMesh>().text = "-";
				}
				else{
					transform.FindChild("Mixed").FindChild("Integer").GetComponent<TextMesh>().text = "";
				}
			}
			else{
				if (isNeg){
					transform.FindChild("Mixed").FindChild("Integer").GetComponent<TextMesh>().text = "-" + integer.ToString();
				}
				else{
					transform.FindChild("Mixed").FindChild("Integer").GetComponent<TextMesh>().text = integer.ToString();
				}
			}
			transform.FindChild("Mixed").FindChild("Fraction").FindChild("Numerator").GetComponent<TextMesh>().text = numerator.ToString ();
			transform.FindChild("Mixed").FindChild("Fraction").FindChild("Denominator").GetComponent<TextMesh>().text = denominator.ToString ();
		}
		
	
	}
}
