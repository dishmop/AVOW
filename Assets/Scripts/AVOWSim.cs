using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class AVOWSim : MonoBehaviour {
	public static AVOWSim singleton = null;
	
	public GameObject graphGO;
	
	AVOWGraph graph;
	
	public class LoopElement{
		public LoopElement(AVOWComponent component, AVOWGraph.Node fromNode){
			this.component = component;
			this.fromNode = fromNode;
		}
			
		public AVOWComponent component;
		public AVOWGraph.Node fromNode;
	}
	
	public List<List<LoopElement>> loops;
	
	// Recording the mouse posiiton
	Vector3			mouseWorldPos;
	AVOWComponent 	mouseOverComponent = null; 	// null if not over any compnent
	float			mouseOverXProp = 0.5f;		// Proportion of width from left of component (or entire diagram if null)
	float			mouseOverYProp = 0.5f;		// Proportio of height from bottom of component (or entire diagram if null)
	
	// Current solving	
	double epsilon = 0.0001;
	float[]						loopCurrents;
	
	
	public void Recalc(){

		FindLoops();
		//DebugPrintLoops();
		
		RecordLoopsInComponents();
		SolveForCurrents();
		//DebugPrintLoopCurrents();
		
		StoreCurrentsInComponents();
		//DebugPrintComponentCurrents();
		
		CalcVoltages();
		//DebugPrintVoltages();
		
		DebugPrintGraph();
		bool finished = false;
		int count = 0;
		while (!finished && count < 10){
			LayoutHOrder();
			count++;
		}
		
		//DebugPrintHOrder();
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
	void FixedUpdate(){

		RecordMousePos();
		Recalc();
		CalcMouseOffset();
		
		//AppHelper.Quit();
	}
	
	
	void DebugPrintGraph(){
		Debug.Log ("Print graph");
		Debug.Log ("Nodes");
		foreach (AVOWGraph.Node node in graph.allNodes){
			Debug.Log("Node " + node.GetID());			
		}
		Debug.Log ("Components");
		foreach (GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.GetCurrent(component.node0) > 0){
				Debug.Log("Component " + component.GetID() + "/" + component.hOrder + ": from " + component.node0.GetID() + " to " + component.node1.GetID());
			}
			else{
					Debug.Log("Component " + component.GetID() + "/" + component.hOrder + ": from " + component.node1.GetID() + " to " + component.node0.GetID());
			}
		}
	}
	
	void DebugPrintLoops(){
		Debug.Log ("Printing loops");
		for (int i = 0; i < loops.Count; ++i){
			AVOWGraph.Node lastNode = loops[i][0].fromNode;
			string loopString = lastNode.GetID ();
			
			for (int j = 0; j < loops[i].Count; ++j){
				AVOWGraph.Node nextNode = loops[i][j].component.GetOtherNode(lastNode);
				// print the connection and the final node
				loopString += "=" + loops[i][j].component.GetID () + "=>" + nextNode.GetID ();
				lastNode = nextNode;
			}
			Debug.Log ("Loop(" + i.ToString() + "): " + loopString );
			
		}
	}
	
	void DebugPrintLoopCurrents(){
		Debug.Log("Printing loop currents");
		for (int i = 0; i < loopCurrents.Length; ++i){
			Debug.Log ("Loop " + i + ": " + loopCurrents[i]);
			
		}
	}
	
	void Start(){
		graph = graphGO.GetComponent<AVOWGraph>();
		
	}
	
	
	// Fins a set of indendpent loops in the graph
	void FindLoops(){
		loops = new List<List<LoopElement>>();

		// If no nodes, then nothing to do
		if (graph.allNodes.Count == 0) return;

		// Get any node which is going to be our starting point for all traversals		
		AVOWGraph.Node startNode = graph.allNodes[0];
		
		
		// We have no components disabled then as we find loops, we disable one component at a time
		// until we can't find any more loops
		graph.ClearDisabledFlags();
		
		// We finish this loop when there are no loops left
		bool finished = false;
		while (!finished){
			graph.ClearVisitedFlags();
			
			// Create a stack of nodes which we use to traverse the graph
			Stack<AVOWGraph.Node> nodeStack = new Stack<AVOWGraph.Node>();
			Stack<AVOWComponent> componentStack = new Stack<AVOWComponent>();
			
			
			nodeStack.Push(startNode);
			
			// We finish this loop when we have found a loop (or we are sure there are none)
			bool foundLoop = false;
			while (!finished && !foundLoop){
			
				// We visit our current node
				AVOWGraph.Node currentNode = nodeStack.Peek();
				currentNode.visited = true;
				
				// Go through each connections from here in order and find one that has not yet been traversed
				AVOWComponent nextConnection = null;
				for (int i = 0; i < currentNode.components.Count; ++i){
					AVOWComponent component = currentNode.components[i].GetComponent<AVOWComponent>();
					if (!component.visited && !component.disable){
						nextConnection = component;
						nextConnection.visited = true;
						componentStack.Push (nextConnection);
						break;
					}
				}
				
				// If we found an untraversed connection then this is our next point in our path
				if (nextConnection != null){
					AVOWGraph.Node nextNode = nextConnection.GetOtherNode(currentNode);
					
					if (nextNode == null){
						Debug.LogError ("Failed to find other end of connection when finding loops");
					}
					nodeStack.Push(nextNode);
					
					// If this node has already been visited, then we have found a loop
					if (nextNode.visited){
						foundLoop = true;
					}
					
				}
				// If we have not managed to find an untraversed connection, pop our current stack element
				else {
					nodeStack.Pop ();
					// if this is the last element in the node stack, then there will be no component associated with it
					if (componentStack.Count > 0)
						componentStack.Pop ();
				}
				
				// If there is nothing left on the stack then we have traversed the whole graph and not found a loop
				if (nodeStack.Count == 0){
					finished = true;
				}

			}
			if (foundLoop){
				
				// If we found a loop, then record it
				// We do this by stepping backwards down our stack until we find our current node again - this ensures thast we just get the loop
				// and no stragglers
				AVOWGraph.Node loopStartNode = nodeStack.Peek();
				AVOWComponent loopStartComponent = componentStack.Peek();
				
				List<LoopElement> thisLoop = new List<LoopElement>();
				thisLoop.Add (new LoopElement(componentStack.Pop (), nodeStack.Pop ()));
				
				while(nodeStack.Peek() != loopStartNode){
					thisLoop.Add (new LoopElement(componentStack.Pop (), nodeStack.Pop ()));
				}
				loops.Add (thisLoop);
				
				// Now disable one of the components in the loop so this loop is not found again
				loopStartComponent.disable = true;
			}
			
			
		}
		
		// Quick check - we should have just traversed a spanning tree for the graph
		// so everyone component should have been visited
		foreach (GameObject componentGO in graph.allComponents){
			AVOWComponent component = componentGO.GetComponent<AVOWComponent>();
			if (!component.visited && !component.disable)
				Debug.LogError ("Spanning tree does not visit very node");
		}
		
	}
	
	
	void RecordLoopsInComponents(){
		graph.ClearLoopRecords();
		
		for (int i = 0; i < loops.Count; ++i){
			for (int j = 0; j < loops[i].Count; ++j){
				loops[i][j].component.AddLoopRecord(i, loops[i][j].fromNode);
			}
		}
	
	}
	
	bool SolveForCurrents(){
	
		// Create arrays need to solve equation coeffs
		double [,] R = new double[loops.Count, loops.Count];
		double [,] V = new double[loops.Count, 1];
		
		// For through each loop in turn (for each row in the matrices)
		for (int i = 0; i < loops.Count; ++i){
			// For each connection in the loop, check the resistance and any voltage drop
			for (int j = 0; j < loops[i].Count; ++j){
			
				LoopElement loopElement = loops[i][j];
				
				// deal with things differently depending on whether this is a load or a voltage source
				if (loopElement.component.type == AVOWComponent.Type.kLoad){
					float resistance = loopElement.component.GetResistance();
					
					// For each component in the loop, add in resistances for each loop that passes through it
					foreach (AVOWComponent.LoopRecord record in loopElement.component.loopRecords){
						if (record.isForward == loopElement.component.IsForward(loopElement.fromNode)){
							R[i, record.loopId] += resistance;
						}
                        else{
							R[i, record.loopId] -= resistance;
						}
					
					}
				}
				else if (loopElement.component.type == AVOWComponent.Type.kVoltageSource){
					V[i, 0] = loopElement.component.GetVoltage(loopElement.fromNode);
				}
				else{
					Debug.LogError ("Unknown type of component");
				}
			}
		}  
		
		// Currents
		double[,] I = new double[0,0];
		
		// IF we do not have full rankm then find a solution using the Moore-Pensrose Pseudo-inverse
		// Method taken from here: http://en.wikipedia.org/wiki/Moore%E2%80%93Penrose_pseudoinverse#The_iterative_method_of_Ben-Israel_and_Cohen
		// search for "A computationally simple and accurate way to compute the pseudo inverse " ont his page
		
		
		//		if (R.GetLength (0) != R.GetLength (1)){
		//			Debug.LogError ("Matrix is not square, yet we expect it to be!");
		//		}
		//		
		
		// First we need to calc the SVD of the matrix
		double[] W = null;	// S matrix as a vector (leading diagonal)
		double[,] U = null;
		double[,] Vt = null;
		alglib.svd.rmatrixsvd(R, R.GetLength (0), R.GetLength (1), 2, 2, 2, ref W, ref U, ref Vt);
		
		double[,] S = new double[W.GetLength (0), W.GetLength (0)];
		
		for (int i = 0; i < R.GetLength (0); ++i){
			S[i,i] = W[i];
		}
		//				MathUtils.Matrix.SVD(R, out S, out U, out Vt);
		// Log the results
		
		//		double[,] testR0 = MathUtils.Matrix.Multiply(U, S);
		//		double[,] testR1 = MathUtils.Matrix.Multiply(testR0, Vt);
		
		double[,] Ut = MathUtils.Matrix.Transpose(U);
		double[,] Vtt = MathUtils.Matrix.Transpose (Vt);
		
		// Get the psuedo inverse of the U matrix (which is diagonal)
		// The way we do this is by taking the recipricol of each diagonal element, leaving the (close to) zero's in place
		// and transpose (actually we don't need to transpose because we always have square matricies)
		
		// I assume this gets initialised with zeros
		double[,] SInv = new double[S.GetLength(0), S.GetLength(0)];
		
		for (int i = 0; i < S.GetLength(0); ++i){
			if (Math.Abs (S[i,i]) > epsilon){
				SInv[i,i] = 1.0 / S[i,i];
				
			}					
		}
		
		//Rinv = Vtt Uinv St
		double[,] RInvTemp = MathUtils.Matrix.Multiply (Vtt, SInv);
		double[,] RInv = MathUtils.Matrix.Multiply (RInvTemp, Ut);
		
		//		// Test thast we have a psueoinverse
		//		double[,] res0 = MathUtils.Matrix.Multiply(R, RInv);
		//		double[,] res1 = MathUtils.Matrix.Multiply(res0, R);
		
		I = new double[loops.Count, 1];
		I = MathUtils.Matrix.Multiply(RInv, V); 
		
//		// Check that we get V - if not we have an unsolvable set of equations and 
//		// it means we have a loop of zero resistance with a voltage different in it
//		double[,] testV = MathUtils.Matrix.Multiply(R, I);
//		
//		bool failed = false;
//		for (int i = 0; i < loops.Count; ++i){
//			// f we find a bad loop
//			if (Math.Abs (V[i, 0] - testV[i, 0]) > epsilon){
//				// Then follow this loop finding all the voltage sources and put them in Emergency mode
//				for (int j = 0; j < loops[i].Count; ++j){
//					BranchAddress thisAddr = loops[i][j];
//					CircuitElement thisElement = Circuit.singleton.GetElement(new GridPoint(thisAddr.x, thisAddr.y));
//					if (Mathf.Abs (thisElement.GetVoltageDrop(thisAddr.dir, true)) > epsilon){
//						thisElement.TriggerEmergency();
//						failed = true;
//					}
//					BranchAddress oppAddr = GetOppositeAddress(thisAddr);
//					CircuitElement oppElement = Circuit.singleton.GetElement(new GridPoint(oppAddr.x, oppAddr.y));
//					if (Mathf.Abs (oppElement.GetVoltageDrop(oppAddr.dir, false)) > epsilon){
//						oppElement.TriggerEmergency();
//						failed = true;
//					}
//				}
//			}
//		}
//		if (failed) return false;
		
		
		
		loopCurrents = new float[loops.Count];
		if (I.GetLength(0) != 0){
			for (int i = 0; i < loops.Count; ++i){
				loopCurrents[i] = (float)I[i,0];
			}
		}
		// all went well
		return true;
		
	}	
	
	
	void StoreCurrentsInComponents(){
		foreach (GameObject componentGO in graph.allComponents){
			AVOWComponent component = componentGO.GetComponent<AVOWComponent>();
			float totalCurrent = 0;
			foreach (AVOWComponent.LoopRecord record in component.loopRecords){
				totalCurrent += loopCurrents[record.loopId] * (record.isForward ? 1 : -1);
			}
			component.fwCurrent = totalCurrent;
		}
	}
	
	void DebugPrintComponentCurrents(){
		Debug.Log ("Print component currents");
		foreach (GameObject componentGO in graph.allComponents){
			AVOWComponent component = componentGO.GetComponent<AVOWComponent>();
			Debug.Log ("Component " + component.GetID() + ": from " + component.node0.GetID () + " to " + component.node1.GetID() + ": current = " + component.fwCurrent + "A");
			
		}
	}
	
	void CalcVoltages(){
		graph.ClearVisitedFlags();

		// First find the voltage source
		AVOWComponent cell = graph.FindVoltageSource().GetComponent<AVOWComponent>();
		
		// Set up the voltage accross it
		cell.node0.voltage = 0;
		
		// We have now visited this cell and the first node. 
		cell.visited = true;
		cell.node0.visited = true;		
		Stack<AVOWGraph.Node> nodeStack = new Stack<AVOWGraph.Node>();
		
		nodeStack.Push(cell.node0);
		
		while(nodeStack.Count != 0){
			AVOWGraph.Node lastNode = nodeStack.Peek();
			
			//Find a component attached to this node which has not yet been visited
			GameObject go = lastNode.components.Find (obj => !obj.GetComponent<AVOWComponent>().visited);
			
			// If we found one, then work out the voltage on the other end and either push it onto the stack (we it is a new node)
			// or just check we are being consistent if we have been there before
			if (go != null){
				AVOWComponent thisComponent = go.GetComponent<AVOWComponent>();
				thisComponent.visited = true;
				
				// Calc the voltage at the other end of this component
				float voltageChange = thisComponent.GetCurrent(lastNode) * thisComponent.GetResistance();
				AVOWGraph.Node nextNode = thisComponent.GetOtherNode(lastNode);
				float nextNodeVoltage = lastNode.voltage - voltageChange;
				
				// If we have not yet visited this node, then set the voltage on it and add it to the stack
				if (!nextNode.visited){
					nextNode.voltage = nextNodeVoltage;
					nextNode.visited = true;
					nodeStack.Push (nextNode);	
				}
				// Otherwise, assert that the voltage is the same as what we just caluclated
				else{
					if (!MathUtils.FP.Feq(nextNode.voltage, nextNodeVoltage)){
						Debug.LogError ("Voltages accross components are not consitent");
					}
				}
			}
			// If we failed to find an unvisited component, then pop this node off the stack (as there is
			// nothing more we can do here) and work on the node below it
			else{
				nodeStack.Pop ();
			}
		}
	}
	
	void DebugPrintVoltages(){
		Debug.Log ("Printing voltages");

		foreach (AVOWGraph.Node node in graph.allNodes){
			Debug.Log ("Node " + node.GetID() + ": " + node.voltage + "V");
		}
	}
	
//	class GOComparer : IComparer<GameObject>
//	{
//		public int Compare(GameObject obj1, GameObject obj2)
//		{
//			return obj1.GetComponent<AVOWComponent>().hOrder.CompareTo (obj2.GetComponent<AVOWComponent>().hOrder);
//		}
//	}
	
	bool LayoutHOrder(){
		// Figure out the width of each node. The current flowing in == current flowing out == width
		foreach (AVOWGraph.Node node in graph.allNodes){
			float currentIn = 0;
			float currentOut = 0;
			foreach (GameObject go in node.components){
				AVOWComponent component = go.GetComponent<AVOWComponent>();
				float current = component.GetCurrent(node);
				if (current > 0) currentOut += Mathf.Abs(current);
				else currentIn += Mathf.Abs(current);
			}
			node.hWidth = currentIn;
		}
		
		
		// Order all the components in the most lists according to their H ordering
		foreach (AVOWGraph.Node node in graph.allNodes){
			//node.components.Sort (new GOComparer());
			node.components.Sort ((obj1, obj2) => (obj1.GetComponent<AVOWComponent>().hOrder.CompareTo (obj2.GetComponent<AVOWComponent>().hOrder)));
			
		}
		// Order the "allComponents" list in the same way
		graph.allComponents.Sort ((obj1, obj2) => (obj1.GetComponent<AVOWComponent>().hOrder.CompareTo (obj2.GetComponent<AVOWComponent>().hOrder)));

		// Now traverse the graph setting up the horizontal positions of each component
		// We say a component is "visited" if it has been positioned
		// We say a node is "visited" if all the components in it have been positioned
		// We add a node to the queue if we can position all its components
		graph.ClearLayoutFlags();
		
		// Find the first component in the H ordering- we know this should be placed as far to the 
		// left as possible and both of the nodes it is attached to wil also be as far to the left as
		// possible
		AVOWComponent firstComponent = graph.allComponents[0].GetComponent<AVOWComponent>();
		firstComponent.h0 = 0;
		firstComponent.h1 = Mathf.Abs(firstComponent.fwCurrent);
		
		// Set up the nodes at either end and start the counters on them
		firstComponent.node0.h0 = 0;
		firstComponent.node0.hIn = 0;
		firstComponent.node0.hOut = 0;
		firstComponent.node0.hasBegunLayout = true;
		
		firstComponent.node1.h0 = 0;
		firstComponent.node1.hIn = 0;
		firstComponent.node1.hOut = 0;
		firstComponent.node1.hasBegunLayout = true;
		
		
		// Layout any component which are also between exactly these nodes
		int lastComponentsDone = 0;
		int componentsDone = 0;
		if (firstComponent.GetCurrent (firstComponent.node0) > 0)
			componentsDone += LayoutAllBetweenOrdered(firstComponent.node0, firstComponent.node1);
		else
			componentsDone += LayoutAllBetweenOrdered(firstComponent.node1, firstComponent.node0);
		
		while (componentsDone < graph.allComponents.Count){
			// Debug - this should never happen
			if (lastComponentsDone == componentsDone){
				Debug.LogError ("Failed to process any compnents - but not finished either");
				return false;
			}
			lastComponentsDone = componentsDone;
			// Go through each of the components in order
			for (int i = 0; i < graph.allComponents.Count; ++i){
				AVOWComponent component = graph.allComponents[i].GetComponent<AVOWComponent>();
				
				// If we have already been visited, then nothing to do
				if (component.visited) continue;
				
				// If this is the lowest ordered component on a node and is the first component on the other node
				// then we can set up the "other" node
				if (IsLowestComponent(component, component.node0) && IsFirstComponentOn(component, component.node1)){
					if (component.GetCurrent (component.node0) > 0){
						component.node1.hOut = component.node0.hOut;
						component.node1.hIn = component.node0.hOut;
						component.node1.hasBegunLayout = true;
					}
					else{
						component.node1.hOut = component.node0.hIn;
						component.node1.hIn = component.node0.hIn;
						component.node1.hasBegunLayout = true;
					}
					
				}
				else if (IsLowestComponent(component, component.node1) && IsFirstComponentOn(component, component.node0)){
					if (component.GetCurrent (component.node1) > 0){
						component.node0.hOut = component.node1.hOut;
						component.node0.hIn = component.node1.hOut;
						component.node0.hasBegunLayout = true;
					}
					else{
						component.node0.hOut = component.node1.hIn;
						component.node0.hIn = component.node1.hIn;
						component.node0.hasBegunLayout = true;
					}
				}
				// If we are the lowest component on one node and if our placement position on that node is also the next placable position on the
				// the "other" node, then we can place it
				if (IsLowestComponent(component, component.node0)){
					// Come from node 0
					if (component.GetCurrent(component.node0) > 0){
						if (MathUtils.FP.Feq (component.node0.hOut, component.node1.hIn)){
							componentsDone += LayoutAllBetweenOrdered(component.node0, component.node1);
						}
					}
					// come from node1
					else{
						if (MathUtils.FP.Feq (component.node0.hIn, component.node1.hOut)){
							componentsDone += LayoutAllBetweenOrdered(component.node1, component.node0);
						}
					}
				}
				else if (IsLowestComponent(component, component.node1)){
					// Come from node 1
					if (component.GetCurrent(component.node1) > 0){
						if (MathUtils.FP.Feq (component.node1.hOut, component.node0.hIn)){
							componentsDone += LayoutAllBetweenOrdered(component.node1, component.node0);
						}
					}
					else{
						if (MathUtils.FP.Feq (component.node1.hIn, component.node0.hOut)){
							componentsDone += LayoutAllBetweenOrdered(component.node0, component.node1);
						}
					}
				}
				
			}
			
		}
		return true;
		
	}
	
	bool IsFirstComponentOn(AVOWComponent component, AVOWGraph.Node node){
		// If we have begun the layout, then there must already be a component here
		if (node.hasBegunLayout) return false;
		
		// Test if this component is the lowest order component in this direction on this node
		for (int i = 0; i < node.components.Count; ++i){
			AVOWComponent testComponent = node.components[i].GetComponent<AVOWComponent>();
			
			// if either both positive, or both negative - then we can test if they are equal
			if (testComponent.GetCurrent (node) * component.GetCurrent(node) > 0){
				return testComponent == component;
			}
		}
		// If we got to here then this component doesn't exist on this node - which is an error
		Debug.LogError ("This component does not exist on this node");
		return false;
	}
	
	bool IsLowestComponent(AVOWComponent component, AVOWGraph.Node node){
		// If ths node has not started to be layed out, then we do not know where it is
		if (!node.hasBegunLayout) return false;
		
		// if this is the lowest ordered component which has not yet been layed out on this node, then
		// we can lay it out (dealing with in and out components seperately
		
//		// DEBUG
//		for (int i = 0; i < node.components.Count; ++i){
//			AVOWComponent testComponent = node.components[i].GetComponent<AVOWComponent>();
//			
//			string idString = testComponent.GetID();
//			Debug.Log (idString);
//		}
		
		
		
		// If this component is flowing OUT of this node
		if (component.GetCurrent (node) > 0){
			for (int i = 0; i < node.components.Count; ++i){
				AVOWComponent testComponent = node.components[i].GetComponent<AVOWComponent>();
				
				if (!testComponent.visited && testComponent.GetCurrent (node) > 0 && testComponent.hOrder < component.hOrder) return false;
				if (testComponent == component) return true;
			}
		}
		else{
			for (int i = 0; i < node.components.Count; ++i){
				AVOWComponent testComponent = node.components[i].GetComponent<AVOWComponent>();
				
				if (!testComponent.visited && testComponent.GetCurrent (node) <= 0 && testComponent.hOrder < component.hOrder) return false;
				if (testComponent == component) return true;
			}
		}

		// We should neve get here
		Debug.LogError("Failed to find component in Node");
		return false;
		
	}
	
	int LayoutAllBetweenOrdered(AVOWGraph.Node nodeA, AVOWGraph.Node nodeB){
		int count = 0;
		
		for (int i = 0; i < graph.allComponents.Count; ++i){
			AVOWComponent component = graph.allComponents[i].GetComponent<AVOWComponent>();
				
			float absCurrent = Mathf.Abs(component.fwCurrent);
			if (!component.visited && component.IsBetweenNodes(nodeA, nodeB) && (component.GetCurrent (nodeA) > 0)){
				component.h0 = nodeA.hOut;
				nodeA.hOut += absCurrent;
				nodeB.hIn += absCurrent;
				component.h1 = nodeA.hOut;

				count++;
				component.visited = true;
				component.isLayedOut = true;
			}
		}
		return count;
		
	}
	
	void DebugPrintHOrder(){
		Debug.Log ("Printing HOrder Nodes");
		foreach(AVOWGraph.Node node in graph.allNodes){
			Debug.Log ("Node " + node.GetID() + ": h0 = " + node.h0 + ", hWidth = " + node.hWidth + ", visited = " + node.visited);
		}
		
		Debug.Log ("Printing HOrder Components");
		foreach(GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			Debug.Log ("Component " + component.GetID() + ": h0 = " + component.h0 + ", h1 = " + component.h1 + ", visited = " + component.visited);
		}
	}
	
	// Records the position of the mouse as a local position in one of the components
	void RecordMousePos(){
		// Find mouse pos in screen space
		Vector3 screenPos = Input.mousePosition;
		screenPos.z = 0;
		
		// Find mouse pos in world space
		mouseWorldPos = Camera.main.ScreenToWorldPoint( screenPos);
		mouseWorldPos.z = 0;
		
		// check which component we are over
		mouseOverComponent = null;
		// Keep track of global bounds of entire diagram (as we will use this if not over ay specific component)
		float xMin = 100;
		float yMin = 100;
		float xMax = -1;
		float yMax = -1;
		foreach(GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			
			
			// If this component has never been layed out, then ignore
			if (!component.isLayedOut) continue;
			
			float lowVoltage = Mathf.Min (component.node0.voltage, component.node1.voltage);
			float highVoltage = Mathf.Max (component.node0.voltage, component.node1.voltage);
			float lowCurrent = component.h0;
			float highCurrent = component.h1;
			

			
			
			// Keep track of global bounds
			xMin = Mathf.Min (xMin, lowCurrent);
			yMin = Mathf.Min (yMin, lowVoltage);
			xMax = Mathf.Max (xMax, highCurrent);
			yMax = Mathf.Max (yMax, highVoltage);
			
			// Only register if over a resistor
			if (component.type == AVOWComponent.Type.kLoad &&
				mouseWorldPos.x > lowCurrent && 
			    mouseWorldPos.x < highCurrent && 
			    mouseWorldPos.y > lowVoltage && 
			    mouseWorldPos.y < highVoltage){
			    
			    if (mouseOverComponent != null){
			    	Debug.Log ("Error1 - Our mouse is over two components");
			    }
				mouseOverComponent = component;
				mouseOverXProp = (mouseWorldPos.x -  lowCurrent) / (highCurrent - lowCurrent);
				mouseOverYProp = (mouseWorldPos.y -  lowVoltage) / (highVoltage - lowVoltage);
					
			}
		}
		if (mouseOverComponent == null){
			mouseOverXProp = (mouseWorldPos.x -  xMin) / (xMax - xMin);
			mouseOverYProp = (mouseWorldPos.y -  yMin) / (yMax - yMin);
		}
		
	}

	
	void CalcMouseOffset(){
		float xMin = 100;
		float yMin = 100;
		float xMax = -1;
		float yMax = -1;
		//Figure out world posiiton of the location in the diagram where the mouse was
		if (mouseOverComponent != null){
			xMin = mouseOverComponent.h0;
			xMax = mouseOverComponent.h1;
			yMin = Mathf.Min (mouseOverComponent.node0.voltage, mouseOverComponent.node1.voltage);
			yMax = Mathf.Max (mouseOverComponent.node0.voltage, mouseOverComponent.node1.voltage);
			// If a voltage source then need to mirror it all
//			if (mouseOverComponent.type == AVOWComponent.Type.kVoltageSource){
//				float temp = xMin;
//				xMin = -xMax;
//				xMax = -temp;
//			}		
			
		}
		else{

			foreach(GameObject go in graph.allComponents){
				AVOWComponent component = go.GetComponent<AVOWComponent>();
				
				float lowVoltage = Mathf.Min (component.node0.voltage, component.node1.voltage);
				float highVoltage = Mathf.Max (component.node0.voltage, component.node1.voltage);
				float lowCurrent = component.h0;
				float highCurrent = component.h1;
				
					
				
				// Keep track of global bounds
				xMin = Mathf.Min (xMin, lowCurrent);
				yMin = Mathf.Min (yMin, lowVoltage);
				xMax = Mathf.Max (xMax, highCurrent);
				yMax = Mathf.Max (yMax, highVoltage);
				
			}
		}
		
		Vector3  worldPos = new Vector3(xMin + mouseOverXProp * (xMax - xMin), yMin +mouseOverYProp * (yMax - yMin), 0);
		worldPos.z = 0;
//		Vector3 screenPos = Camera.main.WorldToScreenPoint( worldPos);
//		screenPos.z = 0;
//		
		Vector3 offset = worldPos - mouseWorldPos;
		Camera.main.gameObject.GetComponent<AVOWCamControl>().AddOffset(offset);
		
//		if (offset.sqrMagnitude != 0){
//			Debug.Log("***Moving****");
//			Debug.Log("prop = " + mouseOverXProp + ", " + mouseOverYProp);
//			Debug.Log ("yMin = " + yMin + " yMax = " + yMax);
//			Debug.Log("worldPos = " + worldPos.x + ", " + worldPos.y);
//			Debug.Log("mouseWorldPos = " + mouseWorldPos.x + ", " + mouseWorldPos.y);
//			Debug.Log("offset = " + offset.x + ", " + offset.y);
//		}
		
		
	}
	
}
