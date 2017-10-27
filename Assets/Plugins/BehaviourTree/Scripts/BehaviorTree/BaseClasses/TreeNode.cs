using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace BehaviorTree {


	/*
	 * This is the base class for all nodes in the behavior tree. 
	 * 
	 * Extends: MonoBehavior
	 * Implements: ExecutableNode
	 * 
	 * Properties
	 * Parent: The parent of this node
	 * nodeName: The string name for this node
	 * childStatus: The return status of the last run child
	 * treeDataDict: A reference to the scriptable object containing the tree's dictionary
	 *
	 */
	[Serializable]
	[ExecuteInEditMode]
	public abstract class TreeNode : MonoBehaviour, ExecutableNode {

		[SerializeField]
		public ParentNode parent;
		public string nodeName;
		protected bool childStatus;


		//Need to make sure every node point to same dictionary on deserialization
		[SerializeField]
		private TreeDict treeDataDict;


#if UNITY_EDITOR

		//The following variables are used only in the edit to properly draw and position the tree
		//No need to serialize as the tree is recalculated base on the tree which is serializeed

		public double xPosition; //The xdepth in the tree for this node
		public double yPosition; //Ther y depth in the tree
		public double offset; //Used throughout the TreeMapper class for calculating the final position
		public double shift;  //Used throughout the TreeMapper class for calculating the final position
		public double change;  //Used throughout the TreeMapper class for calculating the final position
		public int siblingIndex; //Used throughout the TreeMapper class for calculating the final position

		public BTStartNode tree; //A reference to the first node of the tree
		public TreeNode thread; //Used throughout the TreeMapper class for calculating the final position
		public TreeNode ancestor; //Used throughout the TreeMapper class for calculating the final position
		public TreeNode leftMostSibling; //Used throughout the TreeMapper class for calculating the final position

		private Rect windowRect; //The position and size of the window for the node
		public double preferredHeight = 75.0; //The default height for the window
		public double preferredWidth = 150.0; //The default width for the window

		public bool isActive = false; //Stores whether this node is "Running"

		//A Get/Set mostly used in testing
		public Rect WindowRect
		{
			get
			{
				return windowRect;
			}

			set
			{
				windowRect = value;
			}
		}
#endif

		public ParentNode Parent
		{
			get
			{
				return parent;
			}

			set
			{
				parent = value;
			}
		}

		public string NodeName
		{
			get
			{
				return nodeName;
			}

			set
			{
				nodeName = value;
			}
		}


		protected bool ChildStatus
		{
			get
			{
				return childStatus;
			}

			set
			{
				childStatus = value;
			}
		}

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


		/*
			This function sets up variables that will be used by the node during execution
		*/
		public abstract void setup();



		/*
			This function is the actual running part of the node. This could run over one frame or over multiple
			frames, however, it should be fairly short so as not to bog down the game.
		*/
		public abstract IEnumerator tick();



		/*
			This function is a function to tidy up anything used during execution and to set the completion
			status (ie. success or failure). 

			Parameters:
			out bool childReturn: A status to be set based on whether we succeed or fail
		*/
		public abstract void tearDown(out bool childReturn);


		/*
			This is the main function of each node which is called to begin execution of the node. We set our status
			to active (or "Running"), and call our core functions.
		*/
		public IEnumerator Execute() {
			isActive = true;

			setup();
			yield return StartCoroutine(tick());
			tearDown(out Parent.childStatus);

			isActive = false;
		}
		/*
		This is the static generic factory constructor for all TreeNodes (including all subclass nodes. It sets up the basic variables for each
		that all nodes share, and returns the node. Nodes may override the empty constructor if they need something to be instantiated
		there, or use Awake() or the above setup().

		Parameters:
		parent: The parent of the TreeNode to be created
		name: The name of the node to be created
		gameObj: The gameObj to attach the new node to
		treeDict: A reference to the scriptable object that contains the tree's dictionary

		Returns: 
		The generic TreeNode requested when the function was called 
		*/
		public static T createNode<T>(ParentNode parent, string name, GameObject gameObj, TreeDict treeDict) where T : class, new() {
			TreeNode node = (TreeNode)gameObj.AddComponent(typeof(T));
			node.Parent = parent;
			node.NodeName = name;
			node.TreeDataDict = treeDict;
			node.hideFlags = HideFlags.HideInInspector;
			return node as T;
		}


		/*
		This function draws the elements that are to be contained within the node's window using GUILayout class to 
		automatically control an optimal layout. Then we call the drawProperties function so that subclasses can draw
		their own unique properties
		*/
		public void drawWindow() {
			EditorGUIUtility.labelWidth = 50;
			NodeName = EditorGUILayout.TextField("Name:", NodeName);
			EditorGUIUtility.labelWidth = 0;

			hideFlags = (HideFlags)EditorGUILayout.EnumPopup("Hide Flags", hideFlags);

			drawProperties();
		}

		/*
		This function will be overriden by an subclass that has other properties that they want to be draw on the node
		in the editor window
		*/
		protected virtual void drawProperties() { }


		/*
		A simple toString override
		*/
		public override string ToString() {
			return NodeName;
		}

		//Override Hash Function and Equals?

		void OnDestroy() {
			try {
				if (parent.Children.Contains(this)) {
					parent.removeChild(this);
				}
			}
			catch (NullReferenceException) {
			}

		}

		void Start() {
			try {
				if (!(parent.Children.Contains(this))){
					parent.addChild(this);
				}
			}
			catch (NullReferenceException) {
			}
		}

	}
}
