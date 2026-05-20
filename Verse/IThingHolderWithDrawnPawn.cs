using RimWorld;

namespace Verse
{
	public interface IThingHolderWithDrawnPawn : IThingHolder
	{
		float HeldPawnDrawPos_Y { get; }

		float HeldPawnBodyAngle { get; }

		PawnPosture HeldPawnPosture { get; }
	}
}
