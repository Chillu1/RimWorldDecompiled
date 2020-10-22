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

		[MayRequireRoyalty]
		public static PawnKindDef Empire_Royal_Bestower;

		[MayRequireRoyalty]
		public static PawnKindDef Empire_Royal_NobleWimp;

		[MayRequireRoyalty]
		public static PawnKindDef Empire_Fighter_Janissary;

		[MayRequireRoyalty]
		public static PawnKindDef Empire_Fighter_Trooper;

		[MayRequireRoyalty]
		public static PawnKindDef Empire_Fighter_Cataphract;

		[MayRequireRoyalty]
		public static PawnKindDef Empire_Common_Lodger;

		[MayRequireRoyalty]
		public static PawnKindDef Refugee;

		static PawnKindDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf));
		}
	}
}
