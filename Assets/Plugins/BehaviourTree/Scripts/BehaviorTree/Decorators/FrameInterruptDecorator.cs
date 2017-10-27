using System;
using System.Collections;
using UnityEditor;

namespace BehaviorTree {

	/*
	This class is a decorator which waits a given number of frames and
	if the child takes more that the given number of frames, it returns
	a fail status.

	Fields:
	frameOut: The number of frames to wait
	framedOut: Whether we reached the number of frames to frame out
	currentFrame: The current frame number
	*/
	[Serializable]
	public class FrameInterruptDecorator : DecoratorNode {

		public int frameOut; 
		private bool framedOut;
		private int currentFrame;

		public int FrameOut {
			get {
				return frameOut;
			}

			set {
				frameOut = value;
			}
		}

		public FrameInterruptDecorator() {
			FrameOut = 0;
			framedOut = false;
			currentFrame = 0;
		}

		/*
		Sets everything up
		*/
		public override void setup() {
			//if the frameOut wasn't set, grab the data from the dictionary
			if (FrameOut == 0) FrameOut = (int)TreeDataDict.treeData[NodeName];
			framedOut = false;
			currentFrame = 0;
		}

		/*
		Simply starts the frame counter and then starts the child execution
		*/
		public override IEnumerator tick() {
			StartCoroutine(timer());
			yield return StartCoroutine(Child.Execute());
		}

		/*
		If we reached the frameOut return false
		*/
		public override void tearDown(out bool childReturn) {	
			if (framedOut) {
				childReturn = false;
			}
			else {
				childReturn = childStatus;
			} 
		}

		/*
		Counts the number of frames until we reach the frameout
		*/
		public IEnumerator timer() {
			while (currentFrame < FrameOut) {
				currentFrame++;
				yield return null;
			}
			framedOut = true;
		}
		
		/*
		Draws the frameout field so we can see and modify it in the editor
		*/
		protected override void drawProperties () {
			base.drawProperties();
			frameOut = EditorGUILayout.IntField("Frame Out", frameOut);
		}


		/*Deprieciated 
		public override void writeXmlAttributes(XmlTextWriter writer) {
			writer.WriteAttributeString("Name", NodeName);
			writer.WriteAttributeString("frameOut", frameOut.ToString());
		}

		public override void parseXmlAttributes (XmlNodeReader reader) {
			NodeName = reader.GetAttribute("Name");
			frameOut = Convert.ToInt32(reader.GetAttribute("frameOut"));
		}
		*/

	}
}
