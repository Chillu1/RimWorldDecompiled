using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_ZoneAddStockpile_Dumping : Designator_ZoneAddStockpile
	{
		public Designator_ZoneAddStockpile_Dumping()
		{
			preset = StorageSettingsPreset.DumpingStockpile;
			defaultLabel = preset.PresetName();
			defaultDesc = "DesignatorZoneCreateStorageDumpingDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_DumpingStockpile");
			soundSucceeded = SoundDefOf.Designate_ZoneAdd_Dumping;
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.StorageTab, OpportunityType.GoodToKnow);
		}
	}
}
