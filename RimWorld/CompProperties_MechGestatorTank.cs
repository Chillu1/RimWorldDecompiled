using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_MechGestatorTank : CompProperties
{
	[MustTranslate]
	public string triggeredMessage;

	public FloatRange triggerRadiusRange = new FloatRange(6f, 12f);

	public GraphicData dormantGraphic;

	public GraphicData emptyGraphic;

	public SoundDef triggerSound;

	public List<PawnKindDefWeight> mechKindOptions = new List<PawnKindDefWeight>();

	public CompProperties_MechGestatorTank()
	{
		compClass = typeof(CompMechGestatorTank);
	}
}
