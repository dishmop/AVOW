using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class AVOWSim : MonoBehaviour {
	public static AVOWSim singleton = null;
	
	public GameObject graphGO;
	
	public class LoopElement{
		public LoopElement(AVOWComponent component, AVOWGraph.Node fromNode){
			this.component = component;
			this.fromNode = fromNode;
		}
			
		public AVOWComponent component;
		public AVOWGraph.Node fromNode;
	}
	
	public List<List<LoopElement>> loops;
	
	// Current solving	
	double epsilon = 0.0001;
	float[]						loopCurrents;
	
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
	void FixedUpdate(){
		FindLoops();
		DebugPrintLoops();
		
		RecordLoopsInComponents();
		SolveForCurrents();
		DebugPrintLoopCurrents();
		
		StoreCurrentsInComponents();
		DebugPrintComponentCurrents();
		
		CalcVoltages();
		DebugPrintVoltages();
		
		LayoutHOrder();
		DebugPrintHOrder();
		
		//AppHelper.Quit();
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
	
	
	// Fins a set of indendpent loops in the graph
	void FindLoops(){
		loops = new List<List<LoopElement>>();
		AVOWGraph graph = graphGO.GetComponent<AVOWGraph>();

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
		AVOWGraph graph = graphGO.GetComponent<AVOWGraph>();
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
		AVOWGraph graph = graphGO.GetComponent<AVOWGraph>();
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
		AVOWGraph graph = graphGO.GetComponent<AVOWGraph>();
		Debug.Log ("Print component currents");
		foreach (GameObject componentGO in graph.allComponents){
			AVOWComponent component = componentGO.GetComponent<AVOWComponent>();
			Debug.Log ("Component " + component.GetID() + ": from " + component.node0.GetID () + " to " + component.node1.GetID() + ": current = " + component.fwCurrent + "A");
			
		}
	}
	
	void CalcVoltages(){
		AVOWGraph graph = graphGO.GetComponent<AVOWGraph>();
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
		AVOWGraph graph = graphGO.GetComponent<AVOWGraph>();
		
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
	
	void LayoutHOrder(){
		AVOWGraph graph = graphGO.GetComponent<AVOWGraph>();
		
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
		graph.ClearVisitedFlags();
		
		// Find the first component in the H ordering and start with a node connected to it since both of its connecting nodes
		// must be on the far left of the diagram
		AVOWComponent firstComponent = graph.allComponents[0].GetComponent<AVOWComponent>();
		AVOWGraph.Node firstNode = firstComponent.node0;
		firstNode.h0 = 0;
		
		// Rule 1: If a node is in the queu then we know its h0 (and this has been set)
		// Rule 2: If a node has been visited, then we have set the h0 and h1 of all components attached to it
		
		// Set up a stack so we know which nodes we have visited
		Queue<AVOWGraph.Node> nodeQueue = new Queue<AVOWGraph.Node>();
		nodeQueue.Enqueue (firstComponent.node0);
		
		while(nodeQueue.Count != 0){
			// Take the next node to be processed
			AVOWGraph.Node thisNode = nodeQueue.Dequeue ();
			
			// Go through the components attached to this node in order
			float hIn = thisNode.h0;
			float hOut = thisNode.h0;
			for (int i = 0; i < thisNode.components.Count; ++i){
				// Arrange them along the width of the node
				AVOWComponent component = thisNode.components[i].GetComponent<AVOWComponent>();
				if (component.GetCurrent (thisNode) > 0){
					component.h0 = hOut;
					component.h1 = hOut + Mathf.Abs(component.fwCurrent);
					hOut = component.h1;
				}
				else{
					component.h0 = hIn;
					component.h1 = hIn + Mathf.Abs(component.fwCurrent);
					hIn = component.h1;
				}
				component.visited = true;
				
				// Find the node at the other side of this connection and - if this connection is the
				// lowest h-order component, then we can process it - so set up its h0 and add it to the queue
				AVOWGraph.Node otherNode = component.GetOtherNode(thisNode);
				if (!otherNode.visited && otherNode.components[0].GetComponent<AVOWComponent>() == component){
					otherNode.h0 = component.h0;
					nodeQueue.Enqueue(otherNode);
				}
				
			}
			thisNode.visited = true;
		}
	}
	
	void DebugPrintHOrder(){
		AVOWGraph graph = graphGO.GetComponent<AVOWGraph>();
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
}
