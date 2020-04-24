namespace Verse
{
	public class HediffComp_SeverityFromEntropy : HediffComp
	{
		private float EntropyAmount
		{
			get
			{
				if (base.Pawn.psychicEntropy != null)
				{
					return base.Pawn.psychicEntropy.EntropyRelativeValue;
				}
				return 0f;
			}
		}

		public override bool CompShouldRemove => EntropyAmount < float.Epsilon;

		public override void CompPostTick(ref float severityAdjustment)
		{
			parent.Severity = EntropyAmount;
		}
	}
}
