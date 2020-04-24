using System;
using System.Collections.Generic;

namespace Verse
{
	public class DiaNodeMold
	{
		public string name = "Unnamed";

		public bool unique;

		public List<string> texts = new List<string>();

		public List<DiaOptionMold> optionList = new List<DiaOptionMold>();

		[Unsaved(false)]
		public bool isRoot = true;

		[Unsaved(false)]
		public bool used;

		[Unsaved(false)]
		public DiaNodeType nodeType;

		public void PostLoad()
		{
			int num = 0;
			foreach (string item in texts.ListFullCopy())
			{
				texts[num] = item.Replace("\\n", Environment.NewLine);
				num++;
			}
			foreach (DiaOptionMold option in optionList)
			{
				foreach (DiaNodeMold childNode in option.ChildNodes)
				{
					childNode.PostLoad();
				}
			}
		}
	}
}
