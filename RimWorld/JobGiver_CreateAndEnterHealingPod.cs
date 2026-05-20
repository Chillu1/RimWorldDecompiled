using Verse;

namespace RimWorld
{
	public class JobGiver_CreateAndEnterHealingPod : JobGiver_CreateAndEnterDryadHolder
	{
		public override JobDef JobDef => JobDefOf.CreateAndEnterHealingPod;

		public override bool ExtraValidator(Pawn pawn, CompTreeConnection connectionComp)
		{
			if (pawn.mindState != null && pawn.mindState.returnToHealingPod)
			{
				if (HealthAIUtility.ShouldSeekMedicalRest(pawn))
				{
					return true;
				}
				if (pawn.health.hediffSet.GetMissingPartsCommonAncestors().Any())
				{
					return true;
				}
			}
			return base.ExtraValidator(pawn, connectionComp);
		}
	}
}
