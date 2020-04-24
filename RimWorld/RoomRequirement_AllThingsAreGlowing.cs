using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRequirement_AllThingsAreGlowing : RoomRequirement
	{
		public ThingDef thingDef;

		public override bool Met(Room r, Pawn p = null)
		{
			foreach (Thing item in r.ContainedThings(thingDef))
			{
				if (!item.TryGetComp<CompGlower>().Glows)
				{
					return false;
				}
			}
			return true;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (thingDef == null)
			{
				yield return "thingDef is null";
			}
			else if (thingDef.GetCompProperties<CompProperties_Glower>() == null)
			{
				yield return "No comp glower on thingDef";
			}
		}
	}
}
