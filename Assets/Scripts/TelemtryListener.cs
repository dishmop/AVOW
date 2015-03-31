using UnityEngine;

public interface TelemetryListener {

	int RangeIdMin();
	int RangeIdMax();
	
	void OnEvent(TelemEvent e);
}
 