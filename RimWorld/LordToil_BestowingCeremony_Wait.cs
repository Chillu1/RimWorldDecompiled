using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class LordToil_BestowingCeremony_Wait : LordToil_Wait
	{
		public Pawn target;

		public LordToil_BestowingCeremony_Wait(Pawn target)
		{
			this.target = target;
		}

		public override void Init()
		{
			Messages.Message("MessageBestowerWaiting".Translate(target.Named("TARGET"), lord.ownedPawns[0].Named("BESTOWER")), new LookTargets(new Pawn[2]
			{
				target,
				lord.ownedPawns[0]
			}), MessageTypeDefOf.NeutralEvent);
		}

		protected override void DecoratePawnDuty(PawnDuty duty)
		{
			duty.focus = target;
		}

		public override void DrawPawnGUIOverlay(Pawn pawn)
		{
			pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.QuestionMark);
		}

		public override IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn bestower, Pawn forPawn)
		{
			if (forPawn == target)
			{
				yield return new FloatMenuOption("BeginCeremony".Translate(bestower.Named("BESTOWER")), delegate
				{
					((LordJob_BestowingCeremony)lord.LordJob).StartCeremony(forPawn);
				}, MenuOptionPriority.High);
			}
		}
	}
}
