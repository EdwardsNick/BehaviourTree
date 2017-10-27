using BehaviorTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BehaviorTreeEditor {

	/*
	This is the main class that controls the Unity Editor GUI plugin which we use
	to display and modify our behavior tree. It extends Unity's EditorWindow class
	to create a new window available for users to use.

	The general layout and design of this class was derived from this tutorial series:
		https://www.youtube.com/watch?v=gHTJmGGH92w&list=PL1bPKmY0c-wnL5-m6ZsCHoMtuwr5aQaj-
		by C Sharp Accent Tutorials

	Fields:
	mouseLocation: The location of the mouse on an mouse event
	scrollPos: The current position of the scroll pane
	selectedNode: The node that was clicked on
	nodes: A list of all the nodes in the tree (left to right)

	currentTree: The behavior tree of the selected game object in the editor
	*/
	[Serializable]
	public class BTGui : EditorWindow {

		private Vector2 mouseLocation;

		private Vector2 scrollPos = Vector2.zero;

		private TreeNode selectedNode;

		[SerializeField]
		private List<TreeNode> nodes = new List<TreeNode>();

		private GUIStyle titleStyle;
		private GUIStyle yellowBoxStyle;
		private GUIStyle greenBoxStyle;

		[SerializeField]
		private BehaviorTree.BehaviorTree currentTree;
		public BehaviorTree.BehaviorTree CurrentTree
		{
			get
			{
				return currentTree;
			}

			set
			{
				currentTree = value;
				parseTree();
			}
		}



		/*
		This adds the window to the "Window" menu in Unity
		*/
		[MenuItem("Window/BehaviorTree/BTGui %#b", false, 0)]
		static void Init() {
			BTGui window = (BTGui)EditorWindow.GetWindow(typeof(BTGui));
			window.autoRepaintOnSceneChange = true;

			window.hideFlags = HideFlags.HideInHierarchy;

			window.Show();
		}


		/*
		When we are enabled try to parse the behavior tree
		*/
		void OnEnable() {
			Undo.undoRedoPerformed += UndoRedoCallback;

			titleStyle = new GUIStyle();
			titleStyle.alignment = TextAnchor.MiddleCenter;

			yellowBoxStyle = new GUIStyle();
			yellowBoxStyle.alignment = TextAnchor.MiddleCenter;
			yellowBoxStyle.fontSize = 12;

			parseTree();
		}

		/*
		This function attempts to parse the tree using the TreeMapper class
		and generates the list of nodes. Then forces a repaint of the window
		*/
		void parseTree() {
			//If we have a tree 
			if (CurrentTree && CurrentTree.StartNode) {
				if (CurrentTree.integrityCheck()) {
					CurrentTree.deepDepthCheck(CurrentTree.StartNode);
					TreeMapper.MapTree(CurrentTree);
					nodes = CurrentTree.gameObject.GetComponents<TreeNode>().ToList();
					Repaint();
				}
				else {
					Debug.LogFormat("The Integrity of the Tree has failed!\nDid you remove a node from the Inspector?\nRepairing...");
				}
			}
		}


		/*
		This function is called by Unity to draw GUi elements for our window
		*/
		void OnGUI() {

			Event e = Event.current;

			//Finds the mouse location taking into account the scroll window
			mouseLocation = e.mousePosition;
			mouseLocation.x += scrollPos.x;
			mouseLocation.y += scrollPos.y;


			//if we have a tree to draw
			if (CurrentTree) {
				//If the tree has a start node
				if (CurrentTree.StartNode) {
					//if the window was right-clicked on
					if (e.button == 1 && e.type == EventType.MouseDown) {
						bool clickOnWindow = false;

						//Find if we clicked on a window
						for (int i = 0; i < nodes.Count; i++) {
							if (nodes[i].WindowRect.Contains(mouseLocation)) {
								selectedNode = nodes[i];
								clickOnWindow = true;
								break;
							}
						}

						//If we didn't click on a window, show the Reset Positions menu
						if (!clickOnWindow) {
							GenericMenu menu = new GenericMenu();

							menu.AddItem(new GUIContent("Reset Positions"), false, callback, "Reset");
							menu.AddItem(new GUIContent("Visibility/HideAllNodes"), false, callback, "Hide");
							menu.AddItem(new GUIContent("Visibility/ShowAllNodes"), false, callback, "Show");

							menu.ShowAsContext();
							e.Use();
						}

						//If we did click on a window create a add and delete menu
						else {
							GenericMenu menu = new GenericMenu();

							if (selectedNode is ParentNode) {
								//Adds all the types from the BTClassLoader to the list of nodes we can add
								foreach (Type t in BTClassLoader.NodeClasses) {
									string classString = "";
									foreach (string s in t.ToString().Split('.')) {
										classString += "/" + s;
									}
									menu.AddItem(new GUIContent("Add" + classString), false, callback, t);
								}
							}

							menu.AddSeparator("");
							menu.AddItem(new GUIContent("DeleteNode"), false, callback, "Delete");

							menu.ShowAsContext();
							e.Use();
						}
					}

					//Start a scroll view
					scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, true);

					//Begin drawing windows
					BeginWindows();

					//For each node 
					for (int i = 0; i < nodes.Count; i++) {
						//draw a red box above the node window if it is active
						if (nodes[i].isActive) {
							Rect windowRect = nodes[i].WindowRect;
							if (nodes[i] is LeafNode) {
								drawWindowBorder(windowRect, Color.green);
							}
							else {
								drawWindowBorder(windowRect, Color.yellow);
							}
						}
						//draw the window with the specified width and height
						nodes[i].WindowRect = GUILayout.Window(i, nodes[i].WindowRect, drawWindow, nodes[i].GetType().Name + ": " + nodes[i].NodeName,
												GUILayout.MinWidth((float)nodes[i].preferredWidth), GUILayout.MaxWidth((float)nodes[i].preferredWidth),
												GUILayout.Height((float)nodes[i].preferredHeight));
					}

					//For each node draw lines to their children
					foreach (TreeNode node in nodes) {
						if (node is ParentNode) {
							((ParentNode)node).drawRelations();
						}
					}

					//Sent the entire scroll pane size
					if (nodes.Count > 0) {
						GUILayoutUtility.GetRect(nodes[nodes.Count - 1].WindowRect.x * 2f, currentTree.DeepNode.WindowRect.y * 1.2f);
					}

					//Done drawing windows and end scroll pane
					EndWindows();
					EditorGUILayout.EndScrollView();
				}

				//If there is no start node in the tree, show the menu to create one
				else {
					if (e.button == 1 && e.type == EventType.MouseDown) {
						GenericMenu menu = new GenericMenu();

						menu.AddItem(new GUIContent("Add Start Node"), false, callback, "Start");

						menu.ShowAsContext();
						e.Use();
					}
				}

				//Draw the title of the GameObject the tree is attached to at the top
				int titleHeight = (int)Mathf.Max(this.position.height / 20, 20);
				int titleFontSize = (int)Mathf.Max((this.position.height / 1920) * 30, 12f); //24 = MaxFontSizeForTitle
				int titleWidth = (int)this.position.width - 15;

				titleStyle.fontSize = titleFontSize;
				titleStyle.alignment = TextAnchor.MiddleCenter;

				GUI.Box(new Rect(0, 0, titleWidth, titleHeight), "Behavior Tree Belonging to : " + CurrentTree.gameObject.name, titleStyle);
				Handles.color = Color.black;
				Handles.DrawLine(new Vector3(0, titleHeight, 0), new Vector3(titleWidth, titleHeight, 0));
			}
			else {
				GUI.Box(new Rect((this.position.width / 2) - 100, (this.position.height / 2) - 30, (this.position.width / 2), (this.position.height / 10)), "No BehaviorTree Selected!", titleStyle);
			}
		}

		/*
		Desc: Draws a border around the given window's rect with a given color

		Parameters:
		windowRect: The rect of the window to draw the border around
		color: the color of the border
		*/
		private void drawWindowBorder(Rect windowRect, Color color) {
			Handles.color = color;
			Handles.DrawLine(new Vector2(windowRect.x, windowRect.y), new Vector2(windowRect.x + windowRect.width + 2, windowRect.y));
			Handles.DrawLine(new Vector2(windowRect.x, windowRect.y), new Vector2(windowRect.x, windowRect.y + windowRect.height + 1));
			Handles.DrawLine(new Vector2(windowRect.x + windowRect.width + 2, windowRect.y + windowRect.height + 1), new Vector2(windowRect.x + windowRect.width + 2, windowRect.y));
			Handles.DrawLine(new Vector2(windowRect.x + windowRect.width + 2, windowRect.y + windowRect.height + 1), new Vector2(windowRect.x, windowRect.y + windowRect.height + 1));

			Handles.DrawLine(new Vector2(windowRect.x - 1, windowRect.y - 1), new Vector2(windowRect.x + windowRect.width + 3, windowRect.y - 1));
			Handles.DrawLine(new Vector2(windowRect.x - 1, windowRect.y - 1), new Vector2(windowRect.x - 1, windowRect.y + windowRect.height + 2));
			Handles.DrawLine(new Vector2(windowRect.x + windowRect.width + 3, windowRect.y + windowRect.height + 2), new Vector2(windowRect.x + windowRect.width + 3, windowRect.y - 1));
			Handles.DrawLine(new Vector2(windowRect.x + windowRect.width + 3, windowRect.y + windowRect.height + 2), new Vector2(windowRect.x - 1, windowRect.y + windowRect.height + 2));
		}

		/*
		draw the window for each node and make it dragable

		Parameters:
		nodeID: The index of the node to draw
		*/
		private void drawWindow(int nodeID) {
			nodes[nodeID].drawWindow();
			GUI.DragWindow();
		}


		/*
		This is the callback for the menu items

		Parameters:
		obj: The object defining what callback to run
		*/
		private void callback(object obj) {
			//if delete was selected the delete the node, and force a reparse of the tree
			if (obj.ToString() == "Delete") {
				CurrentTree.delete(selectedNode);
				CurrentTree = currentTree;
			}
			//if Reset was chosen, reparse the tree positions
			else if (obj.ToString() == "Reset") {
				CurrentTree = currentTree;
			}
			//if start was chosen then add a new start node and reparse
			else if (obj.ToString() == "Start") {
				//TODO: Allow user to pick this!
				CurrentTree.addStartNode("Origin");
				CurrentTree = currentTree; //BAD!
			}
			else if (obj.ToString() == "Hide") {
				foreach (TreeNode node in nodes) {
					node.hideFlags = HideFlags.HideInInspector;
					Repaint();
					EditorApplication.RepaintHierarchyWindow();
				}
			}
			else if (obj.ToString() == "Show") {
				foreach (TreeNode node in nodes) {
					node.hideFlags = HideFlags.None;
					Repaint();
					EditorApplication.RepaintHierarchyWindow();
				}
			}
			//if Add was selected
			else {
				//if the node selected was a parent node, add a 
				if (selectedNode is ParentNode) {
					if (selectedNode is DecoratorNode && ((DecoratorNode)selectedNode).Child != null) {
						EditorUtility.DisplayDialog("Decorator Warning!", "This decorator already has a child, \nTry deleting the current child first", "Ok");
					}
					Type typeToAdd = (Type)obj;
					ParentNode parent = (ParentNode)selectedNode;

					//This complicated call uses reflection to make a new instance of a node to add 
					//to the parent node with a name bsaed on its type
					string[] split = typeToAdd.ToString().Split('.');
					string typeName;
					if (split.Length > 1) {
						typeName = split[1];
					}
					else {
						typeName = split[0];
					}

					TreeNode node = (TreeNode)typeof(BehaviorTree.BehaviorTree).GetMethod("addNode")
																.MakeGenericMethod(typeToAdd)
																.Invoke(CurrentTree, new object[] { parent, "new" + typeName });
					//Reparse the tree
					CurrentTree = currentTree;
				}

			}
		}

		/*
		Called by Unity when the editor selection changes.
		*/
		void OnSelectionChange() {
			try {
				//if the selection has a BehaviorTree attached AND
				if (Selection.activeGameObject.GetComponent<BehaviorTree.BehaviorTree>() != null &&
							//The currentTree is null, or it is different than the current one
							(!CurrentTree || Selection.activeGameObject.GetComponent<BehaviorTree.BehaviorTree>() != CurrentTree)) {
					CurrentTree = Selection.activeGameObject.GetComponent<BehaviorTree.BehaviorTree>();
				}
				//Force a Repaint
				Repaint();
			}
			catch (NullReferenceException) { }
		}


		/*
		Called by Unity when the editor focus changes to this window.
		*/
		void OnFocus() {
			try {
				//if the selection has a BehaviorTree attached AND
				if (Selection.activeGameObject.GetComponent<BehaviorTree.BehaviorTree>() != null &&
							//The currentTree is null, or it is different than the current one
							(!CurrentTree || Selection.activeGameObject.GetComponent<BehaviorTree.BehaviorTree>() != CurrentTree)) {
					CurrentTree = Selection.activeGameObject.GetComponent<BehaviorTree.BehaviorTree>();
				}
				//Force a Repaint
				Repaint();
			}
			catch (NullReferenceException) { }
		}

		void OnInspectorUpdate() {
			parseTree();
		}

		void UndoRedoCallback() {
			TreeNode[] allNodes = currentTree.GetComponents<TreeNode>();
			ParentNode newParent = null;

			//Looks for the parent missing a child
			foreach (TreeNode node in allNodes) {
				if (node is ParentNode) {
					foreach (TreeNode child in ((ParentNode)node).Children) {
						if (child == null) {
							newParent = (ParentNode)node;
							break;
						}
					}
				}
			}
			if (newParent != null) {
				//Looks for the child who is missing and adds them! Cancel Amber alert!
				foreach (TreeNode node in allNodes) {
					if (node.parent == newParent) {
						if (!(node.parent.Children.Contains(node))) {
							newParent.childrenActual()[newParent.Children.Count - 1] = node;
							break;
						}
					}
				}
			}

			CurrentTree = currentTree;
		}
	}

}
