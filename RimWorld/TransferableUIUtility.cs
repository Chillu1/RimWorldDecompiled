using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class TransferableUIUtility
{
	public struct ExtraInfo
	{
		public string key;

		public string value;

		public string secondValue;

		public string tip;

		public float lastFlashTime;

		public Color color;

		public Color secondColor;

		public ExtraInfo(string key, string value, Color color, string tip, float lastFlashTime = -9999f)
		{
			this.key = key;
			this.value = value;
			this.color = color;
			this.tip = tip;
			this.lastFlashTime = lastFlashTime;
			secondValue = null;
			secondColor = default(Color);
		}

		public ExtraInfo(string key, string value, Color color, string tip, string secondValue, Color secondColor, float lastFlashTime = -9999f)
		{
			this.key = key;
			this.value = value;
			this.color = color;
			this.tip = tip;
			this.lastFlashTime = lastFlashTime;
			this.secondValue = secondValue;
			this.secondColor = secondColor;
		}
	}

	private static List<TransferableCountToTransferStoppingPoint> stoppingPoints = new List<TransferableCountToTransferStoppingPoint>();

	private const float AmountAreaWidth = 90f;

	private const float AmountAreaHeight = 25f;

	private const float AdjustArrowWidth = 30f;

	public const float ResourceIconSize = 27f;

	public const float SortersHeight = 27f;

	public const float ExtraInfoHeight = 40f;

	public const float ExtraInfoMargin = 12f;

	public static readonly Color ZeroCountColor = new Color(0.5f, 0.5f, 0.5f);

	public static readonly Texture2D FlashTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0f, 0f, 0.4f));

	private static readonly Texture2D TradeArrow = ContentFinder<Texture2D>.Get("UI/Widgets/TradeArrow");

	private static readonly Texture2D DividerTex = ContentFinder<Texture2D>.Get("UI/Widgets/Divider");

	private static readonly Texture2D PregnantIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Pregnant");

	private static readonly Texture2D BondIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Bond");

	private static readonly Texture2D RideableIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Rideable");

	private static readonly Texture2D BonusIcon = ContentFinder<Texture2D>.Get("UI/Icons/MoveSpeedBonus");

	private static readonly Texture2D SickIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Sick");

	private static readonly Rect SortersRect = new Rect(0f, 0f, 350f, 27f);

	private static readonly Rect SearcherRect = new Rect(360f, 0f, 170f, 27f);

	[TweakValue("Interface", 0f, 50f)]
	private static float PregnancyIconWidth = 24f;

	[TweakValue("Interface", 0f, 50f)]
	private static float BondIconWidth = 24f;

	[TweakValue("Interface", 0f, 50f)]
	private static float RideableIconWidth = 24f;

	[TweakValue("Interface", 0f, 50f)]
	private static float SlaveTradeIconWidth = 24f;

	[TweakValue("Interface", 0f, 50f)]
	private static float OverseerIconWidth = 36f;

	[TweakValue("Interface", 0f, 50f)]
	private static float SickIconWidth = 24f;

	public static void DoCountAdjustInterface(Rect rect, Transferable trad, int index, int min, int max, bool flash = false, List<TransferableCountToTransferStoppingPoint> extraStoppingPoints = null, bool readOnly = false)
	{
		stoppingPoints.Clear();
		if (extraStoppingPoints != null)
		{
			stoppingPoints.AddRange(extraStoppingPoints);
		}
		for (int num = stoppingPoints.Count - 1; num >= 0; num--)
		{
			if (stoppingPoints[num].threshold != 0 && (stoppingPoints[num].threshold <= min || stoppingPoints[num].threshold >= max))
			{
				stoppingPoints.RemoveAt(num);
			}
		}
		bool flag = false;
		for (int i = 0; i < stoppingPoints.Count; i++)
		{
			if (stoppingPoints[i].threshold == 0)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			stoppingPoints.Add(new TransferableCountToTransferStoppingPoint(0, "0", "0"));
		}
		DoCountAdjustInterfaceInternal(rect, trad, index, min, max, flash, readOnly);
	}

	private static void DoCountAdjustInterfaceInternal(Rect rect, Transferable trad, int index, int min, int max, bool flash, bool readOnly)
	{
		rect = rect.Rounded();
		Rect rect2 = new Rect(rect.center.x - 45f, rect.center.y - 12.5f, 90f, 25f).Rounded();
		if (flash)
		{
			GUI.DrawTexture(rect2, FlashTex);
		}
		bool flag = trad is TransferableOneWay { HasAnyThing: not false } transferableOneWay && transferableOneWay.AnyThing is Pawn && transferableOneWay.MaxCount == 1;
		if (!trad.Interactive || readOnly)
		{
			if (flag)
			{
				bool checkOn = trad.CountToTransfer != 0;
				Widgets.Checkbox(rect2.position, ref checkOn, 24f, disabled: true);
			}
			else
			{
				GUI.color = ((trad.CountToTransfer == 0) ? ZeroCountColor : Color.white);
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect2, trad.CountToTransfer.ToStringCached());
			}
		}
		else if (flag)
		{
			bool flag2 = trad.CountToTransfer != 0;
			bool checkOn2 = flag2;
			Widgets.Checkbox(rect2.position, ref checkOn2, 24f, disabled: false, paintable: true);
			if (checkOn2 != flag2)
			{
				if (checkOn2)
				{
					trad.AdjustTo(trad.GetMaximumToTransfer());
				}
				else
				{
					trad.AdjustTo(trad.GetMinimumToTransfer());
				}
			}
		}
		else
		{
			Rect rect3 = rect2.ContractedBy(2f);
			rect3.xMax -= 15f;
			rect3.xMin += 16f;
			int val = trad.CountToTransfer;
			string buffer = trad.EditBuffer;
			Widgets.TextFieldNumeric(rect3, ref val, ref buffer, min, max);
			trad.AdjustTo(val);
			trad.EditBuffer = buffer;
		}
		Text.Anchor = TextAnchor.UpperLeft;
		GUI.color = Color.white;
		if (trad.Interactive && !flag && !readOnly)
		{
			TransferablePositiveCountDirection positiveCountDirection = trad.PositiveCountDirection;
			int num = ((positiveCountDirection == TransferablePositiveCountDirection.Source) ? 1 : (-1));
			int num2 = GenUI.CurrentAdjustmentMultiplier();
			bool flag3 = trad.GetRange() == 1;
			if (trad.CanAdjustBy(num * num2).Accepted)
			{
				Rect rect4 = new Rect(rect2.x - 30f, rect.y, 30f, rect.height);
				if (flag3)
				{
					rect4.x -= rect4.width;
					rect4.width += rect4.width;
				}
				if (Widgets.ButtonText(rect4, "<"))
				{
					trad.AdjustBy(num * num2);
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				if (!flag3)
				{
					string label = "<<";
					int? num3 = null;
					int num4 = 0;
					for (int i = 0; i < stoppingPoints.Count; i++)
					{
						TransferableCountToTransferStoppingPoint transferableCountToTransferStoppingPoint = stoppingPoints[i];
						if (positiveCountDirection == TransferablePositiveCountDirection.Source)
						{
							if (trad.CountToTransfer < transferableCountToTransferStoppingPoint.threshold && (transferableCountToTransferStoppingPoint.threshold < num4 || !num3.HasValue))
							{
								label = transferableCountToTransferStoppingPoint.leftLabel;
								num3 = transferableCountToTransferStoppingPoint.threshold;
							}
						}
						else if (trad.CountToTransfer > transferableCountToTransferStoppingPoint.threshold && (transferableCountToTransferStoppingPoint.threshold > num4 || !num3.HasValue))
						{
							label = transferableCountToTransferStoppingPoint.leftLabel;
							num3 = transferableCountToTransferStoppingPoint.threshold;
						}
					}
					rect4.x -= rect4.width;
					if (Widgets.ButtonText(rect4, label))
					{
						if (num3.HasValue)
						{
							trad.AdjustTo(num3.Value);
						}
						else if (num == 1)
						{
							trad.AdjustTo(trad.GetMaximumToTransfer());
						}
						else
						{
							trad.AdjustTo(trad.GetMinimumToTransfer());
						}
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
					}
				}
			}
			if (trad.CanAdjustBy(-num * num2).Accepted)
			{
				Rect rect5 = new Rect(rect2.xMax, rect.y, 30f, rect.height);
				if (flag3)
				{
					rect5.width += rect5.width;
				}
				if (Widgets.ButtonText(rect5, ">"))
				{
					trad.AdjustBy(-num * num2);
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
				if (!flag3)
				{
					string label2 = ">>";
					int? num5 = null;
					int num6 = 0;
					for (int j = 0; j < stoppingPoints.Count; j++)
					{
						TransferableCountToTransferStoppingPoint transferableCountToTransferStoppingPoint2 = stoppingPoints[j];
						if (positiveCountDirection == TransferablePositiveCountDirection.Destination)
						{
							if (trad.CountToTransfer < transferableCountToTransferStoppingPoint2.threshold && (transferableCountToTransferStoppingPoint2.threshold < num6 || !num5.HasValue))
							{
								label2 = transferableCountToTransferStoppingPoint2.rightLabel;
								num5 = transferableCountToTransferStoppingPoint2.threshold;
							}
						}
						else if (trad.CountToTransfer > transferableCountToTransferStoppingPoint2.threshold && (transferableCountToTransferStoppingPoint2.threshold > num6 || !num5.HasValue))
						{
							label2 = transferableCountToTransferStoppingPoint2.rightLabel;
							num5 = transferableCountToTransferStoppingPoint2.threshold;
						}
					}
					rect5.x += rect5.width;
					if (Widgets.ButtonText(rect5, label2))
					{
						if (num5.HasValue)
						{
							trad.AdjustTo(num5.Value);
						}
						else if (num == 1)
						{
							trad.AdjustTo(trad.GetMinimumToTransfer());
						}
						else
						{
							trad.AdjustTo(trad.GetMaximumToTransfer());
						}
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					}
				}
			}
		}
		if (trad.CountToTransfer != 0)
		{
			Rect position = new Rect(rect2.x + rect2.width / 2f - (float)(TradeArrow.width / 2), rect2.y + rect2.height / 2f - (float)(TradeArrow.height / 2), TradeArrow.width, TradeArrow.height);
			TransferablePositiveCountDirection positiveCountDirection2 = trad.PositiveCountDirection;
			if ((positiveCountDirection2 == TransferablePositiveCountDirection.Source && trad.CountToTransfer > 0) || (positiveCountDirection2 == TransferablePositiveCountDirection.Destination && trad.CountToTransfer < 0))
			{
				position.x += position.width;
				position.width *= -1f;
			}
			GUI.DrawTexture(position, TradeArrow);
		}
	}

	public static void DrawTransferableInfo(Transferable trad, Rect idRect, Color labelColor)
	{
		if (!trad.HasAnyThing && trad.IsThing)
		{
			return;
		}
		if (Mouse.IsOver(idRect))
		{
			Widgets.DrawHighlight(idRect);
		}
		Rect rect = new Rect(0f, 0f, 27f, 27f);
		if (trad.IsThing)
		{
			try
			{
				Widgets.ThingIcon(rect, trad.AnyThing);
			}
			catch (Exception ex)
			{
				Log.Error("Exception drawing thing icon for " + trad.AnyThing.def.defName + ": " + ex.ToString());
			}
		}
		else
		{
			trad.DrawIcon(rect);
		}
		if (trad.IsThing)
		{
			Widgets.InfoCardButton(40f, 0f, trad.AnyThing);
		}
		Text.Anchor = TextAnchor.MiddleLeft;
		Rect rect2 = new Rect(80f, 0f, idRect.width - 80f, idRect.height);
		Text.WordWrap = false;
		GUI.color = labelColor;
		Widgets.Label(rect2, trad.LabelCap);
		GUI.color = Color.white;
		Text.WordWrap = true;
		if (!Mouse.IsOver(idRect))
		{
			return;
		}
		Transferable localTrad = trad;
		TooltipHandler.TipRegion(idRect, new TipSignal(delegate
		{
			if (!localTrad.HasAnyThing && localTrad.IsThing)
			{
				return "";
			}
			string text = localTrad.LabelCap;
			string tipDescription = localTrad.TipDescription;
			if (localTrad.AnyThing is Book)
			{
				text = tipDescription;
			}
			else if (!tipDescription.NullOrEmpty())
			{
				text = text + ": " + tipDescription + ContentSourceDescription(localTrad.AnyThing);
			}
			CompIngredients compIngredients = localTrad.AnyThing.TryGetComp<CompIngredients>();
			if (compIngredients != null)
			{
				text = text + "\n\n" + compIngredients.CompInspectStringExtra();
			}
			return text;
		}, localTrad.GetHashCode()));
	}

	public static float DefaultListOrderPriority(Transferable transferable)
	{
		if (!transferable.HasAnyThing)
		{
			return 0f;
		}
		return DefaultListOrderPriority(transferable.ThingDef);
	}

	public static float DefaultArchonexusItemListOrderPriority(ThingDef def)
	{
		if (def == ThingDefOf.MealSurvivalPack)
		{
			return 100.2f;
		}
		if (def == ThingDefOf.Pemmican)
		{
			return 100.1f;
		}
		if (def == ThingDefOf.Luciferium)
		{
			return 90.1f;
		}
		if (def.IsNonMedicalDrug)
		{
			return 90f;
		}
		if (def.IsMedicine)
		{
			return 80f;
		}
		if (MoveColonyUtility.IsDistinctArchonexusItem(def))
		{
			return 75f;
		}
		if (def == ThingDefOf.Silver)
		{
			return 50.2f;
		}
		if (def == ThingDefOf.Gold)
		{
			return 50.1f;
		}
		if (def.thingCategories.Contains(ThingCategoryDefOf.ResourcesRaw))
		{
			return 70f;
		}
		if (def.thingCategories.Contains(ThingCategoryDefOf.Manufactured) || def.thingCategories.Contains(ThingCategoryDefOf.Drugs))
		{
			return 60f;
		}
		if (def.thingCategories.Contains(ThingCategoryDefOf.PlantMatter))
		{
			return -10f;
		}
		if (def.IsEgg || def.IsAnimalProduct)
		{
			return -20f;
		}
		if (def.thingCategories.Contains(ThingCategoryDefOf.Foods) || def.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw))
		{
			return -30f;
		}
		if (def.IsLeather || def.IsWool || def.thingCategories.Contains(ThingCategoryDefOf.Textiles))
		{
			return -40f;
		}
		if (def.thingCategories.Contains(ThingCategoryDefOf.StoneBlocks))
		{
			return -50f;
		}
		return 0f;
	}

	public static float DefaultListOrderPriority(ThingDef def)
	{
		if (def == ThingDefOf.Silver)
		{
			return 100f;
		}
		if (def == ThingDefOf.Gold)
		{
			return 99f;
		}
		if (def.Minifiable)
		{
			return 90f;
		}
		if (def.IsApparel)
		{
			return 80f;
		}
		if (def.IsRangedWeapon)
		{
			return 70f;
		}
		if (def.IsMeleeWeapon)
		{
			return 60f;
		}
		if (def.isTechHediff)
		{
			return 50f;
		}
		if (def.CountAsResource)
		{
			return -10f;
		}
		return 20f;
	}

	public static void DoTransferableSorters(TransferableSorterDef sorter1, TransferableSorterDef sorter2, Action<TransferableSorterDef> sorter1Setter, Action<TransferableSorterDef> sorter2Setter)
	{
		Widgets.BeginGroup(SortersRect);
		Text.Font = GameFont.Tiny;
		Rect rect = new Rect(0f, 0f, 60f, 27f);
		Text.Anchor = TextAnchor.MiddleLeft;
		Widgets.Label(rect, "SortBy".Translate());
		Text.Anchor = TextAnchor.UpperLeft;
		Rect rect2 = new Rect(rect.xMax + 10f, 0f, 130f, 27f);
		if (Widgets.ButtonText(rect2, sorter1.LabelCap.Truncate(rect2.width - 2f)))
		{
			OpenSorterChangeFloatMenu(sorter1Setter);
		}
		Rect rect3 = new Rect(rect2.xMax + 10f, 0f, 130f, 27f);
		if (Widgets.ButtonText(rect3, sorter2.LabelCap.Truncate(rect3.width - 2f)))
		{
			OpenSorterChangeFloatMenu(sorter2Setter);
		}
		Widgets.EndGroup();
	}

	public static void DoTransferableSearcher(QuickSearchWidget searchWidget, Action onSearchChange)
	{
		Rect searcherRect = SearcherRect;
		Widgets.BeginGroup(searcherRect);
		Text.Font = GameFont.Small;
		Rect rect = new Rect(0f, (searcherRect.height - 24f) / 2f, 170f, 24f);
		searchWidget.OnGUI(rect, onSearchChange);
		Text.Font = GameFont.Tiny;
		Widgets.EndGroup();
	}

	public static void DoExtraIcons(Transferable trad, Rect rect, ref float curX)
	{
		if (!(trad.AnyThing is Pawn pawn))
		{
			return;
		}
		if (pawn.RaceProps.Animal)
		{
			if (pawn.IsCaravanRideable())
			{
				Rect rect2 = new Rect(curX - RideableIconWidth, (rect.height - RideableIconWidth) / 2f, RideableIconWidth, RideableIconWidth);
				curX -= rect2.width;
				GUI.DrawTexture(rect2, RideableIcon);
				if (Mouse.IsOver(rect2))
				{
					TooltipHandler.TipRegion(rect2, CaravanRideableUtility.GetIconTooltipText(pawn));
				}
			}
			if (pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond) != null)
			{
				DrawBondedIcon(pawn, new Rect(curX - BondIconWidth, (rect.height - BondIconWidth) / 2f, BondIconWidth, BondIconWidth));
				curX -= BondIconWidth;
			}
			if (pawn.health.hediffSet.HasHediff(HediffDefOf.Pregnant, mustBeVisible: true))
			{
				DrawPregnancyIcon(pawn, new Rect(curX - PregnancyIconWidth, (rect.height - PregnancyIconWidth) / 2f, PregnancyIconWidth, PregnancyIconWidth));
				curX -= PregnancyIconWidth;
			}
			if (pawn.health.hediffSet.AnyHediffMakesSickThought)
			{
				DrawSickIcon(pawn, new Rect(curX - SickIconWidth, (rect.height - SickIconWidth) / 2f, SickIconWidth, SickIconWidth));
				curX -= SickIconWidth;
			}
		}
		else if (ModsConfig.BiotechActive && pawn.IsColonyMech)
		{
			Pawn overseer = pawn.GetOverseer();
			if (overseer != null)
			{
				DrawOverseerIcon(pawn, overseer, new Rect(curX - OverseerIconWidth, (rect.height - OverseerIconWidth) / 2f, OverseerIconWidth, OverseerIconWidth));
				curX -= OverseerIconWidth;
			}
		}
		else if (CaravanBonusUtility.HasCaravanBonus(pawn))
		{
			Rect rect3 = new Rect(curX - RideableIconWidth, (rect.height - RideableIconWidth) / 2f, RideableIconWidth, RideableIconWidth);
			curX -= rect3.width;
			GUI.DrawTexture(rect3, BonusIcon);
			if (Mouse.IsOver(rect3))
			{
				TooltipHandler.TipRegion(rect3, CaravanBonusUtility.GetIconTooltipText(pawn));
			}
		}
	}

	public static void DrawBondedIcon(Pawn bondedPawn, Rect rect)
	{
		GUI.DrawTexture(rect, BondIcon);
		if (Mouse.IsOver(rect))
		{
			string iconTooltipText = TrainableUtility.GetIconTooltipText(bondedPawn);
			if (!iconTooltipText.NullOrEmpty())
			{
				TooltipHandler.TipRegion(rect, iconTooltipText);
			}
		}
	}

	public static void DrawPregnancyIcon(Pawn pregnantPawn, Rect rect)
	{
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, PawnColumnWorker_Pregnant.GetTooltipText(pregnantPawn));
		}
		GUI.DrawTexture(rect, PregnantIcon);
	}

	private static void DrawOverseerIcon(Pawn mech, Pawn overseer, Rect rect)
	{
		GUI.DrawTexture(rect, PortraitsCache.Get(overseer, new Vector2(OverseerIconWidth, OverseerIconWidth), Rot4.South));
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegion(rect, "MechOverseer".Translate(overseer));
		}
	}

	private static void DrawSickIcon(Pawn pawn, Rect rect)
	{
		if (Mouse.IsOver(rect))
		{
			IEnumerable<string> entries = from h in pawn.health.hediffSet.hediffs
				where h.def.makesSickThought
				select h.LabelCap;
			TooltipHandler.TipRegion(rect, "CaravanAnimalSick".Translate() + ":\n\n" + entries.ToLineList(" - "));
		}
		GUI.DrawTexture(rect, SickIcon);
	}

	private static void OpenSorterChangeFloatMenu(Action<TransferableSorterDef> sorterSetter)
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		List<TransferableSorterDef> allDefsListForReading = DefDatabase<TransferableSorterDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			TransferableSorterDef def = allDefsListForReading[i];
			list.Add(new FloatMenuOption(def.LabelCap, delegate
			{
				sorterSetter(def);
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	public static void DrawExtraInfo(List<ExtraInfo> info, Rect rect)
	{
		if (rect.width > (float)info.Count * 230f)
		{
			rect.x += Mathf.Floor((rect.width - (float)info.Count * 230f) / 2f);
			rect.width = (float)info.Count * 230f;
		}
		Widgets.BeginGroup(rect);
		float num = Mathf.Floor(rect.width / (float)info.Count);
		float num2 = 0f;
		for (int i = 0; i < info.Count; i++)
		{
			float num3 = ((i == info.Count - 1) ? (rect.width - num2) : num);
			Rect rect2 = new Rect(num2, 0f, num3, rect.height);
			Rect rect3 = new Rect(num2, 0f, num3, rect.height / 2f);
			Rect rect4 = new Rect(num2, rect.height / 2f, num3, rect.height / 2f);
			if (Time.time - info[i].lastFlashTime < 1f)
			{
				GUI.DrawTexture(rect2, FlashTex);
			}
			Text.Anchor = TextAnchor.LowerCenter;
			Text.Font = GameFont.Tiny;
			GUI.color = Color.gray;
			Widgets.Label(new Rect(rect3.x, rect3.y - 2f, rect3.width, rect3.height - -3f), info[i].key);
			Rect rect5 = new Rect(rect4.x, rect4.y + -3f + 2f, rect4.width, rect4.height - -3f);
			Text.Font = GameFont.Small;
			if (info[i].secondValue.NullOrEmpty())
			{
				Text.Anchor = TextAnchor.UpperCenter;
				GUI.color = info[i].color;
				Widgets.Label(rect5, info[i].value);
			}
			else
			{
				Rect rect6 = rect5;
				rect6.width = Mathf.Floor(rect5.width / 2f - 15f);
				Text.Anchor = TextAnchor.UpperRight;
				GUI.color = info[i].color;
				Widgets.Label(rect6, info[i].value);
				Rect rect7 = rect5;
				rect7.xMin += Mathf.Ceil(rect5.width / 2f + 15f);
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = info[i].secondColor;
				Widgets.Label(rect7, info[i].secondValue);
				Rect position = rect5;
				position.x = Mathf.Floor(rect5.x + rect5.width / 2f - 7.5f);
				position.y += 3f;
				position.width = 15f;
				position.height = 15f;
				GUI.color = Color.white;
				GUI.DrawTexture(position, DividerTex);
			}
			GUI.color = Color.white;
			Widgets.DrawHighlightIfMouseover(rect2);
			TooltipHandler.TipRegion(rect2, info[i].tip);
			num2 += num3;
		}
		Widgets.EndGroup();
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public static void DrawCaptiveTradeInfo(Transferable trad, ITrader trader, Rect rect, ref float curX)
	{
		if (!(trad.AnyThing is Pawn { guest: not null } pawn) || !pawn.RaceProps.Humanlike)
		{
			return;
		}
		if (TransferableIsCaptive(trad) && (pawn.IsSlaveOfColony || pawn.IsPrisonerOfColony))
		{
			if (pawn.HomeFaction == trader.Faction)
			{
				Rect rect2 = new Rect(curX - SlaveTradeIconWidth, (rect.height - SlaveTradeIconWidth) / 2f, SlaveTradeIconWidth, SlaveTradeIconWidth);
				curX -= SlaveTradeIconWidth;
				GUI.DrawTexture(rect2, GuestUtility.RansomIcon);
				if (Mouse.IsOver(rect2))
				{
					TooltipHandler.TipRegion(rect2, "SellingAsRansom".Translate());
				}
			}
			else
			{
				Rect rect3 = new Rect(curX - SlaveTradeIconWidth, (rect.height - SlaveTradeIconWidth) / 2f, SlaveTradeIconWidth, SlaveTradeIconWidth);
				curX -= SlaveTradeIconWidth;
				GUI.DrawTexture(rect3, GuestUtility.SlaveIcon);
				if (Mouse.IsOver(rect3))
				{
					TooltipHandler.TipRegion(rect3, "SellingAsSlave".Translate());
				}
			}
		}
		else
		{
			float width = 140f;
			string label = ((pawn.guest.joinStatus == JoinStatus.JoinAsColonist) ? "JoinsAsColonist" : "JoinsAsSlave").Translate();
			Rect rect4 = new Rect(curX, 0f, width, rect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect4, label);
			Text.Anchor = TextAnchor.UpperLeft;
			if (Mouse.IsOver(rect4))
			{
				Widgets.DrawHighlight(rect4);
				string key = ((pawn.guest.joinStatus == JoinStatus.JoinAsColonist) ? "JoinsAsColonistDesc" : "JoinsAsSlaveDesc");
				TooltipHandler.TipRegion(rect4, key.Translate());
			}
		}
	}

	public static bool TransferableIsCaptive(Transferable trad)
	{
		if (trad.AnyThing is Pawn pawn && pawn.RaceProps.Humanlike)
		{
			if (!pawn.IsSlave)
			{
				return pawn.IsPrisoner;
			}
			return true;
		}
		return false;
	}

	public static bool TradeIsPlayerSellingToSlavery(Tradeable trad, Faction traderFaction)
	{
		if (TransferableIsCaptive(trad) && trad.CountHeldBy(Transactor.Colony) > 0)
		{
			return ((Pawn)trad.AnyThing).HomeFaction != traderFaction;
		}
		return false;
	}

	public static string ContentSourceDescription(Thing thing)
	{
		if (thing?.ContentSource == null || thing.ContentSource.IsCoreMod)
		{
			return "";
		}
		return "\n\n" + ("Stat_Source_Label".Translate() + ": " + thing.ContentSource.Name).Resolve().Colorize(ColoredText.SubtleGrayColor);
	}
}
