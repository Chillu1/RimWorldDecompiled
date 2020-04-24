using Verse;

namespace RimWorld
{
	[DefOf]
	public static class MessageTypeDefOf
	{
		public static MessageTypeDef ThreatBig;

		public static MessageTypeDef ThreatSmall;

		public static MessageTypeDef PawnDeath;

		public static MessageTypeDef NegativeHealthEvent;

		public static MessageTypeDef NegativeEvent;

		public static MessageTypeDef NeutralEvent;

		public static MessageTypeDef TaskCompletion;

		public static MessageTypeDef PositiveEvent;

		public static MessageTypeDef SituationResolved;

		public static MessageTypeDef RejectInput;

		public static MessageTypeDef CautionInput;

		public static MessageTypeDef SilentInput;

		static MessageTypeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MessageTypeDefOf));
		}
	}
}
