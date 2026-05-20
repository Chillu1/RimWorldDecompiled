using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_StartRitualOnPawn : CompAbilityEffect_StartRitual
	{
		public new CompProperties_AbilityStartRitualOnPawn Props => (CompProperties_AbilityStartRitualOnPawn)props;

		protected virtual Precept_Ritual RitualForTarget(Pawn pawn)
		{
			return base.Ritual;
		}

		public override Window ConfirmationDialog(LocalTargetInfo target, Action confirmAction)
		{
			Pawn pawn = target.Pawn;
			Precept_Ritual precept_Ritual = RitualForTarget(pawn);
			TargetInfo targetInfo = TargetInfo.Invalid;
			if (precept_Ritual.targetFilter != null)
			{
				targetInfo = precept_Ritual.targetFilter.BestTarget(parent.pawn, target.ToTargetInfo(parent.pawn.MapHeld));
			}
			return precept_Ritual.GetRitualBeginWindow(targetInfo, null, confirmAction, parent.pawn, new Dictionary<string, Pawn> { { Props.targetRoleId, pawn } });
		}
	}
}
