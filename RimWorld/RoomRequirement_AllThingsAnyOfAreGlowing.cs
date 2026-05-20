using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRequirement_AllThingsAnyOfAreGlowing : RoomRequirement_ThingAnyOf
	{
		public override string Label(Room r = null)
		{
			return labelKey.Translate();
		}

		public override bool Met(Room r, Pawn p = null)
		{
			foreach (Thing item in r.ContainedThingsList(things))
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
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			foreach (ThingDef thing in things)
			{
				if (thing.GetCompProperties<CompProperties_Glower>() == null)
				{
					yield return "No comp glower on thingDef " + thing.defName;
				}
			}
		}
	}
}
