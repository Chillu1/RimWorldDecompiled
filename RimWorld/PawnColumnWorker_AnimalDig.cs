using Verse;

namespace RimWorld;

public class PawnColumnWorker_AnimalDig : PawnColumnWorker_Checkbox
{
	private bool anyAnimalWithDig;

	public override bool VisibleCurrently => anyAnimalWithDig;

	public override void Recache()
	{
		anyAnimalWithDig = false;
		foreach (Pawn colonyAnimal in Find.CurrentMap.mapPawns.ColonyAnimals)
		{
			Pawn_TrainingTracker training = colonyAnimal.training;
			if (training != null && training.HasLearned(TrainableDefOf.Dig))
			{
				anyAnimalWithDig = true;
				break;
			}
		}
	}

	protected override bool GetValue(Pawn pawn)
	{
		return pawn.playerSettings.animalDig;
	}

	protected override void SetValue(Pawn pawn, bool value, PawnTable table)
	{
		pawn.playerSettings.animalDig = value;
	}

	protected override bool HasCheckbox(Pawn pawn)
	{
		return pawn.training?.HasLearned(TrainableDefOf.Dig) ?? false;
	}
}
