using Verse;
using Verse.Sound;

namespace RimWorld
{
	public sealed class PlaySettings : IExposable
	{
		public bool showLearningHelper = true;

		public bool showZones = true;

		public bool showBeauty;

		public bool showRoomStats;

		public bool showColonistBar = true;

		public bool showRoofOverlay;

		public bool showFertilityOverlay;

		public bool showTerrainAffordanceOverlay;

		public bool autoHomeArea = true;

		public bool autoRebuild;

		public bool lockNorthUp = true;

		public bool usePlanetDayNightSystem = true;

		public bool showExpandingIcons = true;

		public bool showWorldFeatures = true;

		public bool useWorkPriorities;

		public MedicalCareCategory defaultCareForColonyHumanlike = MedicalCareCategory.Best;

		public MedicalCareCategory defaultCareForColonyAnimal = MedicalCareCategory.HerbalOrWorse;

		public MedicalCareCategory defaultCareForColonyPrisoner = MedicalCareCategory.HerbalOrWorse;

		public MedicalCareCategory defaultCareForNeutralFaction = MedicalCareCategory.HerbalOrWorse;

		public MedicalCareCategory defaultCareForNeutralAnimal = MedicalCareCategory.HerbalOrWorse;

		public MedicalCareCategory defaultCareForHostileFaction = MedicalCareCategory.HerbalOrWorse;

		public void ExposeData()
		{
			Scribe_Values.Look(ref showLearningHelper, "showLearningHelper", defaultValue: false);
			Scribe_Values.Look(ref showZones, "showZones", defaultValue: false);
			Scribe_Values.Look(ref showBeauty, "showBeauty", defaultValue: false);
			Scribe_Values.Look(ref showRoomStats, "showRoomStats", defaultValue: false);
			Scribe_Values.Look(ref showColonistBar, "showColonistBar", defaultValue: false);
			Scribe_Values.Look(ref showRoofOverlay, "showRoofOverlay", defaultValue: false);
			Scribe_Values.Look(ref showFertilityOverlay, "showFertilityOverlay", defaultValue: false);
			Scribe_Values.Look(ref showTerrainAffordanceOverlay, "showTerrainAffordanceOverlay", defaultValue: false);
			Scribe_Values.Look(ref autoHomeArea, "autoHomeArea", defaultValue: false);
			Scribe_Values.Look(ref autoRebuild, "autoRebuild", defaultValue: false);
			Scribe_Values.Look(ref lockNorthUp, "lockNorthUp", defaultValue: false);
			Scribe_Values.Look(ref usePlanetDayNightSystem, "usePlanetDayNightSystem", defaultValue: false);
			Scribe_Values.Look(ref showExpandingIcons, "showExpandingIcons", defaultValue: false);
			Scribe_Values.Look(ref showWorldFeatures, "showWorldFeatures", defaultValue: false);
			Scribe_Values.Look(ref useWorkPriorities, "useWorkPriorities", defaultValue: false);
			Scribe_Values.Look(ref defaultCareForColonyHumanlike, "defaultCareForHumanlikeColonists", MedicalCareCategory.NoCare);
			Scribe_Values.Look(ref defaultCareForColonyAnimal, "defaultCareForAnimalColonists", MedicalCareCategory.NoCare);
			Scribe_Values.Look(ref defaultCareForColonyPrisoner, "defaultCareForHumanlikeColonistPrisoners", MedicalCareCategory.NoCare);
			Scribe_Values.Look(ref defaultCareForNeutralFaction, "defaultCareForHumanlikeNeutrals", MedicalCareCategory.NoCare);
			Scribe_Values.Look(ref defaultCareForNeutralAnimal, "defaultCareForAnimalNeutrals", MedicalCareCategory.NoCare);
			Scribe_Values.Look(ref defaultCareForHostileFaction, "defaultCareForHumanlikeEnemies", MedicalCareCategory.NoCare);
			BackCompatibility.PostExposeData(this);
		}

		public void DoPlaySettingsGlobalControls(WidgetRow row, bool worldView)
		{
			bool num = showColonistBar;
			if (worldView)
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					row.ToggleableIcon(ref showColonistBar, TexButton.ShowColonistBar, "ShowColonistBarToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				}
				bool num2 = lockNorthUp;
				row.ToggleableIcon(ref lockNorthUp, TexButton.LockNorthUp, "LockNorthUpToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				if (num2 != lockNorthUp && lockNorthUp)
				{
					Find.WorldCameraDriver.RotateSoNorthIsUp();
				}
				row.ToggleableIcon(ref usePlanetDayNightSystem, TexButton.UsePlanetDayNightSystem, "UsePlanetDayNightSystemToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				row.ToggleableIcon(ref showExpandingIcons, TexButton.ShowExpandingIcons, "ShowExpandingIconsToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				row.ToggleableIcon(ref showWorldFeatures, TexButton.ShowWorldFeatures, "ShowWorldFeaturesToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
			}
			else
			{
				row.ToggleableIcon(ref showLearningHelper, TexButton.ShowLearningHelper, "ShowLearningHelperWhenEmptyToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				row.ToggleableIcon(ref showZones, TexButton.ShowZones, "ZoneVisibilityToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				row.ToggleableIcon(ref showBeauty, TexButton.ShowBeauty, "ShowBeautyToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				CheckKeyBindingToggle(KeyBindingDefOf.ToggleBeautyDisplay, ref showBeauty);
				row.ToggleableIcon(ref showRoomStats, TexButton.ShowRoomStats, "ShowRoomStatsToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle, "InspectRoomStats");
				CheckKeyBindingToggle(KeyBindingDefOf.ToggleRoomStatsDisplay, ref showRoomStats);
				row.ToggleableIcon(ref showColonistBar, TexButton.ShowColonistBar, "ShowColonistBarToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				row.ToggleableIcon(ref showRoofOverlay, TexButton.ShowRoofOverlay, "ShowRoofOverlayToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				row.ToggleableIcon(ref showFertilityOverlay, TexButton.ShowFertilityOverlay, "ShowFertilityOverlayToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				row.ToggleableIcon(ref showTerrainAffordanceOverlay, TexButton.ShowTerrainAffordanceOverlay, "ShowTerrainAffordanceOverlayToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				row.ToggleableIcon(ref autoHomeArea, TexButton.AutoHomeArea, "AutoHomeAreaToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				row.ToggleableIcon(ref autoRebuild, TexButton.AutoRebuild, "AutoRebuildButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				bool toggleable = Prefs.ResourceReadoutCategorized;
				bool flag = toggleable;
				row.ToggleableIcon(ref toggleable, TexButton.CategorizedResourceReadout, "CategorizedResourceReadoutToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
				if (toggleable != flag)
				{
					Prefs.ResourceReadoutCategorized = toggleable;
				}
			}
			if (num != showColonistBar)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
		}

		private void CheckKeyBindingToggle(KeyBindingDef keyBinding, ref bool value)
		{
			if (keyBinding.KeyDownEvent)
			{
				value = !value;
				if (value)
				{
					SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
				}
			}
		}
	}
}
