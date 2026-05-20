using Verse;

namespace RimWorld;

public class PawnColumnWorker_FollowFieldwork : PawnColumnWorker_Checkbox
{
	private bool anyAnimalWithObedience;

	public override bool VisibleCurrently => anyAnimalWithObedience;

	public override void Recache()
	{
		anyAnimalWithObedience = false;
		foreach (Pawn colonyAnimal in Find.CurrentMap.mapPawns.ColonyAnimals)
		{
			Pawn_TrainingTracker training = colonyAnimal.training;
			if (training != null && training.HasLearned(TrainableDefOf.Obedience))
			{
				anyAnimalWithObedience = true;
				break;
			}
		}
	}

	protected override bool HasCheckbox(Pawn pawn)
	{
		if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
		{
			return pawn.training.HasLearned(TrainableDefOf.Obedience);
		}
		return false;
	}

	protected override bool GetValue(Pawn pawn)
	{
		return pawn.playerSettings.followFieldwork;
	}

	protected override void SetValue(Pawn pawn, bool value, PawnTable table)
	{
		pawn.playerSettings.followFieldwork = value;
	}
}
