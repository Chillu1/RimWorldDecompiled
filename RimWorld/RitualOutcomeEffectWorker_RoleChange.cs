using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_RoleChange : RitualOutcomeEffectWorker
{
	public override bool SupportsAttachableOutcomeEffect => false;

	public RitualOutcomeEffectWorker_RoleChange()
	{
	}

	public RitualOutcomeEffectWorker_RoleChange(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
	{
		RitualOutcomeComp ritualOutcomeComp = def.comps.First();
		float num = ritualOutcomeComp.QualityOffset(jobRitual, DataForComp(ritualOutcomeComp));
		bool num2 = num > 0.5f;
		RitualOutcomePossibility ritualOutcomePossibility = (num2 ? def.outcomeChances[1] : def.outcomeChances[0]);
		string text = ritualOutcomePossibility.description.Formatted(jobRitual.Ritual.Label, jobRitual.Ritual.ideo.Named("IDEO")).CapitalizeFirst();
		text += "\n\n" + "RitualOutcomeQualitySpecific".Translate(jobRitual.Ritual.Label, num.ToStringPercent()).CapitalizeFirst() + ":\n";
		text = text + "\n  - " + ritualOutcomeComp.GetDesc(jobRitual, DataForComp(ritualOutcomeComp)).CapitalizeFirst();
		Find.LetterStack.ReceiveLetter("OutcomeLetterLabel".Translate(ritualOutcomePossibility.label.Named("OUTCOMELABEL"), jobRitual.Ritual.Label.Named("RITUALLABEL")), text, ritualOutcomePossibility.Positive ? LetterDefOf.RitualOutcomePositive : LetterDefOf.RitualOutcomeNegative, jobRitual.selectedTarget);
		if (num2)
		{
			Pawn pawn = jobRitual.assignments.FirstAssignedPawn("role_changer");
			Precept_Role roleChangeSelection = jobRitual.assignments.RoleChangeSelection;
			Precept_Role role = pawn.Ideo.GetRole(pawn);
			if (roleChangeSelection != null)
			{
				role?.Unassign(pawn, generateThoughts: false);
				roleChangeSelection.Assign(pawn, addThoughts: true);
			}
			else
			{
				role?.Unassign(pawn, generateThoughts: true);
			}
		}
	}

	public override RitualOutcomePossibility GetForcedOutcome(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments)
	{
		RitualOutcomeComp ritualOutcomeComp = def.comps.First();
		if (!(ritualOutcomeComp.GetQualityFactor(ritual, ritualTarget, obligation, assignments, DataForComp(ritualOutcomeComp)).quality > 0.5f))
		{
			return def.outcomeChances[0];
		}
		return def.outcomeChances[1];
	}

	public override IEnumerable<string> BlockingIssues(Precept_Ritual ritual, TargetInfo target, RitualRoleAssignments assignments)
	{
		RitualOutcomeComp ritualOutcomeComp = def.comps.First();
		RitualOutcomeComp_DataRoleChangeParticipants ritualOutcomeComp_DataRoleChangeParticipants = (RitualOutcomeComp_DataRoleChangeParticipants)DataForComp(ritualOutcomeComp);
		if (ritualOutcomeComp.GetQualityFactor(ritual, target, null, assignments, ritualOutcomeComp_DataRoleChangeParticipants).quality < 0.5f)
		{
			yield return "MessageNotEnoughSpectators".Translate(ritualOutcomeComp_DataRoleChangeParticipants.desiredParticipantCount, ritual.Label);
		}
		Pawn pawn = assignments.FirstAssignedPawn("role_changer");
		if (pawn != null && assignments.RoleChangeSelection == pawn.Ideo.GetRole(pawn))
		{
			yield return "MessageRoleChangeChooseDifferentRole".Translate(pawn.Named("PAWN"));
		}
	}
}
