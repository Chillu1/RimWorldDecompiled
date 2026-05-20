using Verse;

namespace RimWorld;

public class PawnShamblerScarDrawer : PawnScarDrawer
{
	protected override string ScarTexturePath => "Things/Pawn/Overlays/ShamblerScars/ShamblerScarOverlay";

	public PawnShamblerScarDrawer(Pawn pawn)
		: base(pawn)
	{
	}
}
