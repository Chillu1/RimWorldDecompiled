using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_Fiance : PawnRelationWorker
	{
		public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			float num = 1f;
			num *= GetOldAgeFactor(generated);
			num *= GetOldAgeFactor(other);
			return LovePartnerRelationUtility.LovePartnerRelationGenerationChance(generated, other, request, ex: false) * BaseGenerationChanceFactor(generated, other, request) * num;
		}

		public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			generated.relations.AddDirectRelation(PawnRelationDefOf.Fiance, other);
			LovePartnerRelationUtility.TryToShareChildrenForGeneratedLovePartner(generated, other, request, 0.7f);
		}

		private float GetOldAgeFactor(Pawn pawn)
		{
			return Mathf.Clamp(GenMath.LerpDouble(50f, 80f, 1f, 0.01f, pawn.ageTracker.AgeBiologicalYears), 0.01f, 1f);
		}

		public override void OnRelationCreated(Pawn firstPawn, Pawn secondPawn)
		{
			Pawn pawn = ((firstPawn.Ideo != secondPawn.Ideo) ? ((Rand.Value < 0.5f) ? firstPawn : secondPawn) : firstPawn);
			firstPawn.relations.nextMarriageNameChange = (secondPawn.relations.nextMarriageNameChange = SpouseRelationUtility.Roll_NameChangeOnMarriage(pawn));
		}
	}
}
