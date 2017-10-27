using UnityEngine;
using System.Collections.Generic;
using System;

namespace BehaviorTree {

	/*
	This is a scriptable object class that contains the Dictionary for our
	BehaviorTrees. Scriptable objects serialize as references correctly so
	all the nodes of the BehavoirTree which reference this object will all 
	reference the same object after serialization/deserialization. And that 
	(this) object will have the dictionary reference.

	Fields:
	treeData: The dictionary reference for a behavior tree
	*/
	[Serializable]
	public class TreeDict : ScriptableObject {

		public Dictionary<string, object> treeData;

		public TreeDict() {
			treeData = new Dictionary<string, object>();
		}
	}
}
