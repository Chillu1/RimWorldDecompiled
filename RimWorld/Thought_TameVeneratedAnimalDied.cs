using Verse;

namespace RimWorld
{
	public class Thought_TameVeneratedAnimalDied : Thought_Memory
	{
		public string animalKindLabel;

		public override string LabelCap => base.CurStage.label.Formatted(animalKindLabel.Named("ANIMALKIND")).CapitalizeFirst();

		public override string Description => base.CurStage.description.Formatted(animalKindLabel.Named("ANIMALKIND")).CapitalizeFirst() + base.CausedByBeliefInPrecept;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref animalKindLabel, "animalKindLabel");
		}
	}
}
