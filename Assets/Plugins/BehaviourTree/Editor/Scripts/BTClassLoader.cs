using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using BehaviorTree;

namespace BehaviorTree {

	/*
	This class keeps a static list of all non-abstact node classes that can be
	instantiated in the tree. This allows us to keep a constant list of available 
	nodes to instatiate. We extend AssetPostProcessor so we can catch the 
	OnPostprocessAllAssets message and recheck for what scripts we can have.

	Fields:
	nodeClasses: The list of all node types we can instantiate
	*/
	public class BTClassLoader : AssetPostprocessor {

		private static List<Type> nodeClasses = findAllScripts();

		public static List<Type> NodeClasses {
			get {
				return nodeClasses;
			}

			set {
				nodeClasses = value;
			}
		}

		/*
		Creates a new list of script types and then asks unity for a like of all script assets. Then
		we check if its a non-abstract subclass of TreeNode

		Returns:
		A list of type objects representing all the non-abstract classes available
			that are subclasses of TreeNode
		*/
		private static List<Type> findAllScripts() {
			List<Type> scripts = new List<Type>();

			//Find all script assets
			MonoScript[] script = (MonoScript[])Resources.FindObjectsOfTypeAll(typeof(MonoScript));

			//For each script: 
			foreach (MonoScript s in script) {

				//If the script has a class, and the class is a subclass of TreeNode
				if (s.GetClass() != null && s.GetClass().IsSubclassOf(typeof(TreeNode))) {
					//If the class wasn't already added, and the class is not abstract, and the class is not a BTStartNode
					if (!scripts.Contains(s.GetClass()) && (!s.GetClass().IsAbstract) && s.GetClass() != typeof(BTStartNode)) {
						scripts.Add(s.GetClass()); //Add the type
					}
				}

			}

			return scripts;
		}


		/*
		Called when the editor detects a change in assets. We then recheck what
		types are available.
		*/
		static void OnPostprocessAllAssets(
				   string[] importedAssets,
				 string[] deletedAssets,
				 string[] movedAssets,
				 string[] movedFromAssetPaths) {


			nodeClasses = findAllScripts();
		}
	}
}
