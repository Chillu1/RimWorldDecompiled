using Verse;

namespace RimWorld
{
	public static class DiplomacyTuning
	{
		public const int MaxGoodwill = 100;

		public const int MinGoodwill = -100;

		public const int BecomeHostileThreshold = -75;

		public const int BecomeNeutralThreshold = 0;

		public const int BecomeAllyThreshold = 75;

		public const int InitialHostileThreshold = -10;

		public const int InitialAllyThreshold = 75;

		public static readonly IntRange ForcedStartingEnemyGoodwillRange = new IntRange(-100, -40);

		public const int MinGoodwillToRequestAICoreQuest = 40;

		public const int RequestAICoreQuestSilverCost = 1500;

		public static readonly FloatRange RansomFeeMarketValueFactorRange = new FloatRange(1.2f, 2.2f);

		public const int Goodwill_NaturalChangeStep = 10;

		public const float Goodwill_PerDirectDamageToPawn = -1.3f;

		public const float Goodwill_PerDirectDamageToBuilding = -1f;

		public const int Goodwill_MemberCrushed_Humanlike = -25;

		public const int Goodwill_MemberCrushed_Animal = -15;

		public const int Goodwill_MemberNeutrallyDied_Humanlike = -5;

		public const int Goodwill_MemberNeutrallyDied_Animal = -3;

		public const int Goodwill_BodyPartRemovalViolation = -70;

		public const int Goodwill_PsychicAmplifierRemovalViolation = -200;

		public const int Goodwill_MemberEuthanized = -100;

		public const int Goodwill_AttackedSettlement = -50;

		public const int Goodwill_MilitaryAidRequested = -25;

		public const int Goodwill_TraderRequested = -15;

		public static readonly SimpleCurve Goodwill_PerQuadrumFromSettlementProximity = new SimpleCurve
		{
			new CurvePoint(2f, -30f),
			new CurvePoint(3f, -20f),
			new CurvePoint(4f, -10f),
			new CurvePoint(5f, 0f)
		};

		public const float Goodwill_BaseGiftSilverForOneGoodwill = 40f;

		public static readonly SimpleCurve GiftGoodwillFactorRelationsCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(75f, 0.25f)
		};

		public const float Goodwill_GiftPrisonerOfTheirFactionValueFactor = 2f;

		public const float Goodwill_TradedMarketValueforOneGoodwill = 600f;

		public const int Goodwill_DestroyedMutualEnemyBase = 20;

		public const int Goodwill_MemberExitedMapHealthy = 12;

		public const int Goodwill_MemberExitedMapHealthy_LeaderBonus = 40;

		public const float Goodwill_PerTend = 1f;

		public const int Goodwill_MaxTimesTendedTo = 10;

		public const int Goodwill_QuestTradeRequestCompleted = 12;

		public static readonly IntRange Goodwill_PeaceTalksDisasterRange = new IntRange(-50, -40);

		public static readonly IntRange Goodwill_PeaceTalksBackfireRange = new IntRange(-20, -10);

		public static readonly IntRange Goodwill_PeaceTalksSuccessRange = new IntRange(60, 70);

		public static readonly IntRange Goodwill_PeaceTalksTriumphRange = new IntRange(100, 110);

		public static readonly IntRange RoyalFavor_PeaceTalksSuccessRange = new IntRange(1, 4);

		public const float VisitorGiftChanceBase = 0.25f;

		public static readonly SimpleCurve VisitorGiftChanceFactorFromPlayerWealthCurve = new SimpleCurve
		{
			new CurvePoint(30000f, 1f),
			new CurvePoint(80000f, 0.1f),
			new CurvePoint(300000f, 0f)
		};

		public static readonly SimpleCurve VisitorGiftChanceFactorFromGoodwillCurve = new SimpleCurve
		{
			new CurvePoint(-30f, 0f),
			new CurvePoint(0f, 1f)
		};

		public static readonly FloatRange VisitorGiftTotalMarketValueRangeBase = new FloatRange(100f, 500f);

		public static readonly SimpleCurve VisitorGiftTotalMarketValueFactorFromPlayerWealthCurve = new SimpleCurve
		{
			new CurvePoint(10000f, 0.25f),
			new CurvePoint(100000f, 1f)
		};

		public static readonly FloatRange RequestedMilitaryAidPointsRange = new FloatRange(800f, 1000f);
	}
}
