using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompAbilityEffect_StartAnimaLinking : CompAbilityEffect_StartRitual
	{
		public override bool ShouldHideGizmo
		{
			get
			{
				if (parent.pawn.GetPsylinkLevel() >= parent.pawn.GetMaxPsylinkLevel())
				{
					return true;
				}
				if (!MeditationFocusDefOf.Natural.CanPawnUse(parent.pawn))
				{
					return true;
				}
				if (!parent.pawn.psychicEntropy.IsPsychicallySensitive)
				{
					return true;
				}
				return false;
			}
		}

		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			Pawn pawn = parent.pawn;
			CompPsylinkable compPsylinkable = target.Thing?.TryGetComp<CompPsylinkable>();
			if (compPsylinkable == null)
			{
				return false;
			}
			int requiredPlantCount = compPsylinkable.GetRequiredPlantCount(pawn);
			if (requiredPlantCount == -1)
			{
				return false;
			}
			if (!pawn.CanReserve(target.Thing))
			{
				Pawn pawn2 = pawn.Map.reservationManager.FirstRespectedReserver(target.Thing, pawn);
				if (throwMessages)
				{
					Messages.Message((pawn2 == null) ? "Reserved".Translate() : "ReservedBy".Translate(pawn.LabelShort, pawn2), target.Thing, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			CompSpawnSubplant compSpawnSubplant = target.Thing.TryGetComp<CompSpawnSubplant>();
			if (compSpawnSubplant.SubplantsForReading.Count < requiredPlantCount)
			{
				if (throwMessages)
				{
					Messages.Message("BeginLinkingRitualNeedSubplants".Translate(requiredPlantCount.ToString(), compSpawnSubplant.Props.subplant.label, compSpawnSubplant.SubplantsForReading.Count.ToString()), target.Thing, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			if (!compPsylinkable.TryFindLinkSpot(pawn, out var _) && throwMessages)
			{
				Messages.Message("BeginLinkingRitualNeedLinkSpot".Translate(), target.Thing, MessageTypeDefOf.RejectInput, historical: false);
			}
			return base.Valid(target, throwMessages);
		}

		public override Window ConfirmationDialog(LocalTargetInfo target, Action confirmAction)
		{
			TargetInfo targetInfo = TargetInfo.Invalid;
			if (base.Ritual.targetFilter != null)
			{
				targetInfo = base.Ritual.targetFilter.BestTarget(parent.pawn, target.ToTargetInfo(parent.pawn.MapHeld));
			}
			return base.Ritual.GetRitualBeginWindow(targetInfo, null, confirmAction, null, null, parent.pawn);
		}
	}
}
