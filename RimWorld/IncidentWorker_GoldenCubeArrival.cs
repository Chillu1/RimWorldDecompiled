using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class IncidentWorker_GoldenCubeArrival : IncidentWorker_MeteoriteImpact
{
	private Pawn affectedPawn;

	protected override List<Thing> GenerateMeteorContents(IncidentParms parms)
	{
		Thing item = ThingMaker.MakeThing(ThingDefOf.GoldenCube);
		if (!QuestUtility.TryGetIdealColonist(out affectedPawn, (Map)parms.target, ValidatePawn))
		{
			return null;
		}
		affectedPawn.health.AddHediff(HediffDefOf.CubeInterest);
		return new List<Thing> { item };
	}

	protected override Letter MakeLetter(Skyfaller meteorite, List<Thing> contents)
	{
		return LetterMaker.MakeLetter("GoldenCubeArrivalLabel".Translate(), "GoldenCubeArrivalText".Translate(affectedPawn.Named("PAWN")), LetterDefOf.ThreatSmall, new TargetInfo(meteorite.Position, meteorite.Map));
	}

	private bool ValidatePawn(Pawn pawn)
	{
		if (!pawn.IsColonist && !pawn.IsSlaveOfColony)
		{
			return false;
		}
		if (!pawn.health.hediffSet.HasHediff(HediffDefOf.CubeInterest))
		{
			return !pawn.health.hediffSet.HasHediff(HediffDefOf.CubeComa);
		}
		return false;
	}
}
