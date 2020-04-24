using Verse;

namespace RimWorld
{
	[DefOf]
	public static class DamageDefOf
	{
		public static DamageDef Cut;

		public static DamageDef Crush;

		public static DamageDef Arrow;

		public static DamageDef Blunt;

		public static DamageDef Stab;

		public static DamageDef Bullet;

		public static DamageDef Bomb;

		public static DamageDef Scratch;

		public static DamageDef TornadoScratch;

		public static DamageDef Bite;

		public static DamageDef Flame;

		public static DamageDef Burn;

		public static DamageDef SurgicalCut;

		public static DamageDef ExecutionCut;

		public static DamageDef Frostbite;

		public static DamageDef Stun;

		public static DamageDef EMP;

		public static DamageDef Extinguish;

		public static DamageDef Smoke;

		public static DamageDef Deterioration;

		public static DamageDef Mining;

		public static DamageDef Rotting;

		static DamageDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DamageDefOf));
		}
	}
}
