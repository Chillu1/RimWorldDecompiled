using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class Dialog_InfoCard : Window
	{
		private enum InfoCardTab : byte
		{
			Stats,
			Character,
			Health,
			Records
		}

		public struct Hyperlink
		{
			public Thing thing;

			public ThingDef stuff;

			public Def def;

			public WorldObject worldObject;

			public RoyalTitleDef titleDef;

			public Faction faction;

			public int selectedStatIndex;

			public string Label
			{
				get
				{
					string result = null;
					if (worldObject != null)
					{
						result = worldObject.Label;
					}
					else if (def != null && def is ThingDef && stuff != null)
					{
						result = (def as ThingDef).label;
					}
					else if (def != null)
					{
						result = def.label;
					}
					else if (thing != null)
					{
						result = thing.Label;
					}
					else if (titleDef != null)
					{
						result = titleDef.GetLabelCapForBothGenders();
					}
					return result;
				}
			}

			public Hyperlink(Dialog_InfoCard infoCard, int statIndex = -1)
			{
				def = infoCard.def;
				thing = infoCard.thing;
				stuff = infoCard.stuff;
				worldObject = infoCard.worldObject;
				titleDef = infoCard.titleDef;
				faction = infoCard.faction;
				selectedStatIndex = statIndex;
			}

			public Hyperlink(Def def, int statIndex = -1)
			{
				this.def = def;
				thing = null;
				stuff = null;
				worldObject = null;
				titleDef = null;
				faction = null;
				selectedStatIndex = statIndex;
			}

			public Hyperlink(RoyalTitleDef titleDef, Faction faction, int statIndex = -1)
			{
				def = null;
				thing = null;
				stuff = null;
				worldObject = null;
				this.titleDef = titleDef;
				this.faction = faction;
				selectedStatIndex = statIndex;
			}

			public Hyperlink(Thing thing, int statIndex = -1)
			{
				this.thing = thing;
				stuff = null;
				def = null;
				worldObject = null;
				titleDef = null;
				faction = null;
				selectedStatIndex = statIndex;
			}

			public void OpenDialog()
			{
				Dialog_InfoCard dialog_InfoCard = null;
				if (def == null && thing == null && worldObject == null && titleDef == null)
				{
					dialog_InfoCard = Find.WindowStack.WindowOfType<Dialog_InfoCard>();
				}
				else
				{
					PushCurrentToHistoryAndClose();
					if (worldObject != null)
					{
						dialog_InfoCard = new Dialog_InfoCard(worldObject);
					}
					else if (def != null && def is ThingDef && (stuff != null || GenStuff.DefaultStuffFor((ThingDef)def) != null))
					{
						dialog_InfoCard = new Dialog_InfoCard(def as ThingDef, stuff ?? GenStuff.DefaultStuffFor((ThingDef)def));
					}
					else if (def != null)
					{
						dialog_InfoCard = new Dialog_InfoCard(def);
					}
					else if (thing != null)
					{
						dialog_InfoCard = new Dialog_InfoCard(thing);
					}
					else if (titleDef != null)
					{
						dialog_InfoCard = new Dialog_InfoCard(titleDef, faction);
					}
				}
				if (dialog_InfoCard == null)
				{
					return;
				}
				int localSelectedStatIndex = selectedStatIndex;
				if (selectedStatIndex >= 0)
				{
					dialog_InfoCard.executeAfterFillCardOnce = delegate
					{
						StatsReportUtility.SelectEntry(localSelectedStatIndex);
					};
				}
				Find.WindowStack.Add(dialog_InfoCard);
			}
		}

		private const float ShowMaterialsButtonWidth = 200f;

		private const float ShowMaterialsButtonHeight = 40f;

		private const float ShowMaterialsMargin = 16f;

		private Action executeAfterFillCardOnce;

		private static List<Hyperlink> history = new List<Hyperlink>();

		private Thing thing;

		private ThingDef stuff;

		private Def def;

		private WorldObject worldObject;

		private RoyalTitleDef titleDef;

		private Faction faction;

		private InfoCardTab tab;

		private Def Def
		{
			get
			{
				if (thing != null)
				{
					return thing.def;
				}
				if (worldObject != null)
				{
					return worldObject.def;
				}
				return def;
			}
		}

		private Pawn ThingPawn => thing as Pawn;

		public override Vector2 InitialSize => new Vector2(950f, 760f);

		protected override float Margin => 0f;

		public static IEnumerable<Hyperlink> DefsToHyperlinks(IEnumerable<ThingDef> defs)
		{
			return defs?.Select((ThingDef def) => new Hyperlink(def));
		}

		public static IEnumerable<Hyperlink> DefsToHyperlinks(IEnumerable<DefHyperlink> links)
		{
			return links?.Select((DefHyperlink link) => new Hyperlink(link.def));
		}

		public static IEnumerable<Hyperlink> TitleDefsToHyperlinks(IEnumerable<DefHyperlink> links)
		{
			return links?.Select((DefHyperlink link) => new Hyperlink((RoyalTitleDef)link.def, link.faction));
		}

		public static void PushCurrentToHistoryAndClose()
		{
			Dialog_InfoCard dialog_InfoCard = Find.WindowStack.WindowOfType<Dialog_InfoCard>();
			if (dialog_InfoCard != null)
			{
				history.Add(new Hyperlink(dialog_InfoCard, StatsReportUtility.SelectedStatIndex));
				Find.WindowStack.TryRemove(dialog_InfoCard, doCloseSound: false);
			}
		}

		public Dialog_InfoCard(Thing thing)
		{
			this.thing = thing;
			tab = InfoCardTab.Stats;
			Setup();
		}

		public Dialog_InfoCard(Def onlyDef)
		{
			def = onlyDef;
			Setup();
		}

		public Dialog_InfoCard(ThingDef thingDef, ThingDef stuff)
		{
			def = thingDef;
			this.stuff = stuff;
			Setup();
		}

		public Dialog_InfoCard(RoyalTitleDef titleDef, Faction faction)
		{
			this.titleDef = titleDef;
			this.faction = faction;
			Setup();
		}

		public Dialog_InfoCard(Faction faction)
		{
			this.faction = faction;
			Setup();
		}

		public Dialog_InfoCard(WorldObject worldObject)
		{
			this.worldObject = worldObject;
			Setup();
		}

		public override void Close(bool doCloseSound = true)
		{
			base.Close(doCloseSound);
			history.Clear();
		}

		private void Setup()
		{
			forcePause = true;
			doCloseButton = true;
			doCloseX = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
			soundAppear = SoundDefOf.InfoCard_Open;
			soundClose = SoundDefOf.InfoCard_Close;
			StatsReportUtility.Reset();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InfoCard, KnowledgeAmount.Total);
		}

		private static bool ShowMaterialsButton(Rect containerRect, bool withBackButtonOffset = false)
		{
			float num = containerRect.x + containerRect.width - 14f - 200f - 16f;
			if (withBackButtonOffset)
			{
				num -= 136f;
			}
			return Widgets.ButtonText(new Rect(num, containerRect.y + 18f, 200f, 40f), "ShowMaterials".Translate());
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(inRect);
			rect = rect.ContractedBy(18f);
			rect.height = 34f;
			rect.x += 34f;
			Text.Font = GameFont.Medium;
			Widgets.Label(rect, GetTitle());
			Rect rect2 = new Rect(inRect.x + 9f, rect.y, 34f, 34f);
			if (thing != null)
			{
				Widgets.ThingIcon(rect2, thing);
			}
			else
			{
				Widgets.DefIcon(rect2, def, stuff, 1f, drawPlaceholder: true);
			}
			Rect rect3 = new Rect(inRect);
			rect3.yMin = rect.yMax;
			rect3.yMax -= 38f;
			Rect rect4 = rect3;
			List<TabRecord> list = new List<TabRecord>();
			TabRecord item = new TabRecord("TabStats".Translate(), delegate
			{
				tab = InfoCardTab.Stats;
			}, tab == InfoCardTab.Stats);
			list.Add(item);
			if (ThingPawn != null)
			{
				if (ThingPawn.RaceProps.Humanlike)
				{
					TabRecord item2 = new TabRecord("TabCharacter".Translate(), delegate
					{
						tab = InfoCardTab.Character;
					}, tab == InfoCardTab.Character);
					list.Add(item2);
				}
				TabRecord item3 = new TabRecord("TabHealth".Translate(), delegate
				{
					tab = InfoCardTab.Health;
				}, tab == InfoCardTab.Health);
				list.Add(item3);
				TabRecord item4 = new TabRecord("TabRecords".Translate(), delegate
				{
					tab = InfoCardTab.Records;
				}, tab == InfoCardTab.Records);
				list.Add(item4);
			}
			if (list.Count > 1)
			{
				rect4.yMin += 45f;
				TabDrawer.DrawTabs(rect4, list);
			}
			FillCard(rect4.ContractedBy(18f));
			if (def != null && def is BuildableDef)
			{
				IEnumerable<ThingDef> enumerable = GenStuff.AllowedStuffsFor((BuildableDef)def);
				if (enumerable.Count() > 0 && ShowMaterialsButton(inRect, history.Count > 0))
				{
					List<FloatMenuOption> list2 = new List<FloatMenuOption>();
					foreach (ThingDef item5 in enumerable)
					{
						ThingDef localStuff = item5;
						list2.Add(new FloatMenuOption(item5.label, delegate
						{
							stuff = localStuff;
							Setup();
						}, item5));
					}
					Find.WindowStack.Add(new FloatMenu(list2));
				}
			}
			if (history.Count > 0 && Widgets.BackButtonFor(inRect))
			{
				Hyperlink hyperlink = history[history.Count - 1];
				history.RemoveAt(history.Count - 1);
				Find.WindowStack.TryRemove(typeof(Dialog_InfoCard), doCloseSound: false);
				hyperlink.OpenDialog();
			}
		}

		protected void FillCard(Rect cardRect)
		{
			if (tab == InfoCardTab.Stats)
			{
				if (thing != null)
				{
					Thing innerThing = thing;
					MinifiedThing minifiedThing = thing as MinifiedThing;
					if (minifiedThing != null)
					{
						innerThing = minifiedThing.InnerThing;
					}
					StatsReportUtility.DrawStatsReport(cardRect, innerThing);
				}
				else if (titleDef != null)
				{
					StatsReportUtility.DrawStatsReport(cardRect, titleDef, faction);
				}
				else if (faction != null)
				{
					StatsReportUtility.DrawStatsReport(cardRect, faction);
				}
				else if (worldObject != null)
				{
					StatsReportUtility.DrawStatsReport(cardRect, worldObject);
				}
				else if (def is AbilityDef)
				{
					StatsReportUtility.DrawStatsReport(cardRect, (AbilityDef)def);
				}
				else
				{
					StatsReportUtility.DrawStatsReport(cardRect, def, stuff);
				}
			}
			else if (tab == InfoCardTab.Character)
			{
				CharacterCardUtility.DrawCharacterCard(cardRect, (Pawn)thing);
			}
			else if (tab == InfoCardTab.Health)
			{
				cardRect.yMin += 8f;
				HealthCardUtility.DrawPawnHealthCard(cardRect, (Pawn)thing, allowOperations: false, showBloodLoss: false, null);
			}
			else if (tab == InfoCardTab.Records)
			{
				RecordsCardUtility.DrawRecordsCard(cardRect, (Pawn)thing);
			}
			if (executeAfterFillCardOnce != null)
			{
				executeAfterFillCardOnce();
				executeAfterFillCardOnce = null;
			}
		}

		private string GetTitle()
		{
			if (thing != null)
			{
				return thing.LabelCapNoCount;
			}
			if (worldObject != null)
			{
				return worldObject.LabelCap;
			}
			ThingDef thingDef = Def as ThingDef;
			if (thingDef != null)
			{
				return GenLabel.ThingLabel(thingDef, stuff).CapitalizeFirst();
			}
			AbilityDef abilityDef = Def as AbilityDef;
			if (abilityDef != null)
			{
				return abilityDef.LabelCap;
			}
			if (titleDef != null)
			{
				return titleDef.GetLabelCapForBothGenders();
			}
			if (faction != null)
			{
				return faction.Name;
			}
			return Def.LabelCap;
		}
	}
}
