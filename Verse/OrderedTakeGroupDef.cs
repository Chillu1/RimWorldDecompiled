using System.Collections.Generic;

namespace Verse
{
	public class OrderedTakeGroupDef : Def
	{
		public int max = 3;

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (max <= 0)
			{
				yield return "Max should be greather than zero.";
			}
		}
	}
}
