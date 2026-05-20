using Verse;

namespace RimWorld
{
	public class RoleEffect_NoMeleeWeapons : RoleEffect
	{
		public override bool IsBad => true;

		public RoleEffect_NoMeleeWeapons()
		{
			labelKey = "RoleEffectWontUseMeleeWeapons";
		}

		public override bool CanEquip(Pawn pawn, Thing thing)
		{
			return !thing.def.IsMeleeWeapon;
		}
	}
}
