using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class DialogDatabase
	{
		private static List<DiaNodeMold> Nodes;

		private static List<DiaNodeList> NodeLists;

		static DialogDatabase()
		{
			Nodes = new List<DiaNodeMold>();
			NodeLists = new List<DiaNodeList>();
			LoadAllDialog();
		}

		private static void LoadAllDialog()
		{
			Nodes.Clear();
			Object[] array = Resources.LoadAll("Dialog", typeof(TextAsset));
			foreach (Object @object in array)
			{
				TextAsset ass = @object as TextAsset;
				if (@object.name == "BaseEncounters" || @object.name == "GeneratedDialogs")
				{
					LayerLoader.LoadFileIntoList(ass, Nodes, NodeLists, DiaNodeType.BaseEncounters);
				}
				if (@object.name == "InsanityBattles")
				{
					LayerLoader.LoadFileIntoList(ass, Nodes, NodeLists, DiaNodeType.InsanityBattles);
				}
				if (@object.name == "SpecialEncounters")
				{
					LayerLoader.LoadFileIntoList(ass, Nodes, NodeLists, DiaNodeType.Special);
				}
			}
			foreach (DiaNodeMold node in Nodes)
			{
				node.PostLoad();
			}
			LayerLoader.MarkNonRootNodes(Nodes);
		}

		public static DiaNodeMold GetRandomEncounterRootNode(DiaNodeType NType)
		{
			List<DiaNodeMold> list = new List<DiaNodeMold>();
			foreach (DiaNodeMold node in Nodes)
			{
				if (node.isRoot && (!node.unique || !node.used) && node.nodeType == NType)
				{
					list.Add(node);
				}
			}
			return list.RandomElement();
		}

		public static DiaNodeMold GetNodeNamed(string NodeName)
		{
			foreach (DiaNodeMold node in Nodes)
			{
				if (node.name == NodeName)
				{
					return node;
				}
			}
			foreach (DiaNodeList nodeList in NodeLists)
			{
				if (nodeList.Name == NodeName)
				{
					return nodeList.RandomNodeFromList();
				}
			}
			Log.Error("Did not find node named '" + NodeName + "'.");
			return null;
		}
	}
}
