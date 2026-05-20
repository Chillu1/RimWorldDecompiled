namespace RimWorld;

public class WeaponTraitWorker_Jealous : WeaponTraitWorker
{
	public override void Notify_OtherWeaponWielded(CompBladelinkWeapon weapon)
	{
		if (weapon.CodedPawn != null && !weapon.CodedPawn.Dead && weapon.CodedPawn.needs.mood != null)
		{
			Thought_WeaponTrait thought_WeaponTrait = (Thought_WeaponTrait)ThoughtMaker.MakeThought(ThoughtDefOf.JealousRage);
			thought_WeaponTrait.weapon = weapon.parent;
			weapon.CodedPawn.needs.mood.thoughts.memories.TryGainMemory(thought_WeaponTrait);
		}
	}
}
