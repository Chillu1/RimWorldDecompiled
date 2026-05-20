using Verse;

namespace RimWorld;

public class PawnColumnWorker_AnimalForage : PawnColumnWorker_Checkbox
{
	private bool anyAnimalWithForage;

	public override bool VisibleCurrently => anyAnimalWithForage;

	public override void Recache()
	{
		anyAnimalWithForage = false;
		foreach (Pawn colonyAnimal in Find.CurrentMap.mapPawns.ColonyAnimals)
		{
			Pawn_TrainingTracker training = colonyAnimal.training;
			if (training != null && training.HasLearned(TrainableDefOf.Forage))
			{
				anyAnimalWithForage = true;
				break;
			}
		}
	}

	protected override bool GetValue(Pawn pawn)
	{
		return pawn.playerSettings.animalForage;
	}

	protected override void SetValue(Pawn pawn, bool value, PawnTable table)
	{
		pawn.playerSettings.animalForage = value;
	}

	protected override bool HasCheckbox(Pawn pawn)
	{
		return pawn.training?.HasLearned(TrainableDefOf.Forage) ?? false;
	}
}
