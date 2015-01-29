using UnityEngine;
using System;

public interface AVOWCommand{

	bool ExecuteStep();

	// Undoes part of the command, this may need to be called again
	// We return true when we have finished
	bool UndoStep();
	
}
