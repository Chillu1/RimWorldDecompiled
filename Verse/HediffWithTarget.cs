namespace Verse
{
	public class HediffWithTarget : HediffWithComps
	{
		public Thing target;

		public override string LabelBase => base.LabelBase + " " + def.targetPrefix + " " + target?.LabelShortCap;

		public override bool ShouldRemove
		{
			get
			{
				Pawn pawn;
				if (target != null && ((pawn = target as Pawn) == null || !pawn.Dead))
				{
					return base.ShouldRemove;
				}
				return true;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref target, "target");
		}
	}
}
