using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PawnColumnWorker_MentalState : PawnColumnWorker_Icon
	{
		private static readonly Texture2D IconNonAggro = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MentalStateNonAggro");

		private static readonly Texture2D IconAggro = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MentalStateAggro");

		protected override Texture2D GetIconFor(Pawn pawn)
		{
			if (pawn.InMentalState)
			{
				if (!pawn.InAggroMentalState)
				{
					return IconNonAggro;
				}
				return IconAggro;
			}
			return null;
		}

		protected override string GetIconTip(Pawn pawn)
		{
			return pawn.InMentalState ? "IsInMentalState".Translate(pawn.MentalState.def.LabelCap) : ((TaggedString)null);
		}
	}
}
