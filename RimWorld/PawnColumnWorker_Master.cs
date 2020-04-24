using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class PawnColumnWorker_Master : PawnColumnWorker
	{
		protected override GameFont DefaultHeaderFont => GameFont.Tiny;

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 100);
		}

		public override int GetOptimalWidth(PawnTable table)
		{
			return Mathf.Clamp(170, GetMinWidth(table), GetMaxWidth(table));
		}

		public override void DoHeader(Rect rect, PawnTable table)
		{
			base.DoHeader(rect, table);
			MouseoverSounds.DoRegion(rect);
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (CanAssignMaster(pawn))
			{
				TrainableUtility.MasterSelectButton(rect.ContractedBy(2f), pawn, paintable: true);
			}
		}

		public override int Compare(Pawn a, Pawn b)
		{
			int valueToCompare = GetValueToCompare1(a);
			int valueToCompare2 = GetValueToCompare1(b);
			if (valueToCompare != valueToCompare2)
			{
				return valueToCompare.CompareTo(valueToCompare2);
			}
			return GetValueToCompare2(a).CompareTo(GetValueToCompare2(b));
		}

		private bool CanAssignMaster(Pawn pawn)
		{
			if (!pawn.RaceProps.Animal || pawn.Faction != Faction.OfPlayer)
			{
				return false;
			}
			if (!pawn.training.HasLearned(TrainableDefOf.Obedience))
			{
				return false;
			}
			return true;
		}

		private int GetValueToCompare1(Pawn pawn)
		{
			if (!CanAssignMaster(pawn))
			{
				return 0;
			}
			if (pawn.playerSettings.Master == null)
			{
				return 1;
			}
			return 2;
		}

		private string GetValueToCompare2(Pawn pawn)
		{
			if (pawn.playerSettings != null && pawn.playerSettings.Master != null)
			{
				return pawn.playerSettings.Master.Label;
			}
			return "";
		}
	}
}
