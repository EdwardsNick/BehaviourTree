using UnityEngine;
using System;
using UnityEditor;

namespace BehaviorTree {

	/*
	This class controls the entire behavior tree. It controls adding and removing nodes
	as well as various other utility variables and the starting node

	Fields:
	treeDataDict: A reference to the scriptable object containing the tree's dictionary
	startNode: The initial node in the tree that will we executed on play
	
	*/
	[Serializable]
	public class BehaviorTree : MonoBehaviour {

		[SerializeField]
		private TreeDict treeDataDict;

		[SerializeField]
		private BTStartNode startNode;

		[SerializeField]
		private int depth;

		[SerializeField]
		private TreeNode deepNode;


		public TreeDict TreeDataDict
		{
			get
			{
				return treeDataDict;
			}

			set
			{
				treeDataDict = value;
			}
		}

		public BTStartNode StartNode
		{
			get
			{
				return startNode;
			}

			set
			{
				startNode = value;
			}
		}

		[SerializeField]
		public int Depth
		{
			get
			{
				return depth;
			}

			set
			{
				depth = value;
			}
		}

		[SerializeField]
		public TreeNode DeepNode
		{
			get
			{
				return deepNode;
			}

			set
			{
				deepNode = value;
			}
		}

		/*
		Initializes the dictionary scriptable object and starts the tree's execution
		*/
		public void Start() {
			treeDataDict = (TreeDict)ScriptableObject.CreateInstance(typeof(TreeDict));
			if (StartNode) {
				StartCoroutine(StartNode.Execute());
			}
		}

		public void OnEnable() {
			if (StartNode) deepDepthCheck(StartNode);
		}


		/*
		Traverses backwards recursively from the given node to the root node to find
		the node's depth in the tree

		Parameters:
		node: The TreeNode to check

		Returns:
		An int representing the depth of the given node

		*/
		public int depthCheck(TreeNode node) {
			if (node.Parent == null) {
				return 0;
			}
			else {
				return depthCheck(node.Parent) + 1;
			}
		}

		/*
		Does a depp traversal of the tree to find the deepest node. This is neccessary for
		when a node is deleted to find the new deepest node (it could be at the same level)

		Parameters:
		start: The node to start looking from
		currentDepth: The initial depth to use
		*/
		public void deepDepthCheck(TreeNode start, int currentDepth = 0) {
			if (!(start is ParentNode)) {
				if (currentDepth + 1 > depth) {
					depth = currentDepth + 1;
					deepNode = start;
				}
				else if (currentDepth == depth) {
					if (start.yPosition > deepNode.yPosition) {
						deepNode = start;
					}
				}
			}
			else {
				foreach (TreeNode child in ((ParentNode)start).Children) {
					deepDepthCheck(child, currentDepth + 1);
				}
			}
		}

		public bool integrityCheck() {
			return integrityCheckHelper(StartNode);
		}

		public bool integrityCheckHelper(TreeNode node) {
			if (node == null) {
				return false;
			}
			else {
				if (node is ParentNode) {
					bool test = true;
					foreach (TreeNode child in ((ParentNode)node).Children) {
						test = test & integrityCheckHelper(child);
					}
					return test;
				}
				else {
					return true;
				}
			}
		}

		/*
		Adds a new node onto the tree at the given parent node, while also updating the tree's depth

		Parameters:
		parent: The node to which we are adding a node to
		node: The node to be added to the parent

		Returns:
		The node added.
		*/
		public T addNode<T>(ParentNode parent, string name) where T : class, new() {
			Undo.SetCurrentGroupName("Add Node");
			int group = Undo.GetCurrentGroup();

			TreeNode node = (TreeNode)Undo.AddComponent(gameObject, typeof(T));
			Undo.RecordObject(node, "Setup Node");
			node.parent = parent;
			node.NodeName = name;
			node.TreeDataDict = treeDataDict;
			node.hideFlags = HideFlags.None;

			Undo.RecordObject(parent, "Add Child");
			parent.childrenActual().Add(node);

			Undo.RecordObject(this, "Check Depth");
			int nodeDepth = depthCheck(node);
			if (nodeDepth > depth) {
				depth = nodeDepth;
				deepNode = node;
			}

			Undo.CollapseUndoOperations(group);

			return node as T;
		}


		/*
		A special function to add the first node of the tree. It instatiates the start node and sets up the 
		depth variables accordingly

		Parameters:
		name: The name of the start node to add (Usually "Origin"

		Returns:
		The start node added
		*/
		public TreeNode addStartNode(string name) {
			StartNode = TreeNode.createNode<BTStartNode>(null, name, gameObject, TreeDataDict);
			deepNode = StartNode;
			depth = 1;
			return StartNode;
		}

		/*
		Deletes the given node from the tree. This function sets up the call to a recursive function hidden from
		the implementer

		Parameters:
		nodeToDelete: The node which is to be deleted

		*/
		public void delete(TreeNode nodeToDelete) {
			deleteNode(startNode, nodeToDelete);
		}

		/*
		A recusive function to traverse the tree and find the node to delete. Once we find the node we call another
		function to destroy all of its children, grandchildren, etc. and then destroy it.

		Parameters:
		current: The current node we are evaluating
		nodeToDelete: The node we are looking for

		*/
		private void deleteNode(TreeNode current, TreeNode nodeToDelete) {
			if (current == nodeToDelete) {
				if (nodeToDelete.Parent != null) { //remove our link to our parent
					((ParentNode)nodeToDelete.Parent).removeChild(nodeToDelete);
				}
				destroyNodeRelationships(nodeToDelete); //Destroy all children
#if UNITY_EDITOR
				Undo.DestroyObjectImmediate(nodeToDelete); //Destroy ourselves
#else
					Destroy(nodeToDelete);
#endif
				deepDepthCheck(StartNode);
			}
			else {
				if (current is ParentNode) {
					foreach (TreeNode child in ((ParentNode)current).Children) {
						deleteNode(child, nodeToDelete);
					}
				}
			}
		}


		/*
		This function destroys all nodes attached to the given node and all attached to those
		and so on. 

		Parameters:
		nodeToDestroy: The node whose children we are destroying

		*/
		private void destroyNodeRelationships(TreeNode nodeToDestroy) {
			if (nodeToDestroy is ParentNode) {
				foreach (TreeNode child in ((ParentNode)nodeToDestroy).childrenActual()) {
					destroyNodeRelationships(child);
#if UNITY_EDITOR
					Undo.DestroyObjectImmediate(child);
#else
					Destroy(child);
#endif
				}
			}
		}
	}

}
