using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class AVOWUpdateManager : MonoBehaviour {

	public static AVOWUpdateManager singleton = null;

	public GameObject config;
	public GameObject sim;
	public GameObject gameModes;
	public GameObject ui;
	public GameObject graph;
	public GameObject circuitCreator;
	public GameObject circuitSubsetter;
	public GameObject backStory;
	public GameObject tutorialManager;
	public GameObject tutorialText;
	public GameObject camController;
	public GameObject objectiveManager;
	public GameObject telemetry;
	public GameObject battery;
	
	public float playbackSpeed = 1;
	
	float gameStartTime;
	float gameTime;
	
	public float GetGameTime(){
		return gameTime;
	}
	
	public void ResetGameTime(){
		gameTime = 0;
	
	}
	
	// need to seralise the graph first becuase when we deserialise, we need to refernce objects whcih need to be thte
	public void SerialiseGameState(BinaryWriter bw){
		config.GetComponent<AVOWConfig>().Serialise(bw);
		gameModes.GetComponent<AVOWGameModes>().Serialise(bw);
		graph.GetComponent<AVOWGraph>().Serialise(bw);
		ui.GetComponent<AVOWUI>().Serialise(bw);
		objectiveManager.GetComponent<AVOWObjectiveManager>().Serialise(bw);
		tutorialText.GetComponent<AVOWTutorialText>().Serialise(bw);
	}
	
	public void DeserialiseGameState(BinaryReader br){
		config.GetComponent<AVOWConfig>().Deserialise(br);
		gameModes.GetComponent<AVOWGameModes>().Deserialise(br);
		graph.GetComponent<AVOWGraph>().Deserialise(br);
		ui.GetComponent<AVOWUI>().Deserialise(br);
		objectiveManager.GetComponent<AVOWObjectiveManager>().Deserialise(br);
		tutorialText.GetComponent<AVOWTutorialText>().Deserialise(br);
	}
	
	// Use this for initialization
	void Start () {
		config.GetComponent<AVOWConfig>().Initialise();
		telemetry.GetComponent<Telemetry>().Initialise();
		telemetry.GetComponent<AVOWTelemetry>().Initialise();
		sim.GetComponent<AVOWSim>().Initialise();
		gameModes.GetComponent<AVOWGameModes>().Initialise();
		graph.GetComponent<AVOWGraph>().Initialise();
		tutorialText.GetComponent<AVOWTutorialText>().Initialise();
		circuitSubsetter.GetComponent<AVOWCircuitSubsetter>().Startup();
		backStory.GetComponent<AVOWBackStoryCutscene>().Initialise();
		objectiveManager.GetComponent<AVOWObjectiveManager>().Initialise();
		battery.GetComponent<AVOWBattery>().Initialise();
		
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		gameTime += Time.fixedDeltaTime * (telemetry.GetComponent<Telemetry>().isPlaying ? playbackSpeed : 1);
		if (!telemetry.GetComponent<Telemetry>().isPlaying){
			graph.GetComponent<AVOWGraph>().ResetOptFlags();
			objectiveManager.GetComponent<AVOWObjectiveManager>().ResetOptFlags();
			
			ui.GetComponent<AVOWUI>().GameUpdate();
			gameModes.GetComponent<AVOWGameModes>().GameUpdate();
			circuitCreator.GetComponent<AVOWCircuitCreator>().GameUpdate();
			graph.GetComponent<AVOWGraph>().GameUpdate();
			sim.GetComponent<AVOWSim>().GameUpdate();
			gameModes.GetComponent<AVOWGameModes>().GameUpdate();
			tutorialText.GetComponent<AVOWTutorialText>().GameUpdate();
			objectiveManager.GetComponent<AVOWObjectiveManager>().GameUpdate();
			tutorialManager.GetComponent<AVOWTutorialManager>().GameUpdate();
		}
		if (telemetry.GetComponent<Telemetry>().isRecording){
			AVOWTelemetry.singleton.WriteGameUpdateEvent();
		}
		telemetry.GetComponent<Telemetry>().GameUpdate();
		
		
	}
	
	
	void Update(){
		if (!telemetry.GetComponent<Telemetry>().isPlaying){
			ui.GetComponent<AVOWUI>().RenderUpdate();
		}

		config.GetComponent<AVOWConfig>().RenderUpdate();
		gameModes.GetComponent<AVOWGameModes>().RenderUpdate();
		graph.GetComponent<AVOWGraph>().RenderUpdate();
		backStory.GetComponent<AVOWBackStoryCutscene>().RenderUpdate();
		camController.GetComponent<AVOWCamControl>().RenderUpdate();
		objectiveManager.GetComponent<AVOWObjectiveManager>().RenderUpdate();
		battery.GetComponent<AVOWBattery>().RenderUpdate();
		

	}
	
	
	
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	
	void OnDestroy(){
		singleton = null;
	}	
}
