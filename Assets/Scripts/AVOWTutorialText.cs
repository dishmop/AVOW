using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.UI;

public class AVOWTutorialText : MonoBehaviour {
	public static AVOWTutorialText singleton = null;
	

	public bool			activated = false;
	public SpringValue	border  = new SpringValue(1, SpringValue.Mode.kLinear,0.25f);
	public float		defaultLettersPerSecond = 5;
	public float 		forceCompleteSpeed = 1000;
	public GameObject 	textBox;
	public GameObject 	panel;
	
	public Color		textColor;
	public Color		highlightColor;
	
	float				lettersPerSecond = 0;
	float 				lastNonForcedSpeed = 0;
	
	float				nextletterTime = 0;
	string 				closingString = "</color>";
	string				highlightLetter;
	string				highlightString;
	
	StringBuilder		queuedString = new StringBuilder();
	StringBuilder		displayedString = new StringBuilder();
	
	const string 		kPauseKey = "PAUSE";	
	const string 		kSpeedKey = "SPEED";	
	const string		kTriggerKey = "TRIGGER";
	
	public void AddText(string text){
		queuedString.Append(text + "\n\n");
		
	}
	
	
	public void InturruptText(string text){
		if (queuedString.Length > 5){
			queuedString.Length = 0;
			queuedString.Append("...\n\n");
			ForceTextCompletion();
			queuedString.Append(text + "\n\n");
		}
		else{
			ForceTextCompletion();
			AddText (text);
		}
		Debug.Log("InturruptText: text = " + queuedString.ToString());
		
		
	}
	
	public void AddTextNoLine(string text){
		queuedString.Append(text);
		
	}
	
	public void AddTrigger(){
		// We should insert this before any new line characters which are at the end of the queue
		string thisString = queuedString.ToString();
		int index = thisString.Length-1;
		while (index >= 0){
			if (thisString[index] != '\n') break;
			index--;
		}
		queuedString.Insert(index+1, "["+kTriggerKey+"=" + "0]");
	}
	
	public void AddPause(float seconds){
		queuedString.Append ("["+kPauseKey+"=" + seconds.ToString() +"]");
	}
	
	public void SetSpeed(float lettersPerSec){
		queuedString.Append ("["+kSpeedKey+"=" + lettersPerSec.ToString() +"]");
	}
	
	public void SetSpeedDefault(){
		SetSpeed(-1);
	}
	
	public void ClearText(){
		queuedString = new StringBuilder();
		displayedString = new StringBuilder();
		lettersPerSecond = defaultLettersPerSecond;
		ClearDisplayString();
	}
	
	public void ForceTextCompletion(){
		if (lettersPerSecond != forceCompleteSpeed){
			lastNonForcedSpeed = lettersPerSecond;
		}
		nextletterTime = Time.time;
		lettersPerSecond = forceCompleteSpeed;
		SetSpeed(lastNonForcedSpeed);
//		string tempString = queuedString.ToString();
//		Debug.Log("ForceTextCompletion (" + lettersPerSecond.ToString() + "): text = " + tempString);
	}
	
	

	// Use this for initialization
	void Start () {
		lettersPerSecond = defaultLettersPerSecond;
		border.Force (activated ? 0.15f : 0);
		//ApplyBorder();
		ClearDisplayString();
	
	}
	
	string CreateColorString(Color col){
		int red = (int)(col.r * 255);
		int green = (int)(col.g * 255);
		int blue = (int)(col.b * 255);
		return string.Format("#{0:x2}{1:x2}{2:x2}", red, green, blue);
	}
	
	void ClearDisplayString(){
		displayedString = new StringBuilder();
		displayedString.Append ("<color=" + CreateColorString(textColor) + ">");
	}
	
	
	
	// Update is called once per frame
	void Update () {
//		Debug.Log (lettersPerSecond.ToString() + ": " + queuedString.ToString());
		HandleBorder();
		HandleText();
	}
	
