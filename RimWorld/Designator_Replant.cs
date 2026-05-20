using Verse;

namespace RimWorld;

public class Designator_Replant : Designator_Install
{
	public override string Label => "CommandReplant".Translate();

	public override string Desc => "CommandReplantDesc".Translate();

	public Designator_Replant()
	{
		icon = TexCommand.Replant;
		soundSucceeded = SoundDefOf.Designate_ExtractTree;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		Plant plant = (Plant)base.ThingToInstall;
		Thing blockingThing;
		AcceptanceReport acceptanceReport = plant.def.CanEverPlantAt(c, base.Map, out blockingThing, canWipePlantsExceptTree: true);
		if (!acceptanceReport)
		{
			return new AcceptanceReport("CannotBePlantedHere".Translate() + ": " + acceptanceReport.Reason.CapitalizeFirst());
		}
		if (plant.def.plant.interferesWithRoof && c.Roofed(base.Map))
		{
			return "CannotBePlantedHere".Translate() + ": " + "BlockedByRoof".Translate().CapitalizeFirst();
		}
		if (!plant.def.CanNowPlantAt(c, base.Map, canWipePlantsExceptTree: true))
		{
			return new AcceptanceReport("CannotBePlantedHere".Translate());
		}
		foreach (Thing thing in c.GetThingList(base.Map))
		{
			if (thing is Blueprint_Install blueprint_Install && blueprint_Install.ThingToInstall.def.plant != null && blueprint_Install.ThingToInstall.def.plant.IsTree)
			{
				return "IdenticalThingExists".Translate();
			}
		}
		return base.CanDesignateCell(c);
	}
}
