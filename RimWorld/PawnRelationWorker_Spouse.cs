using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_Spouse : PawnRelationWorker
	{
		public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			return LovePartnerRelationUtility.LovePartnerRelationGenerationChance(generated, other, request, ex: false) * BaseGenerationChanceFactor(generated, other, request);
		}

		public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			generated.relations.AddDirectRelation(PawnRelationDefOf.Spouse, other);
			LovePartnerRelationUtility.TryToShareChildrenForGeneratedLovePartner(generated, other, request, 1f);
			SpouseRelationUtility.ResolveNameForSpouseOnGeneration(ref request, generated);
			ResolveMySkinColor(ref request, generated, other);
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
