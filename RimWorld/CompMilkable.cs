using Verse;

namespace RimWorld
{
	public class CompMilkable : CompHasGatherableBodyResource
	{
		protected override int GatherResourcesIntervalDays => Props.milkIntervalDays;

		protected override int ResourceAmount => Props.milkAmount;

		protected override ThingDef ResourceDef => Props.milkDef;

		protected override string SaveKey => "milkFullness";

		public CompProperties_Milkable Props => (CompProperties_Milkable)props;

		protected override bool Active
		{
			get
			{
				if (!base.Active)
				{
					return false;
				}
				Pawn pawn = parent as Pawn;
				if (Props.milkFemaleOnly && pawn != null && pawn.gender != Gender.Female)
				{
					return false;
				}
				if (pawn != null && !pawn.ageTracker.CurLifeStage.milkable)
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
			return "MilkFullness".Translate() + ": " + base.Fullness.ToStringPercent();
		}
	}
}
