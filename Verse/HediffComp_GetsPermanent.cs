namespace Verse
{
	public class HediffComp_GetsPermanent : HediffComp
	{
		public float permanentDamageThreshold = 9999f;

		public bool isPermanentInt;

		private PainCategory painCategory;

		private const float NonActivePermanentDamageThresholdValue = 9999f;

		public HediffCompProperties_GetsPermanent Props => (HediffCompProperties_GetsPermanent)props;

		public bool IsPermanent
		{
			get
			{
				return isPermanentInt;
			}
			set
			{
				if (value == isPermanentInt)
				{
					return;
				}
				isPermanentInt = value;
				if (isPermanentInt)
				{
					painCategory = HealthTuning.InjuryPainCategories.RandomElementByWeight((HealthTuning.PainCategoryWeighted cat) => cat.weight).category;
					permanentDamageThreshold = 9999f;
				}
			}
		}

		public PainCategory PainCategory => painCategory;

		public float PainFactor => (float)painCategory;

		public override void CompExposeData()
		{
			Scribe_Values.Look(ref isPermanentInt, "isPermanent", defaultValue: false);
			Scribe_Values.Look(ref permanentDamageThreshold, "permanentDamageThreshold", 9999f);
			Scribe_Values.Look(ref painCategory, "painCategory", PainCategory.Painless);
			BackCompatibility.PostExposeData(this);
		}

		public void PreFinalizeInjury()
		{
			if (base.Pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(parent.Part))
			{
				return;
			}
			float num = 0.02f * parent.Part.def.permanentInjuryChanceFactor * Props.becomePermanentChanceFactor;
			if (!parent.Part.def.delicate)
			{
				num *= HealthTuning.BecomePermanentChanceFactorBySeverityCurve.Evaluate(parent.Severity);
			}
			if (Rand.Chance(num))
			{
				if (parent.Part.def.delicate)
				{
					IsPermanent = true;
				}
				else
				{
					permanentDamageThreshold = Rand.Range(1f, parent.Severity / 2f);
				}
			}
		}

		public override void CompPostInjuryHeal(float amount)
		{
			if (!(permanentDamageThreshold >= 9999f) && !IsPermanent && parent.Severity <= permanentDamageThreshold && parent.Severity >= permanentDamageThreshold - amount)
			{
				parent.Severity = permanentDamageThreshold;
				IsPermanent = true;
				base.Pawn.health.Notify_HediffChanged(parent);
			}
		}

		public override string CompDebugString()
		{
			return "isPermanent: " + isPermanentInt.ToString() + "\npermanentDamageThreshold: " + permanentDamageThreshold + "\npainCategory: " + painCategory;
		}
	}
}
