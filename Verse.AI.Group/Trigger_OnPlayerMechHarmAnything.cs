using System.Collections.Generic;

namespace Verse.AI.Group;

public class Trigger_OnPlayerMechHarmAnything : Trigger
{
	private List<Thing> things;

	public Trigger_OnPlayerMechHarmAnything(List<Thing> things)
	{
		this.things = things;
	}

	public override bool ActivateOn(Lord lord, TriggerSignal signal)
	{
		if (ModsConfig.BiotechActive && signal.dinfo.Def != null && signal.dinfo.Def.ExternalViolenceFor(signal.thing) && signal.dinfo.Instigator != null && signal.dinfo.Instigator is Pawn { IsColonyMech: not false } && things.Contains(signal.thing))
		{
			return true;
		}
		return false;
	}
}
