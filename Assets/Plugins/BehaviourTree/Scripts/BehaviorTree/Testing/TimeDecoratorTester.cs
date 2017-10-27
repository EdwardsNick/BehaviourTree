using UnityEngine;
using System.Collections;
using BehaviorTree;
using System.Xml;
using System;

namespace BehaviorTreeTests {

	[Serializable]
	public class TimeDecoratorTester : TreeNode {

		public int time;

		public TimeDecoratorTester() {
			time = 0;
		}

		public override void setup() {
			if (time == 0) time = (int)TreeDataDict.treeData[NodeName];
		}

		public override IEnumerator tick() {
			yield return StartCoroutine(timer());
		}

		public override void tearDown(out bool childReturn) {
			childReturn = true;
		}

		public IEnumerator timer() {
			yield return new WaitForSeconds(time);
		}


		/*Depreciated
		public override void writeXmlAttributes(XmlTextWriter writer) {
			writer.WriteAttributeString("Name", NodeName);
			writer.WriteAttributeString("time", time.ToString());
		}

		public override void parseXmlAttributes(XmlNodeReader reader) {
			NodeName = reader.GetAttribute("Name");
			time = Convert.ToInt32(reader.GetAttribute("time"));
		}*/

	}
}
