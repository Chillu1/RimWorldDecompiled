using RimWorld;

namespace Verse
{
	public class HediffComp_ChangeNeed : HediffComp
	{
		private Need needCached;

		public HediffCompProperties_ChangeNeed Props => (HediffCompProperties_ChangeNeed)props;

		private Need Need
		{
			get
			{
				if (needCached == null)
				{
					needCached = base.Pawn.needs.TryGetNeed(Props.needDef);
				}
				return needCached;
			}
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			if (Need != null)
			{
				Need.CurLevelPercentage += Props.percentPerDay / 60000f;
			}
		}
	}
}
