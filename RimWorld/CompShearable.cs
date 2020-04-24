using Verse;

namespace RimWorld
{
	public class CompShearable : CompHasGatherableBodyResource
	{
		protected override int GatherResourcesIntervalDays => Props.shearIntervalDays;

		protected override int ResourceAmount => Props.woolAmount;

		protected override ThingDef ResourceDef => Props.woolDef;

		protected override string SaveKey => "woolGrowth";

		public CompProperties_Shearable Props => (CompProperties_Shearable)props;

		protected override bool Active
		{
			get
			{
				if (!base.Active)
				{
					return false;
				}
				Pawn pawn = parent as Pawn;
				if (pawn != null && !pawn.ageTracker.CurLifeStage.shearable)
				{
					return false;
				}
				return true;
			}
		}

		public override string CompInspectStringExtra()
		{
			if (!Active)
			{
				return null;
			}
			return "WoolGrowth".Translate() + ": " + base.Fullness.ToStringPercent();
		}
	}
}
