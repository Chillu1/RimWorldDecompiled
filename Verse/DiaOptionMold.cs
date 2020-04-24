using System.Collections.Generic;
using System.Xml.Serialization;

namespace Verse
{
	public class DiaOptionMold
	{
		public string Text = "OK".Translate();

		[XmlElement("Node")]
		public List<DiaNodeMold> ChildNodes = new List<DiaNodeMold>();

		[XmlElement("NodeName")]
		[DefaultValue("")]
		public List<string> ChildNodeNames = new List<string>();

		public DiaNodeMold RandomLinkNode()
		{
			List<DiaNodeMold> list = ChildNodes.ListFullCopy();
			foreach (string childNodeName in ChildNodeNames)
			{
				list.Add(DialogDatabase.GetNodeNamed(childNodeName));
			}
			foreach (DiaNodeMold item in list)
			{
				if (item.unique && item.used)
				{
					list.Remove(item);
				}
			}
			if (list.Count == 0)
			{
				return null;
			}
			return list.RandomElement();
		}
	}
}
