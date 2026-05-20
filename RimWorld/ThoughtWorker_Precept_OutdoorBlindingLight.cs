using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_OutdoorBlindingLight : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (!p.Awake() || PawnUtility.IsBiologicallyOrArtificiallyBlind(p))
			{
				return false;
			}
			return p.Map.glowGrid.PsychGlowAt(p.Position) == PsychGlow.Overlit && !p.Position.Roofed(p.Map);
		}
	}
}
