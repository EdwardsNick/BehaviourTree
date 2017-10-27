using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace BehaviorTree {

	/*
	This class represents all nodes that can have children. It controls the adding and
	removing of children, as well as the ability to draw relationships between nodes

	Fields:
	children: A Generic List of TreeNodes which represent our children
	*/
	[Serializable]
	public abstract class ParentNode : TreeNode {

		[SerializeField]
		protected List<TreeNode> children;


		/*
		Returns a copy of the children to prevent Concurrent modifications exceptions
		during runtime when the list is change while this node is active
		*/
		public List<TreeNode> Children
		{
			get
			{
				return copyChildren();
			}

			set
			{
				children = value;
			}
		}

		/*
		Returns the actual list of children if we need to make modifications to 
		the list itself. 
		*/
		public List<TreeNode> childrenActual() {
			return children;
		}

		public ParentNode() {
			children = new List<TreeNode>();
		}

		/*
		Adds a node to the list of children. Virtual so the decorator class can override

		Parameters:
		child: The node to add
		*/
		public virtual void addChild(TreeNode child) {
			children.Add(child);
		}

		/*
		Removes the given node from the list if it exists 

		Parameters:
		child: The node to remove
		*/
		public virtual void removeChild(TreeNode child) {
			children.Remove(child);
		}

		/*
		Removes all children by recreating the list (GC should grab this at some point)
		*/
		public void removeAllChildren() {
			children = new List<TreeNode>();
		}

		/*
		Returns the number of children
		*/
		public int childCount() {
			return children.Count;
		}

		/*
		Creates a copy of the children list to protect the original. I couldn't find a
		built in copy constructor so I wrote this simple one

		Returns:
		A copy of the children nodes list
		*/
		public List<TreeNode> copyChildren() {
			List<TreeNode> childs = new List<TreeNode>();
			foreach (TreeNode child in children) {
				//Debug.Log("Adding " + child);
				childs.Add(child);
			}
			return childs;
		}

		/*
		This function recursively draws all relations for each node in the tree
		*/
		public void drawRelations() {
			foreach (TreeNode child in Children) {
				if (child is LeafNode) {
					drawRelation(WindowRect, child.WindowRect, child.isActive, true);
				}
				else {
					drawRelation(WindowRect, child.WindowRect, child.isActive, false);
				}
			}
		}


		/*
		Draws lines between the start rect (node window) to the end rect (node window),
		and colors the line based on whether the node is active

		Parameters:
		start: The rect to being drawing from
		end: The rect to end drawing to
		isActive: Whether the nodes represented by the rects are active
		*/
		private void drawRelation(Rect start, Rect end, bool isActive, bool leaf) {
			
			//Start position calculated as the middle bottom of first rect
			Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);

			//End position calculateed as the middle top of the end rect
			Vector3 endPos = new Vector3(end.x + end.width / 2, end.y + 2, 0);

			if (isActive) {
				if (leaf) {
					Handles.color = Color.green;
				}
				else {
					Handles.color = Color.yellow;
				}
			}
			else {
				Handles.color = Color.white;
			}

			Handles.DrawLine(startPos, endPos);
			startPos.x -= 1;
			endPos.x -= 1;
			Handles.DrawLine(startPos, endPos);
			startPos.x += 2;
			endPos.x += 2;
			Handles.DrawLine(startPos, endPos);
		}
	}
}
