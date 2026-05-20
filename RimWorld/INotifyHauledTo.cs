using Verse;

namespace RimWorld;

public interface INotifyHauledTo
{
	void Notify_HauledTo(Pawn hauler, Thing thing, int count);
}
