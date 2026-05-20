using RimWorld;

namespace Verse;

public class Verb_AbilityShoot : Verb_Shoot, IAbilityVerb
{
	private Ability ability;

	public Ability Ability
	{
		get
		{
			return ability;
		}
		set
		{
			ability = value;
		}
	}

	protected override bool TryCastShot()
	{
		bool num = base.TryCastShot();
		if (num)
		{
			ability.StartCooldown(ability.def.cooldownTicksRange.RandomInRange);
		}
		return num;
	}

	public override void ExposeData()
	{
		Scribe_References.Look(ref ability, "ability");
		base.ExposeData();
	}
}
