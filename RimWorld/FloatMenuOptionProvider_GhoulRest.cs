using Verse;

namespace RimWorld;

public class FloatMenuOptionProvider_GhoulRest : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		if (!base.AppliesInt(context))
		{
			return false;
		}
		if (context.FirstSelectedPawn.IsMutant)
		{
			return context.FirstSelectedPawn.mutant.Def == MutantDefOf.Ghoul;
		}
		return false;
	}

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		if (!(clickedThing is Building_Bed building_Bed))
		{
			return null;
		}
		return building_Bed.GetBedRestFloatMenuOption(context.FirstSelectedPawn);
	}
}
