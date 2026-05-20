using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Profile;
using Verse.Steam;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class MainMenuDrawer
{
	private static bool anyMapFiles;

	private static Vector2 translationInfoScrollbarPos;

	private static float webBackgroundYMax;

	private static Rect webRect;

	private static string anomalyReleasedTranslated = null;

	private const float PlayRectWidth = 170f;

	private const float WebRectWidth = 145f;

	private const float RightEdgeMargin = 50f;

	private const float WebBGExpansion = 4f;

	private const float ExpansionDetailsWidth = 350f;

	private static readonly Vector2 PaneSize = new Vector2(450f, 550f);

	private static readonly Vector2 TitleSize = new Vector2(1032f, 146f);

	private static readonly Texture2D TexTitle = ContentFinder<Texture2D>.Get("UI/HeroArt/GameTitle");

	private const float TitleShift = 50f;

	private static readonly Vector2 LudeonLogoSize = new Vector2(200f, 58f);

	private static readonly Texture2D TexLudeonLogo = ContentFinder<Texture2D>.Get("UI/HeroArt/LudeonLogoSmall");

	private static readonly string TranslationsContributeURL = "https://rimworldgame.com/helptranslate";

	private static readonly Texture2D WebBGAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/SubtleGradient");

	private static readonly Color WebBGColor = new Color(0f, 0f, 0f, 0.3f);

	private static readonly Color ExpansionReleaseTextColor = new Color(49f / 51f, 0.96862745f, 43f / 85f);

	private const int NumColumns = 2;

	private const int NumRows = 3;

	private static UI_BackgroundMain BackgroundMain => (UI_BackgroundMain)UIMenuBackgroundManager.background;

	public static void Init()
	{
		PlayerKnowledgeDatabase.Save();
		ShipCountdown.CancelCountdown();
		if (ModsConfig.IdeologyActive)
		{
			ArchonexusCountdown.CancelCountdown();
		}
		anyMapFiles = GenFilePaths.AllSavedGameFiles.Any();
		if (Prefs.RandomBackgroundImage)
		{
			BackgroundMain.overrideBGImage = ModLister.AllExpansions.Where((ExpansionDef exp) => exp.Status == ExpansionStatus.Active).RandomElement().BackgroundImage;
		}
		else
		{
			BackgroundMain.overrideBGImage = Prefs.BackgroundImageExpansion.BackgroundImage;
		}
		BackgroundMain.SetupExpansionFadeData();
		anomalyReleasedTranslated = "MainScreen_AnomalyAvailableNow".Translate();
	}

	public static void MainMenuOnGUI()
	{
		VersionControl.DrawInfoInCorner();
		Rect rect = new Rect((float)UI.screenWidth / 2f - PaneSize.x / 2f, (float)UI.screenHeight / 2f - PaneSize.y / 2f + 50f, PaneSize.x, PaneSize.y);
		rect.x = (float)UI.screenWidth - rect.width - 30f;
		Rect rect2 = new Rect(0f, rect.y - 30f, (float)UI.screenWidth - 85f, 30f);
		Text.Font = GameFont.Medium;
		Text.Anchor = TextAnchor.UpperRight;
		string text = "MainPageCredit".Translate();
		if (UI.screenWidth < 990)
		{
			Rect position = rect2;
			position.xMin = position.xMax - Text.CalcSize(text).x;
			position.xMin -= 4f;
			position.xMax += 4f;
			GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
			GUI.DrawTexture(position, BaseContent.WhiteTex);
			GUI.color = Color.white;
		}
		Widgets.Label(rect2, text);
		Text.Anchor = TextAnchor.UpperLeft;
		Text.Font = GameFont.Small;
		Vector2 titleSize = TitleSize;
		if (titleSize.x > (float)UI.screenWidth)
		{
			titleSize *= (float)UI.screenWidth / titleSize.x;
		}
		titleSize *= 0.5f;
		GUI.DrawTexture(new Rect((float)UI.screenWidth - titleSize.x - 50f, rect2.y - titleSize.y, titleSize.x, titleSize.y), TexTitle, ScaleMode.StretchToFill, alphaBlend: true);
		GUI.color = new Color(1f, 1f, 1f, 0.5f);
		GUI.DrawTexture(new Rect((float)(UI.screenWidth - 8) - LudeonLogoSize.x, 8f, LudeonLogoSize.x, LudeonLogoSize.y), TexLudeonLogo, ScaleMode.StretchToFill, alphaBlend: true);
		GUI.color = Color.white;
		rect.yMin += 17f;
		DoMainMenuControls(rect, anyMapFiles);
		DoTranslationInfoRect(new Rect(8f, 100f, 300f, 400f));
		DoExpansionIcons();
		SteamDeck.ShowSteamDeckMainMenuControlsIfNotKnown();
	}

	public static void DoMainMenuControls(Rect rect, bool anyMapFiles)
	{
		if (webBackgroundYMax > 0f && Current.ProgramState == ProgramState.Entry)
		{
			GUI.color = WebBGColor;
			Rect rect2 = new Rect(rect.x + webRect.x, rect.y + webRect.y, webRect.width, rect.height);
			rect2.yMax = rect.y + webBackgroundYMax - 4f;
			Widgets.DrawAtlas(rect2.ExpandedBy(4f), WebBGAtlas);
			GUI.color = Color.white;
		}
		Widgets.BeginGroup(rect);
		Rect rect3 = new Rect(0f, 0f, 170f, rect.height);
		webRect = new Rect(rect3.xMax + 17f, 0f, 145f, rect.height);
		Text.Font = GameFont.Small;
		List<ListableOption> list = new List<ListableOption>();
		if (Current.ProgramState == ProgramState.Entry)
		{
			string label = ("Tutorial".CanTranslate() ? ((string)"Tutorial".Translate()) : ((string)"LearnToPlay".Translate()));
			list.Add(new ListableOption(label, InitLearnToPlay));
			list.Add(new ListableOption("NewColony".Translate(), delegate
			{
				Find.WindowStack.Add(new Page_SelectScenario());
			}));
			if (Prefs.DevMode)
			{
				list.Add(new ListableOption("DevQuickTest".Translate(), delegate
				{
					LongEventHandler.QueueLongEvent(delegate
					{
						Root_Play.SetupForQuickTestPlay();
						PageUtility.InitGameStart();
					}, "GeneratingMap", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
				}));
			}
		}
		if (Current.ProgramState == ProgramState.Playing && !GameDataSaveLoader.SavingIsTemporarilyDisabled && !Current.Game.Info.permadeathMode)
		{
			list.Add(new ListableOption("Save".Translate(), delegate
			{
				CloseMainTab();
				Find.WindowStack.Add(new Dialog_SaveFileList_Save());
			}));
		}
		ListableOption item;
		if (anyMapFiles && (Current.ProgramState != ProgramState.Playing || !Current.Game.Info.permadeathMode))
		{
			item = new ListableOption("LoadGame".Translate(), delegate
			{
				CloseMainTab();
				Find.WindowStack.Add(new Dialog_SaveFileList_Load());
			});
			list.Add(item);
		}
		if (Current.ProgramState == ProgramState.Playing)
		{
			list.Add(new ListableOption("ReviewScenario".Translate(), delegate
			{
				Find.WindowStack.Add(new Dialog_MessageBox(Find.Scenario.GetFullInformationText(), null, null, null, null, Find.Scenario.name)
				{
					layer = WindowLayer.Super
				});
			}));
		}
		item = new ListableOption("Options".Translate(), delegate
		{
			CloseMainTab();
			Find.WindowStack.Add(new Dialog_Options());
		}, "MenuButton-Options");
		list.Add(item);
		if (Current.ProgramState == ProgramState.Entry)
		{
			item = new ListableOption("Mods".Translate(), delegate
			{
				Find.WindowStack.Add(new Page_ModsConfig());
			});
			list.Add(item);
			if (Prefs.DevMode && LanguageDatabase.activeLanguage == LanguageDatabase.defaultLanguage && LanguageDatabase.activeLanguage.anyError)
			{
				item = new ListableOption("SaveTranslationReport".Translate(), LanguageReportGenerator.SaveTranslationReport);
				list.Add(item);
			}
			item = new ListableOption("Credits".Translate(), delegate
			{
				Find.WindowStack.Add(new Screen_Credits());
			});
			list.Add(item);
		}
		if (Current.ProgramState == ProgramState.Playing)
		{
			if (Current.Game.Info.permadeathMode && !GameDataSaveLoader.SavingIsTemporarilyDisabled)
			{
				item = new ListableOption("SaveAndQuitToMainMenu".Translate(), delegate
				{
					LongEventHandler.QueueLongEvent(delegate
					{
						GameDataSaveLoader.SaveGame(Current.Game.Info.permadeathModeUniqueName);
						MemoryUtility.ClearAllMapsAndWorld();
					}, "Entry", "SavingLongEvent", doAsynchronously: false, null, showExtraUIInfo: false);
				});
				list.Add(item);
				item = new ListableOption("SaveAndQuitToOS".Translate(), delegate
				{
					LongEventHandler.QueueLongEvent(delegate
					{
						GameDataSaveLoader.SaveGame(Current.Game.Info.permadeathModeUniqueName);
						LongEventHandler.ExecuteWhenFinished(Root.Shutdown);
					}, "SavingLongEvent", doAsynchronously: false, null, showExtraUIInfo: false);
				});
				list.Add(item);
			}
			else
			{
				Action action = delegate
				{
					if (GameDataSaveLoader.CurrentGameStateIsValuable)
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmQuit".Translate(), GenScene.GoToMainMenu, destructive: true, null, WindowLayer.Super));
					}
					else
					{
						GenScene.GoToMainMenu();
					}
				};
				item = new ListableOption("QuitToMainMenu".Translate(), action);
				list.Add(item);
				Action action2 = delegate
				{
					if (GameDataSaveLoader.CurrentGameStateIsValuable)
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmQuit".Translate(), Root.Shutdown, destructive: true, null, WindowLayer.Super));
					}
					else
					{
						Root.Shutdown();
					}
				};
				item = new ListableOption("QuitToOS".Translate(), action2);
				list.Add(item);
			}
		}
		else
		{
			item = new ListableOption("QuitToOS".Translate(), Root.Shutdown);
			list.Add(item);
		}
		OptionListingUtility.DrawOptionListing(rect3, list);
		Text.Font = GameFont.Small;
		List<ListableOption> list2 = new List<ListableOption>();
		ListableOption item2 = new ListableOption_WebLink("FictionPrimer".Translate(), "https://rimworldgame.com/backstory", TexButton.IconBlog);
		list2.Add(item2);
		item2 = new ListableOption_WebLink("LudeonBlog".Translate(), "https://ludeon.com/blog", TexButton.IconBlog);
		list2.Add(item2);
		item2 = new ListableOption_WebLink("Subreddit".Translate(), "https://www.reddit.com/r/RimWorld/", TexButton.IconReddit);
		list2.Add(item2);
		item2 = new ListableOption_WebLink("OfficialWiki".Translate(), "https://rimworldwiki.com", TexButton.IconWiki);
		list2.Add(item2);
		item2 = new ListableOption_WebLink("TynansX".Translate(), "https://x.com/TynanSylvester", TexButton.IconX);
		list2.Add(item2);
		item2 = new ListableOption_WebLink("TynansDesignBook".Translate(), "https://tynansylvester.com/book", TexButton.IconBook);
		list2.Add(item2);
		item2 = new ListableOption_WebLink("HelpTranslate".Translate(), TranslationsContributeURL, TexButton.IconForums);
		list2.Add(item2);
		item2 = new ListableOption_WebLink("BuySoundtrack".Translate(), delegate
		{
			List<FloatMenuOption> options = new List<FloatMenuOption>
			{
				new FloatMenuOption("BuySoundtrack_Classic".Translate(), delegate
				{
					Application.OpenURL("https://store.steampowered.com/app/990430/RimWorld_Soundtrack/");
				}),
				new FloatMenuOption("BuySoundtrack_Royalty".Translate(), delegate
				{
					Application.OpenURL("https://store.steampowered.com/app/1244270/RimWorld__Royalty_Soundtrack/");
				}),
				new FloatMenuOption("BuySoundtrack_Anomaly".Translate(), delegate
				{
					Application.OpenURL("https://store.steampowered.com/app/2914900/RimWorld__Anomaly_Soundtrack/");
				}),
				new FloatMenuOption("BuySoundtrack_Odyssey".Translate(), delegate
				{
					Application.OpenURL("https://store.steampowered.com/app/3689230/RimWorld__Odyssey_Soundtrack/");
				})
			};
			Find.WindowStack.Add(new FloatMenu(options));
		}, TexButton.IconSoundtrack);
		list2.Add(item2);
		webBackgroundYMax = OptionListingUtility.DrawOptionListing(webRect, list2);
		Widgets.BeginGroup(webRect);
		if (Current.ProgramState == ProgramState.Entry && Widgets.ButtonText(new Rect(0f, webBackgroundYMax + 10f, webRect.width, 50f), LanguageDatabase.activeLanguage.FriendlyNameNative))
		{
			List<FloatMenuOption> list3 = new List<FloatMenuOption>();
			foreach (LoadedLanguage allLoadedLanguage in LanguageDatabase.AllLoadedLanguages)
			{
				LoadedLanguage localLang = allLoadedLanguage;
				list3.Add(new FloatMenuOption(localLang.DisplayName, delegate
				{
					LanguageDatabase.SelectLanguage(localLang);
					Prefs.Save();
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list3));
		}
		Widgets.EndGroup();
		Widgets.EndGroup();
	}

	public static void DoExpansionIcons()
	{
		List<ExpansionDef> allExpansions = ModLister.AllExpansions;
		int num = -1;
		int num2 = allExpansions.Count((ExpansionDef e) => !e.isCore);
		int num3 = 32 + 64 * num2 + (num2 - 1) * 8 * 2;
		Rect rect = new Rect(8f, UI.screenHeight - 96 - 8, num3, 96f);
		Widgets.DrawWindowBackground(rect);
		Widgets.BeginGroup(rect.ContractedBy(16f));
		float num4 = 0f;
		for (int num5 = 0; num5 < allExpansions.Count; num5++)
		{
			if (!allExpansions[num5].isCore)
			{
				Rect rect2 = new Rect(num4, 0f, 64f, 64f);
				num4 += 72f;
				GUI.DrawTexture(rect2.ContractedBy(2f), allExpansions[num5].IconFromStatus);
				if (Widgets.ButtonInvisible(rect2) && !allExpansions[num5].StoreURL.NullOrEmpty())
				{
					SteamUtility.OpenUrl(allExpansions[num5].StoreURL);
				}
				if (Mouse.IsOver(rect2))
				{
					Widgets.DrawHighlight(rect2);
					num = num5;
				}
				num4 += 8f;
			}
		}
		Widgets.EndGroup();
		if (num >= 0)
		{
			BackgroundMain.Notify_Hovered(allExpansions[num]);
			DoExpansionInfo(num, new Vector2(Mathf.Min(num3 + 16, (float)UI.screenWidth - 350f), rect.yMax));
		}
	}

	private static void DoExpansionInfo(int index, Vector2 offset)
	{
		ExpansionDef expansionDef = ModLister.AllExpansions[index];
		List<Texture2D> previewImages = expansionDef.PreviewImages;
		float num = 16f;
		float num2 = 200f;
		float num3 = num2 * 2f + num * 2f;
		float b = (previewImages.NullOrEmpty() ? 0f : (num2 * 3f + num * 2.5f));
		Text.Font = GameFont.Medium;
		float num4 = Text.CalcHeight(expansionDef.label, 350f - num * 2f);
		Text.Font = GameFont.Small;
		string text = "ClickForMoreInfo".Translate();
		float num5 = Text.CalcHeight(expansionDef.description, 350f - num * 2f);
		float num6 = Text.CalcHeight(text, 350f - num * 2f);
		b = Mathf.Max(num4 + num6 + num + num5 + num * 2f, b);
		Rect rect = new Rect(offset.x, offset.y - b, 350f, b);
		Widgets.DrawWindowBackground(new Rect(rect.x, rect.y, previewImages.NullOrEmpty() ? rect.width : (rect.width + num3), rect.height));
		Rect rect2 = rect.ContractedBy(num);
		Widgets.BeginGroup(rect2);
		float num7 = 0f;
		Text.Font = GameFont.Medium;
		Text.Anchor = TextAnchor.UpperCenter;
		Widgets.Label(new Rect(0f, num7, rect2.width, num4), new GUIContent(" " + expansionDef.label, expansionDef.Icon));
		Text.Font = GameFont.Small;
		num7 += num4;
		GUI.color = Color.grey;
		Widgets.Label(new Rect(0f, num7, rect2.width, num6), text);
		GUI.color = Color.white;
		Text.Anchor = TextAnchor.UpperLeft;
		num7 += num6 + num;
		Widgets.Label(new Rect(0f, num7, rect2.width, rect2.height - num7), expansionDef.description);
		Widgets.EndGroup();
		if (previewImages.NullOrEmpty())
		{
			return;
		}
		Rect rect3 = new Rect(rect.x + rect.width, rect.y, num3, rect.height).ContractedBy(num);
		Widgets.BeginGroup(rect3);
		float num8 = 0f;
		float num9 = 0f;
		for (int i = 0; i < previewImages.Count; i++)
		{
			float num10 = num2 - num / 2f;
			GUI.DrawTexture(new Rect(num8, num9, num10, num10), previewImages[i]);
			num8 += num2 + num / 2f;
			if (num8 >= rect3.width)
			{
				num8 = 0f;
				num9 += num2 + num / 2f;
			}
		}
		Widgets.EndGroup();
	}

	public static void DoTranslationInfoRect(Rect outRect)
	{
		if (LanguageDatabase.activeLanguage == LanguageDatabase.defaultLanguage && !DebugSettings.enableTranslationWindowInEnglish)
		{
			return;
		}
		Widgets.DrawWindowBackground(outRect);
		Rect rect = outRect.ContractedBy(8f);
		Widgets.BeginGroup(rect);
		rect = rect.AtZero();
		Rect rect2 = new Rect(5f, rect.height - 25f, rect.width - 10f, 25f);
		rect.height -= 29f;
		Rect rect3 = new Rect(5f, rect.height - 25f, rect.width - 10f, 25f);
		rect.height -= 29f;
		Rect rect4 = new Rect(5f, rect.height - 25f, rect.width - 10f, 25f);
		rect.height -= 29f;
		string text = "";
		foreach (CreditsEntry credit in LanguageDatabase.activeLanguage.info.credits)
		{
			if (credit is CreditRecord_Role creditRecord_Role)
			{
				text = text + creditRecord_Role.creditee + "\n";
			}
		}
		text = text.TrimEndNewlines();
		string label = "TranslationThanks".Translate(text) + "\n\n" + "TranslationHowToContribute".Translate();
		Widgets.LabelScrollable(rect, label, ref translationInfoScrollbarPos, dontConsumeScrollEventsIfNoScrollbar: false, takeScrollbarSpaceEvenIfNoScrollbar: false);
		if (Widgets.ButtonText(rect4, "LearnMore".Translate()))
		{
			Application.OpenURL(TranslationsContributeURL);
		}
		if (Widgets.ButtonText(rect3, "SaveTranslationReport".Translate()))
		{
			LanguageReportGenerator.SaveTranslationReport();
		}
		if (Widgets.ButtonText(rect2, "CleanupTranslationFiles".Translate()))
		{
			TranslationFilesCleaner.CleanupTranslationFiles();
		}
		Widgets.EndGroup();
	}

	private static void DoDevBuildWarningRect(Rect outRect)
	{
		Widgets.DrawWindowBackground(outRect);
		Widgets.Label(outRect.ContractedBy(17f), "DevBuildWarning".Translate());
	}

	private static void InitLearnToPlay()
	{
		Game.ClearCaches();
		Current.Game = new Game();
		Current.Game.InitData = new GameInitData();
		Current.Game.Scenario = ScenarioDefOf.Tutorial.scenario;
		Find.Scenario.PreConfigure();
		Current.Game.storyteller = new Storyteller(StorytellerDefOf.Tutor, DifficultyDefOf.Easy);
		Find.GameInitData.startedFromEntry = true;
		Page next = Current.Game.Scenario.GetFirstConfigPage().next;
		next.prev = null;
		Find.WindowStack.Add(next);
	}

	private static void CloseMainTab()
	{
		if (Current.ProgramState == ProgramState.Playing)
		{
			Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
		}
	}
}
