using Verse;

namespace RimWorld;

public class PawnScariaSoreDrawer : PawnScarDrawer
{
	protected override string ScarTexturePath => "Things/Pawn/Overlays/Scaria/ScariaSoresOverlay";

	protected override float ScaleFactor => 0.5f;

	public PawnScariaSoreDrawer(Pawn pawn)
		: base(pawn)
	{
	}
}
