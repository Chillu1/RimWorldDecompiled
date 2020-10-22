using Verse;

namespace RimWorld
{
	[DefOf]
	public static class MentalStateDefOf
	{
		public static MentalStateDef Berserk;

		public static MentalStateDef Binging_DrugExtreme;

		[MayRequireRoyalty]
		public static MentalStateDef BerserkMechanoid;

		public static MentalStateDef Wander_Psychotic;

		public static MentalStateDef Binging_DrugMajor;

		public static MentalStateDef Wander_Sad;

		public static MentalStateDef Wander_OwnRoom;

		public static MentalStateDef PanicFlee;

		public static MentalStateDef Manhunter;

		public static MentalStateDef ManhunterPermanent;

		public static MentalStateDef SocialFighting;

		static MentalStateDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MentalStateDefOf));
		}
	}
}
