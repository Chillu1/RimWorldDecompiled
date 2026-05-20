using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_BetrayMTB : QuestPart_MTB
	{
		public List<Pawn> pawns = new List<Pawn>();

		private static readonly SimpleCurve DownedColonistsExcept1PctToMTBDaysCurve = new SimpleCurve
		{
			new CurvePoint(0f, 120f),
			new CurvePoint(1f, 15f)
		};

		protected override float MTBDays
		{
			get
			{
				Map map = null;
				bool flag = false;
				for (int i = 0; i < pawns.Count; i++)
				{
					if (CanBetray(pawns[i]))
					{
						flag = true;
						map = pawns[i].MapHeld;
						break;
					}
				}
				if (!flag)
				{
					return -1f;
				}
				int num = 0;
				int num2 = 0;
				List<Pawn> list = map.mapPawns.PawnsInFaction(Faction.OfPlayer);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].IsColonist)
					{
						num++;
						if (list[j].Downed)
						{
							num2++;
						}
					}
				}
				if (num <= 1)
				{
					return DownedColonistsExcept1PctToMTBDaysCurve.Evaluate(1f);
				}
				return DownedColonistsExcept1PctToMTBDaysCurve.Evaluate((float)num2 / (float)(num - 1));
			}
		}

		private bool CanBetray(Pawn p)
		{
			if (!p.DestroyedOrNull() && !p.Downed && p.IsFreeColonist && !p.InMentalState && !p.IsBurning() && !p.Suspended)
			{
				return p.SpawnedOrAnyParentSpawned;
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}
	}
}
