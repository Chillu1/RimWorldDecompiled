using System;
using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_StartRitual : CompAbilityEffect
	{
		private Precept_Ritual ritualCached;

		public new CompProperties_AbilityStartRitual Props => (CompProperties_AbilityStartRitual)props;

		public Precept_Ritual Ritual
		{
			get
			{
				if (ritualCached == null && parent.pawn.Ideo != null)
				{
					for (int i = 0; i < parent.pawn.Ideo.PreceptsListForReading.Count; i++)
					{
						if (parent.pawn.Ideo.PreceptsListForReading[i].def == Props.ritualDef)
						{
							ritualCached = (Precept_Ritual)parent.pawn.Ideo.PreceptsListForReading[i];
							break;
						}
					}
				}
				return ritualCached;
			}
		}

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
		}

		public override bool GizmoDisabled(out string reason)
		{
			Pawn initiator = GetInitiator();
			if (initiator == null)
			{
				reason = "AbilityStartRitualNoMembersOfIdeo".Translate(Ritual.ideo.memberName);
				return true;
			}
			if (ModsConfig.BiotechActive && !Props.allowedForChild && initiator.DevelopmentalStage.Juvenile())
			{
				reason = "AbilityStartRitualDisabledChild".Translate();
				return true;
			}
			if (initiator.Spawned && GatheringsUtility.AnyLordJobPreventsNewRituals(initiator.Map))
			{
				reason = "AbilitySpeechDisabledAnotherGatheringInProgress".Translate();
				return true;
			}
			if (Ritual != null && Ritual.targetFilter != null && !Ritual.targetFilter.CanStart(initiator, TargetInfo.Invalid, out var rejectionReason))
			{
				reason = rejectionReason;
				return true;
			}
			reason = null;
			return false;
		}

		public override Window ConfirmationDialog(LocalTargetInfo target, Action confirmAction)
		{
			TargetInfo targetInfo = TargetInfo.Invalid;
			Pawn initiator = GetInitiator();
			if (Ritual.targetFilter != null)
			{
				targetInfo = Ritual.targetFilter.BestTarget(initiator, target.ToTargetInfo(initiator.Map));
			}
			return Ritual.GetRitualBeginWindow(targetInfo, null, confirmAction, parent.pawn);
		}

		public Pawn GetInitiator()
		{
			if (parent.pawn != null && parent.pawn.Map == Find.CurrentMap)
			{
				return parent.pawn;
			}
			return Find.CurrentMap.mapPawns.FreeColonistsSpawned.FirstOrDefault((Pawn c) => c.Ideo == Ritual.ideo);
		}
	}
}
