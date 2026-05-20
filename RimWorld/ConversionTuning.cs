using Verse;

namespace RimWorld
{
	public class ConversionTuning
	{
		public const float ConvertAttempt_BaseSelectionWeight = 0.04f;

		public const float ConvertAttempt_Colonist = 1f;

		public const float ConvertAttempt_Slave = 0.5f;

		public const float ConvertAttempt_NPCFreeVsColonist = 0.5f;

		public const float ConvertAttempt_NPCFreeVsNPCFree = 0.25f;

		public const float ConvertAttempt_NPCFreeVsPrisoner = 0.25f;

		public const float ConvertAttempt_NPCFreeVsSlave = 0.5f;

		public const float ConvertAttempt_PrisonerVsColonist = 0.25f;

		public const float ConvertAttempt_PrisonerVsNPCFree = 0.25f;

		public const float ConvertAttempt_PrisonerVsPrisoner = 0.5f;

		public const float ConvertAttempt_PrisonerVsSlave = 0.5f;

		public const float ConvertAttempt_FailOutcomeWeight_Nothing = 0.78f;

		public const float ConvertAttempt_FailOutcomeWeight_Resentment = 0.2f;

		public const float ConvertAttempt_FailOutcomeWeight_SocialFight = 0.02f;

		public const float ConvertAttempt_BaseCertaintyReduction = 0.06f;

		public static readonly SimpleCurve CertaintyPerDayByMoodCurve = new SimpleCurve
		{
			new CurvePoint(0.2f, 0.01f),
			new CurvePoint(0.5f, 0.02f),
			new CurvePoint(0.8f, 0.03f)
		};

		public const float PostConversionCertainty = 0.5f;

		public static readonly FloatRange InitialCertaintyRange = new FloatRange(0.6f, 1f);

		public const float ConversionPowerFactor_AgreeWithMeme = 0.2f;

		public const float ConversionPowerFactor_DisagreeWithMeme = -0.2f;

		public const float ConversionPowerFactor_Min = -0.4f;

		public const float ChildCertaintyChangeFactor = 2f;
	}
}
