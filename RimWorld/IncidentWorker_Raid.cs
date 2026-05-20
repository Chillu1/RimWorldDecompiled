using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class IncidentWorker_Raid : IncidentWorker_PawnsArrive
	{
		protected abstract bool TryResolveRaidFaction(IncidentParms parms);

		public abstract void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind);

		protected abstract string GetLetterLabel(IncidentParms parms);

		protected abstract string GetLetterText(IncidentParms parms, List<Pawn> pawns);

		protected abstract LetterDef GetLetterDef();

		protected abstract string GetRelatedPawnsInfoLetterText(IncidentParms parms);

		protected abstract void ResolveRaidPoints(IncidentParms parms);

		public virtual bool TryResolveRaidArriveMode(IncidentParms parms)
		{
			if (parms.raidArrivalMode != null)
			{
				return false;
			}
			if (parms.raidArrivalModeForQuickMilitaryAid && !DefDatabase<PawnsArrivalModeDef>.AllDefs.Where((PawnsArrivalModeDef mode) => mode.forQuickMilitaryAid && mode.Worker.CanUseWith(parms)).Any((PawnsArrivalModeDef mode) => ModeWeight(parms, mode) > 0f))
			{
				parms.raidArrivalMode = ((Rand.Value < 0.6f) ? PawnsArrivalModeDefOf.EdgeDrop : PawnsArrivalModeDefOf.CenterDrop);
				return true;
			}
			if (parms.raidStrategy == null)
			{
				Log.Error("parms raidStrategy was null but shouldn't be. Defaulting to ImmediateAttack.");
				parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
			}
			return parms.raidStrategy.arriveModes.Where((PawnsArrivalModeDef mode) => mode.Worker.CanUseWith(parms)).TryRandomElementByWeight((PawnsArrivalModeDef mode) => ModeWeight(parms, mode), out parms.raidArrivalMode);
		}

		private static float ModeWeight(IncidentParms parms, PawnsArrivalModeDef mode)
		{
			if (parms.target is Map { Tile: { Valid: not false }, Tile: var tile2 } && tile2.LayerDef.isSpace)
			{
				return Mathf.Max(mode.Worker.GetSelectionWeight(parms), mode.minSpaceSelectionWeight);
			}
			return mode.Worker.GetSelectionWeight(parms);
		}

		public virtual void ResolveRaidArriveMode(IncidentParms parms)
		{
			if (!TryResolveRaidArriveMode(parms))
			{
				Log.Error($"Could not resolve arrival mode for raid. Defaulting to EdgeWalkIn. parms={parms}");
				parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
			}
		}

		public virtual void ResolveRaidAgeRestriction(IncidentParms parms)
		{
		}

		protected virtual void GenerateRaidLoot(IncidentParms parms, float raidLootPoints, List<Pawn> pawns)
		{
		}

		public bool TryGenerateRaidInfo(IncidentParms parms, out List<Pawn> pawns, bool debugTest = false)
		{
			pawns = null;
			ResolveRaidPoints(parms);
			if (!TryResolveRaidFaction(parms))
			{
				return false;
			}
			PawnGroupKindDef groupKind = parms.pawnGroupKind ?? PawnGroupKindDefOf.Combat;
			ResolveRaidStrategy(parms, groupKind);
			if (parms.raidArrivalMode == null && !TryResolveRaidArriveMode(parms))
			{
				return false;
			}
			ResolveRaidAgeRestriction(parms);
			if (!debugTest)
			{
				parms.raidStrategy.Worker.TryGenerateThreats(parms);
			}
			if (!debugTest && !parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
			{
				return false;
			}
			float points = parms.points;
			parms.points = AdjustedRaidPoints(parms.points, parms.raidArrivalMode, parms.raidStrategy, parms.faction, groupKind, parms.target, parms.raidAgeRestriction);
			if (!debugTest)
			{
				pawns = parms.raidStrategy.Worker.SpawnThreats(parms);
			}
			if (pawns == null)
			{
				PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(groupKind, parms);
				pawns = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
				if (pawns.Count == 0)
				{
					if (debugTest)
					{
						Log.Error("Got no pawns spawning raid from parms " + parms);
					}
					return false;
				}
				if (!debugTest)
				{
					parms.raidArrivalMode.Worker.Arrive(pawns, parms);
				}
			}
			parms.pawnCount = pawns.Count;
			PostProcessSpawnedPawns(parms, pawns);
			if (debugTest)
			{
				parms.target.StoryState.lastRaidFaction = parms.faction;
			}
			else
			{
				GenerateRaidLoot(parms, points, pawns);
			}
			return true;
		}

		protected virtual void PostProcessSpawnedPawns(IncidentParms parms, List<Pawn> pawns)
		{
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (!TryGenerateRaidInfo(parms, out var pawns))
			{
				return false;
			}
			TaggedString letterLabel = GetLetterLabel(parms);
			TaggedString letterText = GetLetterText(parms, pawns);
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref letterLabel, ref letterText, GetRelatedPawnsInfoLetterText(parms), informEvenIfSeenBefore: true);
			List<TargetInfo> list = new List<TargetInfo>();
			if (parms.pawnGroups != null)
			{
				List<List<Pawn>> list2 = IncidentParmsUtility.SplitIntoGroups(pawns, parms.pawnGroups);
				List<Pawn> list3 = list2.MaxBy((List<Pawn> x) => x.Count);
				if (list3.Any())
				{
					list.Add(list3[0]);
				}
				for (int num = 0; num < list2.Count; num++)
				{
					if (list2[num] != list3 && list2[num].Any())
					{
						list.Add(list2[num][0]);
					}
				}
			}
			else if (pawns.Any())
			{
				foreach (Pawn item in pawns)
				{
					list.Add(item);
				}
			}
			SendStandardLetter(letterLabel, letterText, GetLetterDef(), parms, list);
			if (parms.controllerPawn == null || parms.controllerPawn.Faction != Faction.OfPlayer)
			{
				parms.raidStrategy.Worker.MakeLords(parms, pawns);
			}
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
			if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.ShieldBelts))
			{
				for (int num2 = 0; num2 < pawns.Count; num2++)
				{
					Pawn pawn = pawns[num2];
					if (pawn.apparel != null && pawn.apparel.WornApparel.Any((Apparel ap) => ap.def == ThingDefOf.Apparel_ShieldBelt))
					{
						LessonAutoActivator.TeachOpportunity(ConceptDefOf.ShieldBelts, OpportunityType.Critical);
						break;
					}
				}
			}
			if (DebugSettings.logRaidInfo)
			{
				Log.Message($"Raid: {parms.faction.Name} ({parms.faction.def.defName}) {parms.raidArrivalMode.defName} {parms.raidStrategy.defName} c={parms.spawnCenter} p={parms.points}");
			}
			return true;
		}

		public static float AdjustedRaidPoints(float points, PawnsArrivalModeDef raidArrivalMode, RaidStrategyDef raidStrategy, Faction faction, PawnGroupKindDef groupKind, IIncidentTarget target, RaidAgeRestrictionDef ageRestriction = null)
		{
			if (raidArrivalMode.pointsFactorCurve != null)
			{
				points *= raidArrivalMode.pointsFactorCurve.Evaluate(points);
			}
			if (raidStrategy.pointsFactorCurve != null)
			{
				points *= raidStrategy.pointsFactorCurve.Evaluate(points);
			}
			if (ageRestriction != null)
			{
				points *= ageRestriction.threatPointsFactor;
			}
			if (target.Tile.Valid)
			{
				points *= target.Tile.LayerDef.raidPointsFactor;
			}
			points = Mathf.Max(points, raidStrategy.Worker.MinimumPoints(faction, groupKind) * 1.05f);
			return points;
		}

		public void DoTable_RaidFactionSampled()
		{
			int ticksGame = Find.TickManager.TicksGame;
			Find.TickManager.DebugSetTicksGame(36000000);
			Faction lastRaidFaction = Find.CurrentMap.StoryState.lastRaidFaction;
			List<TableDataGetter<Faction>> list = new List<TableDataGetter<Faction>>();
			list.Add(new TableDataGetter<Faction>("name", (Faction f) => f.Name + " (" + f.def.defName + ")"));
			foreach (float item in DebugActionsUtility.PointsOptions(extended: false))
			{
				Dictionary<Faction, int> factionCount = new Dictionary<Faction, int>();
				foreach (Faction allFaction in Find.FactionManager.AllFactions)
				{
					factionCount.Add(allFaction, 0);
				}
				for (int num = 0; num < 500; num++)
				{
					IncidentParms incidentParms = new IncidentParms();
					incidentParms.target = Find.CurrentMap;
					incidentParms.points = item;
					if (TryResolveRaidFaction(incidentParms))
					{
						factionCount[incidentParms.faction]++;
						Find.CurrentMap.StoryState.lastRaidFaction = incidentParms.faction;
					}
				}
				list.Add(new TableDataGetter<Faction>(item.ToString("F0"), (Faction str) => ((float)factionCount[str] / 500f).ToStringPercent()));
			}
			Find.TickManager.DebugSetTicksGame(ticksGame);
			Find.CurrentMap.StoryState.lastRaidFaction = lastRaidFaction;
			DebugTables.MakeTablesDialog(Find.FactionManager.AllFactions, list.ToArray());
		}

		public void DoTable_RaidStrategySampled(Faction fac)
		{
			int ticksGame = Find.TickManager.TicksGame;
			Find.TickManager.DebugSetTicksGame(36000000);
			FactionRelationKind? factionRelationKind = null;
			int? num = null;
			if (fac != null && !fac.HostileTo(Faction.OfPlayer))
			{
				if (fac.HasGoodwill)
				{
					num = fac.GoodwillToMakeHostile(Faction.OfPlayer);
					fac.ChangeGoodwill_Debug(Faction.OfPlayer, num.Value);
				}
				else
				{
					factionRelationKind = fac.RelationKindWith(Faction.OfPlayer);
					fac.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Hostile, canSendHostilityLetter: false);
				}
			}
			List<TableDataGetter<RaidStrategyDef>> list = new List<TableDataGetter<RaidStrategyDef>>();
			list.Add(new TableDataGetter<RaidStrategyDef>("defName", (RaidStrategyDef d) => d.defName));
			foreach (float item in DebugActionsUtility.PointsOptions(extended: false))
			{
				if (fac != null && item < fac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat))
				{
					list.Add(new TableDataGetter<RaidStrategyDef>(item.ToString("F0"), (RaidStrategyDef str) => "0%"));
					continue;
				}
				Dictionary<RaidStrategyDef, int> strats = new Dictionary<RaidStrategyDef, int>();
				foreach (RaidStrategyDef allDef in DefDatabase<RaidStrategyDef>.AllDefs)
				{
					strats.Add(allDef, 0);
				}
				for (int num2 = 0; num2 < 500; num2++)
				{
					IncidentParms incidentParms = new IncidentParms();
					incidentParms.target = Find.CurrentMap;
					incidentParms.points = item;
					incidentParms.faction = fac;
					if (TryResolveRaidFaction(incidentParms))
					{
						ResolveRaidStrategy(incidentParms, PawnGroupKindDefOf.Combat);
						Find.CurrentMap.StoryState.lastRaidFaction = incidentParms.faction;
						if (incidentParms.raidStrategy != null)
						{
							strats[incidentParms.raidStrategy]++;
						}
					}
				}
				list.Add(new TableDataGetter<RaidStrategyDef>(item.ToString("F0"), (RaidStrategyDef str) => ((float)strats[str] / 500f).ToStringPercent()));
			}
			Find.TickManager.DebugSetTicksGame(ticksGame);
			if (factionRelationKind.HasValue)
			{
				fac.SetRelationDirect(Faction.OfPlayer, factionRelationKind.Value, canSendHostilityLetter: false);
			}
			else if (num.HasValue)
			{
				fac.ChangeGoodwill_Debug(Faction.OfPlayer, -num.Value);
			}
			DebugTables.MakeTablesDialog(DefDatabase<RaidStrategyDef>.AllDefs, list.ToArray());
		}

		public void DoTable_RaidArrivalModeSampled(Faction fac)
		{
			int ticksGame = Find.TickManager.TicksGame;
			Find.TickManager.DebugSetTicksGame(36000000);
			Faction lastRaidFaction = Find.CurrentMap.StoryState.lastRaidFaction;
			FactionRelationKind? factionRelationKind = null;
			int? num = null;
			if (fac != null && !fac.HostileTo(Faction.OfPlayer))
			{
				if (fac.HasGoodwill)
				{
					num = fac.GoodwillToMakeHostile(Faction.OfPlayer);
					fac.ChangeGoodwill_Debug(Faction.OfPlayer, num.Value);
				}
				else
				{
					factionRelationKind = fac.RelationKindWith(Faction.OfPlayer);
					fac.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Hostile, canSendHostilityLetter: false);
				}
			}
			List<TableDataGetter<PawnsArrivalModeDef>> list = new List<TableDataGetter<PawnsArrivalModeDef>>();
			list.Add(new TableDataGetter<PawnsArrivalModeDef>("mode", (PawnsArrivalModeDef f) => f.defName));
			foreach (float item in DebugActionsUtility.PointsOptions(extended: false))
			{
				if (item < fac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat))
				{
					list.Add(new TableDataGetter<PawnsArrivalModeDef>(item.ToString("F0"), (PawnsArrivalModeDef str) => "0%"));
					continue;
				}
				Dictionary<PawnsArrivalModeDef, int> modeCount = new Dictionary<PawnsArrivalModeDef, int>();
				foreach (PawnsArrivalModeDef allDef in DefDatabase<PawnsArrivalModeDef>.AllDefs)
				{
					modeCount.Add(allDef, 0);
				}
				for (int num2 = 0; num2 < 500; num2++)
				{
					IncidentParms incidentParms = new IncidentParms();
					incidentParms.target = Find.CurrentMap;
					incidentParms.points = item;
					incidentParms.faction = fac;
					if (TryResolveRaidFaction(incidentParms))
					{
						Find.CurrentMap.storyState.lastRaidFaction = incidentParms.faction;
						ResolveRaidStrategy(incidentParms, PawnGroupKindDefOf.Combat);
						if (incidentParms.raidStrategy != null)
						{
							ResolveRaidArriveMode(incidentParms);
							modeCount[incidentParms.raidArrivalMode]++;
						}
					}
				}
				list.Add(new TableDataGetter<PawnsArrivalModeDef>(item.ToString("F0"), (PawnsArrivalModeDef str) => ((float)modeCount[str] / 500f).ToStringPercent()));
			}
			Find.TickManager.DebugSetTicksGame(ticksGame);
			Find.CurrentMap.storyState.lastRaidFaction = lastRaidFaction;
			if (factionRelationKind.HasValue)
			{
				fac.SetRelationDirect(Faction.OfPlayer, factionRelationKind.Value, canSendHostilityLetter: false);
			}
			else if (num.HasValue)
			{
				fac.ChangeGoodwill_Debug(Faction.OfPlayer, -num.Value);
			}
			DebugTables.MakeTablesDialog(DefDatabase<PawnsArrivalModeDef>.AllDefs, list.ToArray());
		}
	}
}
