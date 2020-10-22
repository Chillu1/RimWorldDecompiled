using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class CompAbilityEffect_StartSpeech : CompAbilityEffect
	{
		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			parent.pawn.drafter.Drafted = false;
			parent.pawn.Map.lordsStarter.TryStartReigningSpeech(parent.pawn);
		}

		public override bool GizmoDisabled(out string reason)
		{
			LordJob_Joinable_Speech lordJob_Joinable_Speech = parent.pawn.GetLord()?.LordJob as LordJob_Joinable_Speech;
			if (lordJob_Joinable_Speech != null && lordJob_Joinable_Speech.Organizer == parent.pawn)
			{
				reason = "AbilitySpeechDisabledAlreadyGivingSpeech".Translate();
				return true;
			}
			if (GatheringsUtility.AnyLordJobPreventsNewGatherings(parent.pawn.Map))
			{
				reason = "AbilitySpeechDisabledAnotherGatheringInProgress".Translate();
				return true;
			}
			Building_Throne assignedThrone = parent.pawn.ownership.AssignedThrone;
			if (assignedThrone == null)
			{
				reason = "AbilitySpeechDisabledNoThroneAssigned".Translate();
				return true;
			}
			if (!parent.pawn.CanReserveAndReach(assignedThrone, PathEndMode.InteractionCell, parent.pawn.NormalMaxDanger()))
			{
				reason = "AbilitySpeechDisabledNoThroneIsNotAccessible".Translate();
				return true;
			}
			if (parent.pawn.royalty.GetUnmetThroneroomRequirements().Any())
			{
				reason = "AbilitySpeechDisabledNoThroneUndignified".Translate();
				return true;
			}
			reason = null;
			return false;
		}
	}
}
