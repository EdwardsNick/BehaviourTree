using System.Collections;
using System;

namespace BehaviorTree {

	/*
	This class is a decorator class which simply executes the child node
	and returns success no matter what the child returns
	*/
	[Serializable]
	public class SucceederDecorator : DecoratorNode {


		public override void setup() {}

		public override IEnumerator tick() {
			yield return StartCoroutine(Child.Execute());
		}

		public override void tearDown(out bool childReturn) {
			childReturn = true;
		}
	}
}
