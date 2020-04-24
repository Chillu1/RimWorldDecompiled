using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRequirement_Thing : RoomRequirement
	{
		public ThingDef thingDef;

		public override bool Met(Room r, Pawn p = null)
		{
			return r.ContainsThing(thingDef);
		}

		public override bool SameOrSubsetOf(RoomRequirement other)
		{
			if (!base.SameOrSubsetOf(other))
			{
				return false;
			}
			RoomRequirement_Thing roomRequirement_Thing = (RoomRequirement_Thing)other;
			return thingDef == roomRequirement_Thing.thingDef;
		}

		public override string Label(Room r = null)
		{
			return ((!labelKey.NullOrEmpty()) ? ((string)labelKey.Translate()) : thingDef.label) + ((r != null) ? " 0/1" : "");
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (thingDef == null)
			{
				yield return "thingDef is null";
			}
		}

		public override bool PlayerHasResearched()
		{
			return thingDef.IsResearchFinished;
		}
	}
}
