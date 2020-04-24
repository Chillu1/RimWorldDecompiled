using System.Collections.Generic;

namespace Verse
{
	public class DiaNode
	{
		public TaggedString text;

		public List<DiaOption> options = new List<DiaOption>();

		protected DiaNodeMold def;

		public DiaNode(TaggedString text)
		{
			this.text = text;
		}

		public DiaNode(DiaNodeMold newDef)
		{
			def = newDef;
			def.used = true;
			text = def.texts.RandomElement();
			if (def.optionList.Count > 0)
			{
				foreach (DiaOptionMold option in def.optionList)
				{
					options.Add(new DiaOption(option));
				}
			}
			else
			{
				options.Add(new DiaOption("OK".Translate()));
			}
		}

		public void PreClose()
		{
		}
	}
}
