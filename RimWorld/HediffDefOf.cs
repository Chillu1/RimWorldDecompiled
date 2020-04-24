using Verse;

namespace RimWorld
{
	[DefOf]
	public static class HediffDefOf
	{
		public static HediffDef Misc;

		public static HediffDef Burn;

		public static HediffDef Cut;

		public static HediffDef SurgicalCut;

		public static HediffDef Stab;

		public static HediffDef Gunshot;

		public static HediffDef Shredded;

		public static HediffDef Bruise;

		public static HediffDef Bite;

		public static HediffDef Scratch;

		public static HediffDef MissingBodyPart;

		public static HediffDef BloodLoss;

		public static HediffDef Hypothermia;

		public static HediffDef Heatstroke;

		public static HediffDef Malnutrition;

		public static HediffDef ToxicBuildup;

		public static HediffDef PsychicShock;

		public static HediffDef ResurrectionSickness;

		public static HediffDef ResurrectionPsychosis;

		public static HediffDef Anesthetic;

		public static HediffDef CryptosleepSickness;

		public static HediffDef FoodPoisoning;

		public static HediffDef Pregnant;

		public static HediffDef CatatonicBreakdown;

		[MayRequireRoyalty]
		public static HediffDef PsychicEntropy;

		[MayRequireRoyalty]
		public static HediffDef PsychicHangover;

		[MayRequireRoyalty]
		public static HediffDef PsychicSuppression;

		public static HediffDef Flu;

		public static HediffDef Plague;

		public static HediffDef Malaria;

		public static HediffDef WoundInfection;

		public static HediffDef AlcoholHigh;

		public static HediffDef Hangover;

		public static HediffDef DrugOverdose;

		public static HediffDef BadBack;

		public static HediffDef Cataract;

		public static HediffDef Blindness;

		public static HediffDef Frail;

		public static HediffDef Carcinoma;

		public static HediffDef Asthma;

		public static HediffDef Dementia;

		public static HediffDef PegLeg;

		public static HediffDef Denture;

		public static HediffDef SimpleProstheticLeg;

		public static HediffDef SimpleProstheticArm;

		public static HediffDef BionicEye;

		public static HediffDef BionicArm;

		public static HediffDef BionicLeg;

		public static HediffDef PowerClaw;

		[MayRequireRoyalty]
		public static HediffDef LoveEnhancer;

		[MayRequireRoyalty]
		public static HediffDef NuclearStomach;

		[MayRequireRoyalty]
		public static HediffDef PsychicAmplifier;

		[MayRequireRoyalty]
		public static HediffDef PsychicHarmonizer;

		[MayRequireRoyalty]
		public static HediffDef PsychicSilencer;

		static HediffDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
		}
	}
}
