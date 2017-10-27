using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

namespace BehaviorTree {


	/*
	This class is a decorator which waits a given number of seconds and
	if the child takes more that the given number of seconds, it returns
	a fail status.

	Fields:
	timeOut: The number of seconds to wait
	timedOut: Whether we reached the number of seconds to second out
	*/
	[Serializable]
	public class TimedDecorator : DecoratorNode {

		private int timeout;
		private bool timedOut;

		public int Timeout {
			get {
				return timeout;
			}

			set {
				timeout = value;
			}
		}

		public TimedDecorator() {
			Timeout = 0;
			timedOut = false;
		}

		/*
		Sets everything up
		*/
		public override void setup() {
			if (Timeout == 0) Timeout = (int)TreeDataDict.treeData[NodeName];
			timedOut = false;
		}


		/*
		Simply starts the second counter and then starts the child execution
		*/
		public override IEnumerator tick() {
			StartCoroutine(timer());
			yield return StartCoroutine(Child.Execute());
		}


		/*
		If we reached the timeOut return false
		*/
		public override void tearDown(out bool childReturn) {
			if (timedOut) {
				childReturn = false;
			}
			else {
				childReturn = this.childStatus;
			}
		}

		/*
		Counts the number of seconds until we reach the timeout
		*/
		public IEnumerator timer() {
			yield return new WaitForSeconds(Timeout);
			timedOut = true;
		}


		/*
		Draws the timeout field so we can see and modify it in the editor
		*/
		protected override void drawProperties() {
			base.drawProperties();
			timeout = EditorGUILayout.IntField("Timeout", timeout);
		}

		/*Depreciated
		public override void writeXmlAttributes (XmlTextWriter writer) {
			writer.WriteAttributeString("Name", NodeName);
			writer.WriteAttributeString("timeout", timeout.ToString());
		}
		
		public override void parseXmlAttributes(XmlNodeReader reader) {
			NodeName = reader.GetAttribute("Name");
			timeout = Convert.ToInt32(reader.GetAttribute("timeout"));
		}*/
	}

}
