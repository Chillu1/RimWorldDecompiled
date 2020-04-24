using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Gender : PawnColumnWorker_Icon
	{
		protected override Texture2D GetIconFor(Pawn pawn)
		{
			return pawn.gender.GetIcon();
		}

		protected override string GetIconTip(Pawn pawn)
		{
			return pawn.GetGenderLabel().CapitalizeFirst();
		}
	}
}
