using System;
using System.Collections;

namespace BehaviorTree {

	/*
	This class is the implementation the start node for any behavior tree. It simply interates over all children continuously

	Fields:
	currentIndex: The index for the current child to be executed
	*/

	[Serializable]
	public class BTStartNode : ParentNode {

		private int currentIndex;

		/* 
		Sets up the current index
		*/
		public override void setup() {
			currentIndex = 0;
		}

		/* 
		Ensures that there are children and then continously cycles through each child in turn forever (Realistically, until the gameObject is destroyed).
		*/
		public override IEnumerator tick() {
			while(true) {
				if (Children.Count > 0) {
					yield return StartCoroutine(Children[currentIndex].Execute());
					currentIndex++;
					if (currentIndex >= Children.Count) {
						currentIndex = 0;
					}
				}
				//if we have no children, wait
				else {
					yield return null;
				}
			}
		}


		/* 
		Theorectically Unreachable
		*/
		public override void tearDown(out bool childReturn) {
			childReturn = true;
		}

	}

}
