using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_BondedAnimalMaster : ThoughtWorker
{
	private static List<string> tmpAnimals = new List<string>();

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		tmpAnimals.Clear();
		GetAnimals(p, tmpAnimals);
		if (tmpAnimals.Any())
		{
			if (tmpAnimals.Count == 1)
			{
				return ThoughtState.ActiveAtStage(0, tmpAnimals[0]);
			}
			return ThoughtState.ActiveAtStage(1, tmpAnimals.ToCommaList(useAnd: true));
		}
		return false;
	}

	protected virtual bool AnimalMasterCheck(Pawn p, Pawn animal)
	{
		return animal.playerSettings.RespectedMaster == p;
	}

	public void GetAnimals(Pawn p, List<string> outAnimals)
	{
		outAnimals.Clear();
		List<DirectPawnRelation> directRelations = p.relations.DirectRelations;
		for (int i = 0; i < directRelations.Count; i++)
		{
			DirectPawnRelation directPawnRelation = directRelations[i];
			Pawn otherPawn = directPawnRelation.otherPawn;
			if (directPawnRelation.def == PawnRelationDefOf.Bond && !otherPawn.Dead && otherPawn.Spawned && otherPawn.Faction == Faction.OfPlayer && otherPawn.training.HasLearned(TrainableDefOf.Obedience) && AnimalMasterCheck(p, otherPawn))
			{
				outAnimals.Add(otherPawn.LabelShort);
			}
		}
	}

	public int GetAnimalsCount(Pawn p)
	{
		tmpAnimals.Clear();
		GetAnimals(p, tmpAnimals);
		return tmpAnimals.Count;
	}
}
