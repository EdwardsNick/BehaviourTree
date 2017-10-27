using System;
using System.Collections.Generic;

namespace BehaviorTree {

	/*
	This is the base class for all decorator nodes. It limits the number of children to one
	and overrides the ParentNode functions so it will still act correctly as a ParentNode
	*/

	[Serializable]
	public abstract class DecoratorNode : ParentNode {

		
		public DecoratorNode() {
			children = new List<TreeNode>();
		}

		/*
		This is a getter/Setter for a non-existant field child. It gets and sets the value of our one
		child from the Children list inherited from our ParentNode. This hides the fact that we inherit
		from ParentNode to an implementer
		*/
		public TreeNode Child {
			get {
				if (children.Count > 0) {
					return children[0];
				}
				else return null;
			}

			set {
				if (children.Count == 0) {
					children.Add(value);
				}
				else {
					children[0] = value;
				}
			}
		}

		/*
		This function overrides the addChild function of ParentNode to represent the fact that we only
		have one child.

		Parameters: 
		child: The node to add as a child
		*/
		public override void addChild(TreeNode child) {
			if (children.Count == 0) {
				children.Add(child);
			}
			else {
				children[0] = child;
			}
		}


		/*
		This function overrides the removeChild function of ParentNode to represent the fact that we only
		have one child

		Parameters:
		child: The child node to remove if it is our child
		*/
		public override void removeChild(TreeNode child) {
			if (children.Count > 0) {
				if (children[0] == child) {
					children.RemoveAt(0);
				}
			}
		}
	}

}
