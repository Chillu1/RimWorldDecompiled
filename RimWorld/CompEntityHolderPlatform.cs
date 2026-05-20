using Verse;

namespace RimWorld;

public class CompEntityHolderPlatform : CompEntityHolder
{
	public override bool Available => !base.HoldingPlatform.Occupied;

	public override Pawn HeldPawn => base.HoldingPlatform.HeldPawn;

	public override ThingOwner Container => base.HoldingPlatform.innerContainer;

	public new CompProperties_EntityHolderPlatform Props => (CompProperties_EntityHolderPlatform)props;

	public override void EjectContents()
	{
		base.HoldingPlatform.EjectContents();
	}
}
