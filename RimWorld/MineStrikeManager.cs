using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class MineStrikeManager : IExposable
	{
		private List<StrikeRecord> strikeRecords = new List<StrikeRecord>();

		private const int RecentStrikeIgnoreRadius = 12;

		private static readonly int RadialVisibleCells = GenRadial.NumCellsInRadius(5.9f);

		public void ExposeData()
		{
			Scribe_Collections.Look(ref strikeRecords, "strikeRecords", LookMode.Deep);
		}

		public void CheckStruckOre(IntVec3 justMinedPos, ThingDef justMinedDef, Thing miner)
		{
			if (miner.Faction != Faction.OfPlayer)
			{
				return;
			}
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = justMinedPos + GenAdj.CardinalDirections[i];
				if (intVec.InBounds(miner.Map))
				{
					Building edifice = intVec.GetEdifice(miner.Map);
					if (edifice != null && edifice.def != justMinedDef && MineableIsValuable(edifice.def) && !AlreadyVisibleNearby(intVec, miner.Map, edifice.def) && !RecentlyStruck(intVec, edifice.def))
					{
						StrikeRecord item = default(StrikeRecord);
						item.cell = intVec;
						item.def = edifice.def;
						item.ticksGame = Find.TickManager.TicksGame;
						strikeRecords.Add(item);
						Messages.Message("StruckMineable".Translate(edifice.def.label), edifice, MessageTypeDefOf.PositiveEvent);
						TaleRecorder.RecordTale(TaleDefOf.StruckMineable, miner, edifice);
					}
				}
			}
		}

		public bool AlreadyVisibleNearby(IntVec3 center, Map map, ThingDef mineableDef)
		{
			CellRect cellRect = CellRect.CenteredOn(center, 1);
			for (int i = 1; i < RadialVisibleCells; i++)
			{
				IntVec3 c = center + GenRadial.RadialPattern[i];
				if (c.InBounds(map) && !c.Fogged(map) && !cellRect.Contains(c))
				{
					Building edifice = c.GetEdifice(map);
					if (edifice != null && edifice.def == mineableDef)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool RecentlyStruck(IntVec3 cell, ThingDef def)
		{
			for (int num = strikeRecords.Count - 1; num >= 0; num--)
			{
				if (strikeRecords[num].Expired)
				{
					strikeRecords.RemoveAt(num);
				}
				else if (strikeRecords[num].def == def && strikeRecords[num].cell.InHorDistOf(cell, 12f))
				{
					return true;
				}
			}
			return false;
		}

		public static bool MineableIsValuable(ThingDef mineableDef)
		{
			if (!mineableDef.mineable || mineableDef.building.mineableThing == null)
			{
				return false;
			}
			return mineableDef.building.mineableThing.GetStatValueAbstract(StatDefOf.MarketValue) * (float)mineableDef.building.mineableYield > 10f;
		}

		public static bool MineableIsVeryValuable(ThingDef mineableDef)
		{
			if (!mineableDef.mineable || mineableDef.building.mineableThing == null)
			{
				return false;
			}
			return mineableDef.building.mineableThing.GetStatValueAbstract(StatDefOf.MarketValue) * (float)mineableDef.building.mineableYield > 100f;
		}

		public string DebugStrikeRecords()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (StrikeRecord strikeRecord in strikeRecords)
			{
				stringBuilder.AppendLine(strikeRecord.ToString());
			}
			return stringBuilder.ToString();
		}
	}
}
