using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_AnyBodyPartCovered : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return HasUnnecessarilyCoveredBodyParts(p);
		}

		public static bool HasUnnecessarilyCoveredBodyParts(Pawn p)
		{
			if (p.apparel != null && p.apparel.AnyClothing && PawnUtility.HasClothingNotRequiredByKind(p))
			{
				return GenTemperature.SafeTemperatureRange(p.def).Includes(p.AmbientTemperature);
			}
			return false;
		}
	}
}
