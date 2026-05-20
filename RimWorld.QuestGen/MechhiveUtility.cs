using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public static class MechhiveUtility
{
	public static void FireRaid(Map map, float points, int fireTick = -1, string customLetterLabel = null, string customLetterText = null)
	{
		IncidentParms incidentParms = new IncidentParms();
		incidentParms.forced = true;
		incidentParms.target = map;
		incidentParms.points = Mathf.Max(points, Faction.OfMechanoids.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
		incidentParms.faction = Faction.OfMechanoids;
		incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop;
		incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
		incidentParms.customLetterLabel = customLetterLabel ?? ((string)"MechhiveRaidLetterLabel".Translate());
		incidentParms.customLetterText = customLetterText ?? ((string)"MechhiveRaidLetterText".Translate());
		incidentParms.canRoofPunch = false;
		if (fireTick < 0)
		{
			fireTick = Find.TickManager.TicksGame;
		}
		Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, fireTick, incidentParms);
	}
}
