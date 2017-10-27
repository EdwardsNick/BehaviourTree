using System.Collections;
using BehaviorTree;
using System;

namespace BehaviorTreeTests {

	[Serializable]
	public class FrameDecoratorTester : TreeNode {

		public int framesToCount;

		public FrameDecoratorTester() {
			framesToCount = 0;
		}

		public override void setup() {
			if (framesToCount == 0) framesToCount = (int)TreeDataDict.treeData[NodeName];
		}

		public override IEnumerator tick() {
			for (int i = 0; i < framesToCount; i++) {
				yield return null;
			}
		}

		public override void tearDown(out bool childReturn) {
			childReturn = true;
		}

		/*Depreciated
		public override void writeXmlAttributes(XmlTextWriter writer) {
			writer.WriteAttributeString("Name", NodeName);
			writer.WriteAttributeString("framesToCount", framesToCount.ToString());
		}

		public override void parseXmlAttributes(XmlNodeReader reader) {
			NodeName = reader.GetAttribute("Name");
			framesToCount = Convert.ToInt32(reader.GetAttribute("framesToCount"));
		}*/
	}
}
