using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public interface AVOWCommand{

	bool ExecuteStep();

	// Undoes part of the command, this may need to be called again
	// We return true when we have finished
	bool UndoStep();
	
	bool IsFinished();
	
	GameObject GetNewComponent();
	
	GameObject GetNewNode();
	
	void Serialise(BinaryWriter bw);
	void Deserialise(BinaryReader br);
	
}
