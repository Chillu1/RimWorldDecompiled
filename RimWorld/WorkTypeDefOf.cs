using Verse;

namespace RimWorld
{
	[DefOf]
	public static class WorkTypeDefOf
	{
		public static WorkTypeDef Mining;

		public static WorkTypeDef Growing;

		public static WorkTypeDef Construction;

		public static WorkTypeDef Warden;

		public static WorkTypeDef Doctor;

		public static WorkTypeDef Firefighter;

		public static WorkTypeDef Hunting;

		public static WorkTypeDef Handling;

		public static WorkTypeDef Crafting;

		public static WorkTypeDef Hauling;

		public static WorkTypeDef Research;

		static WorkTypeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(WorkTypeDefOf));
		}
	}
}
