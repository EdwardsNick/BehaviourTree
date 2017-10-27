


/*

DEPRECIATED CLASS


using BehaviorTree;
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;

public static class BT_XML_Handler {

	///<exception cref="FileNotFoundException">Thown if the given filename was unable to be written to</exception>
	public static void xmlWrite(string filename, BehaviorTree.BehaviorTree BTree) {

		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		settings.IndentChars = "    ";

		using (XmlTextWriter writer = (XmlTextWriter)XmlTextWriter.Create(filename, settings)) {
			writer.WriteStartDocument();
			parseTree(BTree.StartNode, writer);
			writer.Close();
		}


	}

	///<exception cref="FileNotFoundException">Thown if the given filename was unable to be read</exception>
	public static void xmlRead(string filename, out BehaviorTree.BehaviorTree BTree, GameObject target) {

		XmlDocument treeDoc = new XmlDocument();
		treeDoc.Load(filename);
		XmlNodeReader reader = new XmlNodeReader(treeDoc);

		BehaviorTree.BehaviorTree newTree = (BehaviorTree.BehaviorTree)target.AddComponent(typeof(BehaviorTree.BehaviorTree));
		reader.Read();
		reader.Read();
		newTree.StartNode = TreeNode.createNode<BTStartNode>(null, reader.Name, target, newTree.TreeData);
		newTree.StartNode.parseXmlAttributes(reader);
		buildTree(reader, newTree, target);

		BTree = newTree;

		reader.Close();
	}


	private static void parseTree(TreeNode currentNode, XmlTextWriter writer) {
		writer.WriteStartElement(currentNode.GetType().ToString());
		currentNode.writeXmlAttributes(writer);
		if (currentNode is ParentNode) {
			ParentNode parentNode = (ParentNode)currentNode;
			foreach (TreeNode node in parentNode.Children) {
				parseTree(node, writer);
			}
		}
		else if (currentNode is DecoratorNode) {
			DecoratorNode decoratorNode = (DecoratorNode)currentNode;
			parseTree(decoratorNode.Child, writer);
		}
		writer.WriteEndElement();
	}

	private static void buildTree(XmlNodeReader reader, BehaviorTree.BehaviorTree tree, GameObject target) {
		TreeNode currentNode = tree.StartNode;
		while (reader.Read()) {
			switch (reader.NodeType) {
				case XmlNodeType.Element:
					Type type = Type.GetType(reader.Name);
					MethodInfo method = Type.GetType(reader.Name).GetMethod("createNode", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
					MethodInfo createNode = method.MakeGenericMethod(type);
					System.Object[] parameters = { currentNode, reader.Name, target, tree.TreeData };
					currentNode = (TreeNode)createNode.Invoke(null, parameters);
					currentNode.parseXmlAttributes(reader);
					if (currentNode.Parent is ParentNode) {
						ParentNode parent = (ParentNode)currentNode.Parent;
						parent.addChild(currentNode);
					}
					else if (currentNode.Parent is DecoratorNode) {
						DecoratorNode decorator = (DecoratorNode)currentNode.Parent;
						decorator.Child = currentNode;
					}
					else {
						//Bad
					}
					break;
				case XmlNodeType.EndElement:
					currentNode = currentNode.Parent;
					break;
			}
		}
	}
}


*/
