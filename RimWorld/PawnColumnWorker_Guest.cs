using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Guest : PawnColumnWorker_Icon
	{
		protected override Texture2D GetIconFor(Pawn pawn)
		{
			return pawn?.guest.GetIcon();
		}

		protected override string GetIconTip(Pawn pawn)
		{
			string str = pawn?.guest.GetLabel();
			if (!str.NullOrEmpty())
			{
				return str.CapitalizeFirst();
			}
			return null;
		}
	}
}
