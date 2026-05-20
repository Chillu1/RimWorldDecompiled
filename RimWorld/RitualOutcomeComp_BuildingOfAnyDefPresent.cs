using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class RitualOutcomeComp_BuildingOfAnyDefPresent : RitualOutcomeComp_BuildingsPresent
	{
		public List<ThingDef> defs;

		protected override string LabelForDesc => "RitualOutcomeLabelAnyOfThese".Translate() + ": " + defs.Select((ThingDef d) => d.LabelCap.Resolve()).ToCommaList();

		protected override Thing LookForBuilding(IntVec3 cell, Map map, Precept_Ritual ritual)
		{
			foreach (ThingDef def in defs)
			{
				Thing firstThing = cell.GetFirstThing(map, def);
				if (firstThing != null)
				{
					return firstThing;
				}
			}
			return null;
		}
	}
}
