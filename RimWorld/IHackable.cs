using Verse;

namespace RimWorld;

public interface IHackable
{
	void OnLockedOut(Pawn pawn = null);

	void OnHacked(Pawn pawn = null);
}
