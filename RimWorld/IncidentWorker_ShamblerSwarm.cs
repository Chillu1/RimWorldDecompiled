using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class IncidentWorker_ShamblerSwarm : IncidentWorker_EntitySwarm
{
	protected virtual IntRange ShamblerLifespanTicksRange { get; } = new IntRange(150000, 210000);

	protected override PawnGroupKindDef GroupKindDef => PawnGroupKindDefOf.Shamblers;

	protected override LordJob GenerateLordJob(IntVec3 entry, IntVec3 dest)
	{
		return new LordJob_ShamblerSwarm(entry, dest);
	}

	protected override List<Pawn> GenerateEntities(IncidentParms parms, float points)
	{
		List<Pawn> list = base.GenerateEntities(parms, points);
		SetupShamblerHediffs(list, ShamblerLifespanTicksRange);
		return list;
	}

	protected void SetupShamblerHediffs(List<Pawn> shamblers, IntRange lifespanRange)
	{
		foreach (Pawn shambler in shamblers)
		{
			Hediff firstHediffOfDef = shambler.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Shambler);
			HediffComp_DisappearsAndKills hediffComp_DisappearsAndKills = firstHediffOfDef?.TryGetComp<HediffComp_DisappearsAndKills>();
			if (firstHediffOfDef == null || hediffComp_DisappearsAndKills == null)
			{
				Log.ErrorOnce("ShamblerSwarm spawned pawn without Shambler hediff", 63426234);
				continue;
			}
			hediffComp_DisappearsAndKills.disabled = false;
			hediffComp_DisappearsAndKills.ticksToDisappear = lifespanRange.RandomInRange;
		}
	}

	protected override void SendLetter(IncidentParms parms, List<Pawn> entities)
	{
		TaggedString baseLetterLabel = ((entities.Count > 1) ? "LetterLabelShamblerSwarmArrived".Translate() : "LetterLabelShamblerArrived".Translate());
		TaggedString baseLetterText = ((entities.Count > 1) ? "LetterShamblerSwarmArrived".Translate(entities.Count) : "LetterShamblerArrived".Translate());
		int num = entities.Count((Pawn e) => e.kindDef == PawnKindDefOf.ShamblerGorehulk);
		if (num == 1)
		{
			baseLetterText += "\n\n" + "LetterText_ShamblerGorehulk".Translate();
		}
		else if (num > 1)
		{
			baseLetterText += "\n\n" + "LetterText_ShamblerGorehulkPlural".Translate();
		}
		SendStandardLetter(baseLetterLabel, baseLetterText, def.letterDef, parms, entities);
	}
}
