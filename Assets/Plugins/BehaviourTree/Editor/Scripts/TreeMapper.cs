using System;
using UnityEngine;

namespace BehaviorTree {

	/*
	This class is an implementation of a tree drawing algorithm first proposed in:

	C. Buchheim, M. J Unger, and S. Leipert. Improving Walker's algorithm to run in linear time. In Proc. Graph Drawing (GD), 2002. http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.16.8757

	and based on the python implementation by Bill Mill from https://github.com/llimllib/pymag-trees/

	with some modifications tailored to my needs. This algoritm finds the position of all nodes in 
	linear time. Documentation may be somewhat light, as I don't fully understand everything that's
	going on here.

	Fields:
	smallestX: Set to the maximum possible value for use later in the script
	smallestY: Set  to the maximum possible value for use later in the script
	*/
	public static class TreeMapper {

		static double smallestX = Int32.MaxValue;
		static double smallestY = Int32.MaxValue;

		/*
		This is the main function to be called that will take a behavior tree
		and map the position of all the nodes in it. 

		Parameters:
		treeToMap: The behaviorTree we will try to map out
		*/
		public static void MapTree(BehaviorTree treeToMap) {
			//Sets node variables up for use later during the algorithm
			initializeVariables(treeToMap.StartNode, treeToMap.StartNode);

			//Traverse over the tree to find positions
			TreeNode tree = firstTraverse(treeToMap.StartNode);
			
			//Do a second traversal over the tree adjusting for offets, etc
			double min = (double)secondTraverse(tree);

			//Traverse over again if our min value was negative
			if (min < 0) thirdTraverse(tree, -min);

			//Finally, adjust all the node positions to be positive values 
			absoluteValuePositions(treeToMap.StartNode);

			//Iterates over the tree and sets the windowRects based on the positions
			calculateRects(treeToMap.StartNode);
		}

		/*
		Sets up the initial variables for all the nodes in the tree.

		Parameters:
		node: The current node we are working on
		tree: The root node of the behavior tree
		depth: The y depth of the current node
		index: The current node's sibling index
		*/
		private static void initializeVariables(TreeNode node, BTStartNode tree, double depth = 0, int index = 1) {
			node.xPosition = -1;
			node.yPosition = depth;
			node.tree = tree;
			if (node is ParentNode) {
				ParentNode parent = (ParentNode)node;
				for (int i = 0; i < parent.Children.Count; i++) {
					initializeVariables(parent.Children[i], tree, depth + 1, i + 1);
				}
			}
			node.thread = null;
			node.offset = 0;
			node.ancestor = node;
			node.change = 0;
			node.shift = 0;
			node.leftMostSibling = null;
			node.siblingIndex = index;
		}

		/*
		Traverse over the tree and find a starting x position for each node

		Parameters:
		node: The current node being processed
		distance: The distance to use for calculations

		Return: 
		The current node
		*/
		private static TreeNode firstTraverse(TreeNode node, double distance = 1.0) {
			if (!(node is ParentNode) || ((ParentNode)node).Children.Count == 0) {
				if (getLeftMostSibling(node)) {
					node.xPosition = leftBrother(node).xPosition + (int)distance;
				}
				else {
					node.xPosition = 0;
				}
			}
			else {
				ParentNode parent = (ParentNode)node;
				TreeNode defaultAncestor = parent.Children[0];
				foreach (TreeNode child in parent.Children) {
					firstTraverse(child, 1);
					defaultAncestor = apportion(child, defaultAncestor, distance);
				}

				shiftNodes(node);

				double midpoint = ((parent.Children[0].xPosition + parent.Children[parent.Children.Count - 1].xPosition) / 2);

				TreeNode left = leftBrother(node);
				if (left != null) {
					node.xPosition = left.xPosition + distance;
					node.offset = node.xPosition - midpoint;
				}
				else {
					node.xPosition = midpoint;
				}
			}
			return node;
		}

		/*
		Now we traverse over the tree again and find y positions for the nodes and fix x positions

		Parameters:
		node: The current node being processed
		m: An offset that is added to the x position
		depth: The current y depth
		min: The minimum x position currently

		Returns:
		The calculated min value
		*/
		private static double? secondTraverse(TreeNode node, double m = 0, double depth = 0, double? min = null) {
			node.xPosition += m;
			node.yPosition = depth;

			if (min == null || node.xPosition < min) {
				min = node.xPosition;
			}

			if (node is ParentNode) {
				ParentNode parent = (ParentNode)node;
				foreach (TreeNode child in parent.Children) {
					min = secondTraverse(child, m + node.offset, depth + 1, min);
				}
			}
			return min;
		}

