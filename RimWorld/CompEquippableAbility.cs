using Verse;

namespace RimWorld;

public class CompEquippableAbility : CompEquippable
{
	private Ability ability;

	private CompProperties_EquippableAbility Props => props as CompProperties_EquippableAbility;

	public Ability AbilityForReading
	{
		get
		{
			if (ability == null && Props.abilityDef != null)
			{
				ability = AbilityUtility.MakeAbility(Props.abilityDef, base.Holder);
			}
			return ability;
		}
	}

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		if (base.Holder != null)
		{
			AbilityForReading.pawn = base.Holder;
			AbilityForReading.verb.caster = base.Holder;
		}
	}

	public virtual void UsedOnce()
	{
	}

	public override void Notify_Equipped(Pawn pawn)
	{
		if (AbilityForReading != null)
		{
			AbilityForReading.pawn = pawn;
			AbilityForReading.verb.caster = pawn;
			pawn.abilities.Notify_TemporaryAbilitiesChanged();
		}
	}

	public override void Notify_Unequipped(Pawn pawn)
	{
		if (AbilityForReading != null)
		{
			pawn.abilities.Notify_TemporaryAbilitiesChanged();
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Deep.Look(ref ability, "ability");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Holder != null && AbilityForReading != null)
		{
			AbilityForReading.pawn = base.Holder;
			AbilityForReading.verb.caster = base.Holder;
		}
	}
}
