using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Dialog_Options : Window
{
	public OptionCategoryDef selectedCategory = OptionCategoryDefOf.General;

	public Mod selectedMod;

	private bool hasModSettings;

	private Vector2 categoryScrollPosition;

	private Vector2 optionsScrollPosition;

	private float optionsViewRectHeight;

	private QuickSearchWidget quickSearchWidget = new QuickSearchWidget();

	private string modFilter = "";

	private IEnumerable<Mod> cachedModsWithSettings;

	private const float CategoryListWidth = 160f;

	private const float CategoryRowHeight = 48f;

	private const float CategoryRowSpacing = 2f;

	private const float CategoryIconSize = 20f;

	private const float BottomRowHeight = 50f;

	private const float ListingVerticalSpacing = 5f;

	private const float SearchBarHeight = 30f;

	private const float ModIconSize = 32f;

	private static readonly Vector2 OkayButtonSize = new Vector2(160f, 40f);

	public static readonly float[] UIScales = new float[9] { 1f, 1.25f, 1.5f, 1.75f, 2f, 2.5f, 3f, 3.5f, 4f };

	public static readonly Texture2D RandomBackgroundTex = ContentFinder<Texture2D>.Get("UI/Icons/RandomBackground");

	public static readonly Texture2D LanguageIconTex = ContentFinder<Texture2D>.Get("UI/Icons/Language");

	private Dictionary<string, string> modOptionTruncationCache = new Dictionary<string, string>();

	public override Vector2 InitialSize => new Vector2(650f, 600f);

	public Dialog_Options(OptionCategoryDef initialCategory)
		: this()
	{
		selectedCategory = initialCategory;
		selectedMod = null;
	}

	public Dialog_Options(Mod initialMod)
		: this()
	{
		selectedCategory = null;
		selectedMod = initialMod;
	}

	public Dialog_Options()
	{
		doCloseX = true;
		forcePause = true;
		absorbInputAroundWindow = true;
	}

	public override void PostOpen()
	{
		base.PostOpen();
		quickSearchWidget.Reset();
		cachedModsWithSettings = from mod in LoadedModManager.ModHandles
			where !mod.SettingsCategory().NullOrEmpty()
			orderby mod.SettingsCategory()
			select mod;
		foreach (OptionCategoryDef allDef in DefDatabase<OptionCategoryDef>.AllDefs)
		{
			if (!allDef.modContentPack.IsOfficialMod)
			{
				Log.Error("Unofficial OptionCategoryDef: " + allDef.label + " found, ignoring");
			}
		}
		hasModSettings = LoadedModManager.ModHandles.Any((Mod mod) => !mod.SettingsCategory().NullOrEmpty());
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Small;
		IEnumerable<OptionCategoryDef> allDefs = DefDatabase<OptionCategoryDef>.AllDefs;
		Text.Anchor = TextAnchor.MiddleLeft;
		int num = 0;
		foreach (OptionCategoryDef item in allDefs)
		{
			if ((Prefs.DevMode || !item.isDev) && item.modContentPack.IsOfficialMod && (item != OptionCategoryDefOf.Mods || hasModSettings))
			{
				Rect rect = new Rect(0f, (float)num * 50f, 160f, 48f);
				DoCategoryRow(rect.ContractedBy(4f), item);
				num++;
			}
		}
		Text.Anchor = TextAnchor.UpperLeft;
		float num2 = 60f;
		DoOptions(inRect: new Rect(177f, 0f, inRect.width - 160f - 17f, inRect.height - num2), category: selectedCategory);
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.MiddleCenter;
		if (Widgets.ButtonText(new Rect(inRect.width / 2f - OkayButtonSize.x / 2f, inRect.yMax - OkayButtonSize.y, OkayButtonSize.x, OkayButtonSize.y), "OK".Translate()))
		{
			Close();
		}
		Text.Anchor = TextAnchor.UpperLeft;
	}

	private void DoCategoryRow(Rect r, OptionCategoryDef optionCategory)
	{
		Widgets.DrawOptionBackground(r, optionCategory == selectedCategory);
		if (Widgets.ButtonInvisible(r))
		{
			selectedCategory = optionCategory;
			selectedMod = null;
			SoundDefOf.Click.PlayOneShotOnCamera();
		}
		float num = r.x + 10f;
		Rect position = new Rect(num, r.y + (r.height - 20f) / 2f, 20f, 20f);
		Texture2D image = ContentFinder<Texture2D>.Get(optionCategory.texPath);
		GUI.DrawTexture(position, image);
		num += 30f;
		Widgets.Label(new Rect(num, r.y, r.width - num, r.height), optionCategory.label.CapitalizeFirst());
	}

	private void DoOptions(OptionCategoryDef category, Rect inRect)
	{
		bool flag = optionsViewRectHeight > inRect.height;
		Rect outRect = new Rect(inRect);
		Rect viewRect = new Rect(outRect.x, outRect.y, outRect.width - (flag ? 26f : 0f), optionsViewRectHeight);
		Widgets.BeginScrollView(outRect, ref optionsScrollPosition, viewRect);
		Listing_Standard listing_Standard = new Listing_Standard();
		Rect rect = new Rect(viewRect.x, viewRect.y, viewRect.width, 999999f);
		listing_Standard.Begin(rect);
		listing_Standard.verticalSpacing = 5f;
		listing_Standard.Gap();
		if (category == OptionCategoryDefOf.General)
		{
			DoGeneralOptions(listing_Standard);
		}
		else if (category == OptionCategoryDefOf.Graphics)
		{
			DoVideoOptions(listing_Standard);
		}
		else if (category == OptionCategoryDefOf.Audio)
		{
			DoAudioOptions(listing_Standard);
		}
		else if (category == OptionCategoryDefOf.Gameplay)
		{
			DoGameplayOptions(listing_Standard);
		}
		else if (category == OptionCategoryDefOf.Interface)
		{
			DoUIOptions(listing_Standard);
		}
		else if (category == OptionCategoryDefOf.Controls)
		{
			DoControlsOptions(listing_Standard);
		}
		else if (category == OptionCategoryDefOf.Dev)
		{
			DoDevOptions(listing_Standard);
		}
		else if (category == OptionCategoryDefOf.Mods)
		{
			DoModOptions(listing_Standard);
		}
		if (selectedMod != null)
		{
			float num = 15f;
			Rect inRect2 = new Rect(0f, num, viewRect.width, inRect.height - num);
			selectedMod?.DoSettingsWindowContents(inRect2);
		}
		optionsViewRectHeight = listing_Standard.CurHeight;
		listing_Standard.End();
		Widgets.EndScrollView();
	}

	private void DoGeneralOptions(Listing_Standard listing)
	{
		if (listing.ButtonTextLabeledPct("ChooseLanguage".Translate(), LanguageDatabase.activeLanguage.DisplayName, 0.6f, TextAnchor.MiddleLeft, null, null, LanguageIconTex))
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				Messages.Message("ChangeLanguageFromMainMenu".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			else
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (LoadedLanguage allLoadedLanguage in LanguageDatabase.AllLoadedLanguages)
				{
					LoadedLanguage localLang = allLoadedLanguage;
					list.Add(new FloatMenuOption(localLang.DisplayName, delegate
					{
						LanguageDatabase.SelectLanguage(localLang);
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}
		float autosaveIntervalDays = Prefs.AutosaveIntervalDays;
		string text = "Days".Translate();
		string text2 = "day".Translate();
		if (listing.ButtonTextLabeledPct("AutosaveInterval".Translate(), autosaveIntervalDays + " " + ((autosaveIntervalDays == 1f) ? text2 : text), 0.6f, TextAnchor.MiddleLeft, null, "AutosaveIntervalTooltip".Translate()))
		{
			List<FloatMenuOption> list2 = new List<FloatMenuOption>();
			if (Prefs.DevMode)
			{
				list2.Add(new FloatMenuOption("0.05 " + text + "(debug)", delegate
				{
					Prefs.AutosaveIntervalDays = 0.05f;
				}));
				list2.Add(new FloatMenuOption("0.075 " + text + "(debug)", delegate
				{
					Prefs.AutosaveIntervalDays = 0.075f;
				}));
				list2.Add(new FloatMenuOption("0.1 " + text + "(debug)", delegate
				{
					Prefs.AutosaveIntervalDays = 0.1f;
				}));
				list2.Add(new FloatMenuOption("0.125 " + text + "(debug)", delegate
				{
					Prefs.AutosaveIntervalDays = 0.125f;
				}));
				list2.Add(new FloatMenuOption("0.25 " + text + "(debug)", delegate
				{
					Prefs.AutosaveIntervalDays = 0.25f;
				}));
			}
			list2.Add(new FloatMenuOption("0.5 " + text, delegate
			{
				Prefs.AutosaveIntervalDays = 0.5f;
			}));
			list2.Add(new FloatMenuOption(1 + " " + text2, delegate
			{
				Prefs.AutosaveIntervalDays = 1f;
			}));
			list2.Add(new FloatMenuOption(3 + " " + text, delegate
			{
				Prefs.AutosaveIntervalDays = 3f;
			}));
			list2.Add(new FloatMenuOption(7 + " " + text, delegate
			{
				Prefs.AutosaveIntervalDays = 7f;
			}));
			list2.Add(new FloatMenuOption(14 + " " + text, delegate
			{
				Prefs.AutosaveIntervalDays = 14f;
			}));
			Find.WindowStack.Add(new FloatMenu(list2));
		}
		if (Current.ProgramState == ProgramState.Playing && Current.Game.Info.permadeathMode && Prefs.AutosaveIntervalDays > 1f)
		{
			GUI.color = Color.red;
			listing.Label("MaxPermadeathAutosaveIntervalInfo".Translate(1f));
			GUI.color = Color.white;
		}
		int autosavesCount = Prefs.AutosavesCount;
		Prefs.AutosavesCount = Mathf.RoundToInt(listing.SliderLabeled("AutosavesCount".Translate(autosavesCount), autosavesCount, 1f, 25f, 0.6f));
		bool checkOn = Prefs.RunInBackground;
		listing.CheckboxLabeled("RunInBackground".Translate(), ref checkOn, null, 30f, 0.6f);
		Prefs.RunInBackground = checkOn;
		if (!DevModePermanentlyDisabledUtility.Disabled || Prefs.DevMode)
		{
			bool checkOn2 = Prefs.DevMode;
			listing.CheckboxLabeled("DevelopmentMode".Translate(), ref checkOn2, null, 30f, 0.6f);
			Prefs.DevMode = checkOn2;
		}
		if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
		{
			listing.verticalSpacing = 0f;
			if (listing.ButtonTextLabeledPct("SaveGameDataFolder".Translate(), "OpenFolder".Translate(), 0.6f, TextAnchor.MiddleLeft))
			{
				Application.OpenURL(GenFilePaths.SavedGamesFolderPath);
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			listing.SubLabel(GenFilePaths.SavedGamesFolderPath, 0.6f);
			listing.Gap();
			if (listing.ButtonTextLabeledPct("LogFileFolder".Translate(), "OpenFolder".Translate(), 0.6f, TextAnchor.MiddleLeft))
			{
				Application.OpenURL(Application.persistentDataPath);
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			listing.SubLabel(Application.persistentDataPath, 0.6f);
			listing.Gap();
			listing.verticalSpacing = 5f;
		}
		else
		{
			if (listing.ButtonTextLabeledPct("SaveGameDataLocation".Translate(), "ShowFolder".Translate(), 0.6f, TextAnchor.MiddleLeft))
			{
				Find.WindowStack.Add(new Dialog_MessageBox(Path.GetFullPath(GenFilePaths.SaveDataFolderPath)));
			}
			if (listing.ButtonTextLabeledPct("LogFileLocation".Translate(), "ShowFolder".Translate(), 0.6f, TextAnchor.MiddleLeft))
			{
				Find.WindowStack.Add(new Dialog_MessageBox(Path.GetFullPath(Application.consoleLogPath)));
			}
		}
		if (listing.ButtonTextLabeledPct("RestoreToDefaultSettingsLabel".Translate(), "RestoreToDefaultSettings".Translate(), 0.6f, TextAnchor.MiddleLeft))
		{
			Find.WindowStack.Add(new Dialog_MessageBox("ResetAndRestartConfirmationDialog".Translate(), buttonAAction: RestoreToDefaultSettings, buttonAText: "Yes".Translate(), buttonBText: "No".Translate()));
		}
	}

	private void DoVideoOptions(Listing_Standard listing)
	{
		if (listing.ButtonTextLabeledPct("Resolution".Translate(), ResToString(Screen.width, Screen.height), 0.6f, TextAnchor.MiddleLeft))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (Resolution res in from x in UnityGUIBugsFixer.ScreenResolutionsWithoutDuplicates
				where x.width >= 1024 && x.height >= 768
				orderby x.width, x.height
				select x)
			{
				list.Add(new FloatMenuOption(ResToString(res.width, res.height), delegate
				{
					if (!ResolutionUtility.UIScaleSafeWithResolution(Prefs.UIScale, res.width, res.height))
					{
						Messages.Message("MessageScreenResTooSmallForUIScale".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					}
					else
					{
						ResolutionUtility.SafeSetResolution(res);
					}
				}));
			}
			if (!list.Any())
			{
				list.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		if (ResolutionUtility.BorderlessFullscreen)
		{
			listing.Label("BorderlessFullscreen".Translate());
		}
		else
		{
			bool checkOn = Screen.fullScreen;
			bool flag = checkOn;
			listing.CheckboxLabeled("Fullscreen".Translate(), ref checkOn, null, 30f, 0.6f);
			if (checkOn != flag)
			{
				ResolutionUtility.SafeSetFullscreen(checkOn);
			}
			if (!SteamDeck.IsSteamDeck && listing.ButtonText("BorderlessFullscreen".Translate()))
			{
				Find.WindowStack.Add(new Dialog_MessageBox("BorderlessFullscreenInstructions".Translate()));
			}
		}
		bool textureCompression = Prefs.TextureCompression;
		listing.CheckboxLabeled("TextureCompression".Translate(), ref textureCompression, "TextureCompression_Tooltip".Translate(), 30f, 0.6f);
		if (textureCompression != Prefs.TextureCompression)
		{
			Find.WindowStack.Add(new Dialog_MessageBox("ChangedTextureCompressionRestart".Translate(), "Yes".Translate(), delegate
			{
				Prefs.TextureCompression = textureCompression;
				Prefs.Save();
				GenCommandLine.Restart();
			}, "No".Translate()));
		}
		Prefs.TextureCompression = textureCompression;
		bool checkOn2 = Prefs.PlantWindSway;
		listing.CheckboxLabeled("PlantWindSway".Translate(), ref checkOn2, null, 30f, 0.6f);
		Prefs.PlantWindSway = checkOn2;
		float screenShakeIntensity = Prefs.ScreenShakeIntensity;
		Prefs.ScreenShakeIntensity = (float)Math.Round(listing.SliderLabeled(string.Format("{0}: {1}", "ScreenShakeIntensity".Translate(), screenShakeIntensity.ToStringPercent("N0")), screenShakeIntensity, 0f, 2f, 0.6f), 1);
		bool checkOn3 = Prefs.SmoothCameraJumps;
		listing.CheckboxLabeled("SmoothCameraJumps".Translate(), ref checkOn3, "SmoothCameraJumpsDesc".Translate(), 30f, 0.6f);
		Prefs.SmoothCameraJumps = checkOn3;
		bool checkOn4 = Prefs.GravshipCutscenes;
		listing.CheckboxLabeled("GravshipCutscenes".Translate(), ref checkOn4, "GravshipCutscenesDesc".Translate(), 30f, 0.6f);
		Prefs.GravshipCutscenes = checkOn4;
	}

	private void DoAudioOptions(Listing_Standard listing)
	{
		Prefs.VolumeMaster = listing.SliderLabeled(string.Format("{0}: {1}", "MasterVolume".Translate(), Prefs.VolumeMaster.ToStringPercent()), Prefs.VolumeMaster, 0f, 1f, 0.6f, "MasterVolumeTooltip".Translate());
		Prefs.VolumeGame = listing.SliderLabeled(string.Format("{0}: {1}", "GameVolume".Translate(), Prefs.VolumeGame.ToStringPercent()), Prefs.VolumeGame, 0f, 1f, 0.6f, "GameVolumeTooltip".Translate());
		Prefs.VolumeMusic = listing.SliderLabeled(string.Format("{0}: {1}", "MusicVolume".Translate(), Prefs.VolumeMusic.ToStringPercent()), Prefs.VolumeMusic, 0f, 1f, 0.6f, "MusicVolumeTooltip".Translate());
		Prefs.VolumeAmbient = listing.SliderLabeled(string.Format("{0}: {1}", "AmbientVolume".Translate(), Prefs.VolumeAmbient.ToStringPercent()), Prefs.VolumeAmbient, 0f, 1f, 0.6f, "AmbientVolumeTooltip".Translate());
		Prefs.VolumeUI = listing.SliderLabeled(string.Format("{0}: {1}", "UIVolume".Translate(), Prefs.VolumeUI.ToStringPercent()), Prefs.VolumeUI, 0f, 1f, 0.6f, "UIVolumeTooltip".Translate());
	}

	private void DoGameplayOptions(Listing_Standard listing)
	{
		if (Current.ProgramState == ProgramState.Playing && listing.ButtonTextLabeledPct("ChangeStoryteller".Translate(), "Modify".Translate(), 0.6f, TextAnchor.UpperLeft, "OptionsButton-ChooseStoryteller") && TutorSystem.AllowAction("ChooseStoryteller"))
		{
			Find.WindowStack.Add(new Page_SelectStorytellerInGame());
		}
		int maxNumberOfPlayerSettlements = Prefs.MaxNumberOfPlayerSettlements;
		int num = (Prefs.MaxNumberOfPlayerSettlements = Mathf.RoundToInt(listing.SliderLabeled("MaxNumberOfPlayerSettlements".Translate(maxNumberOfPlayerSettlements), maxNumberOfPlayerSettlements, 1f, 5f, 0.6f)));
		if (maxNumberOfPlayerSettlements != num && num > 1)
		{
			TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.MaxNumberOfPlayerSettlements);
		}
		bool checkOn = Prefs.PauseOnLoad;
		listing.CheckboxLabeled("PauseOnLoad".Translate(), ref checkOn, null, 30f, 0.6f);
		Prefs.PauseOnLoad = checkOn;
		AutomaticPauseMode automaticPauseMode = Prefs.AutomaticPauseMode;
		if (listing.ButtonTextLabeledPct("AutomaticPauseModeSetting".Translate(), Prefs.AutomaticPauseMode.ToStringHuman(), 0.6f, TextAnchor.MiddleLeft))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (AutomaticPauseMode value in Enum.GetValues(typeof(AutomaticPauseMode)))
			{
				AutomaticPauseMode localPmode = value;
				list.Add(new FloatMenuOption(localPmode.ToStringHuman(), delegate
				{
					Prefs.AutomaticPauseMode = localPmode;
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		Prefs.AutomaticPauseMode = automaticPauseMode;
		bool checkOn2 = Prefs.AdaptiveTrainingEnabled;
		listing.CheckboxLabeled("LearningHelper".Translate(), ref checkOn2, "LearningHelperTooltip".Translate(), 30f, 0.6f);
		Prefs.AdaptiveTrainingEnabled = checkOn2;
		if (listing.ButtonTextLabeledPct("ResetAdaptiveTutor".Translate(), "Reset".Translate(), 0.6f, TextAnchor.MiddleLeft))
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmResetLearningHelper".Translate(), delegate
			{
				Messages.Message("AdaptiveTutorIsReset".Translate(), MessageTypeDefOf.TaskCompletion, historical: false);
				PlayerKnowledgeDatabase.ResetPersistent();
			}, destructive: true));
		}
		listing.verticalSpacing = 0f;
		if (Prefs.PreferredNames.Count < 6)
		{
			if (listing.ButtonTextLabeledPct("NamesYouWantToSee".Translate(), "AddName".Translate() + "...", 0.6f, TextAnchor.MiddleLeft))
			{
				Find.WindowStack.Add(new Dialog_AddPreferredName());
			}
		}
		else
		{
			listing.Label("NamesYouWantToSee".Translate());
		}
		listing.SubLabel("NamesYouWantToSeeSubText".Translate(), 0.6f);
		listing.verticalSpacing = 5f;
		Prefs.PreferredNames.RemoveAll((string n) => n.NullOrEmpty());
		float num3 = 20f;
		listing.Indent(num3);
		for (int num4 = 0; num4 < Prefs.PreferredNames.Count; num4++)
		{
			string name = Prefs.PreferredNames[num4];
			PawnBio pawnBio = SolidBioDatabase.allBios.Where((PawnBio b) => b.name.ToString() == name).FirstOrDefault();
			if (pawnBio == null)
			{
				name += " [N]";
			}
			else
			{
				switch (pawnBio.BioType)
				{
				case PawnBioType.BackstoryInGame:
					name += " [B]";
					break;
				case PawnBioType.PirateKing:
					name += " [PK]";
					break;
				}
			}
			Rect rect = listing.GetRect(24f);
			rect.width -= num3;
			if (num4 % 2 == 0)
			{
				Widgets.DrawLightHighlight(rect);
			}
			Widgets.Label(new Rect(rect.x + 4f, rect.y, rect.width - 4f, rect.height), name);
			if (Widgets.ButtonImage(new Rect(rect.xMax - 24f, rect.y, 24f, 24f), TexButton.Delete, Color.white, GenUI.SubtleMouseoverColor))
			{
				Prefs.PreferredNames.RemoveAt(num4);
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			}
		}
		listing.Outdent(num3);
		listing.Gap();
	}

	private void DoUIOptions(Listing_Standard listing)
	{
		if (listing.ButtonTextLabeledPct("UIScale".Translate(), Prefs.UIScale + "x", 0.6f, TextAnchor.MiddleLeft))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			for (int i = 0; i < UIScales.Length; i++)
			{
				float scale = UIScales[i];
				list.Add(new FloatMenuOption(UIScales[i] + "x", delegate
				{
					if (scale != 1f && !ResolutionUtility.UIScaleSafeWithResolution(scale, Screen.width, Screen.height))
					{
						Messages.Message("MessageScreenResTooSmallForUIScale".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					}
					else
					{
						ResolutionUtility.SafeSetUIScale(scale);
					}
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		if (listing.ButtonTextLabeledPct("TemperatureMode".Translate(), Prefs.TemperatureMode.ToStringHuman(), 0.6f, TextAnchor.MiddleLeft))
		{
			List<FloatMenuOption> list2 = new List<FloatMenuOption>();
			foreach (TemperatureDisplayMode value in Enum.GetValues(typeof(TemperatureDisplayMode)))
			{
				TemperatureDisplayMode localTmode = value;
				list2.Add(new FloatMenuOption(localTmode.ToStringHuman(), delegate
				{
					Prefs.TemperatureMode = localTmode;
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list2));
		}
		if (listing.ButtonTextLabeledPct("ShowWeaponsUnderPortrait".Translate(), Prefs.ShowWeaponsUnderPortraitMode.ToStringHuman(), 0.6f, TextAnchor.MiddleLeft, null, "ShowWeaponsUnderPortraitTooltip".Translate()))
		{
			List<FloatMenuOption> list3 = new List<FloatMenuOption>();
			foreach (ShowWeaponsUnderPortraitMode mode in Enum.GetValues(typeof(ShowWeaponsUnderPortraitMode)))
			{
				list3.Add(new FloatMenuOption(mode.ToStringHuman(), delegate
				{
					Prefs.ShowWeaponsUnderPortraitMode = mode;
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list3));
		}
		if (listing.ButtonTextLabeledPct("ShowAnimalNames".Translate(), Prefs.AnimalNameMode.ToStringHuman(), 0.6f, TextAnchor.MiddleLeft))
		{
			List<FloatMenuOption> list4 = new List<FloatMenuOption>();
			foreach (AnimalNameDisplayMode value2 in Enum.GetValues(typeof(AnimalNameDisplayMode)))
			{
				AnimalNameDisplayMode localMode = value2;
				list4.Add(new FloatMenuOption(localMode.ToStringHuman(), delegate
				{
					Prefs.AnimalNameMode = localMode;
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list4));
		}
		if (ModsConfig.BiotechActive && listing.ButtonTextLabeledPct("ShowMechNames".Translate(), Prefs.MechNameMode.ToStringHuman(), 0.6f, TextAnchor.MiddleLeft))
		{
			List<FloatMenuOption> list5 = new List<FloatMenuOption>();
			foreach (MechNameDisplayMode value3 in Enum.GetValues(typeof(MechNameDisplayMode)))
			{
				MechNameDisplayMode localMode2 = value3;
				list5.Add(new FloatMenuOption(localMode2.ToStringHuman(), delegate
				{
					Prefs.MechNameMode = localMode2;
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list5));
		}
		if (listing.ButtonTextLabeledPct("DotHighlightDisplayMode".Translate(), Prefs.DotHighlightDisplayMode.ToStringHuman(), 0.6f, TextAnchor.MiddleLeft))
		{
			List<FloatMenuOption> list6 = new List<FloatMenuOption>();
			foreach (DotHighlightDisplayMode value4 in Enum.GetValues(typeof(DotHighlightDisplayMode)))
			{
				DotHighlightDisplayMode localMode3 = value4;
				list6.Add(new FloatMenuOption(localMode3.ToStringHuman(), delegate
				{
					Prefs.DotHighlightDisplayMode = localMode3;
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list6));
		}
		bool flag = Prefs.DotHighlightDisplayMode != DotHighlightDisplayMode.None;
		if (!flag)
		{
			GUI.color = Color.gray;
		}
		if (listing.ButtonTextLabeledPct("HighlightStyleMode".Translate(), Prefs.HighlightStyleMode.ToStringHuman(), 0.6f, TextAnchor.MiddleLeft))
		{
			List<FloatMenuOption> list7 = new List<FloatMenuOption>();
			foreach (HighlightStyleMode value5 in Enum.GetValues(typeof(HighlightStyleMode)))
			{
				HighlightStyleMode localMode4 = value5;
				list7.Add(new FloatMenuOption(localMode4.ToStringHuman(), delegate
				{
					Prefs.HighlightStyleMode = localMode4;
				}));
			}
			if (flag)
			{
				Find.WindowStack.Add(new FloatMenu(list7));
			}
		}
		if (!flag)
		{
			GUI.color = Color.white;
		}
		TaggedString taggedString = (Prefs.RandomBackgroundImage ? "Random".Translate() : Prefs.BackgroundImageExpansion.LabelCap);
		if (ModLister.AllExpansions.Where((ExpansionDef e) => !e.isCore && e.Status != ExpansionStatus.NotInstalled).Any() && listing.ButtonTextLabeledPct("SetBackgroundImage".Translate(), taggedString, 0.6f, TextAnchor.MiddleLeft))
		{
			List<FloatMenuOption> list8 = new List<FloatMenuOption>();
			foreach (ExpansionDef allExpansion in ModLister.AllExpansions)
			{
				if (allExpansion.Status != ExpansionStatus.NotInstalled)
				{
					ExpansionDef localExpansion = allExpansion;
					list8.Add(new FloatMenuOption(allExpansion.label, delegate
					{
						Prefs.BackgroundImageExpansion = localExpansion;
						Prefs.RandomBackgroundImage = false;
					}, allExpansion.Icon, Color.white));
				}
			}
			list8.Add(new FloatMenuOption("Random".Translate(), delegate
			{
				Prefs.RandomBackgroundImage = true;
				((UI_BackgroundMain)UIMenuBackgroundManager.background).overrideBGImage = ModLister.AllExpansions.Where((ExpansionDef exp) => exp.Status == ExpansionStatus.Active).RandomElement().BackgroundImage;
			}, RandomBackgroundTex, Color.white));
			Find.WindowStack.Add(new FloatMenu(list8));
		}
		bool checkOn = Prefs.ShowRealtimeClock;
		listing.CheckboxLabeled("ShowRealtimeClock".Translate(), ref checkOn, null, 30f, 0.6f);
		Prefs.ShowRealtimeClock = checkOn;
		bool checkOn2 = Prefs.TwelveHourClockMode;
		listing.CheckboxLabeled("TwelveHourClockMode".Translate(), ref checkOn2, null, 30f, 0.6f);
		Prefs.TwelveHourClockMode = checkOn2;
		bool checkOn3 = Prefs.HatsOnlyOnMap;
		listing.CheckboxLabeled("HatsShownOnlyOnMap".Translate(), ref checkOn3, null, 30f, 0.6f);
		if (checkOn3 != Prefs.HatsOnlyOnMap)
		{
			PortraitsCache.Clear();
		}
		Prefs.HatsOnlyOnMap = checkOn3;
		if (!SteamDeck.IsSteamDeck)
		{
			bool checkOn4 = Prefs.DisableTinyText;
			listing.CheckboxLabeled("DisableTinyText".Translate(), ref checkOn4, null, 30f, 0.6f);
			if (Prefs.DisableTinyText != checkOn4)
			{
				Prefs.DisableTinyText = checkOn4;
				Widgets.ClearLabelCache();
				GenUI.ClearLabelWidthCache();
				if (Current.ProgramState == ProgramState.Playing)
				{
					Find.ColonistBar.drawer.ClearLabelCache();
				}
			}
		}
		bool checkOn5 = !Prefs.CustomCursorEnabled;
		listing.CheckboxLabeled("CustomCursor".Translate(), ref checkOn5, null, 30f, 0.6f);
		Prefs.CustomCursorEnabled = !checkOn5;
		bool checkOn6 = Prefs.VisibleMood;
		listing.CheckboxLabeled("VisibleMood".Translate(), ref checkOn6, "VisibleMoodDesc".Translate(), 30f, 0.6f);
		Prefs.VisibleMood = checkOn6;
	}

	private void DoControlsOptions(Listing_Standard listing)
	{
		if (!SteamDeck.IsSteamDeckInNonKeyboardMode && listing.ButtonTextLabeledPct("KeyboardConfig".Translate(), "ModifyConfig".Translate(), 0.6f, TextAnchor.MiddleLeft))
		{
			Find.WindowStack.Add(new Dialog_KeyBindings());
		}
		if (SteamDeck.IsSteamDeck && listing.ButtonTextLabeledPct("ControllerConfig".Translate(), "ModifyConfig".Translate(), 0.6f, TextAnchor.MiddleLeft))
		{
			SteamDeck.ShowConfigPage();
		}
		if (SteamDeck.IsSteamDeck)
		{
			bool checkOn = Prefs.SteamDeckKeyboardMode;
			listing.CheckboxLabeled("SteamDeckKeyboardMode".Translate(), ref checkOn, null, 30f, 0.6f);
			Prefs.SteamDeckKeyboardMode = checkOn;
		}
		float mapDragSensitivity = Prefs.MapDragSensitivity;
		Prefs.MapDragSensitivity = (float)Math.Round(listing.SliderLabeled("MapDragSensitivity".Translate() + ": " + mapDragSensitivity.ToStringPercent("F0"), mapDragSensitivity, 0.8f, 2.5f, 0.6f), 2);
		bool checkOn2 = Prefs.EdgeScreenScroll;
		listing.CheckboxLabeled("EdgeScreenScroll".Translate(), ref checkOn2, null, 30f, 0.6f);
		Prefs.EdgeScreenScroll = checkOn2;
		bool checkOn3 = Prefs.ZoomToMouse;
		listing.CheckboxLabeled("ZoomToMouse".Translate(), ref checkOn3, null, 30f, 0.6f);
		Prefs.ZoomToMouse = checkOn3;
		bool checkOn4 = Prefs.ZoomSwitchWorldLayer;
		listing.CheckboxLabeled("ZoomSwitchLayer".Translate(), ref checkOn4, "ZoomSwitchLayer_Tooltip".Translate(), 30f, 0.6f);
		Prefs.ZoomSwitchWorldLayer = checkOn4;
		bool checkOn5 = Prefs.RememberDrawStlyes;
		listing.CheckboxLabeled("RememberDrawStyle".Translate(), ref checkOn5, "RememberDrawStyle_Tooltip".Translate(), 30f, 0.6f);
		Prefs.RememberDrawStlyes = checkOn5;
	}

	private void DoDevOptions(Listing_Standard listing)
	{
		bool checkOn = Prefs.TestMapSizes;
		listing.CheckboxLabeled("EnableTestMapSizes".Translate(), ref checkOn, null, 30f, 0.6f);
		Prefs.TestMapSizes = checkOn;
		bool checkOn2 = Prefs.LogVerbose;
		listing.CheckboxLabeled("LogVerbose".Translate(), ref checkOn2, null, 30f, 0.6f);
		Prefs.LogVerbose = checkOn2;
		bool checkOn3 = Prefs.ResetModsConfigOnCrash;
		listing.CheckboxLabeled("ResetModsConfigOnCrash".Translate(), ref checkOn3, null, 30f, 0.6f);
		Prefs.ResetModsConfigOnCrash = checkOn3;
		bool checkOn4 = Prefs.DisableQuickStartCryptoSickness;
		listing.CheckboxLabeled("DisableQuickStartCryptoSickness".Translate(), ref checkOn4, null, 30f, 0.6f);
		Prefs.DisableQuickStartCryptoSickness = checkOn4;
		bool checkOn5 = Prefs.StartDevPaletteOn;
		listing.CheckboxLabeled("StartDevPaletteOn".Translate(), ref checkOn5, null, 30f, 0.6f);
		Prefs.StartDevPaletteOn = checkOn5;
		bool checkOn6 = Prefs.OpenLogOnWarnings;
		listing.CheckboxLabeled("OpenLogOnWarnings".Translate(), ref checkOn6, null, 30f, 0.6f);
		Prefs.OpenLogOnWarnings = checkOn6;
		bool checkOn7 = Prefs.CloseLogWindowOnEscape;
		listing.CheckboxLabeled("CloseLogWindowOnEscape".Translate(), ref checkOn7, null, 30f, 0.6f);
		Prefs.CloseLogWindowOnEscape = checkOn7;
		if (!DevModePermanentlyDisabledUtility.Disabled && listing.ButtonTextLabeledPct("PermanentlyDisableDevMode".Translate(), "Disable".Translate(), 0.6f, TextAnchor.MiddleLeft))
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmPermanentlyDisableDevMode".Translate(), DevModePermanentlyDisabledUtility.Disable, destructive: true));
		}
	}

	private void DoModOptions(Listing_Standard listing)
	{
		Rect rect = listing.GetRect(30f);
		quickSearchWidget.OnGUI(rect);
		modFilter = quickSearchWidget.filter.Text;
		listing.Gap();
		Rect rect2 = default(Rect);
		int num = 0;
		foreach (Mod cachedModsWithSetting in cachedModsWithSettings)
		{
			if (cachedModsWithSetting.SettingsCategory().ToLower().Contains(modFilter.ToLower()) || cachedModsWithSetting.Content.Name.ToLower().Contains(modFilter.ToLower()))
			{
				if (num % 2 == 0)
				{
					rect2 = listing.GetRect(40f);
				}
				Rect rect3 = ((num % 2 == 0) ? rect2.LeftHalf() : rect2.RightHalf()).ContractedBy(2f);
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.DrawOptionBackground(rect3, selected: false);
				if (Widgets.ButtonInvisible(rect3))
				{
					Find.WindowStack.Add(new Dialog_ModSettings(cachedModsWithSetting));
				}
				if (Mouse.IsOver(rect3) && Text.CalcSize(cachedModsWithSetting.SettingsCategory()).x > rect3.width - rect3.height)
				{
					TooltipHandler.TipRegion(rect3, cachedModsWithSetting.SettingsCategory());
				}
				if (!cachedModsWithSetting.Content.ModMetaData.Icon.NullOrBad())
				{
					GUI.DrawTexture(new Rect(rect3.x, rect3.y, rect3.height, rect3.height).ContractedBy(2f), cachedModsWithSetting.Content.ModMetaData.Icon);
				}
				rect3.xMin += rect3.height;
				Widgets.Label(rect3, cachedModsWithSetting.SettingsCategory().Truncate(rect3.width, modOptionTruncationCache));
				Text.Anchor = TextAnchor.UpperLeft;
				num++;
			}
		}
	}

	public override void PreClose()
	{
		base.PreClose();
		Prefs.Save();
		if (selectedMod != null)
		{
			selectedMod.WriteSettings();
		}
	}

	public void RestoreToDefaultSettings()
	{
		FileInfo[] files = new DirectoryInfo(GenFilePaths.ConfigFolderPath).GetFiles("*.xml");
		foreach (FileInfo fileInfo in files)
		{
			try
			{
				fileInfo.Delete();
			}
			catch (SystemException)
			{
			}
		}
		Find.WindowStack.Add(new Dialog_MessageBox("ResetAndRestart".Translate(), null, GenCommandLine.Restart));
	}

	public static string ResToString(int width, int height)
	{
		string text = width + "x" + height;
		if (width == 1280 && height == 720)
		{
			text += " (720p)";
		}
		if (width == 1920 && height == 1080)
		{
			text += " (1080p)";
		}
		return text;
	}
}
