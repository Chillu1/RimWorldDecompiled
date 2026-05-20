using Verse;

namespace RimWorld
{
	public abstract class RitualOutcomeComp_Quality : RitualOutcomeComp
	{
		public SimpleCurve curve;

		protected float MaxValue => curve.Points[curve.PointsCount - 1].x;

		public override bool Applies(LordJob_Ritual ritual)
		{
			return true;
		}

		public abstract float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data);

		public override string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null)
		{
			if ((DataRequired || ritual == null) && data == null)
			{
				return label + " (" + "MaxValue".Translate(MaxValue) + "): " + "OutcomeBonusDesc_Quality".Translate("+" + curve.Points[curve.PointsCount - 1].y.ToStringPercent()) + ".";
			}
			return Count(ritual, data) + " / " + MaxValue + " " + label + ": " + "OutcomeBonusDesc_QualitySingleOffset".Translate("+" + QualityOffset(ritual, data).ToStringPercent()) + ".";
		}

		public override string GetBonusDescShort()
		{
			return "OutcomeBonusDesc_Quality".Translate("+" + curve.Points[curve.PointsCount - 1].y.ToStringPercent()) + ".";
		}

		public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
		{
			if (curve == null)
			{
				return 0f;
			}
			return curve.Evaluate(Count(ritual, data));
		}

		protected override string ExpectedOffsetDesc(bool positive, float quality = 0f)
		{
			return "QualityOutOf".Translate(quality.ToStringWithSign("0.#%"), curve.Points[curve.PointsCount - 1].y.ToStringWithSign("0.#%"));
		}

		public override string GetDescAbstract(bool positive, float quality = -1f)
		{
			quality = ((quality == -1f) ? qualityOffset : quality);
			return "OutcomeBonusDesc_QualitySingleOffset".Translate((TaggedString)quality.ToStringWithSign("0.#%"));
		}
	}
}
