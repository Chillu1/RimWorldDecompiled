using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_Siege : LordToilData
	{
		public IntVec3 siegeCenter;

		public float baseRadius = 16f;

		public float blueprintPoints;

		public float desiredBuilderFraction = 0.5f;

		public List<Blueprint> blueprints = new List<Blueprint>();

		public override void ExposeData()
		{
			Scribe_Values.Look(ref siegeCenter, "siegeCenter");
			Scribe_Values.Look(ref baseRadius, "baseRadius", 16f);
			Scribe_Values.Look(ref blueprintPoints, "blueprintPoints", 0f);
			Scribe_Values.Look(ref desiredBuilderFraction, "desiredBuilderFraction", 0.5f);
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				blueprints.RemoveAll((Blueprint blue) => blue.Destroyed);
			}
			Scribe_Collections.Look(ref blueprints, "blueprints", LookMode.Reference);
		}
	}
}
