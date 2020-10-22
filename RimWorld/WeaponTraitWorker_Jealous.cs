namespace RimWorld
{
	public class WeaponTraitWorker_Jealous : WeaponTraitWorker
	{
		public override void Notify_OtherWeaponWielded(CompBladelinkWeapon weapon)
		{
			if (weapon.bondedPawn != null && !weapon.bondedPawn.Dead && weapon.bondedPawn.needs.mood != null)
			{
				Thought_WeaponTrait thought_WeaponTrait = (Thought_WeaponTrait)ThoughtMaker.MakeThought(ThoughtDefOf.JealousRage);
				thought_WeaponTrait.weapon = weapon.parent;
				weapon.bondedPawn.needs.mood.thoughts.memories.TryGainMemory(thought_WeaponTrait);
			}
		}
	}
}
