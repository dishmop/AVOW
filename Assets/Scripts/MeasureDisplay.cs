using UnityEngine;
using System.Collections;

public class MeasureDisplay : MonoBehaviour {

	public Color okCol;
	public Color notOkCol;
	bool shouldDisplay = true;
	bool isDisplay = true;
	int frameDelay = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		shouldDisplay = (!AVOWGraph.singleton.HasHalfFinishedComponents());
		
		if (shouldDisplay != isDisplay){
			frameDelay  = 5;
			isDisplay= shouldDisplay;
		
		}
		--frameDelay;
		
		if (shouldDisplay == isDisplay && frameDelay < 0){
			if (shouldDisplay){
				GetComponent<FractionCalc>().value = AVOWSim.singleton.xMax * AVOWCircuitCreator.singleton.GetLCM();// * AVOWCircuitCreator.singleton.currentLCM;;
				GetComponent<FractionCalc>().color = okCol;
				
			}
			else{
				GetComponent<FractionCalc>().color = notOkCol;
			}
			
		}
		
		if (!AVOWConfig.singleton.showTotals){
			GetComponent<FractionCalc>().color = new Color(0, 0, 0, 0);
			transform.parent.FindChild("Display").gameObject.SetActive(false);
		}		
		else{
			transform.parent.FindChild("Display").gameObject.SetActive(true);
		}
	
	}
}
