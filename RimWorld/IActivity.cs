namespace RimWorld;

public interface IActivity
{
	void OnActivityActivated();

	void OnPassive();

	bool ShouldGoPassive();

	bool CanBeSuppressed();

	bool CanActivate();

	string ActivityTooltipExtra();
}
