using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class IncidentWorker_RaidEnemy : IncidentWorker_Raid
{
	public override bool FactionCanBeGroupSource(Faction f, IncidentParms parms, bool desperate = false)
	{
		Map map = (Map)parms.target;
		if (!base.FactionCanBeGroupSource(f, parms, desperate))
		{
			return false;
		}
		if (!f.HostileTo(Faction.OfPlayer) || f.deactivated)
		{
			return false;
		}
		if (!desperate && (float)GenDate.DaysPassedSinceSettle < f.def.earliestRaidDays)
		{
			return false;
		}
		if (!f.def.raidArrivalLayerWhitelist.NullOrEmpty() && !f.def.raidArrivalLayerWhitelist.Contains(map.Tile.LayerDef))
		{
			return false;
		}
		if (!f.def.raidArrivalLayerBlacklist.NullOrEmpty() && f.def.raidArrivalLayerBlacklist.Contains(map.Tile.LayerDef))
		{
			return false;
		}
		Faction faction = parms.faction;
		parms.faction = f;
		if (!DefDatabase<RaidStrategyDef>.AllDefs.Any((RaidStrategyDef strategy) => strategy.Worker.CanUseWith(parms, null)))
		{
			parms.faction = faction;
			return false;
		}
		parms.faction = faction;
		return true;
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (!base.TryExecuteWorker(parms))
		{
			return false;
		}
		if (!parms.silent)
		{
			Find.TickManager.slower.SignalForceNormalSpeedShort();
		}
		Find.StoryWatcher.statsRecord.numRaidsEnemy++;
		parms.target.StoryState.lastRaidFaction = parms.faction;
		return true;
	}

	protected override bool TryResolveRaidFaction(IncidentParms parms)
	{
		if (parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer) && (!parms.faction.deactivated || parms.forced))
		{
			return true;
		}
		if (PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroupWeighted(parms, out parms.faction, (Faction f) => FactionCanBeGroupSource(f, parms), allowNonHostileToPlayer: true, allowHidden: true, allowDefeated: true))
		{
			return true;
		}
		if (PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroupWeighted(parms, out parms.faction, (Faction f) => FactionCanBeGroupSource(f, parms, desperate: true), allowNonHostileToPlayer: true, allowHidden: true, allowDefeated: true))
		{
			return true;
		}
		return false;
	}

	public override void ResolveRaidAgeRestriction(IncidentParms parms)
	{
		if (ModsConfig.BiotechActive && RaidAgeRestrictionDefOf.Children.Worker.CanUseWith(parms) && Rand.Chance(RaidAgeRestrictionDefOf.Children.chance))
		{
			parms.raidAgeRestriction = RaidAgeRestrictionDefOf.Children;
		}
	}

	protected override void ResolveRaidPoints(IncidentParms parms)
	{
		if (!(parms.points > 0f))
		{
			Log.Error("RaidEnemy is resolving raid points. They should always be set before initiating the incident.");
			parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
		}
	}

	public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
	{
		if (parms.raidStrategy == null)
		{
			Map map = (Map)parms.target;
			if (!DefDatabase<RaidStrategyDef>.AllDefs.Where(CanUseStrategy).TryRandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionWeightForFaction(map, parms.faction, parms.points), out parms.raidStrategy))
			{
				Log.Error("No raid strategy found, defaulting to ImmediateAttack. Faction=" + parms.faction.def.defName + ", points=" + parms.points + ", groupKind=" + groupKind?.ToString() + ", parms=" + parms);
				parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
			}
		}
		bool CanUseStrategy(RaidStrategyDef def)
		{
			if (def != null && def.Worker.CanUseWith(parms, groupKind))
			{
				if (parms.raidArrivalMode == null)
				{
					if (def.arriveModes != null)
					{
						return def.arriveModes.Any((PawnsArrivalModeDef x) => x.Worker.CanUseWith(parms));
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	protected override string GetLetterLabel(IncidentParms parms)
	{
		return parms.raidStrategy.letterLabelEnemy + ": " + parms.faction.Name;
	}

	protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
	{
		string text = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name.ApplyTag(parms.faction)).CapitalizeFirst();
		text += "\n\n";
		text += parms.raidStrategy.arrivalTextEnemy;
		Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
		if (pawn != null)
		{
			text += "\n\n";
			text += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER")).Resolve();
		}
		if (parms.raidAgeRestriction != null && !parms.raidAgeRestriction.arrivalTextExtra.NullOrEmpty())
		{
			text += "\n\n";
			text += parms.raidAgeRestriction.arrivalTextExtra.Formatted(parms.faction.def.pawnsPlural.Named("PAWNSPLURAL")).Resolve();
		}
		return text;
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
			raidLootPoints *= Find.Storyteller.difficulty.EffectiveRaidLootPointsFactor;
			float num = parms.faction.def.raidLootValueFromPointsCurve.Evaluate(raidLootPoints);
			if (parms.raidStrategy != null)
			{
				num *= parms.raidStrategy.raidLootValueFactor;
			}
			ThingSetMakerParams parms2 = new ThingSetMakerParams
			{
				totalMarketValueRange = new FloatRange(num, num),
				makingFaction = parms.faction
			};
			List<Thing> loot = parms.faction.def.raidLootMaker.root.Generate(parms2);
			new RaidLootDistributor(parms, pawns, loot).DistributeLoot();
		}
	}
}
