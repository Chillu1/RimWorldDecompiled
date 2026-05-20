using Verse;

namespace RimWorld;

public class CompSchedule : ThingComp
{
	public const string ScheduledOnSignal = "ScheduledOn";

	public const string ScheduledOffSignal = "ScheduledOff";

	private bool intAllowed;

	public CompProperties_Schedule Props => (CompProperties_Schedule)props;

	public bool Allowed
	{
		get
		{
			return intAllowed;
		}
		set
		{
			if (intAllowed != value)
			{
				intAllowed = value;
				parent.BroadcastCompSignal(intAllowed ? "ScheduledOn" : "ScheduledOff");
			}
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		RecalculateAllowed();
	}

	public override void CompTickRare()
	{
		base.CompTickRare();
		RecalculateAllowed();
	}

	public void RecalculateAllowed()
	{
		float num = GenLocalDate.DayPercent(parent);
		if (Props.startTime <= Props.endTime)
		{
			Allowed = num > Props.startTime && num < Props.endTime;
		}
		else
		{
			Allowed = num < Props.endTime || num > Props.startTime;
		}
	}

	public override string CompInspectStringExtra()
	{
		if (!Allowed)
		{
			return Props.offMessage;
		}
		return null;
	}
}
