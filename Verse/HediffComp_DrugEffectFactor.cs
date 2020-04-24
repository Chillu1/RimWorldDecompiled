using RimWorld;

namespace Verse
{
	public class HediffComp_DrugEffectFactor : HediffComp
	{
		private static readonly SimpleCurve EffectFactorSeverityCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 0.25f)
		};

		public HediffCompProperties_DrugEffectFactor Props => (HediffCompProperties_DrugEffectFactor)props;

		private float CurrentFactor => EffectFactorSeverityCurve.Evaluate(parent.Severity);

		public override string CompTipStringExtra => "DrugEffectMultiplier".Translate(Props.chemical.label, CurrentFactor.ToStringPercent()).CapitalizeFirst();

		public override void CompModifyChemicalEffect(ChemicalDef chem, ref float effect)
		{
			if (Props.chemical == chem)
			{
				effect *= CurrentFactor;
			}
		}
	}
}
