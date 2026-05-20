using Verse;

namespace RimWorld;

public class Thought_MemoryObservation_Chimera : Thought_MemoryObservation
{
	private string animalLabel;

	public override string LabelCap => base.CurStage.label.Formatted(animalLabel.Named("ANIMAL")).CapitalizeFirst();

	public override string Description => base.CurStage.description.Formatted(animalLabel.Named("ANIMAL"));

	public override bool ShouldDiscard
	{
		get
		{
			if (!base.ShouldDiscard)
			{
				return animalLabel.NullOrEmpty();
			}
			return true;
		}
	}

	public override Thing Target
	{
		set
		{
			targetThingID = value.thingIDNumber;
			animalLabel = value.TryGetComp<CompChimera>()?.Props?.simpleAnimalLabel;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref animalLabel, "animalLabel");
	}
}
