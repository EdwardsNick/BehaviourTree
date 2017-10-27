using System;
using System.Collections;


namespace BehaviorTree {


	/*
	A basic sequence class which excutes it's children in order and if all succeed
	we return our own success

	Fields:
	listIndex: The index of the currently executing child
	*/
	[Serializable]
	public class SequenceNode : ParentNode {


		private int listIndex;


		/*
		Sets up the listIndex and checks that we have children
		*/
		public override void setup () {
			if (Children.Count < 1) childStatus = false;
			listIndex = 0;
		}


		/*
		Goes through all children and breaks if we get a failure
		*/
		public override IEnumerator tick () {
			while (listIndex < Children.Count) {
				yield return StartCoroutine(Children[listIndex].Execute());
				if (!childStatus) break;
				listIndex++;
			}
		}

		/*
		Returns the status of our children
		*/
		public override void tearDown (out bool childStatus) {
			childStatus = this.childStatus;
		}
}

}
