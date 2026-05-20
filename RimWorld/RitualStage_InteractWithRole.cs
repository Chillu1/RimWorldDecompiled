using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualStage_InteractWithRole : RitualStage
	{
		public class PawnTarget : IExposable
		{
			[NoTranslate]
			public string pawnId;

			[NoTranslate]
			public string targetId;

			public void ExposeData()
			{
				Scribe_Values.Look(ref pawnId, "pawnId");
				Scribe_Values.Look(ref targetId, "targetId");
			}
		}

		[NoTranslate]
		public string targetId;

		public List<PawnTarget> targets;

		public override TargetInfo GetSecondFocus(LordJob_Ritual ritual)
		{
			if (targetId != null)
			{
				return ritual.PawnWithRole(targetId);
			}
			return base.GetSecondFocus(ritual);
		}

		public override IEnumerable<RitualStagePawnSecondFocus> GetPawnSecondFoci(LordJob_Ritual ritual)
		{
			if (targets.NullOrEmpty())
			{
				yield break;
			}
			foreach (PawnTarget target in targets)
			{
				Pawn pawn = ritual.assignments.FirstAssignedPawn(target.pawnId);
				Pawn pawn2 = ritual.assignments.FirstAssignedPawn(target.targetId);
				yield return new RitualStagePawnSecondFocus
				{
					pawn = pawn,
					target = pawn2
				};
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref targetId, "targetId");
			Scribe_Collections.Look(ref targets, "targets", LookMode.Deep);
		}
	}
}
