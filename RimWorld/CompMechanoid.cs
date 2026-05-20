using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class CompMechanoid : CompCanBeDormant
{
	private bool active = true;

	private bool deactivated;

	private Thing chargerBeforeTakeoff;

	public bool Deactivated
	{
		get
		{
			if (deactivated)
			{
				return true;
			}
			Faction ofMechanoids = Faction.OfMechanoids;
			if (ofMechanoids == null || !ofMechanoids.deactivated)
			{
				return false;
			}
			if (parent.Faction != Faction.OfMechanoids)
			{
				return parent.Faction == null;
			}
			return true;
		}
	}

	protected override bool ShowZs
	{
		get
		{
			if (!Deactivated && base.Props.showSleepingZs)
			{
				if (!base.Props.delayedWakeUpDoesZs)
				{
					return wakeUpOnTick == int.MinValue;
				}
				return true;
			}
			return false;
		}
	}

	protected override JobDef SleepJob
	{
		get
		{
			Faction faction = parent.Faction;
			if (faction == null || !faction.deactivated)
			{
				return base.SleepJob;
			}
			return JobDefOf.Deactivated;
		}
	}

	private Pawn Pawn => parent as Pawn;

	private Building Building => parent as Building;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref active, "active", defaultValue: true);
		Scribe_Values.Look(ref deactivated, "deactivated", defaultValue: false);
		Scribe_References.Look(ref chargerBeforeTakeoff, "chargerBeforeTakeoff");
	}

	public override void CompTick()
	{
		base.CompTick();
		if (Deactivated && active)
		{
			ToSleep();
		}
	}

	public void Deactivate()
	{
		deactivated = true;
		ToSleep();
		Pawn.health.CheckForStateChange(null, null);
	}

	public override void ToSleep()
	{
		base.ToSleep();
		Pawn?.jobs.EndCurrentJob(JobCondition.InterruptForced);
		Pawn?.GetLord()?.Notify_PawnLost(Pawn, PawnLostCondition.Incapped);
		Building?.GetLord()?.Notify_BuildingLost(Building);
		active = false;
	}

	public override void WakeUp()
	{
		if (!Deactivated)
		{
			base.WakeUp();
			active = true;
		}
	}

	public override void PreSwapMap()
	{
		chargerBeforeTakeoff = Pawn?.needs?.energy?.currentCharger;
	}

	public override void PostSwapMap()
	{
		if (chargerBeforeTakeoff != null)
		{
			Job newJob = JobMaker.MakeJob(JobDefOf.MechCharge, chargerBeforeTakeoff);
			Pawn.jobs.StartJob(newJob, JobCondition.InterruptForced);
			chargerBeforeTakeoff = null;
		}
	}

	public override string CompInspectStringExtra()
	{
		if (Deactivated)
		{
			return "Deactivated".Translate() + ".";
		}
		return null;
	}
}
