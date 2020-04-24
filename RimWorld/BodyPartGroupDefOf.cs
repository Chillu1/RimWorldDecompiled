using Verse;

namespace RimWorld
{
	[DefOf]
	public static class BodyPartGroupDefOf
	{
		public static BodyPartGroupDef Torso;

		public static BodyPartGroupDef Legs;

		public static BodyPartGroupDef LeftHand;

		public static BodyPartGroupDef RightHand;

		public static BodyPartGroupDef FullHead;

		public static BodyPartGroupDef UpperHead;

		public static BodyPartGroupDef Eyes;

		static BodyPartGroupDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(BodyPartGroupDefOf));
		}
	}
}
