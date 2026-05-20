using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Dialog_ChooseThingsForNewColony : Window
{
	private int maxColonists;

	private int maxAnimals;

	private int maxRelics;

	private int maxItems;

	private Action<List<Thing>> postAccepted;

	private List<Thing> colonists = new List<Thing>();

	private List<Thing> animals = new List<Thing>();

	private List<Thing> relics = new List<Thing>();

	private List<Thing> items = new List<Thing>();

	private Dictionary<Thing, int> itemArchonexusAllowedStackCount = new Dictionary<Thing, int>();

	private HashSet<Thing> selected = new HashSet<Thing>();

	private int selectedItemCount;

	private Vector2 scrollPosition = Vector2.zero;

	private Action cancel;

	private const float TitleHeight = 40f;

	private const float DescriptionHeight = 25f;

	private const float BottomAreaHeight = 70f;

	private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

	private const float RowHeight = 30f;

	private const float CategoryHeaderHeight = 30f;

	private const float MiscIconSize = 27f;

	private const float CheckboxSize = 27f;

	private const float ItemCountSize = 60f;

	private const float ErrorAndWarningTextWidth = 160f;

	private const float MiscIconOffset = 320f;

	private const float NameOffset = 80f;

	private const float ItemNameWidth = 250f;

	private const float ThingNameWidth = 196f;

	private readonly Comparer<ThingDef> itemComparator = Comparer<ThingDef>.Create((ThingDef td1, ThingDef td2) => TransferableComparer_Archonexus.Compare(td1, td2));

	public override Vector2 InitialSize => new Vector2(540f, 800f);

	public int ColonistCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < colonists.Count; i++)
			{
				if (selected.Contains(colonists[i]))
				{
					num++;
				}
			}
			return num;
		}
	}

	public int SlaveCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < colonists.Count; i++)
			{
				if (selected.Contains(colonists[i]) && colonists[i] is Pawn { IsSlaveOfColony: not false })
				{
					num++;
				}
			}
			return num;
		}
	}

	public int AnimalCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < animals.Count; i++)
			{
				if (selected.Contains(animals[i]))
				{
					num++;
				}
			}
			return num;
		}
	}

	public int RelicCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < relics.Count; i++)
			{
				if (selected.Contains(relics[i]))
				{
					num++;
				}
			}
			return num;
		}
	}

	private AcceptanceReport AcceptanceReport
	{
		get
		{
			int colonistCount = ColonistCount;
			if (colonistCount > maxColonists)
			{
				return "MessageNewColonyMax".Translate(maxColonists, "People".Translate());
			}
			if (colonistCount == 0)
			{
				return "MessageNewColoyRequiresOneColonist".Translate();
			}
			if (AnimalCount > maxAnimals)
			{
				return "MessageNewColonyMax".Translate(maxAnimals, "AnimalsLower".Translate());
			}
			if (RelicCount > maxRelics)
			{
				return "MessageNewColonyMax".Translate(maxRelics, (maxRelics == 1) ? "RelicLower".Translate() : "RelicsLower".Translate());
			}
			if (selectedItemCount > maxItems)
			{
				return "MessageNewColonyMax".Translate(maxItems, "ItemsLower".Translate());
			}
			return AcceptanceReport.WasAccepted;
		}
	}

	public Dialog_ChooseThingsForNewColony(Action<List<Thing>> postAccepted, int maxColonists = 5, int maxAnimals = 5, int maxRelics = 1, int maxItems = 7, Action cancel = null)
	{
		if (ModLister.CheckIdeology("Choose new colony"))
		{
			this.postAccepted = postAccepted;
			this.maxAnimals = maxAnimals;
			this.maxColonists = maxColonists;
			this.maxRelics = maxRelics;
			this.maxItems = maxItems;
			forcePause = true;
			closeOnCancel = true;
			absorbInputAroundWindow = true;
			forceCatchAcceptAndCancelEventEvenIfUnfocused = true;
			preventSave = true;
			this.cancel = cancel;
		}
	}

	public override void PostOpen()
	{
		colonists.Clear();
		animals.Clear();
		relics.Clear();
		items.Clear();
		selected.Clear();
		selectedItemCount = 0;
		itemArchonexusAllowedStackCount.Clear();
		foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction)
		{
			if (!item.IsQuestLodger())
			{
				if (item.IsColonist)
				{
					colonists.Add(item);
				}
				else if (item.IsAnimal)
				{
					animals.Add(item);
				}
			}
		}
		foreach (Precept item2 in Faction.OfPlayer.ideos.PrimaryIdeo.PreceptsListForReading)
		{
			if (item2 is Precept_Relic { RelicInPlayerPossession: not false } precept_Relic)
			{
				relics.Add(precept_Relic.GeneratedRelic);
			}
		}
		Dictionary<ThingDef, List<Thing>> itemDefMap = new Dictionary<ThingDef, List<Thing>>();
		List<Thing> distinctItem = new List<Thing>();
		List<Thing> list = new List<Thing>();
		foreach (TravellingTransporters travellingTransporter in Find.WorldObjects.TravellingTransporters)
		{
			if (!travellingTransporter.IsPlayerControlled)
			{
				continue;
			}
			ThingOwnerUtility.GetAllThingsRecursively(travellingTransporter, list, allowUnreal: false, WealthWatcher.WealthItemsFilter);
			foreach (Thing item3 in list)
			{
				CountThing(item3);
			}
		}
		List<Thing> list2 = new List<Thing>();
		foreach (Caravan caravan in Find.WorldObjects.Caravans)
		{
			if (!caravan.IsPlayerControlled)
			{
				continue;
			}
			ThingOwnerUtility.GetAllThingsRecursively(caravan, list2, allowUnreal: false, WealthWatcher.WealthItemsFilter);
			foreach (Thing item4 in list2)
			{
				CountThing(item4);
			}
		}
		List<Thing> list3 = new List<Thing>();
		foreach (Map map in Find.Maps)
		{
			ThingOwnerUtility.GetAllThingsRecursively(map, list3, allowUnreal: false, WealthWatcher.WealthItemsFilter);
			foreach (Thing item5 in list3)
			{
				CountThing(item5);
			}
		}
		foreach (List<Thing> value in itemDefMap.Values)
		{
			foreach (Thing item6 in value)
			{
				items.Add(item6);
			}
		}
		foreach (Thing item7 in distinctItem)
		{
			items.Add(item7);
		}
		colonists.SortBy((Thing t) => t.Label);
		animals.SortBy((Thing t) => t.Label);
		relics.SortBy((Thing t) => t.Label);
		items = items.OrderByDescending((Thing i) => i.def, itemComparator).ThenBy((Thing i) => i.def.label).ThenByDescending(delegate(Thing i)
		{
			i.TryGetQuality(out var qc);
			return qc;
		})
			.ToList();
		void CountThing(Thing t)
		{
			if (t.def.ArchonexusMaxAllowedCount != 0 && MoveColonyUtility.IsBringableItem(t))
			{
				if (!itemDefMap.ContainsKey(t.def))
				{
					itemDefMap[t.def] = new List<Thing>();
				}
				if (MoveColonyUtility.IsDistinctArchonexusItem(t.def))
				{
					distinctItem.Add(t);
				}
				else
				{
					foreach (Thing item8 in itemDefMap[t.def])
					{
						if (t.CanStackWith(item8))
						{
							itemArchonexusAllowedStackCount[item8] = Mathf.Min(t.def.ArchonexusMaxAllowedCount, t.stackCount + itemArchonexusAllowedStackCount[item8]);
							return;
						}
					}
					itemArchonexusAllowedStackCount[t] = Mathf.Min(t.def.ArchonexusMaxAllowedCount, t.stackCount);
					itemDefMap[t.def].Add(t);
				}
			}
		}
	}

	public override void DoWindowContents(Rect rect)
	{
		Text.Font = GameFont.Medium;
		Widgets.Label(new Rect(0f, 0f, rect.width, 40f), "ChooseThingsForNewColonyTitle".Translate());
		Text.Font = GameFont.Small;
		Widgets.Label(new Rect(0f, 40f, rect.width, 25f), "ChooseThingsForNewColonyDesc".Translate());
		float num = rect.width;
		Rect outRect = new Rect(rect.x, rect.y + 40f + 25f + 10f, rect.width, rect.height - 40f - 25f - 70f);
		float num2 = CalculateSectionHeight(colonists.Count) + CalculateSectionHeight(animals.Count) + CalculateSectionHeight(relics.Count) + CalculateSectionHeight(items.Count);
		if (num2 > outRect.height)
		{
			num -= 16f;
		}
		Rect rect2 = new Rect(0f, 0f, num, num2);
		Widgets.BeginScrollView(outRect, ref scrollPosition, rect2);
		if (colonists.Count > 0)
		{
			DrawThingList(colonists, rect2, "ChoosePeopleDesc".Translate(maxColonists), isItem: false);
			rect2.yMin += CalculateSectionHeight(colonists.Count);
		}
		if (animals.Count > 0)
		{
			DrawThingList(animals, rect2, "ChooseThingsDesc".Translate(maxAnimals, "AnimalsLower".Translate()), isItem: false);
			rect2.yMin += CalculateSectionHeight(animals.Count);
		}
		if (relics.Count > 0)
		{
			TaggedString taggedString = ((maxRelics == 1) ? "RelicLower".Translate() : "RelicsLower".Translate());
			DrawThingList(relics, rect2, "ChooseThingsDesc".Translate(maxRelics, taggedString), isItem: false);
			rect2.yMin += CalculateSectionHeight(relics.Count);
		}
		if (items.Count > 0)
		{
			DrawThingList(items, rect2, "ChooseThingsDesc".Translate(maxItems, "ItemsLower".Translate()), isItem: true);
			rect2.yMin += CalculateSectionHeight(items.Count);
		}
		Widgets.EndScrollView();
		Rect rect3 = new Rect(0f, rect.yMax - 70f, rect.width, 70f);
		Rect rect4 = new Rect(rect3.xMax - BottomButtonSize.x, rect3.yMax - BottomButtonSize.y, BottomButtonSize.x, BottomButtonSize.y);
		AcceptanceReport acceptanceReport = AcceptanceReport;
		if (!acceptanceReport.Accepted)
		{
			Rect rect5 = rect4;
			rect5.x -= 162f;
			rect5.width = 160f;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleRight;
			GUI.color = Color.red;
			Widgets.Label(rect5, acceptanceReport.Reason);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		}
		int slaveCount = SlaveCount;
		bool flag = slaveCount > 0 && slaveCount == ColonistCount;
		if (acceptanceReport.Accepted && flag)
		{
			Rect rect6 = rect4;
			rect6.x -= 162f;
			rect6.width = 160f;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = Color.yellow;
			Widgets.Label(rect6, "ChooseOnlySlavesInfo".Translate());
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		}
		if (Widgets.ButtonText(rect4, "AcceptButton".Translate()))
		{
			if (acceptanceReport.Accepted)
			{
				ConfirmArchonexusSettlementConsequences(selected.ToList(), flag);
			}
			else
			{
				Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
			}
		}
		Rect rect7 = rect4;
		rect7.x = rect3.x;
		if (Widgets.ButtonText(rect7, "Cancel".Translate()))
		{
			if (cancel != null)
			{
				cancel();
			}
			Close();
		}
		static float CalculateSectionHeight(int numberOfThings)
		{
			if (numberOfThings == 0)
			{
				return 0f;
			}
			return 40f + (float)numberOfThings * 30f;
		}
	}

	public override void OnCancelKeyPressed()
	{
		base.OnCancelKeyPressed();
		if (cancel != null)
		{
			cancel();
		}
	}

	private void ConfirmArchonexusSettlementConsequences(List<Thing> selectedList, bool onlySlavesSelected)
	{
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder();
		if (items.Count > selectedItemCount && selectedItemCount < maxItems)
		{
			if (flag)
			{
				stringBuilder.Append("\n\n");
			}
			flag = true;
			stringBuilder.Append("ArchonexusItemsNotChosen".Translate(selectedItemCount, Mathf.Min(items.Count, maxItems)).Colorize(ColoredText.WarningColor));
		}
		List<Thing> abandonedRelicsCarriedByPawns = MoveColonyUtility.GetAbandonedRelicsCarriedByPawns(selectedList);
		if (relics.Count > RelicCount && RelicCount <= 0)
		{
			if (flag)
			{
				stringBuilder.Append("\n\n");
			}
			flag = true;
			stringBuilder.Append("ArchonexusRelicNotChosen".Translate().Colorize(ColoredText.WarningColor));
		}
		else if (abandonedRelicsCarriedByPawns.Count > 0)
		{
			if (flag)
			{
				stringBuilder.Append("\n\n");
			}
			flag = true;
			stringBuilder.Append("RemoveEquippedRelicsDescription".Translate().Colorize(ColoredText.WarningColor) + "\n\n" + abandonedRelicsCarriedByPawns.Select((Thing r) => r.Label.Colorize(ColoredText.WarningColor)).ToLineList(" - ".Colorize(ColoredText.WarningColor)));
		}
		WorldObject escapeShip = Find.WorldObjects.AllWorldObjects.FirstOrDefault((WorldObject wo) => wo.def == WorldObjectDefOf.EscapeShip);
		if (escapeShip != null)
		{
			if (flag)
			{
				stringBuilder.Append("\n\n");
			}
			flag = true;
			stringBuilder.Append("ShipEscapeWillEndDescription".Translate());
		}
		if (flag)
		{
			stringBuilder.Append("\n\n");
		}
		stringBuilder.Append("WantToProceed".Translate());
		Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(stringBuilder.ToString(), CloseAction, destructive: false, "ConfirmDecisionsTitle".Translate()));
		void CloseAction()
		{
			Close();
			if (onlySlavesSelected)
			{
				foreach (Thing selected in selectedList)
				{
					if (selected is Pawn { IsSlaveOfColony: not false } pawn)
					{
						pawn.guest.SetGuestStatus(null);
					}
				}
			}
			foreach (Thing selected2 in selectedList)
			{
				if (itemArchonexusAllowedStackCount.ContainsKey(selected2))
				{
					selected2.stackCount = itemArchonexusAllowedStackCount[selected2];
				}
				if (itemArchonexusAllowedStackCount.ContainsKey(selected2) || items.Contains(selected2))
				{
					selected2.HitPoints = selected2.MaxHitPoints;
				}
			}
			itemArchonexusAllowedStackCount.Clear();
			if (escapeShip != null)
			{
				escapeShip.Destroy();
			}
			postAccepted(selectedList);
		}
	}

	private void DrawThingList(List<Thing> things, Rect rect, string title, bool isItem)
	{
		Text.Anchor = TextAnchor.MiddleLeft;
		Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), title);
		Widgets.DrawLineHorizontal(rect.x, rect.y + 21f, Text.CalcSize(title).x);
		Text.Anchor = TextAnchor.UpperLeft;
		int count = things.Count;
		float num = rect.y + 30f;
		for (int i = 0; i < count; i++)
		{
			Rect rect2 = new Rect(rect.x, num + (float)i * 30f, rect.width, 30f);
			DoRow(rect2, things[i], i, isItem);
		}
	}

	private void DoRow(Rect rect, Thing t, int index, bool isItem)
	{
		if (index % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect);
		}
		Widgets.BeginGroup(rect);
		float num = 4f;
		Pawn p = t as Pawn;
		string label = t.LabelCap;
		Pawn pawn = p;
		if (pawn != null && pawn.RaceProps?.Animal == true)
		{
			label = p.LabelCap + $" ({p.GetGenderLabel()}, {Mathf.FloorToInt(p.ageTracker.AgeBiologicalYearsFloat)})";
		}
		else if (isItem)
		{
			label = GenLabel.ThingLabel(t, 1, includeHp: false).CapitalizeFirst();
		}
		float num2 = (isItem ? 250f : 196f);
		Rect rect2 = new Rect(0f, 0f, 80f + num2, rect.height);
		Color color = ((p != null && p.IsSlave) ? PawnNameColorUtility.PawnNameColorOf(p) : Color.white);
		if (Mouse.IsOver(rect2))
		{
			Widgets.DrawHighlight(rect2);
			TooltipHandler.TipRegion(rect2, new TipSignal(delegate
			{
				string text = label;
				string text2 = ((p != null) ? (p.MainDesc(writeFaction: true) + "\n\n" + p.def.description) : t.DescriptionDetailed);
				if (!text2.NullOrEmpty())
				{
					text = text + ": " + text2 + TransferableUIUtility.ContentSourceDescription(t);
				}
				return text;
			}, t.GetHashCode()));
		}
		Widgets.InfoCardButton(0f, 0f, t);
		Widgets.ThingIcon(new Rect(36f, 0f, 27f, 27f), t);
		Text.Anchor = TextAnchor.MiddleLeft;
		Rect rect3 = new Rect(80f, 0f, num2, rect2.height);
		Text.WordWrap = false;
		GUI.color = color;
		Widgets.Label(rect3, label);
		GUI.color = Color.white;
		Text.WordWrap = true;
		num += 320f;
		if (p != null && p.Ideo != null && !Find.IdeoManager.classicMode)
		{
			Rect rect4 = new Rect(num, 0f, 27f, rect.height);
			Widgets.DrawHighlightIfMouseover(rect4);
			p.Ideo.DrawIcon(rect4);
			TooltipHandler.TipRegion(rect4, p.Ideo.name);
		}
		else
		{
			Pawn pawn2 = p;
			if (pawn2 != null && pawn2.RaceProps?.Animal == true)
			{
				if (p.health.hediffSet.HasHediff(HediffDefOf.Pregnant, mustBeVisible: true))
				{
					TransferableUIUtility.DrawPregnancyIcon(p, new Rect(num, 2f, 23f, rect.height - 4f));
					num += 27f;
				}
				if (p.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond) != null)
				{
					TransferableUIUtility.DrawBondedIcon(p, new Rect(num, 2f, 23f, rect.height - 4f));
					num += 27f;
				}
			}
		}
		num += 27f;
		if (p != null && p.IsSlave)
		{
			Rect rect5 = new Rect(num, 0f, 27f, rect.height);
			Widgets.DrawHighlightIfMouseover(rect5);
			GUI.DrawTexture(rect5, p.guest.GetIcon());
			TooltipHandler.TipRegion(rect5, p.guest.GetLabel());
			num += 27f;
		}
		if (isItem)
		{
			Rect rect6 = new Rect(rect.width - 27f - 4f - 60f - 10f, 1f, 60f, rect.height);
			if (Mouse.IsOver(rect6))
			{
				Widgets.DrawHighlight(rect6);
				TooltipHandler.TipRegion(rect6, new TipSignal(() => "ItemQuantityTooltip".Translate(label, t.def.ArchonexusMaxAllowedCount), t.GetHashCode()));
			}
			GUI.color = Color.white;
			Text.WordWrap = false;
			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleRight;
			int num3 = (MoveColonyUtility.IsDistinctArchonexusItem(t.def) ? t.stackCount : itemArchonexusAllowedStackCount[t]);
			Widgets.Label(rect6.LeftPartPixels(rect6.width - 10f), num3.ToString());
			Text.Anchor = anchor;
			Text.WordWrap = true;
		}
		Rect rect7 = new Rect(rect.width - 27f - 4f, 1f, 27f, 27f);
		Rect butRect = new Rect(rect7);
		butRect.xMin -= 6f;
		butRect.width += 12f;
		butRect.yMin -= 1.5f;
		butRect.height += 3f;
		if (Widgets.ButtonInvisible(butRect))
		{
			if (selected.Contains(t))
			{
				selected.Remove(t);
				if (isItem)
				{
					selectedItemCount--;
				}
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
			}
			else
			{
				selected.Add(t);
				if (isItem)
				{
					selectedItemCount++;
				}
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
			}
		}
		GUI.DrawTexture(rect7.ContractedBy(1f), selected.Contains(t) ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
		GenUI.ResetLabelAlign();
		Widgets.EndGroup();
	}
}
