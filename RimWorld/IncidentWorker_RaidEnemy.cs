using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_RaidEnemy : IncidentWorker_Raid
	{
		protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
		{
			if (base.FactionCanBeGroupSource(f, map, desperate) && f.HostileTo(Faction.OfPlayer))
			{
				if (!desperate)
				{
					return (float)GenDate.DaysPassed >= f.def.earliestRaidDays;
				}
				return true;
			}
			return false;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (!base.TryExecuteWorker(parms))
			{
				return false;
			}
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			Find.StoryWatcher.statsRecord.numRaidsEnemy++;
			return true;
		}

		protected override bool TryResolveRaidFaction(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (parms.faction != null)
			{
				return true;
			}
			float num = parms.points;
			if (num <= 0f)
			{
				num = 999999f;
			}
			if (PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(num, out parms.faction, (Faction f) => FactionCanBeGroupSource(f, map), allowNonHostileToPlayer: true, allowHidden: true, allowDefeated: true))
			{
				return true;
			}
			if (PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(num, out parms.faction, (Faction f) => FactionCanBeGroupSource(f, map, desperate: true), allowNonHostileToPlayer: true, allowHidden: true, allowDefeated: true))
			{
				return true;
			}
			return false;
		}

		protected override void ResolveRaidPoints(IncidentParms parms)
		{
			if (parms.points <= 0f)
			{
				Log.Error("RaidEnemy is resolving raid points. They should always be set before initiating the incident.");
				parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
			}
		}

		public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
		{
			if (parms.raidStrategy != null)
			{
				return;
			}
			Map map = (Map)parms.target;
			DefDatabase<RaidStrategyDef>.AllDefs.Where((RaidStrategyDef d) => d.Worker.CanUseWith(parms, groupKind) && (parms.raidArrivalMode != null || (d.arriveModes != null && d.arriveModes.Any((PawnsArrivalModeDef x) => x.Worker.CanUseWith(parms))))).TryRandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionWeight(map, parms.points), out var result);
			parms.raidStrategy = result;
			if (parms.raidStrategy == null)
			{
				Log.Error(string.Concat("No raid stategy found, defaulting to ImmediateAttack. Faction=", parms.faction.def.defName, ", points=", parms.points, ", groupKind=", groupKind, ", parms=", parms));
				parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
			}
		}

		protected override string GetLetterLabel(IncidentParms parms)
		{
			return parms.raidStrategy.letterLabelEnemy + ": " + parms.faction.Name;
		}

		protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
		{
			string str = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name.ApplyTag(parms.faction)).CapitalizeFirst();
			str += "\n\n";
			str += parms.raidStrategy.arrivalTextEnemy;
			Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
			if (pawn != null)
			{
				str += "\n\n";
				str += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
			}
			return str;
		}

		protected override LetterDef GetLetterDef()
		{
			return LetterDefOf.ThreatBig;
		}

		protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
		{
			return "LetterRelatedPawnsRaidEnemy".Translate(Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
		}

		protected override void GenerateRaidLoot(IncidentParms parms, float raidLootPoints, List<Pawn> pawns)
		{
			if (parms.faction.def.raidLootMaker != null && pawns.Any())
			{
				raidLootPoints *= Find.Storyteller.difficultyValues.EffectiveRaidLootPointsFactor;
				float num = parms.faction.def.raidLootValueFromPointsCurve.Evaluate(raidLootPoints);
				if (parms.raidStrategy != null)
				{
					num *= parms.raidStrategy.raidLootValueFactor;
				}
				ThingSetMakerParams parms2 = default(ThingSetMakerParams);
				parms2.totalMarketValueRange = new FloatRange(num, num);
				parms2.makingFaction = parms.faction;
				List<Thing> loot = parms.faction.def.raidLootMaker.root.Generate(parms2);
				new RaidLootDistributor(parms, pawns, loot).DistributeLoot();
			}
		}
	}
}