		/*
		Finds the best ancestor for the given node

		innerLeftNode: The node to our far left
		node: The current node
		defaultAncestor: The current default ancestor

		Returns:
		The new default ancestor
		*/
		private static TreeNode ancestor(TreeNode innerLeftNode, TreeNode node, TreeNode defaultAncestor) {
			ParentNode parent = (ParentNode)node.Parent;
			if (parent.Children.Contains(innerLeftNode)) {
				return innerLeftNode.ancestor;
			}
			else {
				return defaultAncestor;
			}
		}

		/*
		Pass over the tree one more time to adjust to the min x value

		Parameters:
		node: The current node
		min: The min value to add
		*/
		private static void thirdTraverse(TreeNode node, double min) {
			node.xPosition += min;
			if (node is ParentNode) {
				ParentNode parent = (ParentNode)node;
				foreach (TreeNode child in parent.Children) {
					thirdTraverse(child, min);
				}
			}
		}

		/*
		Find all smallest x,y values and then adjust all the positions in the tree to be positive values

		Parameters:
		node: The current node to adjust
		*/
		private static void absoluteValuePositions(TreeNode node) {
			smallestX = Int32.MaxValue;
			findSmallestX(node);
			if (smallestX < 0) {
				smallestX = Mathf.Abs((float)smallestX);
			}

			smallestY = Int32.MaxValue;
			findSmallestY(node);
			if (smallestY < 0) {
				smallestY = Mathf.Abs((float)smallestY);
			}

			normalizeTree(node);
		}

		/*
		Set the window rects for all the nodes

		Parameters:
		node: The current node to set
		*/
		private static void calculateRects(TreeNode node) {
			node.WindowRect = new Rect((int)(node.preferredWidth * node.xPosition), (int)(node.preferredHeight * node.yPosition), (int)(node.preferredWidth * node.xPosition - 20), (int)(node.preferredHeight * node.yPosition - 20));
			if (node is ParentNode) {
				foreach (TreeNode child in ((ParentNode)node).Children) {
					calculateRects(child);
				}
			}
		}

		/*
		Uses the smallest x,y values to normalize all the values in the tree

		Parameters:
		node: The current node
		*/
		private static void normalizeTree(TreeNode node) {
			node.xPosition = (node.xPosition + smallestX + 1) * 2.01;
			node.yPosition = (node.yPosition + smallestY + 1) * 4.01;
			if (node is ParentNode) {
				foreach (TreeNode child in ((ParentNode)node).Children) {
					normalizeTree(child);
				}
			}
		}

		/*
		Apportion is a very complex function that elludes my understanding. From what I understand
		its about finding the nodes around us to determine where our final position in the tree will
		be. 

		Parameters:
		node: The node to apportion
		defaultAncestor: The default ancestor to use
		distance: The distance to use for the shift value

		Returns:
		The new default ancestor
		*/
		private static TreeNode apportion(TreeNode node, TreeNode defaultAncestor, double distance) {
			TreeNode left = leftBrother(node);
			if (left != null) {
				TreeNode innerRight = node;
				TreeNode outerRight = node;
				TreeNode innerLeft = left;
				TreeNode outerLeft = getLeftMostSibling(node);

				double innerRightShift = node.offset;
				double outerRightShift = node.offset;
				double innerLeftShift = innerLeft.offset;
				double outerLeftShift = outerLeft.offset;

				while (rightNode(innerLeft) != null && leftNode(innerRight) != null) {
					innerLeft = rightNode(innerLeft);
					innerRight = leftNode(innerRight);
					outerLeft = leftNode(outerLeft);
					outerRight = rightNode(outerRight);

					outerRight.ancestor = node;
					double shift = (innerLeft.xPosition + innerLeftShift) - (innerRight.xPosition + innerRightShift) + distance;

					if (shift > 0) {
						TreeNode ancest = ancestor(innerLeft, node, defaultAncestor);
						moveSubTree(ancest, node, shift);
						innerRightShift += shift;
						outerRightShift += shift;
					}

					innerLeftShift += innerLeft.offset;
					innerRightShift += innerRight.offset;
					outerLeftShift += outerLeft.offset;
					outerRightShift += outerRight.offset;
				}

				if (rightNode(innerLeft) != null && rightNode(outerRight) == null) {
					outerRight.thread = rightNode(innerLeft);
					outerRight.offset += innerLeftShift - outerRightShift;
				}
				else {
					if (leftNode(innerRight) != null && leftNode(outerLeft) == null) {
						outerLeft.thread = leftNode(innerRight);
						outerLeft.offset += innerRightShift - outerLeftShift;
					}
					defaultAncestor = node;
				}
			}
			return defaultAncestor;
		}


