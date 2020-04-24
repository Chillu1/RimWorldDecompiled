using Verse;

namespace RimWorld
{
	[DefOf]
	public static class PawnKindDefOf
	{
		public static PawnKindDef Colonist;

		public static PawnKindDef Slave;

		public static PawnKindDef Villager;

		public static PawnKindDef Drifter;

		public static PawnKindDef SpaceRefugee;

		public static PawnKindDef AncientSoldier;

		public static PawnKindDef WildMan;

		public static PawnKindDef Thrumbo;

		public static PawnKindDef Alphabeaver;

		public static PawnKindDef Muffalo;

		public static PawnKindDef Megascarab;

		public static PawnKindDef Spelopede;

		public static PawnKindDef Megaspider;

		static PawnKindDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf));
		}
	}
}
