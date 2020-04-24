using System.Collections.Generic;
using System.Xml;

namespace Verse
{
	public class PatchOperationFindMod : PatchOperation
	{
		private List<string> mods;

		private PatchOperation match;

		private PatchOperation nomatch;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			bool flag = false;
			for (int i = 0; i < mods.Count; i++)
			{
				if (ModLister.HasActiveModWithName(mods[i]))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (match != null)
				{
					return match.Apply(xml);
				}
			}
			else if (nomatch != null)
			{
				return nomatch.Apply(xml);
			}
			return true;
		}

		public override string ToString()
		{
			return $"{base.ToString()}({mods.ToCommaList()})";
		}
	}
}
