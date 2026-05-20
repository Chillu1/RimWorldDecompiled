using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Precept_RitualSeat : Precept_ThingDef
	{
		public override string UIInfoSecondLine => base.ThingDef.LabelCap;

		public override IEnumerable<FloatMenuOption> EditFloatMenuOptions()
		{
			foreach (PreceptThingChance d in def.Worker.ThingDefs)
			{
				if (d.def != base.ThingDef)
				{
					yield return new FloatMenuOption("PreceptReplaceWith".Translate() + ": " + d.def.LabelCap, delegate
					{
						base.ThingDef = d.def;
					}, d.def);
				}
			}
		}
	}
}
