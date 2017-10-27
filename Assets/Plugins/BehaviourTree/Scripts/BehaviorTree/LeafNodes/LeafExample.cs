using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

namespace BehaviorTree {

	/*
	This class is a leaf node that waits a set number of time and 
	then returns success

	Parameters:
	wait: The amount of time to wait
	ret: The value we will return at the end
	*/
	[Serializable]
	public class LeafExample : LeafNode {

		public int wait = 0;
		public bool ret = true;

		public override void setup() { }

		public override IEnumerator tick() {
			yield return new WaitForSeconds(wait);

		}

		public override void tearDown(out bool children) {
			children = ret;
			return;
		}

		/*
		Draw our two fields in editor
		*/
		protected override void drawProperties () {
			wait = EditorGUILayout.IntField("Wait", wait);

			ret = EditorGUILayout.Toggle("Return: ", ret);
		}

	}

}
