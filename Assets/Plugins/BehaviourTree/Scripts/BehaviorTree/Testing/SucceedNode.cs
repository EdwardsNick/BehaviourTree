using System.Collections;
using BehaviorTree;
using System;

namespace BehaviorTreeTests {

	[Serializable]
	public class SucceedNode : TreeNode {

		public override void setup() { }

		public override IEnumerator tick() {
			yield return null;
		}

		public override void tearDown(out bool childReturn) {
			childReturn = true;
		}
	}

}
