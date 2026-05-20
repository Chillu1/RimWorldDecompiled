using System.Collections.Generic;

namespace Verse.AI.Group;

public class Trigger_OnHumanlikeHarmAnyThing : Trigger
{
	private List<Thing> things;

	public Trigger_OnHumanlikeHarmAnyThing(List<Thing> things)
	{
		this.things = things;
	}

	public override bool ActivateOn(Lord lord, TriggerSignal signal)
	{
		if (signal.dinfo.Def != null && signal.dinfo.Def.ExternalViolenceFor(signal.thing) && signal.dinfo.Instigator != null && signal.dinfo.Instigator is Pawn pawn && pawn.RaceProps.Humanlike && things.Contains(signal.thing))
		{
			return true;
		}
		return false;
	}
}
