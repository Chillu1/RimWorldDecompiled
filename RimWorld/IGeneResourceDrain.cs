using Verse;

namespace RimWorld
{
	public interface IGeneResourceDrain
	{
		Gene_Resource Resource { get; }

		bool CanOffset { get; }

		float ResourceLossPerDay { get; }

		Pawn Pawn { get; }

		string DisplayLabel { get; }
	}
}
