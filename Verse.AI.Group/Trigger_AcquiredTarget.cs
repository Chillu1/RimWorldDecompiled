using RimWorld;

namespace Verse.AI.Group;

public class Trigger_AcquiredTarget : Trigger
{
	private Faction faction;

	public Trigger_AcquiredTarget(Faction requireTargetWithSpecificFaction)
	{
		faction = requireTargetWithSpecificFaction;
	}

	public override bool ActivateOn(Lord lord, TriggerSignal signal)
	{
		if (signal.type != TriggerSignalType.AcquiredTarget)
		{
			return false;
		}
		if (faction != null && signal.otherThing?.Faction != faction)
		{
			return false;
		}
		return true;
	}
}
