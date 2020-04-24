using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRequirement_ThingAnyOf : RoomRequirement
	{
		public List<ThingDef> things;

		public override string Label(Room r = null)
		{
			return ((!labelKey.NullOrEmpty()) ? ((string)labelKey.Translate()) : things[0].label) + ((r != null) ? " 0/1" : "");
		}

		public override bool Met(Room r, Pawn p = null)
		{
			foreach (ThingDef thing in things)
			{
				if (r.ContainsThing(thing))
				{
					return true;
				}
			}
			return false;
		}

		public override bool SameOrSubsetOf(RoomRequirement other)
		{
			if (!base.SameOrSubsetOf(other))
			{
				return false;
			}
			RoomRequirement_ThingAnyOf roomRequirement_ThingAnyOf = (RoomRequirement_ThingAnyOf)other;
			foreach (ThingDef thing in things)
			{
				if (!roomRequirement_ThingAnyOf.things.Contains(thing))
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
			if (things.NullOrEmpty())
			{
				yield return "things are null or empty";
			}
		}

		public override bool PlayerHasResearched()
		{
			for (int i = 0; i < things.Count; i++)
			{
				if (things[i].IsResearchFinished)
				{
					return true;
				}
			}
			return false;
		}
	}
}
