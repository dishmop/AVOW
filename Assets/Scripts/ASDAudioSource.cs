using UnityEngine;
using System.Collections;
using System;

public class ASDAudioSource : MonoBehaviour {
	public AudioClip	attackClip;
	public AudioClip	sustainClip;
	public AudioClip	decayClip;
	
	bool play = false;
	bool isInitialised = false;
	
	float[][] clipData = new float[3][];
	
	int playIndex = 0;
	int clipNum = -1;
	
	
	public void Play(){
		play = true;
		clipNum = 0;
	}
	
	public void Stop(){
		play = false;
	}
	
	public bool IsPlaying(){
		return play || clipNum != -1;
	}
	
	
	void OnAudioFilterRead(float[] data, int channels){
		if (clipNum < 0 || !isInitialised) return;
		
		if (channels != 2){
			Debug.Log ("We are only expecting to see stereo channel");
		}
		int fillIndex = 0;
		while (fillIndex < data.Length){
			int samplesToCopy = Mathf.Min (clipData[clipNum].Length - playIndex, data.Length - fillIndex);
			Array.Copy(clipData[clipNum], playIndex, data, fillIndex, samplesToCopy);
			playIndex += samplesToCopy;
			fillIndex += samplesToCopy;
			
			// If we have reached the end of the playClip, then need to decide what to do next
			if (playIndex == clipData[clipNum].Length){
				playIndex = 0;
				if (!play){
					if (clipNum != 2 && decayClip != null){
						clipNum = 2;
					}else{
						clipNum = -1;
						Array.Clear(data, fillIndex, data.Length - fillIndex);
						fillIndex = data.Length;
						            
					}
				}
				else{
					if (clipNum == 0 && sustainClip != null){
						clipNum = 1;
					}
				}
			}
		}
	
	}
	
	// Use this for initialization
	void Start () {

		
		if (attackClip != null && attackClip.channels != 2){
			Debug.Log ("We only deal with stereo channels clips at the moment");
		}
		if (sustainClip != null && sustainClip.channels != 2){
			Debug.Log ("We only deal with stereo channels clips at the moment");
		}
		if (decayClip != null && decayClip.channels != 2){
			Debug.Log ("We only deal with stereo channels clips at the moment");
		}
		
		if (attackClip != null){
			clipData[0] = new float[attackClip.samples * attackClip.channels];
			attackClip.GetData(clipData[0], 0);
		}
		if (sustainClip != null){
			clipData[1] = new float[sustainClip.samples * sustainClip.channels];
			sustainClip.GetData(clipData[1], 0);
		}
		if (decayClip != null){
			clipData[2] = new float[decayClip.samples * decayClip.channels];
			decayClip.GetData(clipData[2], 0);
		}
		isInitialised = true;


	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
