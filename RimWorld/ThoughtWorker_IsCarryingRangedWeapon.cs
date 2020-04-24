using Verse;

namespace RimWorld
{
	public class ThoughtWorker_IsCarryingRangedWeapon : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.equipment.Primary != null && p.equipment.Primary.def.IsRangedWeapon;
		}
	}
}
