using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class MainMenuDrawer
	{
		private static bool anyMapFiles;

		private static Vector2 translationInfoScrollbarPos;

		private const float PlayRectWidth = 170f;

		private const float WebRectWidth = 145f;

		private const float RightEdgeMargin = 50f;

		private static readonly Vector2 PaneSize = new Vector2(450f, 450f);

		private static readonly Vector2 TitleSize = new Vector2(1032f, 146f);

		private static readonly Texture2D TexTitle = ContentFinder<Texture2D>.Get("UI/HeroArt/GameTitle");

		private const float TitleShift = 50f;

		private static readonly Vector2 LudeonLogoSize = new Vector2(200f, 58f);

		private static readonly Texture2D TexLudeonLogo = ContentFinder<Texture2D>.Get("UI/HeroArt/LudeonLogoSmall");

		private static readonly string TranslationsContributeURL = "https://rimworldgame.com/helptranslate";

		private static readonly Color PurchasedColor = new Color(1f, 1f, 1f, 0.35f);

		private static UI_BackgroundMain BackgroundMain => (UI_BackgroundMain)UIMenuBackgroundManager.background;

		public static void Init()
		{
			PlayerKnowledgeDatabase.Save();
			ShipCountdown.CancelCountdown();
			anyMapFiles = GenFilePaths.AllSavedGameFiles.Any();
			foreach (ExpansionDef allExpansion in ModLister.AllExpansions)
			{
				if (allExpansion.Status != ExpansionStatus.NotInstalled && !allExpansion.isCore)
				{
					BackgroundMain.overrideBGImage = allExpansion.BackgroundImage;
					break;
				}
			}
		}

		public static void MainMenuOnGUI()
		{
			VersionControl.DrawInfoInCorner();
			Rect rect = new Rect((float)(UI.screenWidth / 2) - PaneSize.x / 2f, (float)(UI.screenHeight / 2) - PaneSize.y / 2f + 50f, PaneSize.x, PaneSize.y);
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
		}

		public static void DoMainMenuControls(Rect rect, bool anyMapFiles)
		{
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 0f, 170f, rect.height);
			Rect rect3 = new Rect(rect2.xMax + 17f, 0f, 145f, rect.height);
			Text.Font = GameFont.Small;
			List<ListableOption> list = new List<ListableOption>();
			if (Current.ProgramState == ProgramState.Entry)
			{
				string label = "Tutorial".CanTranslate() ? ((string)"Tutorial".Translate()) : ((string)"LearnToPlay".Translate());
				list.Add(new ListableOption(label, delegate
				{
					InitLearnToPlay();
				}));
				list.Add(new ListableOption("NewColony".Translate(), delegate
				{
					Find.WindowStack.Add(new Page_SelectScenario());
				}));
			}
			if (Current.ProgramState == ProgramState.Playing && !Current.Game.Info.permadeathMode)
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
					Find.WindowStack.Add(new Dialog_MessageBox(Find.Scenario.GetFullInformationText(), null, null, null, null, Find.Scenario.name));
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
					item = new ListableOption("SaveTranslationReport".Translate(), delegate
					{
						LanguageReportGenerator.SaveTranslationReport();
					});
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
				if (Current.Game.Info.permadeathMode)
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
							LongEventHandler.ExecuteWhenFinished(delegate
							{
								Root.Shutdown();
							});
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
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmQuit".Translate(), delegate
							{
								GenScene.GoToMainMenu();
							}, destructive: true));
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
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmQuit".Translate(), delegate
							{
								Root.Shutdown();
							}, destructive: true));
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
				item = new ListableOption("QuitToOS".Translate(), delegate
				{
					Root.Shutdown();
				});
				list.Add(item);
			}
			OptionListingUtility.DrawOptionListing(rect2, list);
			Text.Font = GameFont.Small;
			List<ListableOption> list2 = new List<ListableOption>();
			ListableOption item2 = new ListableOption_WebLink("FictionPrimer".Translate(), "https://rimworldgame.com/backstory", TexButton.IconBlog);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("LudeonBlog".Translate(), "https://ludeon.com/blog", TexButton.IconBlog);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("Forums".Translate(), "https://ludeon.com/forums", TexButton.IconForums);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("OfficialWiki".Translate(), "https://rimworldwiki.com", TexButton.IconBlog);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("TynansTwitter".Translate(), "https://twitter.com/TynanSylvester", TexButton.IconTwitter);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("TynansDesignBook".Translate(), "https://tynansylvester.com/book", TexButton.IconBook);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("HelpTranslate".Translate(), TranslationsContributeURL, TexButton.IconForums);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("BuySoundtrack".Translate(), "http://www.lasgameaudio.co.uk/#!store/t04fw", TexButton.IconSoundtrack);
			list2.Add(item2);
			float num = OptionListingUtility.DrawOptionListing(rect3, list2);
			GUI.BeginGroup(rect3);
			if (Current.ProgramState == ProgramState.Entry && Widgets.ButtonText(new Rect(0f, num + 10f, rect3.width, 50f), LanguageDatabase.activeLanguage.FriendlyNameNative))
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
			GUI.EndGroup();
			GUI.EndGroup();
		}

		public static void DoExpansionIcons()
		{
			List<ExpansionDef> allExpansions = ModLister.AllExpansions;
			int num = -1;
			int num2 = 64;
			int num3 = allExpansions.Count((ExpansionDef e) => !e.isCore);
			int num4 = num2 / 2 + num2 * num3 + (num3 - 1) * 8;
			int num5 = num2 + num2 / 2;
			Rect rect = new Rect(8f, UI.screenHeight - num5 - 8, num4, num5);
			Widgets.DrawWindowBackground(rect);
			GUI.BeginGroup(rect.ContractedBy((rect.height - (float)num2) / 2f));
			float num6 = 0f;
			for (int i = 0; i < allExpansions.Count; i++)
			{
				if (allExpansions[i].isCore)
				{
					continue;
				}
				Rect rect2 = new Rect(num6, 0f, num2, num2);
				num6 += (float)num2;
				if (Widgets.ButtonImage(rect2, allExpansions[i].Icon, (allExpansions[i].Status != ExpansionStatus.NotInstalled) ? Color.white : PurchasedColor) && !allExpansions[i].StoreURL.NullOrEmpty())
				{
					SteamUtility.OpenUrl(allExpansions[i].StoreURL);
				}
				GUI.color = Color.white;
				if (Mouse.IsOver(rect2))
				{
					if (allExpansions[i].Status == ExpansionStatus.NotInstalled)
					{
						BackgroundMain.SetOverlayImage(allExpansions[i].BackgroundImage);
					}
					num = i;
				}
			}
			GUI.EndGroup();
			if (num < 0)
			{
				BackgroundMain.FadeOut();
			}
			else
			{
				DoExpansionInfo(num, rect.y - 8f);
			}
		}

		private static void DoExpansionInfo(int index, float yOffset)
		{
			ExpansionDef expansionDef = ModLister.AllExpansions[index];
			float num = 500f;
			float num2 = 16f;
			Text.Font = GameFont.Medium;
			float num3 = Text.CalcHeight(expansionDef.label, num - num2 * 2f);
			Text.Font = GameFont.Small;
			string text = "ClickForMoreInfo".Translate();
			float num4 = Text.CalcHeight(text, num - num2 * 2f);
			float num5 = Text.CalcHeight(expansionDef.description, num - num2 * 2f);
			float num6 = num3 + num4 + num2 + num5 + num2 * 2f;
			Rect rect = new Rect(8f, yOffset - num6, num, num6);
			Widgets.DrawWindowBackground(rect);
			Rect position = rect.ContractedBy(num2);
			GUI.BeginGroup(position);
			float num7 = 0f;
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.Label(new Rect(0f, num7, position.width, num3), new GUIContent(" " + expansionDef.label, expansionDef.Icon));
			Text.Font = GameFont.Small;
			num7 += num3;
			GUI.color = Color.grey;
			Widgets.Label(new Rect(0f, num7, position.width, num4), text);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			num7 += num4 + num2;
			Widgets.Label(new Rect(0f, num7, position.width, position.height - num7), expansionDef.description);
			GUI.EndGroup();
		}

		public static void DoTranslationInfoRect(Rect outRect)
		{
			if (LanguageDatabase.activeLanguage == LanguageDatabase.defaultLanguage)
			{
				return;
			}
			Widgets.DrawWindowBackground(outRect);
			Rect rect = outRect.ContractedBy(8f);
			GUI.BeginGroup(rect);
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
				CreditRecord_Role creditRecord_Role = credit as CreditRecord_Role;
				if (creditRecord_Role != null)
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
			GUI.EndGroup();
		}

		private static void DoDevBuildWarningRect(Rect outRect)
		{
			Widgets.DrawWindowBackground(outRect);
			Widgets.Label(outRect.ContractedBy(17f), "DevBuildWarning".Translate());
		}

		private static void InitLearnToPlay()
		{
			Current.Game = new Game();
			Current.Game.InitData = new GameInitData();
			Current.Game.Scenario = ScenarioDefOf.Tutorial.scenario;
			Find.Scenario.PreConfigure();
			Current.Game.storyteller = new Storyteller(StorytellerDefOf.Tutor, DifficultyDefOf.Easy);
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
}
