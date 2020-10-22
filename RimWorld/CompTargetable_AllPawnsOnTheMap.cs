using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompTargetable_AllPawnsOnTheMap : CompTargetable
	{
		protected override bool PlayerChoosesTarget => false;

		protected override TargetingParameters GetTargetingParameters()
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				canTargetBuildings = false,
				validator = (TargetInfo x) => BaseTargetValidator(x.Thing)
			};
		}

		public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
		{
			if (parent.MapHeld == null)
			{
				yield break;
			}
			TargetingParameters tp = GetTargetingParameters();
			foreach (Pawn item in parent.MapHeld.mapPawns.AllPawnsSpawned)
			{
				if (tp.CanTarget(item))
				{
					yield return item;
				}
			}
		}
	}
}
