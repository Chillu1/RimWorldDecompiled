using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_WieldingNobleOrDespisedWeapon : ThoughtWorker_Precept
{
	public override string PostProcessLabel(Pawn p, string label)
	{
		ThingWithComps thingWithComps = p.equipment?.Primary;
		if (thingWithComps == null)
		{
			return string.Empty;
		}
		return label.Formatted(thingWithComps.Named("WEAPON"));
	}

	public override string PostProcessDescription(Pawn p, string description)
	{
		ThingWithComps thingWithComps = p.equipment?.Primary;
		if (thingWithComps == null)
		{
			return string.Empty;
		}
		return description.Formatted(thingWithComps.Named("WEAPON"));
	}

	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (p.Ideo == null || p.equipment?.Primary == null)
		{
			return false;
		}
		return p.Ideo.GetDispositionForWeapon(p.equipment.Primary.def) switch
		{
			IdeoWeaponDisposition.Noble => ThoughtState.ActiveAtStage(0), 
			IdeoWeaponDisposition.Despised => ThoughtState.ActiveAtStage(1), 
			_ => ThoughtState.Inactive, 
		};
	}
}
