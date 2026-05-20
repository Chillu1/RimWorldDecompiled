using UnityEngine;

namespace RimWorld
{
	public class CompRitualEffect_ConstantCenter : CompRitualEffect_Constant
	{
		protected override Vector3 SpawnPos(LordJob_Ritual ritual)
		{
			return ritual.selectedTarget.Cell.ToVector3Shifted();
		}
	}
}
