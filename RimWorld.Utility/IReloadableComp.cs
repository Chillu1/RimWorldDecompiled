using Verse;

namespace RimWorld.Utility;

public interface IReloadableComp : ICompWithCharges
{
	Thing ReloadableThing { get; }

	ThingDef AmmoDef { get; }

	int BaseReloadTicks { get; }

	int MaxCharges { get; }

	string LabelRemaining { get; }

	bool NeedsReload(bool allowForceReload);

	int MinAmmoNeeded(bool allowForcedReload);

	int MaxAmmoNeeded(bool allowForcedReload);

	int MaxAmmoAmount();

	void ReloadFrom(Thing ammo);

	string DisabledReason(int minNeeded, int maxNeeded);
}
