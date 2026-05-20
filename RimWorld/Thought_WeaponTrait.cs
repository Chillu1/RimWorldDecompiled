using Verse;

namespace RimWorld;

public class Thought_WeaponTrait : Thought_Memory
{
	public ThingWithComps weapon;

	public override bool ShouldDiscard
	{
		get
		{
			if (!base.ShouldDiscard)
			{
				return !HasWeapon;
			}
			return true;
		}
	}

	public override string LabelCap => base.CurStage.label.Formatted(Name.Named("WEAPON")).CapitalizeFirst();

	public override string Description => base.CurStage.description.Formatted(Name.Named("WEAPON")).CapitalizeFirst();

	protected bool HasWeapon
	{
		get
		{
			if (weapon != null)
			{
				return !weapon.Destroyed;
			}
			return false;
		}
	}

	private string Name
	{
		get
		{
			CompGeneratedNames compGeneratedNames = weapon.TryGetComp<CompGeneratedNames>();
			if (compGeneratedNames != null)
			{
				return compGeneratedNames.Name;
			}
			return weapon.LabelNoCount;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref weapon, "weapon");
	}
}
