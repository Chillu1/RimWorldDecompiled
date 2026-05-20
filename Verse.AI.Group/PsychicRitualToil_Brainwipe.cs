using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI.Group;

public class PsychicRitualToil_Brainwipe : PsychicRitualToil
{
	public PsychicRitualRoleDef targetRole;

	public PsychicRitualRoleDef invokerRole;

	public int comaDurationTicks;

	protected PsychicRitualToil_Brainwipe()
	{
	}

	public PsychicRitualToil_Brainwipe(PsychicRitualRoleDef invokerRole, PsychicRitualRoleDef targetRole)
	{
		this.invokerRole = invokerRole;
		this.targetRole = targetRole;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		base.Start(psychicRitual, parent);
		PsychicRitualDef_Brainwipe psychicRitualDef_Brainwipe = (PsychicRitualDef_Brainwipe)psychicRitual.def;
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		Pawn pawn2 = psychicRitual.assignments.FirstAssignedPawn(targetRole);
		comaDurationTicks = Mathf.RoundToInt(psychicRitualDef_Brainwipe.comaDurationDaysFromQualityCurve.Evaluate(psychicRitual.PowerPercent) * 60000f);
		if (pawn != null && pawn2 != null)
		{
			ApplyOutcome(psychicRitual, pawn, pawn2);
		}
	}

	private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, Pawn target)
	{
		Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BrainwipeComa, target);
		hediff.TryGetComp<HediffComp_Disappears>()?.SetDuration(comaDurationTicks);
		target.health.AddHediff(hediff);
		bool flag = target.Inhumanized();
		if (flag)
		{
			target.Rehumanize();
		}
		bool recruitable = target.guest.Recruitable;
		target.guest.Recruitable = true;
		MemoryThoughtHandler memories = target.needs.mood.thoughts.memories;
		List<Thought_Memory> list = new List<Thought_Memory>();
		List<Thought_Memory> list2 = new List<Thought_Memory>();
		foreach (Thought_Memory memory in memories.Memories)
		{
			if (memory.MoodOffset() != 0f && memory.DurationTicks > 0)
			{
				list.Add(memory);
			}
			if (memory is ISocialThought)
			{
				list2.Add(memory);
			}
		}
		foreach (Thought_Memory item in list)
		{
			memories.RemoveMemory(item);
		}
		foreach (Thought_Memory item2 in list2)
		{
			memories.RemoveMemory(item2);
		}
		target.guest.resistance = 0f;
		target.guest.will = 0f;
		if (ModsConfig.IdeologyActive)
		{
			target.ideo.OffsetCertainty(0f - target.ideo.Certainty);
		}
		TaggedString text = "BrainwipeCompleteText".Translate(invoker.Named("INVOKER"), psychicRitual.def.Named("RITUAL"), target.Named("TARGET"));
		text += "\n\n" + "BrainwipeTargetComa".Translate(target.Named("TARGET"), comaDurationTicks.ToStringTicksToDays()).CapitalizeFirst();
		if (!recruitable)
		{
			text += "\n\n" + "BrainwipeUnwaveringlyLoyal".Translate(target.Named("TARGET"));
		}
		if (flag)
		{
			text += "\n\n" + "BrainwipeRehumanized".Translate(target.Named("TARGET"));
		}
		Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), text, LetterDefOf.NeutralEvent, new LookTargets(invoker, target));
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
		Scribe_Defs.Look(ref targetRole, "targetRole");
		Scribe_Values.Look(ref comaDurationTicks, "comaDurationTicks", 0);
	}
}
