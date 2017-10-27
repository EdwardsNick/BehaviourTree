using System;
using System.Collections;

namespace BehaviorTree {


	/*	
	This class is a decorator class which simply executes the child node
	and returns the opposite status of what the child was
	*/
	[Serializable]
	public class InverterDecorator : DecoratorNode {

		public override void setup() { }

		public override IEnumerator tick() {
			yield return StartCoroutine(Child.Execute());
		}

		public override void tearDown(out bool childReturn) {
			childReturn = !childStatus;
		}
	}
}
