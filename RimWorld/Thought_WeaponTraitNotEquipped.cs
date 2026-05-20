namespace RimWorld;

public class Thought_WeaponTraitNotEquipped : Thought_WeaponTrait
{
	public override bool ShouldDiscard
	{
		get
		{
			if (!base.ShouldDiscard)
			{
				return pawn.equipment.Primary == weapon;
			}
			return true;
		}
	}
}
