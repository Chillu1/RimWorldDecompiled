using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_SeekSafeTemperature : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.health.hediffSet.HasTemperatureInjury(TemperatureInjuryStage.Serious))
			{
				return null;
			}
			FloatRange tempRange = pawn.ComfortableTemperatureRange();
			if (tempRange.Includes(pawn.AmbientTemperature))
			{
				return JobMaker.MakeJob(JobDefOf.Wait_SafeTemperature, 500, checkOverrideOnExpiry: true);
			}
			Region region = ClosestRegionWithinTemperatureRange(pawn.Position, pawn.Map, tempRange, TraverseParms.For(pawn));
			if (region != null)
			{
				return JobMaker.MakeJob(JobDefOf.GotoSafeTemperature, region.RandomCell);
			}
			return null;
		}

		private static Region ClosestRegionWithinTemperatureRange(IntVec3 root, Map map, FloatRange tempRange, TraverseParms traverseParms, RegionType traversableRegionTypes = RegionType.Set_Passable)
		{
			Region region = root.GetRegion(map, traversableRegionTypes);
			if (region == null)
			{
				return null;
			}
			RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParms, isDestination: false);
			Region foundReg = null;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (r.IsDoorway)
				{
					return false;
				}
				if (tempRange.Includes(r.Room.Temperature))
				{
					foundReg = r;
					return true;
				}
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999, traversableRegionTypes);
			return foundReg;
		}
	}
}
