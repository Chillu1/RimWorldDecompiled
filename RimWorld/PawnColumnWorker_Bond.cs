using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PawnColumnWorker_Bond : PawnColumnWorker_Icon
	{
		private static readonly Texture2D BondIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Bond");

		private static readonly Texture2D BondBrokenIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/BondBroken");

		protected override Texture2D GetIconFor(Pawn pawn)
		{
			IEnumerable<Pawn> allColonistBondsFor = TrainableUtility.GetAllColonistBondsFor(pawn);
			if (!allColonistBondsFor.Any())
			{
				return null;
			}
			if (allColonistBondsFor.Any((Pawn bond) => bond == pawn.playerSettings.Master))
			{
				return BondIcon;
			}
			return BondBrokenIcon;
		}

		protected override string GetIconTip(Pawn pawn)
		{
			return TrainableUtility.GetIconTooltipText(pawn);
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return GetCompareValueFor(a).CompareTo(GetCompareValueFor(b));
		}

		public int GetCompareValueFor(Pawn a)
		{
			Texture2D iconFor = GetIconFor(a);
			if (iconFor == null)
			{
				return 0;
			}
			if (iconFor == BondBrokenIcon)
			{
				return 1;
			}
			if (iconFor == BondIcon)
			{
				return 2;
			}
			Log.ErrorOnce("Unknown bond type when trying to sort", 20536378);
			return 0;
		}

		protected override void PaintedIcon(Pawn pawn)
		{
			if (!(GetIconFor(pawn) != BondBrokenIcon) && pawn.training.HasLearned(TrainableDefOf.Obedience))
			{
				pawn.playerSettings.Master = (from master in TrainableUtility.GetAllColonistBondsFor(pawn)
					where TrainableUtility.CanBeMaster(master, pawn)
					select master).FirstOrDefault();
			}
		}
	}
}
