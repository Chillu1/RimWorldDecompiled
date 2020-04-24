using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_Ambush_EnemyFaction : IncidentWorker_Ambush
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Faction faction;
			return PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(parms.points, out faction);
		}

		protected override List<Pawn> GeneratePawns(IncidentParms parms)
		{
			if (!PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(parms.points, out parms.faction))
			{
				Log.Error("Could not find any valid faction for " + def + " incident.");
				return new List<Pawn>();
			}
			PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, parms);
			defaultPawnGroupMakerParms.generateFightersOnly = true;
			defaultPawnGroupMakerParms.dontUseSingleUseRocketLaunchers = true;
			return PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
		}

		protected override LordJob CreateLordJob(List<Pawn> generatedPawns, IncidentParms parms)
		{
			return new LordJob_AssaultColony(parms.faction, canKidnap: true, canTimeoutOrFlee: false);
		}

		protected override string GetLetterText(Pawn anyPawn, IncidentParms parms)
		{
			Caravan caravan = parms.target as Caravan;
			return string.Format(def.letterText, (caravan != null) ? caravan.Name : "yourCaravan".TranslateSimple(), parms.faction.def.pawnsPlural, parms.faction.Name).CapitalizeFirst();
		}
	}
}
