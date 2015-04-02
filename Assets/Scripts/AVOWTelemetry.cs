using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

public class AVOWTelemetry : MonoBehaviour, TelemetryListener{
	public static AVOWTelemetry singleton = null;
	
	
	public enum AVOWEventType{
		kRangeIdMin = 100,
		kStartLevel,
		kStartGoal,
		kGameUpdate,
		kRangeIdMax
	}

	public void WriteStartGoalEvent(int goalNum){
		if (!Telemetry.singleton.enableTelemetry) return;
		TelemEvent e = Telemetry.singleton.ConstructWriteEvent((int)AVOWEventType.kStartGoal, AVOWUpdateManager.singleton.GetGameTime());
		BinaryWriter bw = new BinaryWriter(e.stream);
		bw.Write(goalNum);
	}
	
	public void ReadStartGoalEvent(TelemEvent e){
		BinaryReader br = new BinaryReader(e.stream);
		/*int goalNum =*/ br.ReadInt32();
	}
	
	public void WriteStartLevelEvent(int levelNum){
		if (!Telemetry.singleton.enableTelemetry) return;
		TelemEvent e = Telemetry.singleton.ConstructWriteEvent((int)AVOWEventType.kStartLevel, AVOWUpdateManager.singleton.GetGameTime());
		BinaryWriter bw = new BinaryWriter(e.stream);
		bw.Write(levelNum);
	}
	
	public void ReadStartLevelEvent(TelemEvent e){
		BinaryReader br = new BinaryReader(e.stream);
		/*int level = */br.ReadInt32();
		// Need to do this manually as we don't record all the scenery moving around stuff - but it can get updated by virtue of being in different states
		AVOWGameModes.singleton.ResetScenery();
	}
	
	public void WriteGameUpdateEvent(){
		if (!Telemetry.singleton.enableTelemetry) return;
		TelemEvent e = Telemetry.singleton.ConstructWriteEvent((int)AVOWEventType.kGameUpdate, AVOWUpdateManager.singleton.GetGameTime());
		BinaryWriter bw = new BinaryWriter(e.stream);
		AVOWUpdateManager.singleton.SerialiseGameState(bw);
	}
	
	public void ReadGameUpdateEvent(TelemEvent e){
		BinaryReader br = new BinaryReader(e.stream);
		AVOWUpdateManager.singleton.DeserialiseGameState(br);
	}
	
	public void Initialise(){
		Telemetry.singleton.RegisterListener(this);
		Telemetry.singleton.AddEventType((int)AVOWEventType.kStartLevel, AVOWEventType.kStartLevel.ToString());
		Telemetry.singleton.AddEventType((int)AVOWEventType.kStartGoal, AVOWEventType.kStartGoal.ToString());
		Telemetry.singleton.AddEventType((int)AVOWEventType.kGameUpdate, AVOWEventType.kGameUpdate.ToString());
		
	}
	
	
	
	public int RangeIdMin(){
		return (int)AVOWEventType.kRangeIdMin;
	}
	
	public int RangeIdMax(){
		return  (int)AVOWEventType.kRangeIdMax;
		
	}
	
	
	public void OnEvent(TelemEvent e){
		switch ((AVOWEventType)e.id){
			case AVOWEventType.kStartLevel:{
				ReadStartLevelEvent(e);
				break;
			}
			case AVOWEventType.kStartGoal:{
				ReadStartGoalEvent(e);
				break;
			}
			case AVOWEventType.kGameUpdate:{
				ReadGameUpdateEvent(e);
				break;
			}
		}
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

