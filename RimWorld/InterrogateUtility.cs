using Verse;

namespace RimWorld;

public static class InterrogateUtility
{
	private const float SocialFactor = 0.15f;

	public static float GetChance(Pawn pawn)
	{
		return pawn.GetStatValue(StatDefOf.SocialImpact) * 0.15f;
	}
}
