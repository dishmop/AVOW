using UnityEngine;
using System.Collections;

public class LighteningAudio : MonoBehaviour {

	const float fundamentalF = 2 * 32.625f; // c
	//const float fundamentalF = 34.65f; // c#
	int index = 0;
	int periodInSamples = (int)(44100f / fundamentalF);

	void OnAudioFilterRead(float[] data, int channels){
//		int halfPeriod = periodInSamples / 2;
		for (int i = 0; i < data.Length; i += 2){
			int inPeriodIndex = index % periodInSamples;
			data[i] = 2 * (float)inPeriodIndex / (float)periodInSamples - 1;
			data[i+1] = 2 * (float)inPeriodIndex / (float)periodInSamples - 1;
			
			// Sinwave
//			data[i] = Mathf.Sin (2 * 3.14f * fundamentalF * index / 44100f);
//			data[i+1] = data[i];
			
			
			index++;
		}
		
		
	}

	// Use this for initialization
	void Start () {

	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
