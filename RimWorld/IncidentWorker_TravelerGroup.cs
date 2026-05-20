using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class IncidentWorker_TravelerGroup : IncidentWorker_NeutralGroup
{
	private static readonly SimpleCurve PointsCurve = new SimpleCurve
	{
		new CurvePoint(40f, 0f),
		new CurvePoint(50f, 1f),
		new CurvePoint(100f, 1f),
		new CurvePoint(200f, 0.5f),
		new CurvePoint(300f, 0.1f),
		new CurvePoint(500f, 0f)
	};

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!TryResolveParms(parms))
		{
			return false;
		}
		if (!RCellFinder.TryFindTravelDestFrom(parms.spawnCenter, map, out var travelDest))
		{
			IntVec3 spawnCenter = parms.spawnCenter;
			Log.Warning("Failed to do traveler incident from " + spawnCenter.ToString() + ": Couldn't find anywhere for the traveler to go.");
			return false;
		}
		List<Pawn> list = SpawnPawns(parms);
		if (list.Count == 0)
		{
			return false;
		}
		string text;
		if (list.Count == 1)
		{
			text = "SingleTravelerPassing".Translate(list[0].story.Title, parms.faction.Name, list[0].Name.ToStringFull, list[0].Named("PAWN"));
			text = text.AdjustedFor(list[0]);
		}
		else
		{
			text = "GroupTravelersPassing".Translate(parms.faction.Name);
		}
		Messages.Message(text, list[0], MessageTypeDefOf.NeutralEvent);
		LordJob_TravelAndExit lordJob = new LordJob_TravelAndExit(travelDest);
		LordMaker.MakeNewLord(parms.faction, lordJob, map, list);
		PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(list, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, informEvenIfSeenBefore: true);
		return true;
	}

	protected override void ResolveParmsPoints(IncidentParms parms)
	{
		if (!(parms.points >= 0f))
		{
			parms.points = Rand.ByCurve(PointsCurve);
		}
	}
}
