using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_ZoneAdd_Growing : Designator_ZoneAdd
	{
		protected override string NewZoneLabel => "GrowingZone".Translate();

		public Designator_ZoneAdd_Growing()
		{
			zoneTypeToPlace = typeof(Zone_Growing);
			defaultLabel = "GrowingZone".Translate();
			defaultDesc = "DesignatorGrowingZoneDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Growing");
			tutorTag = "ZoneAdd_Growing";
			hotKey = KeyBindingDefOf.Misc2;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!base.CanDesignateCell(c).Accepted)
			{
				return false;
			}
			if (base.Map.fertilityGrid.FertilityAt(c) < ThingDefOf.Plant_Potato.plant.fertilityMin)
			{
				return false;
			}
			return true;
		}

		protected override Zone MakeNewZone()
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.GrowingFood, KnowledgeAmount.Total);
			return new Zone_Growing(Find.CurrentMap.zoneManager);
		}
	}
}
