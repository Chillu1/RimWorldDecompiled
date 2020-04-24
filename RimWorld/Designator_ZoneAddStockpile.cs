using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class Designator_ZoneAddStockpile : Designator_ZoneAdd
	{
		protected StorageSettingsPreset preset;

		protected override string NewZoneLabel => preset.PresetName();

		protected override Zone MakeNewZone()
		{
			return new Zone_Stockpile(preset, Find.CurrentMap.zoneManager);
		}

		public Designator_ZoneAddStockpile()
		{
			zoneTypeToPlace = typeof(Zone_Stockpile);
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			AcceptanceReport result = base.CanDesignateCell(c);
			if (!result.Accepted)
			{
				return result;
			}
			if (c.GetTerrain(base.Map).passability == Traversability.Impassable)
			{
				return false;
			}
			List<Thing> list = base.Map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].def.CanOverlapZones)
				{
					return false;
				}
			}
			return true;
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Stockpiles, KnowledgeAmount.Total);
		}
	}
}
