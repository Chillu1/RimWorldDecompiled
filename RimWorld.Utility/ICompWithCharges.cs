namespace RimWorld.Utility;

public interface ICompWithCharges
{
	int RemainingCharges { get; }

	bool CanBeUsed(out string reason);
}
