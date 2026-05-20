using Verse;

namespace RimWorld;

public class ThoughtWorker_NotBondedAnimalMaster : ThoughtWorker_BondedAnimalMaster
{
	protected override bool AnimalMasterCheck(Pawn p, Pawn animal)
	{
		return animal.playerSettings.RespectedMaster != p;
	}
}
