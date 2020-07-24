using Verse;

namespace RimWorld
{
	public class FocusStrengthOffset_Quality : FocusStrengthOffset_Curve
	{
		protected override string ExplanationKey => "StatsReport_FromQuality";

		protected override float SourceValue(Thing parent)
		{
			parent.TryGetQuality(out QualityCategory qc);
			return (int)qc;
		}

		public override float MaxOffset(bool forAbstract = false)
		{
			if (!forAbstract)
			{
				return 0f;
			}
			return base.MaxOffset(forAbstract: true);
		}
	}
}
