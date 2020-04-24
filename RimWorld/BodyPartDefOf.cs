using Verse;

namespace RimWorld
{
	[DefOf]
	public static class BodyPartDefOf
	{
		public static BodyPartDef Heart;

		public static BodyPartDef Leg;

		public static BodyPartDef Liver;

		public static BodyPartDef Brain;

		public static BodyPartDef Eye;

		public static BodyPartDef Arm;

		public static BodyPartDef Jaw;

		public static BodyPartDef Hand;

		public static BodyPartDef Neck;

		public static BodyPartDef Head;

		public static BodyPartDef Body;

		public static BodyPartDef Torso;

		public static BodyPartDef InsectHead;

		public static BodyPartDef SnakeBody;

		public static BodyPartDef Stomach;

		static BodyPartDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(BodyPartDefOf));
		}
	}
}
