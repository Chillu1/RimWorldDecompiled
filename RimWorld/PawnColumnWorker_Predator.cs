using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PawnColumnWorker_Predator : PawnColumnWorker_Icon
	{
		private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Predator");

		protected override Texture2D GetIconFor(Pawn pawn)
		{
			if (pawn.RaceProps.predator)
			{
				return Icon;
			}
			return null;
		}

		protected override string GetIconTip(Pawn pawn)
		{
			return "IsPredator".Translate();
		}
	}
}
