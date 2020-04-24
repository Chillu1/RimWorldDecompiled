using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_AssignableToPawn : CompProperties
	{
		public int maxAssignedPawnsCount = 1;

		public bool drawAssignmentOverlay = true;

		public string singleton;

		public CompProperties_AssignableToPawn()
		{
			compClass = typeof(CompAssignableToPawn);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			return base.ConfigErrors(parentDef);
		}

		public override void PostLoadSpecial(ThingDef parent)
		{
			if (parent.thingClass == typeof(Building_Bed))
			{
				maxAssignedPawnsCount = BedUtility.GetSleepingSlotsCount(parent.size);
			}
		}
	}
}
