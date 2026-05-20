using Verse;

namespace RimWorld
{
	public abstract class RitualOutcomeComp_QualitySingleOffset : RitualOutcomeComp_Quality
	{
		protected virtual string LabelForDesc => label.CapitalizeFirst();

		public override string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null)
		{
			string text = ((qualityOffset < 0f) ? "" : "+");
			return LabelForDesc + ": " + "OutcomeBonusDesc_QualitySingleOffset".Translate(text + qualityOffset.ToStringPercent()) + ".";
		}

		public override string GetBonusDescShort()
		{
			return "OutcomeBonusDesc_QualitySingleOffset".Translate("+" + qualityOffset.ToStringPercent()) + ".";
		}

		public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
		{
			return 1f;
		}

		public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
		{
			return qualityOffset;
		}

		protected override string ExpectedOffsetDesc(bool positive, float quality = -1f)
		{
			quality = ((quality == -1f) ? qualityOffset : quality);
			return positive ? ((TaggedString)quality.ToStringWithSign("0.#%")) : "QualityOutOf".Translate("+0", quality.ToStringWithSign("0.#%"));
		}
	}
}
