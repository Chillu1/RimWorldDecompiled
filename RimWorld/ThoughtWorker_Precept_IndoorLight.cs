using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_IndoorLight : ThoughtWorker_Precept
{
	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!p.Awake() || PawnUtility.IsBiologicallyOrArtificiallyBlind(p))
		{
			return false;
		}
		return p.Map.glowGrid.PsychGlowAt(p.Position) != PsychGlow.Dark && p.Position.Roofed(p.Map) && !DarklightUtility.IsDarklightAt(p.Position, p.Map);
	}
}
