namespace RimWorld
{
	[DefOf]
	public static class IssueDefOf
	{
		[MayRequireIdeology]
		public static IssueDef Charity;

		[MayRequireIdeology]
		public static IssueDef Ranching;

		[MayRequireIdeology]
		public static IssueDef Blindness;

		[MayRequireIdeology]
		public static IssueDef IdeoRitualSeat;

		static IssueDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(IssueDefOf));
		}
	}
}
