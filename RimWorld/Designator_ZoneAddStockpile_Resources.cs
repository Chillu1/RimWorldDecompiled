using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_ZoneAddStockpile_Resources : Designator_ZoneAddStockpile
	{
		public Designator_ZoneAddStockpile_Resources()
		{
			preset = StorageSettingsPreset.DefaultStockpile;
			defaultLabel = preset.PresetName();
			defaultDesc = "DesignatorZoneCreateStorageResourcesDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Stockpile");
			hotKey = KeyBindingDefOf.Misc1;
			tutorTag = "ZoneAddStockpile_Resources";
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.StorageTab, OpportunityType.GoodToKnow);
		}
	}
}
