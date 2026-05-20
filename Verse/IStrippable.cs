namespace Verse;

public interface IStrippable
{
	bool AnythingToStrip();

	void Strip(bool notifyFaction = true);
}
