using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class TransferableOneWayWidget
{
	private struct Section
	{
		public string title;

		public IEnumerable<TransferableOneWay> transferables;

		public List<TransferableOneWay> cachedTransferables;
	}

	private List<Section> sections = new List<Section>();

	private string sourceLabel;

	private string destinationLabel;

	private string sourceCountDesc;

	private bool drawMass;

	private IgnorePawnsInventoryMode ignorePawnInventoryMass = IgnorePawnsInventoryMode.DontIgnore;

	private bool includePawnsMassInMassUsage;

	private Func<float> availableMassGetter;

	public float extraHeaderSpace;

	private bool ignoreSpawnedCorpseGearAndInventoryMass;

	private PlanetTile tile;

	private bool drawMarketValue;

	private bool drawEquippedWeapon;

	private bool drawNutritionEatenPerDay;

	private bool drawMechEnergy;

	private bool drawItemNutrition;

	private bool drawForagedFoodPerDay;

	private bool drawDaysUntilRot;

	private bool playerPawnsReadOnly;

	public bool readOnly;

	public bool drawIdeo;

	public bool drawXenotype;

	private bool transferablesCached;

	private Vector2 scrollPosition;

	private QuickSearchWidget quickSearchWidget = new QuickSearchWidget();

	private TransferableSorterDef sorter1;

	private TransferableSorterDef sorter2;

	private Dictionary<TransferableOneWay, int> cachedTicksUntilRot = new Dictionary<TransferableOneWay, int>();

	private static List<TransferableCountToTransferStoppingPoint> stoppingPoints = new List<TransferableCountToTransferStoppingPoint>();

	public const float TopAreaHeight = 37f;

	protected readonly Vector2 AcceptButtonSize = new Vector2(160f, 40f);

	protected readonly Vector2 OtherBottomButtonSize = new Vector2(160f, 40f);

	private const float ColumnWidth = 120f;

	private const float FirstTransferableY = 6f;

	private const float RowInterval = 30f;

	public const float CountColumnWidth = 75f;

	public const float AdjustColumnWidth = 240f;

	public const float MassColumnWidth = 100f;

	public static readonly Color ItemMassColor = new Color(0.7f, 0.7f, 0.7f);

	private const float MarketValueColumnWidth = 100f;

	private const float ExtraSpaceAfterSectionTitle = 5f;

	private const float DaysUntilRotColumnWidth = 75f;

	private const float NutritionEatenPerDayColumnWidth = 75f;

	private const float ItemNutritionColumnWidth = 75f;

	private const float ForagedFoodPerDayColumnWidth = 75f;

	private const float GrazeabilityInnerColumnWidth = 40f;

	private const float MiscIconSize = 30f;

	public const float TopAreaWidth = 515f;

	private static readonly Texture2D CanGrazeIcon = ContentFinder<Texture2D>.Get("UI/Icons/CanGraze");

	public float TotalNumbersColumnsWidths
	{
		get
		{
			float num = 315f;
			if (drawMass)
			{
				num += 100f;
			}
			if (drawMarketValue)
			{
				num += 100f;
			}
			if (drawDaysUntilRot)
			{
				num += 75f;
			}
			if (drawItemNutrition)
			{
				num += 75f;
			}
			if (drawNutritionEatenPerDay || drawMechEnergy)
			{
				num += 75f;
			}
			if (drawForagedFoodPerDay)
			{
				num += 75f;
			}
			return num;
		}
	}

	private bool AnyTransferable
	{
		get
		{
			if (!transferablesCached)
			{
				CacheTransferables();
			}
			for (int i = 0; i < sections.Count; i++)
			{
				if (sections[i].cachedTransferables.Any())
				{
					return true;
				}
			}
			return false;
		}
	}

	public TransferableOneWayWidget(IEnumerable<TransferableOneWay> transferables, string sourceLabel, string destinationLabel, string sourceCountDesc, bool drawMass = false, IgnorePawnsInventoryMode ignorePawnInventoryMass = IgnorePawnsInventoryMode.DontIgnore, bool includePawnsMassInMassUsage = false, Func<float> availableMassGetter = null, float extraHeaderSpace = 0f, bool ignoreSpawnedCorpseGearAndInventoryMass = false, PlanetTile? tile = null, bool drawMarketValue = false, bool drawEquippedWeapon = false, bool drawNutritionEatenPerDay = false, bool drawMechEnergy = false, bool drawItemNutrition = false, bool drawForagedFoodPerDay = false, bool drawDaysUntilRot = false, bool playerPawnsReadOnly = false, bool drawIdeo = false, bool drawXenotype = false)
	{
		if (transferables != null)
		{
			AddSection(null, transferables);
		}
		this.sourceLabel = sourceLabel;
		this.destinationLabel = destinationLabel;
		this.sourceCountDesc = sourceCountDesc;
		this.drawMass = drawMass;
		this.ignorePawnInventoryMass = ignorePawnInventoryMass;
		this.includePawnsMassInMassUsage = includePawnsMassInMassUsage;
		this.availableMassGetter = availableMassGetter;
		this.extraHeaderSpace = extraHeaderSpace;
		this.ignoreSpawnedCorpseGearAndInventoryMass = ignoreSpawnedCorpseGearAndInventoryMass;
		this.tile = tile ?? PlanetTile.Invalid;
		this.drawMarketValue = drawMarketValue;
		this.drawEquippedWeapon = drawEquippedWeapon;
		this.drawNutritionEatenPerDay = drawNutritionEatenPerDay;
		this.drawMechEnergy = drawMechEnergy;
		this.drawItemNutrition = drawItemNutrition;
		this.drawForagedFoodPerDay = drawForagedFoodPerDay;
		this.drawDaysUntilRot = drawDaysUntilRot;
		this.playerPawnsReadOnly = playerPawnsReadOnly;
		this.drawIdeo = drawIdeo;
		this.drawXenotype = drawXenotype;
		if (drawIdeo && Find.IdeoManager.classicMode)
		{
			drawIdeo = false;
		}
		sorter1 = TransferableSorterDefOf.Category;
		sorter2 = TransferableSorterDefOf.MarketValue;
	}

	public void AddSection(string title, IEnumerable<TransferableOneWay> transferables)
	{
		Section item = new Section
		{
			title = title,
			transferables = transferables,
			cachedTransferables = new List<TransferableOneWay>()
		};
		sections.Add(item);
		transferablesCached = false;
	}

	private void CacheTransferables()
	{
		transferablesCached = true;
		for (int i = 0; i < sections.Count; i++)
		{
			List<TransferableOneWay> cachedTransferables = sections[i].cachedTransferables;
			cachedTransferables.Clear();
			cachedTransferables.AddRange(sections[i].transferables.Where((TransferableOneWay tr) => quickSearchWidget.filter.Matches(tr.Label)).OrderBy((TransferableOneWay tr) => tr, sorter1.Comparer).ThenBy((TransferableOneWay tr) => tr, sorter2.Comparer)
				.ThenBy((TransferableOneWay tr) => TransferableUIUtility.DefaultListOrderPriority(tr))
				.ToList());
		}
	}

	public void OnGUI(Rect inRect)
	{
		OnGUI(inRect, out var _);
	}

	public void OnGUI(Rect inRect, out bool anythingChanged)
	{
		if (!transferablesCached)
		{
			CacheTransferables();
		}
		TransferableUIUtility.DoTransferableSorters(sorter1, sorter2, delegate(TransferableSorterDef x)
		{
			sorter1 = x;
			CacheTransferables();
		}, delegate(TransferableSorterDef x)
		{
			sorter2 = x;
			CacheTransferables();
		});
		quickSearchWidget.noResultsMatched = !AnyTransferable;
		TransferableUIUtility.DoTransferableSearcher(quickSearchWidget, CacheTransferables);
		if (!sourceLabel.NullOrEmpty() || !destinationLabel.NullOrEmpty())
		{
			float num = inRect.width - 515f;
			Rect rect = new Rect(inRect.x + num, inRect.y, inRect.width - num, 37f);
			Widgets.BeginGroup(rect);
			Text.Font = GameFont.Medium;
			if (!sourceLabel.NullOrEmpty())
			{
				Rect rect2 = new Rect(0f, 0f, rect.width / 2f, rect.height);
				Text.Anchor = TextAnchor.UpperLeft;
				Widgets.Label(rect2, sourceLabel);
			}
			if (!destinationLabel.NullOrEmpty())
			{
				Rect rect3 = new Rect(rect.width / 2f, 0f, rect.width / 2f, rect.height);
				Text.Anchor = TextAnchor.UpperRight;
				Widgets.Label(rect3, destinationLabel);
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.EndGroup();
		}
		Rect mainRect = new Rect(inRect.x, inRect.y + 37f + extraHeaderSpace, inRect.width, inRect.height - 37f - extraHeaderSpace);
		FillMainRect(mainRect, out anythingChanged);
	}

	private void FillMainRect(Rect mainRect, out bool anythingChanged)
	{
		anythingChanged = false;
		Text.Font = GameFont.Small;
		if (AnyTransferable)
		{
			float num = 6f;
			for (int i = 0; i < sections.Count; i++)
			{
				num += (float)sections[i].cachedTransferables.Count * 30f;
				if (sections[i].title != null)
				{
					num += 30f;
				}
			}
			float curY = 6f;
			float availableMass = ((availableMassGetter != null) ? availableMassGetter() : float.MaxValue);
			Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, num);
			Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
			float num2 = scrollPosition.y - 30f;
			float num3 = scrollPosition.y + mainRect.height;
			for (int j = 0; j < sections.Count; j++)
			{
				List<TransferableOneWay> cachedTransferables = sections[j].cachedTransferables;
				if (!cachedTransferables.Any())
				{
					continue;
				}
				if (sections[j].title != null)
				{
					Widgets.ListSeparator(ref curY, viewRect.width, sections[j].title);
					curY += 5f;
				}
				for (int k = 0; k < cachedTransferables.Count; k++)
				{
					if (curY > num2 && curY < num3)
					{
						Rect rect = new Rect(0f, curY, viewRect.width, 30f);
						int countToTransfer = cachedTransferables[k].CountToTransfer;
						DoRow(rect, cachedTransferables[k], k, availableMass);
						if (countToTransfer != cachedTransferables[k].CountToTransfer)
						{
							anythingChanged = true;
						}
					}
					curY += 30f;
				}
			}
			Widgets.EndScrollView();
		}
		else
		{
			GUI.color = Color.gray;
			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.Label(mainRect, "NoneBrackets".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
		}
	}

	private void DoRow(Rect rect, TransferableOneWay trad, int index, float availableMass)
	{
		if (index % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect);
		}
		Text.Font = GameFont.Small;
		Widgets.BeginGroup(rect);
		float width = rect.width;
		int maxCount = trad.MaxCount;
		Rect rect2 = new Rect(width - 240f, 0f, 240f, rect.height);
		stoppingPoints.Clear();
		if (availableMassGetter != null && (!(trad.AnyThing is Pawn) || includePawnsMassInMassUsage))
		{
			float num = availableMass + GetMass(trad.AnyThing) * (float)trad.CountToTransfer;
			int threshold = ((!(num <= 0f)) ? Mathf.FloorToInt(num / GetMass(trad.AnyThing)) : 0);
			stoppingPoints.Add(new TransferableCountToTransferStoppingPoint(threshold, "M<", ">M"));
		}
		Pawn pawn = trad.AnyThing as Pawn;
		bool flag = pawn != null && (pawn.IsColonist || pawn.IsPrisonerOfColony);
		TransferableUIUtility.DoCountAdjustInterface(rect2, trad, index, 0, maxCount, flash: false, stoppingPoints, (playerPawnsReadOnly && flag) || readOnly);
		width -= 240f;
		if (drawMarketValue)
		{
			Rect rect3 = new Rect(width - 100f, 0f, 100f, rect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			DrawMarketValue(rect3, trad);
			width -= 100f;
		}
		if (drawMass)
		{
			Rect rect4 = new Rect(width - 100f, 0f, 100f, rect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			DrawMass(rect4, trad, availableMass);
			width -= 100f;
		}
		if (drawDaysUntilRot)
		{
			Rect rect5 = new Rect(width - 75f, 0f, 75f, rect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			DrawDaysUntilRot(rect5, trad);
			width -= 75f;
		}
		if (drawItemNutrition)
		{
			Rect rect6 = new Rect(width - 75f, 0f, 75f, rect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			DrawItemNutrition(rect6, trad);
			width -= 75f;
		}
		if (drawForagedFoodPerDay)
		{
			Rect rect7 = new Rect(width - 75f, 0f, 75f, rect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			if (!DrawGrazeability(rect7, trad))
			{
				DrawForagedFoodPerDay(rect7, trad);
			}
			width -= 75f;
		}
		if (drawNutritionEatenPerDay || drawMechEnergy)
		{
			bool flag2 = false;
			if (drawNutritionEatenPerDay)
			{
				Rect rect8 = new Rect(width - 75f, 0f, 75f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				flag2 = DrawNutritionEatenPerDay(rect8, trad);
			}
			if (ModsConfig.BiotechActive && drawMechEnergy && !flag2)
			{
				Rect rect9 = new Rect(width - 75f, 0f, 75f, rect.height);
				DrawMechEnergy(rect9, trad);
			}
			width -= 75f;
		}
		if (ShouldShowCount(trad))
		{
			Rect rect10 = new Rect(width - 75f, 0f, 75f, rect.height);
			Widgets.DrawHighlightIfMouseover(rect10);
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect11 = rect10;
			rect11.xMin += 5f;
			rect11.xMax -= 5f;
			Widgets.Label(rect11, maxCount.ToStringCached());
			TooltipHandler.TipRegion(rect10, sourceCountDesc);
		}
		width -= 75f;
		if (drawIdeo)
		{
			if (pawn != null && pawn.Ideo != null)
			{
				Rect rect12 = new Rect(width - 30f, 0f, 30f, rect.height);
				Widgets.DrawHighlightIfMouseover(rect12);
				pawn.Ideo.DrawIcon(rect12);
				TooltipHandler.TipRegion(rect12, pawn.Ideo.name);
			}
			width -= 30f;
		}
		if (drawXenotype && pawn != null && pawn.genes?.Xenotype != null)
		{
			Rect rect13 = new Rect(width - 30f, 0f, 30f, rect.height);
			Widgets.DrawHighlightIfMouseover(rect13);
			GUI.color = XenotypeDef.IconColor;
			GUI.DrawTexture(rect13, pawn.genes.XenotypeIcon);
			GUI.color = Color.white;
			TooltipHandler.TipRegion(rect13, pawn.genes.XenotypeLabelCap);
		}
		if (pawn != null && pawn.IsSlave)
		{
			Rect rect14 = new Rect(width - 30f, 0f, 30f, rect.height);
			Widgets.DrawHighlightIfMouseover(rect14);
			GUI.DrawTexture(rect14, pawn.guest.GetIcon());
			TooltipHandler.TipRegion(rect14, pawn.guest.GetLabel());
			width -= 30f;
		}
		if (drawEquippedWeapon)
		{
			Rect rect15 = new Rect(width - 30f, 0f, 30f, rect.height);
			Rect iconRect = new Rect(width - 30f, (rect.height - 30f) / 2f, 30f, 30f);
			DrawEquippedWeapon(rect15, iconRect, trad);
			width -= 30f;
		}
		TransferableUIUtility.DoExtraIcons(trad, rect, ref width);
		Rect idRect = new Rect(0f, 0f, width, rect.height);
		Color labelColor = ((pawn != null && pawn.IsSlave) ? PawnNameColorUtility.PawnNameColorOf(pawn) : Color.white);
		TransferableUIUtility.DrawTransferableInfo(trad, idRect, labelColor);
		GenUI.ResetLabelAlign();
		Widgets.EndGroup();
	}

	private bool ShouldShowCount(TransferableOneWay trad)
	{
		if (!trad.HasAnyThing)
		{
			return true;
		}
		if (trad.AnyThing is Pawn pawn && pawn.RaceProps.Humanlike)
		{
			return trad.MaxCount != 1;
		}
		return true;
	}

	private void DrawDaysUntilRot(Rect rect, TransferableOneWay trad)
	{
		if (!trad.HasAnyThing || !trad.ThingDef.IsNutritionGivingIngestible)
		{
			return;
		}
		if (!cachedTicksUntilRot.TryGetValue(trad, out var value))
		{
			value = int.MaxValue;
			for (int i = 0; i < trad.things.Count; i++)
			{
				CompRottable compRottable = trad.things[i].TryGetComp<CompRottable>();
				if (compRottable != null)
				{
					value = Mathf.Min(value, DaysUntilRotCalculator.ApproxTicksUntilRot_AssumeTimePassesBy(compRottable, tile));
				}
			}
			cachedTicksUntilRot.Add(trad, value);
		}
		if (value < 36000000 && !((float)value >= 36000000f))
		{
			Widgets.DrawHighlightIfMouseover(rect);
			float num = (float)value / 60000f;
			GUI.color = Color.yellow;
			Widgets.Label(rect, num.ToString("0.#"));
			GUI.color = Color.white;
			TooltipHandler.TipRegionByKey(rect, "DaysUntilRotTip");
		}
	}

	private void DrawItemNutrition(Rect rect, TransferableOneWay trad)
	{
		if (trad.HasAnyThing && trad.ThingDef.IsNutritionGivingIngestible)
		{
			Widgets.DrawHighlightIfMouseover(rect);
			GUI.color = Color.green;
			Widgets.Label(rect, trad.AnyThing.GetStatValue(StatDefOf.Nutrition).ToString("0.##"));
			GUI.color = Color.white;
			if (Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, "ItemNutritionTip".Translate((1.6f * ThingDefOf.Human.race.baseHungerRate).ToString("0.##")));
			}
		}
	}

	private bool DrawGrazeability(Rect rect, TransferableOneWay trad)
	{
		if (!trad.HasAnyThing)
		{
			return false;
		}
		if (!(trad.AnyThing is Pawn p) || !VirtualPlantsUtility.CanEverEatVirtualPlants(p))
		{
			return false;
		}
		rect.width = 40f;
		Rect position = new Rect(rect.x + (float)(int)((rect.width - 28f) / 2f), rect.y + (float)(int)((rect.height - 28f) / 2f), 28f, 28f);
		Widgets.DrawHighlightIfMouseover(rect);
		GUI.DrawTexture(position, CanGrazeIcon);
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, delegate
			{
				TaggedString taggedString = "AnimalCanGrazeTip".Translate();
				if (tile.Valid)
				{
					taggedString += "\n\n" + VirtualPlantsUtility.GetVirtualPlantsStatusExplanationAt(tile, Find.TickManager.TicksAbs);
				}
				return taggedString;
			}, trad.GetHashCode() ^ 0x7424D7F2);
		}
		return true;
	}

	private void DrawForagedFoodPerDay(Rect rect, TransferableOneWay trad)
	{
		if (!trad.HasAnyThing)
		{
			return;
		}
		Pawn p = trad.AnyThing as Pawn;
		if (p == null)
		{
			return;
		}
		bool skip;
		float foragedNutritionPerDay = ForagedFoodPerDayCalculator.GetBaseForagedNutritionPerDay(p, out skip);
		if (skip)
		{
			return;
		}
		Widgets.DrawHighlightIfMouseover(rect);
		GUI.color = ((foragedNutritionPerDay == 0f) ? Color.gray : Color.green);
		Widgets.Label(rect, "+" + foragedNutritionPerDay.ToString("0.##"));
		GUI.color = Color.white;
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, () => "NutritionForagedPerDayTip".Translate(StatDefOf.ForagedNutritionPerDay.Worker.GetExplanationFull(StatRequest.For(p), StatDefOf.ForagedNutritionPerDay.toStringNumberSense, foragedNutritionPerDay)), trad.GetHashCode() ^ 0x74BEF43E);
		}
	}

	private bool DrawNutritionEatenPerDay(Rect rect, TransferableOneWay trad)
	{
		if (!trad.HasAnyThing)
		{
			return false;
		}
		Pawn p = trad.AnyThing as Pawn;
		if (p == null || !p.RaceProps.EatsFood || p.Dead || p.needs.food == null)
		{
			return false;
		}
		Widgets.DrawHighlightIfMouseover(rect);
		string text = RaceProperties.NutritionEatenPerDay(p);
		DietCategory resolvedDietCategory = p.RaceProps.ResolvedDietCategory;
		if (resolvedDietCategory != DietCategory.Omnivorous)
		{
			text = text + " (" + resolvedDietCategory.ToStringHumanShort() + ")";
		}
		GUI.color = new Color(1f, 0.5f, 0f);
		Widgets.Label(rect, text);
		GUI.color = Color.white;
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, () => RaceProperties.NutritionEatenPerDayExplanation(p, showDiet: true, showLegend: true, showCalculations: false), trad.GetHashCode() ^ 0x17016B3E);
		}
		return true;
	}

	private void DrawMechEnergy(Rect rect, TransferableOneWay trad)
	{
		if (ModsConfig.BiotechActive && trad.HasAnyThing && trad.AnyThing is Pawn { Dead: false } pawn && pawn.needs.energy != null)
		{
			Widgets.DrawHighlightIfMouseover(rect);
			GUI.color = new Color32(104, 190, byte.MaxValue, byte.MaxValue);
			Widgets.Label(rect, pawn.needs.energy.CurLevelPercentage.ToStringPercent());
			GUI.color = Color.white;
			if (Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, "MechEnergy".Translate());
			}
		}
	}

	private void DrawMarketValue(Rect rect, TransferableOneWay trad)
	{
		if (trad.HasAnyThing && trad.ThingDef.tradeability != Tradeability.None)
		{
			Widgets.DrawHighlightIfMouseover(rect);
			Widgets.Label(rect, trad.AnyThing.MarketValue.ToStringMoney());
			TooltipHandler.TipRegionByKey(rect, "MarketValueTip");
		}
	}

	private void DrawMass(Rect rect, TransferableOneWay trad, float availableMass)
	{
		if (!trad.HasAnyThing)
		{
			return;
		}
		Thing anyThing = trad.AnyThing;
		Pawn pawn = anyThing as Pawn;
		if (pawn != null && !includePawnsMassInMassUsage && !MassUtility.CanEverCarryAnything(pawn))
		{
			return;
		}
		Widgets.DrawHighlightIfMouseover(rect);
		if (pawn == null || includePawnsMassInMassUsage)
		{
			float mass = GetMass(anyThing);
			if (Mouse.IsOver(rect))
			{
				if (pawn != null)
				{
					float gearMass = 0f;
					float invMass = 0f;
					gearMass = MassUtility.GearMass(pawn);
					if (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignorePawnInventoryMass))
					{
						invMass = MassUtility.InventoryMass(pawn);
					}
					TooltipHandler.TipRegion(rect, () => GetPawnMassTip(trad, 0f, mass - gearMass - invMass, gearMass, invMass), trad.GetHashCode() * 59);
				}
				else
				{
					TooltipHandler.TipRegion(rect, "ItemWeightTip".Translate());
				}
			}
			if (mass > availableMass)
			{
				GUI.color = ColorLibrary.RedReadable;
			}
			else
			{
				GUI.color = ItemMassColor;
			}
			Widgets.Label(rect, mass.ToStringMass());
		}
		else
		{
			float cap = MassUtility.Capacity(pawn);
			float gearMass2 = MassUtility.GearMass(pawn);
			float invMass2 = (InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignorePawnInventoryMass) ? 0f : MassUtility.InventoryMass(pawn));
			float num = cap - gearMass2 - invMass2;
			if (num > 0f)
			{
				GUI.color = Color.green;
			}
			else if (num < 0f)
			{
				GUI.color = ColorLibrary.RedReadable;
			}
			else
			{
				GUI.color = Color.gray;
			}
			Widgets.Label(rect, num.ToStringMassOffset());
			if (Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, () => GetPawnMassTip(trad, cap, 0f, gearMass2, invMass2), trad.GetHashCode() * 59);
			}
		}
		GUI.color = Color.white;
	}

	private void DrawEquippedWeapon(Rect rect, Rect iconRect, TransferableOneWay trad)
	{
		if (trad.HasAnyThing && trad.AnyThing is Pawn { equipment: not null } pawn && pawn.equipment.Primary != null)
		{
			ThingWithComps primary = pawn.equipment.Primary;
			Widgets.DrawHighlightIfMouseover(rect);
			Widgets.ThingIcon(iconRect, primary);
			if (Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, primary.LabelCap);
			}
		}
	}

	private string GetPawnMassTip(TransferableOneWay trad, float capacity, float pawnMass, float gearMass, float invMass)
	{
		if (!trad.HasAnyThing)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (capacity != 0f)
		{
			stringBuilder.Append("MassCapacity".Translate() + ": " + capacity.ToStringMass());
		}
		else
		{
			stringBuilder.Append("Mass".Translate() + ": " + pawnMass.ToStringMass());
		}
		if (gearMass != 0f)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("EquipmentAndApparelMass".Translate() + ": " + gearMass.ToStringMass());
		}
		if (invMass != 0f)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("InventoryMass".Translate() + ": " + invMass.ToStringMass());
		}
		return stringBuilder.ToString();
	}

	private float GetMass(Thing thing)
	{
		if (thing == null)
		{
			return 0f;
		}
		float num = thing.GetStatValue(StatDefOf.Mass);
		if (thing is Pawn pawn)
		{
			if (InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignorePawnInventoryMass))
			{
				num -= MassUtility.InventoryMass(pawn);
			}
		}
		else if (ignoreSpawnedCorpseGearAndInventoryMass && thing is Corpse { Spawned: not false } corpse)
		{
			num -= MassUtility.GearAndInventoryMass(corpse.InnerPawn);
		}
		return num;
	}
}
