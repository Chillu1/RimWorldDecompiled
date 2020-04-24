using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Bill_Production : Bill, IExposable
	{
		public BillRepeatModeDef repeatMode = BillRepeatModeDefOf.RepeatCount;

		public int repeatCount = 1;

		private BillStoreModeDef storeMode = BillStoreModeDefOf.BestStockpile;

		private Zone_Stockpile storeZone;

		public int targetCount = 10;

		public bool pauseWhenSatisfied;

		public int unpauseWhenYouHave = 5;

		public bool includeEquipped;

		public bool includeTainted;

		public Zone_Stockpile includeFromZone;

		public FloatRange hpRange = FloatRange.ZeroToOne;

		public QualityRange qualityRange = QualityRange.All;

		public bool limitToAllowedStuff;

		public bool paused;

		protected override string StatusString
		{
			get
			{
				if (paused)
				{
					return " " + "Paused".Translate();
				}
				return "";
			}
		}

		protected override float StatusLineMinHeight
		{
			get
			{
				if (!CanUnpause())
				{
					return 0f;
				}
				return 24f;
			}
		}

		public string RepeatInfoText
		{
			get
			{
				if (repeatMode == BillRepeatModeDefOf.Forever)
				{
					return "Forever".Translate();
				}
				if (repeatMode == BillRepeatModeDefOf.RepeatCount)
				{
					return repeatCount.ToString() + "x";
				}
				if (repeatMode == BillRepeatModeDefOf.TargetCount)
				{
					return recipe.WorkerCounter.CountProducts(this).ToString() + "/" + targetCount.ToString();
				}
				throw new InvalidOperationException();
			}
		}

		public Bill_Production()
		{
		}

		public Bill_Production(RecipeDef recipe)
			: base(recipe)
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref repeatMode, "repeatMode");
			Scribe_Values.Look(ref repeatCount, "repeatCount", 0);
			Scribe_Defs.Look(ref storeMode, "storeMode");
			Scribe_References.Look(ref storeZone, "storeZone");
			Scribe_Values.Look(ref targetCount, "targetCount", 0);
			Scribe_Values.Look(ref pauseWhenSatisfied, "pauseWhenSatisfied", defaultValue: false);
			Scribe_Values.Look(ref unpauseWhenYouHave, "unpauseWhenYouHave", 0);
			Scribe_Values.Look(ref includeEquipped, "includeEquipped", defaultValue: false);
			Scribe_Values.Look(ref includeTainted, "includeTainted", defaultValue: false);
			Scribe_References.Look(ref includeFromZone, "includeFromZone");
			Scribe_Values.Look(ref hpRange, "hpRange", FloatRange.ZeroToOne);
			Scribe_Values.Look(ref qualityRange, "qualityRange", QualityRange.All);
			Scribe_Values.Look(ref limitToAllowedStuff, "limitToAllowedStuff", defaultValue: false);
			Scribe_Values.Look(ref paused, "paused", defaultValue: false);
			if (repeatMode == null)
			{
				repeatMode = BillRepeatModeDefOf.RepeatCount;
			}
			if (storeMode == null)
			{
				storeMode = BillStoreModeDefOf.BestStockpile;
			}
		}

		public override BillStoreModeDef GetStoreMode()
		{
			return storeMode;
		}

		public override Zone_Stockpile GetStoreZone()
		{
			return storeZone;
		}

		public override void SetStoreMode(BillStoreModeDef mode, Zone_Stockpile zone = null)
		{
			storeMode = mode;
			storeZone = zone;
			if (storeMode == BillStoreModeDefOf.SpecificStockpile != (storeZone != null))
			{
				Log.ErrorOnce("Inconsistent bill StoreMode data set", 75645354);
			}
		}

		public override bool ShouldDoNow()
		{
			if (repeatMode != BillRepeatModeDefOf.TargetCount)
			{
				paused = false;
			}
			if (suspended)
			{
				return false;
			}
			if (repeatMode == BillRepeatModeDefOf.Forever)
			{
				return true;
			}
			if (repeatMode == BillRepeatModeDefOf.RepeatCount)
			{
				return repeatCount > 0;
			}
			if (repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				int num = recipe.WorkerCounter.CountProducts(this);
				if (pauseWhenSatisfied && num >= targetCount)
				{
					paused = true;
				}
				if (num <= unpauseWhenYouHave || !pauseWhenSatisfied)
				{
					paused = false;
				}
				if (paused)
				{
					return false;
				}
				return num < targetCount;
			}
			throw new InvalidOperationException();
		}

		public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
		{
			if (repeatMode == BillRepeatModeDefOf.RepeatCount)
			{
				if (repeatCount > 0)
				{
					repeatCount--;
				}
				if (repeatCount == 0)
				{
					Messages.Message("MessageBillComplete".Translate(LabelCap), (Thing)billStack.billGiver, MessageTypeDefOf.TaskCompletion);
				}
			}
			recipe.Worker.Notify_IterationCompleted(billDoer, ingredients);
		}

		protected override void DoConfigInterface(Rect baseRect, Color baseColor)
		{
			Rect rect = new Rect(28f, 32f, 100f, 30f);
			GUI.color = new Color(1f, 1f, 1f, 0.65f);
			Widgets.Label(rect, RepeatInfoText);
			GUI.color = baseColor;
			WidgetRow widgetRow = new WidgetRow(baseRect.xMax, baseRect.y + 29f, UIDirection.LeftThenUp);
			if (widgetRow.ButtonText("Details".Translate() + "..."))
			{
				Find.WindowStack.Add(new Dialog_BillConfig(this, ((Thing)billStack.billGiver).Position));
			}
			if (widgetRow.ButtonText(repeatMode.LabelCap.Resolve().PadRight(20)))
			{
				BillRepeatModeUtility.MakeConfigFloatMenu(this);
			}
			if (widgetRow.ButtonIcon(TexButton.Plus))
			{
				if (repeatMode == BillRepeatModeDefOf.Forever)
				{
					repeatMode = BillRepeatModeDefOf.RepeatCount;
					repeatCount = 1;
				}
				else if (repeatMode == BillRepeatModeDefOf.TargetCount)
				{
					int num = recipe.targetCountAdjustment * GenUI.CurrentAdjustmentMultiplier();
					targetCount += num;
					unpauseWhenYouHave += num;
				}
				else if (repeatMode == BillRepeatModeDefOf.RepeatCount)
				{
					repeatCount += GenUI.CurrentAdjustmentMultiplier();
				}
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
				if (TutorSystem.TutorialMode && repeatMode == BillRepeatModeDefOf.RepeatCount)
				{
					TutorSystem.Notify_Event(recipe.defName + "-RepeatCountSetTo-" + repeatCount);
				}
			}
			if (widgetRow.ButtonIcon(TexButton.Minus))
			{
				if (repeatMode == BillRepeatModeDefOf.Forever)
				{
					repeatMode = BillRepeatModeDefOf.RepeatCount;
					repeatCount = 1;
				}
				else if (repeatMode == BillRepeatModeDefOf.TargetCount)
				{
					int num2 = recipe.targetCountAdjustment * GenUI.CurrentAdjustmentMultiplier();
					targetCount = Mathf.Max(0, targetCount - num2);
					unpauseWhenYouHave = Mathf.Max(0, unpauseWhenYouHave - num2);
				}
				else if (repeatMode == BillRepeatModeDefOf.RepeatCount)
				{
					repeatCount = Mathf.Max(0, repeatCount - GenUI.CurrentAdjustmentMultiplier());
				}
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
				if (TutorSystem.TutorialMode && repeatMode == BillRepeatModeDefOf.RepeatCount)
				{
					TutorSystem.Notify_Event(recipe.defName + "-RepeatCountSetTo-" + repeatCount);
				}
			}
		}

		private bool CanUnpause()
		{
			if (repeatMode == BillRepeatModeDefOf.TargetCount && paused && pauseWhenSatisfied)
			{
				return recipe.WorkerCounter.CountProducts(this) < targetCount;
			}
			return false;
		}

		public override void DoStatusLineInterface(Rect rect)
		{
			if (paused && new WidgetRow(rect.xMax, rect.y, UIDirection.LeftThenUp).ButtonText("Unpause".Translate()))
			{
				paused = false;
			}
		}

		public override void ValidateSettings()
		{
			base.ValidateSettings();
			if (storeZone != null)
			{
				if (!storeZone.zoneManager.AllZones.Contains(storeZone))
				{
					if (this != BillUtility.Clipboard)
					{
						Messages.Message("MessageBillValidationStoreZoneDeleted".Translate(LabelCap, billStack.billGiver.LabelShort.CapitalizeFirst(), storeZone.label), billStack.billGiver as Thing, MessageTypeDefOf.NegativeEvent);
					}
					SetStoreMode(BillStoreModeDefOf.DropOnFloor);
				}
				else if (base.Map != null && !base.Map.zoneManager.AllZones.Contains(storeZone))
				{
					if (this != BillUtility.Clipboard)
					{
						Messages.Message("MessageBillValidationStoreZoneUnavailable".Translate(LabelCap, billStack.billGiver.LabelShort.CapitalizeFirst(), storeZone.label), billStack.billGiver as Thing, MessageTypeDefOf.NegativeEvent);
					}
					SetStoreMode(BillStoreModeDefOf.DropOnFloor);
				}
			}
			else if (storeMode == BillStoreModeDefOf.SpecificStockpile)
			{
				SetStoreMode(BillStoreModeDefOf.DropOnFloor);
				Log.ErrorOnce("Found SpecificStockpile bill store mode without associated stockpile, recovering", 46304128);
			}
			if (includeFromZone == null)
			{
				return;
			}
			if (!includeFromZone.zoneManager.AllZones.Contains(includeFromZone))
			{
				if (this != BillUtility.Clipboard)
				{
					Messages.Message("MessageBillValidationIncludeZoneDeleted".Translate(LabelCap, billStack.billGiver.LabelShort.CapitalizeFirst(), includeFromZone.label), billStack.billGiver as Thing, MessageTypeDefOf.NegativeEvent);
				}
				includeFromZone = null;
			}
			else if (base.Map != null && !base.Map.zoneManager.AllZones.Contains(includeFromZone))
			{
				if (this != BillUtility.Clipboard)
				{
					Messages.Message("MessageBillValidationIncludeZoneUnavailable".Translate(LabelCap, billStack.billGiver.LabelShort.CapitalizeFirst(), includeFromZone.label), billStack.billGiver as Thing, MessageTypeDefOf.NegativeEvent);
				}
				includeFromZone = null;
			}
		}

		public override Bill Clone()
		{
			Bill_Production obj = (Bill_Production)base.Clone();
			obj.repeatMode = repeatMode;
			obj.repeatCount = repeatCount;
			obj.storeMode = storeMode;
			obj.storeZone = storeZone;
			obj.targetCount = targetCount;
			obj.pauseWhenSatisfied = pauseWhenSatisfied;
			obj.unpauseWhenYouHave = unpauseWhenYouHave;
			obj.includeEquipped = includeEquipped;
			obj.includeTainted = includeTainted;
			obj.includeFromZone = includeFromZone;
			obj.hpRange = hpRange;
			obj.qualityRange = qualityRange;
			obj.limitToAllowedStuff = limitToAllowedStuff;
			obj.paused = paused;
			return obj;
		}
	}
}
