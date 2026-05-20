using Verse;

namespace RimWorld;

public class SignalAction_StartWick : SignalAction_Delay
{
	public Thing instigator;

	public Thing thingWithWick;

	private Alert_ActionDelay cachedAlert;

	public override Alert_ActionDelay Alert
	{
		get
		{
			if (cachedAlert == null && thingWithWick.def == ThingDefOf.AncientFuelNode)
			{
				cachedAlert = new Alert_FuelNodeIgnition(this);
			}
			return cachedAlert;
		}
	}

	public override bool ShouldRemoveNow
	{
		get
		{
			if (thingWithWick == null || thingWithWick.Destroyed)
			{
				return true;
			}
			CompExplosive compExplosive = thingWithWick.TryGetComp<CompExplosive>();
			if (compExplosive == null || compExplosive.wickStarted)
			{
				return true;
			}
			return false;
		}
	}

	protected override void Complete()
	{
		base.Complete();
		if (!ShouldRemoveNow)
		{
			thingWithWick.TryGetComp<CompExplosive>()?.StartWick(instigator);
			if (delayTicks > 0)
			{
				Messages.Message("MessageFuelNodeDelayActivated".Translate(thingWithWick.LabelShort), thingWithWick, MessageTypeDefOf.NegativeEvent);
			}
			else
			{
				Messages.Message("MessageFuelNodeTriggered".Translate(thingWithWick.LabelShort), thingWithWick, MessageTypeDefOf.ThreatBig);
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref instigator, "instigator");
		Scribe_References.Look(ref thingWithWick, "thingWithWick");
	}
}