		/*
		Finds the smallest x value in the tree

		Parameters: 
		node: The current node to check
		*/
		private static void findSmallestX(TreeNode node) {
			if (node.xPosition < smallestX) {
				smallestX = node.xPosition;
			}
			if (node is ParentNode) {
				foreach (TreeNode child in ((ParentNode)node).Children) {
					findSmallestX(child);
				}
			}
		}

		/*
		Finds the smallest y value in the tree

		Parameters: 
		node: The current node to check
		*/
		private static void findSmallestY(TreeNode node) {
			if (node.yPosition < smallestY) {
				smallestY = node.yPosition;
			}
			if (node is ParentNode) {
				foreach (TreeNode child in ((ParentNode)node).Children) {
					findSmallestY(child);
				}
			}
		}

		/*
		Finds our left sibling

		Parameters:
		node: The node to find the left node of

		Returns:
		The left sibling
		*/
		private static TreeNode leftBrother(TreeNode node) {
			TreeNode left = null;
			if (node.Parent) {
				ParentNode parent = (ParentNode)node.Parent;
				foreach (TreeNode child in parent.Children) {
					if (node == child) return left;
					else left = child;
				}
			}
			return left;
		}

		/*
		Finds the left most sibling

		Parameters:
		node: The node to find the left most of

		Returns:
		The node's left most sibling
		*/
		private static TreeNode getLeftMostSibling(TreeNode node) {
			ParentNode parent = (ParentNode)node.Parent;
			if (node.leftMostSibling == null && node.Parent != null && node != parent.Children[0]) {
				node.leftMostSibling = parent.Children[0];
			}
			return node.leftMostSibling;
		}


		/*
		Shifts the nodes in the tree based on the calculated offsets and change values

		Parameters:
		The node to shift from
		*/
		private static void shiftNodes(TreeNode node) {
			double shift = 0;
			double change = 0;
			if (node is ParentNode) {
				ParentNode parent = (ParentNode)node;
				for (int i = parent.Children.Count - 1; i >= 0; i--) {
					parent.Children[i].xPosition += shift;
					parent.Children[i].offset += shift;
					change += parent.Children[i].change;
					shift += parent.Children[i].shift + change;
				}
			}
		}

		/*
		Moves the node and subtree based on a ishift value

		Parameters:
		leftNode: The node to our left
		rightNode: The node to our tight
		shift: The value we use for our shift
		*/
		private static void moveSubTree(TreeNode leftNode, TreeNode rightNode, double shift) {
			int subtrees = rightNode.siblingIndex - leftNode.siblingIndex;
			rightNode.change -= shift / subtrees;
			rightNode.shift += shift;
			leftNode.change += shift / subtrees;
			rightNode.xPosition += shift;
			rightNode.offset += shift;
		}

		/*
		Finds the node to our left which is either a thread to another subtree
		or our left most sibling

		Parameters:
		node: The node to find the left node of

		Returns:
		The left node
		*/
		private static TreeNode leftNode(TreeNode node) {
			if (node.thread != null) {
				return node.thread;
			}
			else {
				if (node is ParentNode) {
					ParentNode parent = (ParentNode)node;
					if (parent.Children.Count > 0) {
						return parent.Children[0];
					}
					else {
						return null;
					}
				}
				else {
					return null;
				}
			}
		}

		/*
		Finds the node to our right which is either a thread to another subtree
		or our right most sibling

		Parameters:
		node: The node to find the right node of

		Returns:
		The right node
		*/
		private static TreeNode rightNode(TreeNode node) {
			if (node.thread != null) {
				return node.thread;
			}
			else {
				if (node is ParentNode) {
					ParentNode parent = (ParentNode)node;
					if (parent.Children.Count > 0) {
						return parent.Children[parent.Children.Count - 1];
					}
					else {
						return null;
					}
				}
				else {
					return null;
				}

			}
		}
	}

}
