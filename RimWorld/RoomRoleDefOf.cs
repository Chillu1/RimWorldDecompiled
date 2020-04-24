using Verse;

namespace RimWorld
{
	[DefOf]
	public static class RoomRoleDefOf
	{
		public static RoomRoleDef None;

		public static RoomRoleDef Bedroom;

		public static RoomRoleDef Barracks;

		public static RoomRoleDef PrisonCell;

		public static RoomRoleDef PrisonBarracks;

		public static RoomRoleDef DiningRoom;

		public static RoomRoleDef RecRoom;

		public static RoomRoleDef Hospital;

		public static RoomRoleDef Laboratory;

		[MayRequireRoyalty]
		public static RoomRoleDef ThroneRoom;

		static RoomRoleDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(RoomRoleDefOf));
		}
	}
}
