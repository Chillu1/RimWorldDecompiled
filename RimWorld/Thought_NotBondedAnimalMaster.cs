using UnityEngine;

namespace RimWorld
{
	public class Thought_NotBondedAnimalMaster : Thought_Situational
	{
		private const int MaxAnimals = 3;

		protected override float BaseMoodOffset => base.CurStage.baseMoodEffect * (float)Mathf.Min(((ThoughtWorker_NotBondedAnimalMaster)def.Worker).GetAnimalsCount(pawn), 3);
	}
}
