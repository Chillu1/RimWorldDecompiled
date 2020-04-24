using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_Skygaze : JoyGiver
	{
		public override float GetChance(Pawn pawn)
		{
			float num = pawn.Map.gameConditionManager.AggregateSkyGazeChanceFactor(pawn.Map);
			return base.GetChance(pawn) * num;
		}

		public override Job TryGiveJob(Pawn pawn)
		{
			if (!JoyUtility.EnjoyableOutsideNow(pawn) || pawn.Map.weatherManager.curWeather.rainRate > 0.1f)
			{
				return null;
			}
			if (!RCellFinder.TryFindSkygazeCell(pawn.Position, pawn, out IntVec3 result))
			{
				return null;
			}
			return JobMaker.MakeJob(def.jobDef, result);
		}
	}
}
