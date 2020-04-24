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
			ResolveMySkinColor(ref request, generated, other);
		}

		private float GetOldAgeFactor(Pawn pawn)
		{
			return Mathf.Clamp(GenMath.LerpDouble(50f, 80f, 1f, 0.01f, pawn.ageTracker.AgeBiologicalYears), 0.01f, 1f);
		}

		public override void OnRelationCreated(Pawn firstPawn, Pawn secondPawn)
		{
			firstPawn.relations.nextMarriageNameChange = (secondPawn.relations.nextMarriageNameChange = SpouseRelationUtility.Roll_NameChangeOnMarriage());
		}

		private static void ResolveMySkinColor(ref PawnGenerationRequest request, Pawn generated, Pawn other)
		{
			if (!request.FixedMelanin.HasValue)
			{
				request.SetFixedMelanin(PawnSkinColors.GetRandomMelaninSimilarTo(other.story.melanin));
			}
		}
	}
}
