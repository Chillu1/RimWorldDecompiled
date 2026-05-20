using Verse;

namespace RimWorld;

public class CompMaintainable : ThingComp
{
	public int ticksSinceMaintain;

	public CompProperties_Maintainable Props => (CompProperties_Maintainable)props;

	public MaintainableStage CurStage
	{
		get
		{
			if (ticksSinceMaintain < Props.ticksHealthy)
			{
				return MaintainableStage.Healthy;
			}
			if (ticksSinceMaintain < Props.ticksHealthy + Props.ticksNeedsMaintenance)
			{
				return MaintainableStage.NeedsMaintenance;
			}
			return MaintainableStage.Damaging;
		}
	}

	private bool Active
	{
		get
		{
			if (parent is Hive hive)
			{
				return hive.CompDormant.Awake;
			}
			return true;
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref ticksSinceMaintain, "ticksSinceMaintain", 0);
	}

	public override void CompTick()
	{
		base.CompTick();
		if (Active)
		{
			ticksSinceMaintain++;
			if (parent.IsHashIntervalTick(250))
			{
				CheckTakeDamage();
			}
		}
	}

	public override void CompTickRare()
	{
		base.CompTickRare();
		if (Active)
		{
			ticksSinceMaintain += 250;
			CheckTakeDamage();
		}
	}

	private void CheckTakeDamage()
	{
		if (CurStage == MaintainableStage.Damaging)
		{
			parent.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, Props.damagePerTickRare));
		}
	}

	public void Maintained()
	{
		ticksSinceMaintain = 0;
	}

	public override string CompInspectStringExtra()
	{
		return CurStage switch
		{
			MaintainableStage.NeedsMaintenance => "DueForMaintenance".Translate(), 
			MaintainableStage.Damaging => "DeterioratingDueToLackOfMaintenance".Translate(), 
			_ => null, 
		};
	}
}
