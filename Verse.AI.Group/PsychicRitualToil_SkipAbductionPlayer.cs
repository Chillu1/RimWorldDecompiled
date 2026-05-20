using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse.AI.Group;

public class PsychicRitualToil_SkipAbductionPlayer : PsychicRitualToil
{
	private const float ChanceForLeader = 0.08f;

	private const float ChanceForWorldPawn = 0.4f;

	public PsychicRitualRoleDef invokerRole;

	protected PsychicRitualToil_SkipAbductionPlayer()
	{
	}

	public PsychicRitualToil_SkipAbductionPlayer(PsychicRitualRoleDef invokerRole)
	{
		this.invokerRole = invokerRole;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		if (pawn != null)
		{
			ApplyOutcome(psychicRitual, pawn);
		}
	}

	private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker)
	{
		IntVec3 cell = psychicRitual.assignments.Target.Cell;
		bool flag = false;
		Pawn[] source = psychicRitual.Map.attackTargetsCache.TargetsHostileToColony.Where((IAttackTarget t) => t.Thing is Pawn pawn2 && pawn2.RaceProps.Humanlike && !pawn2.IsSubhuman && pawn2.Faction != Faction.OfPlayer && !t.ThreatDisabled(invoker) && !pawn2.IsOnHoldingPlatform).Cast<Pawn>().ToArray();
		Pawn pawn = null;
		if (source.TryRandomElement(out var result))
		{
			pawn = result;
			psychicRitual.Map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_EntryNoDelay.Spawn(pawn, pawn.Map), pawn.PositionHeld, 60);
			SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(pawn.PositionHeld, pawn.Map));
			SkipUtility.SkipTo(pawn, cell, psychicRitual.Map);
		}
		else
		{
			List<Pawn> list = Find.WorldPawns.AllPawnsAlive.Where((Pawn p) => p.RaceProps.Humanlike && p.HostileTo(invoker) && !p.IsSubhuman && p.Faction?.leader == p).ToList();
			List<Pawn> list2 = Find.WorldPawns.AllPawnsAlive.Where((Pawn p) => p.RaceProps.Humanlike && p.HostileTo(invoker) && !p.IsSubhuman && p.Faction?.leader != p).ToList();
			float chance = 0.4f * Mathf.Clamp01((float)list2.Count / 20f);
			Pawn result2 = null;
			if (Rand.Chance(0.08f) && !list.NullOrEmpty())
			{
				list.TryRandomElement(out result2);
			}
			else if (Rand.Chance(chance))
			{
				list2.TryRandomElement(out result2);
			}
			Faction result3;
			if (result2 != null)
			{
				pawn = (Pawn)GenSpawn.Spawn(result2, cell, psychicRitual.Map);
				flag = true;
			}
			else if (Find.FactionManager.AllFactionsVisible.Where((Faction f) => f.def.humanlikeFaction && f.HostileTo(Faction.OfPlayer)).TryRandomElement(out result3))
			{
				pawn = (Pawn)GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(result3.RandomPawnKind(), result3)), cell, psychicRitual.Map);
			}
		}
		if (pawn == null)
		{
			Log.Error("Could not find target pawn for player's skip abduction ritual.");
			return;
		}
		if (pawn.Dead)
		{
			Log.Error($"Skip abduction ritual abducted a dead pawn. World pawn abducted: {flag}");
		}
		if (pawn.IsSubhuman)
		{
			Log.Error($"Skip abduction ritual abducted a mutant. World pawn abducted: {flag}");
		}
		psychicRitual.Map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(cell, psychicRitual.Map), cell, 60);
		SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(cell, psychicRitual.Map));
		int ticksToDisappear = Mathf.RoundToInt(((PsychicRitualDef_SkipAbductionPlayer)psychicRitual.def).comaDurationDaysFromQualityCurve.Evaluate(psychicRitual.PowerPercent) * 60000f);
		Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.DarkPsychicShock, pawn);
		hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = ticksToDisappear;
		pawn.health.AddHediff(hediff);
		pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.PsychicRitualVictim);
		TaggedString text = "SkipAbductionPlayerCompleteText".Translate(invoker.Named("INVOKER"), psychicRitual.def.Named("RITUAL"), pawn.Named("TARGET"), pawn.Faction.Named("FACTION"));
		Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), text, LetterDefOf.NeutralEvent, new LookTargets(pawn));
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
	}
}
