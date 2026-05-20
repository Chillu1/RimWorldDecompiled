using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Skygaze : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			pawn.jobs.posture = PawnPosture.LayingOnGroundFaceUp;
		};
		toil.tickIntervalAction = delegate(int delta)
		{
			float extraJoyGainFactor = pawn.Map.gameConditionManager.AggregateSkyGazeJoyGainFactor(pawn.Map);
			JoyUtility.JoyTickCheckEnd(pawn, delta, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = job.def.joyDuration;
		toil.FailOn(() => pawn.Position.Roofed(pawn.Map));
		toil.FailOn(() => pawn.Map.weatherManager.curWeather.preventSkygaze);
		toil.FailOn(() => !JoyUtility.EnjoyableOutsideNow(pawn));
		yield return toil;
	}

	public override string GetReport()
	{
		if (base.Map.Tile.Valid && base.Map.Tile.LayerDef.isSpace)
		{
			return "Stargazing".Translate();
		}
		if (base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse))
		{
			return "WatchingEclipse".Translate();
		}
		if (base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Aurora))
		{
			return "WatchingAurora".Translate();
		}
		float num = GenCelestial.CurCelestialSunGlow(base.Map);
		if (num < 0.1f)
		{
			return "Stargazing".Translate();
		}
		if (num < 0.65f)
		{
			if (GenLocalDate.DayPercent(pawn) < 0.5f)
			{
				return "WatchingSunrise".Translate();
			}
			return "WatchingSunset".Translate();
		}
		return "CloudWatching".Translate();
	}
}
