using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BehaviorTree {

	/*
	This is a selector class which will choose its children at random and
	if any child returns success, we will immediately return success

	Fields:
	currentIndex: The currentIndex of the node we are activating
	availableIndexes: The list of child nodes that we have not used yet
		This is a necessary list as we only want to traverse each node once
		albeit in a random order
	*/
	[Serializable]
	public class RandomSelector : ParentNode {

		private int currentIndex;
		private List<int> availableIndexes;

		/*
		Sets up the available indexes list and the currentIndex
		*/
		public override void setup () {
			if(Children.Count < 1) childStatus = false;
			availableIndexes = new List<int>(Children.Count);
			for (int i = 0; i < Children.Count; i++) {
				availableIndexes.Add(i);
			}
			currentIndex = 0;
		}

		/*
		Goes through the available indexes and chooses one at random to execute next.
		If the child returns success then we break out immediately
		*/
		public override IEnumerator tick() {
			while (availableIndexes.Count > 0) {
				currentIndex = (int)Mathf.Floor(UnityEngine.Random.value * availableIndexes.Count);
				yield return StartCoroutine(Children[availableIndexes[currentIndex]].Execute());
				if (childStatus) break;
				availableIndexes.RemoveAt(currentIndex);
			}
		}

		/*
		Returns the childReturn. If we didn't have any successes this wil still be false
		*/
		public override void tearDown (out bool childReturn) {
			childReturn = childStatus;
		}

	}

}