	void HandleBorder(){
		border.Set (activated ? 0.15f : 0);
		border.Update ();
		ApplyBorder();
	}
	
	void ApplyBorder(){
		Rect rect = Camera.main.rect;
		rect.yMin = border.GetValue();
		Camera.main.rect = rect;
		Vector2 anchorMax = new Vector2(1, border.GetValue());
		panel.GetComponent<RectTransform>().anchorMax = anchorMax;
	}
	
	bool HandleSpecials(){
		string totalString = queuedString.ToString();
		string firstLetter = totalString.Substring(0, 1);
		if (firstLetter == "["){
			int index = totalString.IndexOf("]");
			if (index == -1){
				Debug.LogError("No Closing ] in:" + totalString);
			}
			queuedString.Remove(0, index+1);
			string subString = totalString.Substring(1, index-1);
			bool ok = ParseCommand(subString);
			if (!ok){
				Debug.LogError("Error parsing command" + subString);
			}
			return true;
		}
		else{
			return false;
		}
	}
	
	bool ParseCommand(string cmdString){
		// remove any blank spaces
		cmdString = cmdString.Replace (" ", "");
		
		// Seperate out the command from the data
		int index = cmdString.IndexOf("=");
		
		if (index == -1){
			return false;
		}
		string cmd = cmdString.Substring(0, index);
		string data = cmdString.Substring(index+1, cmdString.Length - index - 1);
		switch (cmd){
			case kPauseKey:{
				nextletterTime = Time.time + (1.0f/AVOWConfig.singleton.tutorialSpeed) * float.Parse(data);
				break;
			}
			case kSpeedKey:{
				lettersPerSecond = float.Parse (data);
				if (lettersPerSecond < 0) lettersPerSecond = defaultLettersPerSecond;
				Debug.Log("ParseCommand(kSpeedKey): " + lettersPerSecond.ToString());
			    break;
			}
			case kTriggerKey:{
				AVOWBackStoryCutscene.singleton.Trigger();
				AVOWTutorialManager.singleton.Trigger();
				break;
			}
		}
		return true;
		
	}
	
	
	
	void HandleText(){
		while (Time.time > nextletterTime){
			bool mayHaveSpecial = true;
			while (mayHaveSpecial && queuedString.Length > 0 && Time.time > nextletterTime){
				mayHaveSpecial = HandleSpecials();
			}
			if (Time.time > nextletterTime){
				if (queuedString.Length > 0){
					if (highlightLetter != null) displayedString.Append(highlightLetter);
					highlightLetter = queuedString.ToString().Substring(0, 1);
					highlightString = "<color=" + CreateColorString(highlightColor) + ">" + highlightLetter + "</color>";
					queuedString.Remove(0,1);
					textBox.GetComponent<Text>().text = displayedString.ToString() + closingString + highlightString;
					if (highlightLetter != " " && highlightLetter != "\n"){
						if (!GetComponent<AudioSource>().isPlaying) GetComponent<AudioSource>().Play();
						AVOWBackStoryCutscene.singleton.TriggerLight();
						AVOWUI.singleton.TriggerLight();
					}
				}
				float timeDeltaRaw = (1.0f/AVOWConfig.singleton.tutorialSpeed)* 1.0f/lettersPerSecond;
				float timeDelta = Random.Range(0.1f * timeDeltaRaw, 2.0f * timeDeltaRaw);
				if (highlightLetter == "," || highlightLetter == "-"){
					nextletterTime = Time.time + 2 * timeDelta;
				}
				else if (highlightLetter == "."){
					nextletterTime = Time.time + 5 * timeDelta;
				}
				else if (highlightLetter == "\n"){
					nextletterTime = Time.time + 5 * timeDelta;
				}			
				else{
					nextletterTime = Time.time + 1 * timeDelta;
				}
			}
		}

		
	}
	
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	

}
