using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NotBondedAnimalMaster : ThoughtWorker_BondedAnimalMaster
	{
		protected override bool AnimalMasterCheck(Pawn p, Pawn animal)
		{
			if (animal.playerSettings.RespectedMaster != p)
			{
				return TrainableUtility.MinimumHandlingSkill(animal) <= p.skills.GetSkill(SkillDefOf.Animals).Level;
			}
			return false;
		}
	}
}
