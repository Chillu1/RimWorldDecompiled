using System.Collections.Generic;

namespace Verse
{
	public class DiaNodeList
	{
		public string Name = "NeedsName";

		public List<DiaNodeMold> Nodes = new List<DiaNodeMold>();

		public List<string> NodeNames = new List<string>();

		public DiaNodeMold RandomNodeFromList()
		{
			List<DiaNodeMold> list = Nodes.ListFullCopy();
			foreach (string nodeName in NodeNames)
			{
				list.Add(DialogDatabase.GetNodeNamed(nodeName));
			}
			foreach (DiaNodeMold item in list)
			{
				if (item.unique && item.used)
				{
					list.Remove(item);
				}
			}
			return list.RandomElement();
		}
	}
}
