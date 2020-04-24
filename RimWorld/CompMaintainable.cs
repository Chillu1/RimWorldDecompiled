using Verse;

namespace RimWorld
{
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

		private bool Active => (parent as Hive)?.CompDormant.Awake ?? true;

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
				if (Find.TickManager.TicksGame % 250 == 0)
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
			switch (CurStage)
			{
			case MaintainableStage.NeedsMaintenance:
				return "DueForMaintenance".Translate();
			case MaintainableStage.Damaging:
				return "DeterioratingDueToLackOfMaintenance".Translate();
			default:
				return null;
			}
		}
	}
}
