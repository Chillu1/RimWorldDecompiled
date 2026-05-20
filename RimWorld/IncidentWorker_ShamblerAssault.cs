using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class IncidentWorker_ShamblerAssault : IncidentWorker_RaidEnemy
{
	private static readonly IntRange ShamblerLifespanTicksRange = new IntRange(10000, 25000);

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		parms.pawnGroupKind = PawnGroupKindDefOf.Shamblers;
		return base.TryExecuteWorker(parms);
	}

	protected override bool TryResolveRaidFaction(IncidentParms parms)
	{
		parms.faction = Faction.OfEntities;
		return true;
	}

	public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
	{
		parms.raidStrategy = RaidStrategyDefOf.ShamblerAssault;
	}

	protected override void PostProcessSpawnedPawns(IncidentParms parms, List<Pawn> pawns)
	{
		foreach (Pawn pawn in pawns)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Shambler);
			HediffComp_DisappearsAndKills hediffComp_DisappearsAndKills = firstHediffOfDef?.TryGetComp<HediffComp_DisappearsAndKills>();
			if (firstHediffOfDef == null || hediffComp_DisappearsAndKills == null)
			{
				Log.ErrorOnce("ShamblerAssault spawned pawn without Shambler hediff", 23407834);
				continue;
			}
			hediffComp_DisappearsAndKills.disabled = false;
			hediffComp_DisappearsAndKills.ticksToDisappear = ShamblerLifespanTicksRange.RandomInRange;
		}
		if (AnomalyIncidentUtility.IncidentShardChance(parms.points))
		{
			AnomalyIncidentUtility.PawnShardOnDeath(pawns.RandomElement());
		}
	}

	protected override string GetLetterLabel(IncidentParms parms)
	{
		return parms.raidStrategy.letterLabelEnemy;
	}

	protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
	{
		string text = string.Format(parms.raidArrivalMode.textEnemy, parms.pawnCount).CapitalizeFirst();
		text += "\n\n";
		text += parms.raidStrategy.arrivalTextEnemy;
		if (parms.pawnGroups != null && parms.PawnGroupCount > 1)
		{
			text += "\n\n" + "AttackingFromMultipleDirections".Translate();
		}
		int num = pawns.Count((Pawn e) => e.kindDef == PawnKindDefOf.ShamblerGorehulk);
		if (num == 1)
		{
			text += "\n\n" + "LetterText_ShamblerGorehulk".Translate();
		}
		else if (num > 1)
		{
			text += "\n\n" + "LetterText_ShamblerGorehulkPlural".Translate();
		}
		return text;
	}
}
