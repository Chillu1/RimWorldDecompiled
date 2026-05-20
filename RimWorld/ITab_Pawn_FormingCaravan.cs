using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class ITab_Pawn_FormingCaravan : ITab
{
	private Vector2 scrollPosition;

	private float lastDrawnHeight;

	private List<Thing> thingsToSelect = new List<Thing>();

	private static List<Thing> tmpSingleThing = new List<Thing>();

	private const float TopPadding = 20f;

	private const float StandardLineHeight = 22f;

	private const float ExtraSpaceBetweenSections = 4f;

	private const float SpaceBetweenItemsLists = 10f;

	private const float ThingRowHeight = 28f;

	private const float ThingIconSize = 28f;

	private const float ThingLeftX = 36f;

	private static readonly Color ThingLabelColor = ITab_Pawn_Gear.ThingLabelColor;

	private static readonly Color ThingHighlightColor = ITab_Pawn_Gear.HighlightColor;

	private static List<Thing> tmpPawns = new List<Thing>();

	public override bool IsVisible => SelPawn.IsFormingCaravan();

	public ITab_Pawn_FormingCaravan()
	{
		size = new Vector2(480f, 450f);
		labelKey = "TabFormingCaravan";
	}

	protected override void FillTab()
	{
		thingsToSelect.Clear();
		Rect outRect = new Rect(default(Vector2), size).ContractedBy(10f);
		outRect.yMin += 20f;
		Rect rect = new Rect(0f, 0f, outRect.width - 16f, Mathf.Max(lastDrawnHeight, outRect.height));
		Widgets.BeginScrollView(outRect, ref scrollPosition, rect);
		float num = 0f;
		string status = ((LordJob_FormAndSendCaravan)SelPawn.GetLord().LordJob).Status;
		Widgets.Label(new Rect(0f, num, rect.width, 100f), status);
		num += 22f;
		num += 4f;
		DoPeopleAndAnimals(rect, ref num);
		num += 4f;
		DoItemsLists(rect, ref num);
		lastDrawnHeight = num;
		Widgets.EndScrollView();
		if (thingsToSelect.Any())
		{
			SelectNow(thingsToSelect);
			thingsToSelect.Clear();
		}
	}

	public override void TabUpdate()
	{
		base.TabUpdate();
		if (SelPawn != null && SelPawn.GetLord() != null)
		{
			Lord lord = SelPawn.GetLord();
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				TargetHighlighter.Highlight(lord.ownedPawns[i], arrow: false, colonistBar: false, circleOverlay: true);
			}
		}
	}

	private void DoPeopleAndAnimals(Rect inRect, ref float curY)
	{
		Widgets.ListSeparator(ref curY, inRect.width, "CaravanMembers".Translate());
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		Lord lord = SelPawn.GetLord();
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			Pawn pawn = lord.ownedPawns[i];
			if (pawn.IsFreeColonist)
			{
				num++;
				if (pawn.InMentalState)
				{
					num2++;
				}
			}
			else if (pawn.IsPrisoner)
			{
				num3++;
				if (pawn.InMentalState)
				{
					num4++;
				}
			}
			else if (pawn.RaceProps.Animal)
			{
				num5++;
				if (pawn.InMentalState)
				{
					num6++;
				}
				if (pawn.RaceProps.packAnimal)
				{
					num7++;
				}
			}
		}
		string pawnsCountLabel = GetPawnsCountLabel(num, num2, -1);
		string pawnsCountLabel2 = GetPawnsCountLabel(num3, num4, -1);
		string pawnsCountLabel3 = GetPawnsCountLabel(num5, num6, num7);
		float y = curY;
		DoPeopleAndAnimalsEntry(inRect, Faction.OfPlayer.def.pawnsPlural.CapitalizeFirst(), pawnsCountLabel, ref curY, out var drawnWidth);
		float y2 = curY;
		DoPeopleAndAnimalsEntry(inRect, "CaravanPrisoners".Translate(), pawnsCountLabel2, ref curY, out var drawnWidth2);
		float y3 = curY;
		DoPeopleAndAnimalsEntry(inRect, "CaravanAnimals".Translate(), pawnsCountLabel3, ref curY, out var drawnWidth3);
		float width = Mathf.Max(drawnWidth, drawnWidth2, drawnWidth3) + 2f;
		Rect rect = new Rect(0f, y, width, 22f);
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			HighlightColonists();
		}
		if (Widgets.ButtonInvisible(rect))
		{
			SelectColonistsLater();
		}
		Rect rect2 = new Rect(0f, y2, width, 22f);
		if (Mouse.IsOver(rect2))
		{
			Widgets.DrawHighlight(rect2);
			HighlightPrisoners();
		}
		if (Widgets.ButtonInvisible(rect2))
		{
			SelectPrisonersLater();
		}
		Rect rect3 = new Rect(0f, y3, width, 22f);
		if (Mouse.IsOver(rect3))
		{
			Widgets.DrawHighlight(rect3);
			HighlightAnimals();
		}
		if (Widgets.ButtonInvisible(rect3))
		{
			SelectAnimalsLater();
		}
	}

	private void DoPeopleAndAnimalsEntry(Rect inRect, string leftLabel, string rightLabel, ref float curY, out float drawnWidth)
	{
		Rect rect = new Rect(0f, curY, inRect.width, 100f);
		Widgets.Label(rect, leftLabel);
		rect.xMin += 120f;
		Widgets.Label(rect, rightLabel);
		curY += 22f;
		drawnWidth = 120f + Text.CalcSize(rightLabel).x;
	}

	private void DoItemsLists(Rect inRect, ref float curY)
	{
		LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan = (LordJob_FormAndSendCaravan)SelPawn.GetLord().LordJob;
		Rect rect = new Rect(0f, curY, (inRect.width - 10f) / 2f, inRect.height);
		float curY2 = 0f;
		Widgets.BeginGroup(rect);
		Widgets.ListSeparator(ref curY2, rect.width, "ItemsToLoad".Translate());
		bool flag = false;
		for (int i = 0; i < lordJob_FormAndSendCaravan.transferables.Count; i++)
		{
			TransferableOneWay transferableOneWay = lordJob_FormAndSendCaravan.transferables[i];
			if (transferableOneWay.CountToTransfer > 0 && transferableOneWay.HasAnyThing)
			{
				flag = true;
				DoThingRow(transferableOneWay.AnyThing.GetInnerIfMinified().def, transferableOneWay.CountToTransfer, transferableOneWay.things, rect.width, ref curY2);
			}
		}
		if (!flag)
		{
			Widgets.NoneLabel(ref curY2, rect.width);
		}
		Widgets.EndGroup();
		Rect rect2 = new Rect((inRect.width + 10f) / 2f, curY, (inRect.width - 10f) / 2f, inRect.height);
		float curY3 = 0f;
		Widgets.BeginGroup(rect2);
		Widgets.ListSeparator(ref curY3, rect2.width, "LoadedItems".Translate());
		bool flag2 = false;
		for (int j = 0; j < lordJob_FormAndSendCaravan.lord.ownedPawns.Count; j++)
		{
			Pawn pawn = lordJob_FormAndSendCaravan.lord.ownedPawns[j];
			if (!pawn.inventory.UnloadEverything)
			{
				for (int k = 0; k < pawn.inventory.innerContainer.Count; k++)
				{
					Thing thing = pawn.inventory.innerContainer[k];
					flag2 = true;
					tmpSingleThing.Clear();
					tmpSingleThing.Add(thing);
					DoThingRow(thing.def, thing.stackCount, tmpSingleThing, rect2.width, ref curY3);
					tmpSingleThing.Clear();
				}
			}
		}
		if (!flag2)
		{
			Widgets.NoneLabel(ref curY3, rect.width);
		}
		Widgets.EndGroup();
		curY += Mathf.Max(curY2, curY3);
	}

	private void SelectColonistsLater()
	{
		Lord lord = SelPawn.GetLord();
		tmpPawns.Clear();
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			if (lord.ownedPawns[i].IsFreeColonist)
			{
				tmpPawns.Add(lord.ownedPawns[i]);
			}
		}
		SelectLater(tmpPawns);
		tmpPawns.Clear();
	}

	private void HighlightColonists()
	{
		Lord lord = SelPawn.GetLord();
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			if (lord.ownedPawns[i].IsFreeColonist)
			{
				TargetHighlighter.Highlight(lord.ownedPawns[i]);
			}
		}
	}

	private void SelectPrisonersLater()
	{
		Lord lord = SelPawn.GetLord();
		tmpPawns.Clear();
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			if (lord.ownedPawns[i].IsPrisoner)
			{
				tmpPawns.Add(lord.ownedPawns[i]);
			}
		}
		SelectLater(tmpPawns);
		tmpPawns.Clear();
	}

	private void HighlightPrisoners()
	{
		Lord lord = SelPawn.GetLord();
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			if (lord.ownedPawns[i].IsPrisoner)
			{
				TargetHighlighter.Highlight(lord.ownedPawns[i]);
			}
		}
	}

	private void SelectAnimalsLater()
	{
		Lord lord = SelPawn.GetLord();
		tmpPawns.Clear();
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			if (lord.ownedPawns[i].RaceProps.Animal)
			{
				tmpPawns.Add(lord.ownedPawns[i]);
			}
		}
		SelectLater(tmpPawns);
		tmpPawns.Clear();
	}

	private void HighlightAnimals()
	{
		Lord lord = SelPawn.GetLord();
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			if (lord.ownedPawns[i].RaceProps.Animal)
			{
				TargetHighlighter.Highlight(lord.ownedPawns[i]);
			}
		}
	}

	private void SelectLater(List<Thing> things)
	{
		thingsToSelect.Clear();
		thingsToSelect.AddRange(things);
	}

	public static void SelectNow(List<Thing> things)
	{
		if (!things.Any())
		{
			return;
		}
		Find.Selector.ClearSelection();
		bool flag = false;
		for (int i = 0; i < things.Count; i++)
		{
			if (things[i].SpawnedOrAnyParentSpawned)
			{
				if (!flag)
				{
					CameraJumper.TryJump(things[i]);
				}
				Find.Selector.Select(things[i]);
				flag = true;
			}
		}
		if (!flag)
		{
			CameraJumper.TryJumpAndSelect(things[0]);
		}
	}

	private string GetPawnsCountLabel(int count, int countInMentalState, int countPackAnimals)
	{
		string text = count.ToString();
		bool flag = countInMentalState > 0;
		bool flag2 = count > 0 && countPackAnimals >= 0;
		if (flag || flag2)
		{
			text += " (";
			if (flag2)
			{
				text += countPackAnimals + " " + "PackAnimalsCountLower".Translate();
			}
			if (flag)
			{
				if (flag2)
				{
					text += ", ";
				}
				text += countInMentalState + " " + "InMentalStateCountLower".Translate();
			}
			text += ")";
		}
		return text;
	}

	private void EndJobForEveryoneHauling(TransferableOneWay t, LordJob_FormAndSendCaravan lordJob)
	{
		List<Pawn> freeColonistsSpawned = base.SelThing.Map.mapPawns.FreeColonistsSpawned;
		for (int i = 0; i < freeColonistsSpawned.Count; i++)
		{
			if (freeColonistsSpawned[i].CurJobDef == JobDefOf.PrepareCaravan_GatherItems && freeColonistsSpawned[i].jobs.curDriver is JobDriver_PrepareCaravan_GatherItems jobDriver_PrepareCaravan_GatherItems && jobDriver_PrepareCaravan_GatherItems.job.lord == lordJob.lord && jobDriver_PrepareCaravan_GatherItems.ToHaul != null && jobDriver_PrepareCaravan_GatherItems.ToHaul.def == t.ThingDef)
			{
				freeColonistsSpawned[i].jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
	}

	private void DropThingToLoad(TransferableOneWay t, int count)
	{
		t.ForceTo(t.CountToTransfer - count);
		EndJobForEveryoneHauling(t, (LordJob_FormAndSendCaravan)SelPawn.GetLord().LordJob);
	}

	private void DoThingRow(ThingDef thingDef, int count, List<Thing> things, float width, ref float curY)
	{
		Rect rect = new Rect(0f, curY, width, 28f);
		Thing singleThingForClosure = ((things.Count == 1) ? things[0] : null);
		if (count != 1 && Widgets.ButtonImage(new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f), CaravanThingsTabUtility.AbandonSpecificCountButtonTex))
		{
			Find.WindowStack.Add(new Dialog_Slider("RemoveSliderText".Translate(thingDef.label), 1, count, DoDropThing));
		}
		rect.width -= 24f;
		if (Widgets.ButtonImage(new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f), CaravanThingsTabUtility.AbandonButtonTex))
		{
			string text = thingDef.label;
			if (things.Count == 1 && things[0] is Pawn)
			{
				text = ((Pawn)things[0]).LabelShortCap;
			}
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmRemoveItemDialog".Translate(text), delegate
			{
				DoDropThing(count);
			}));
		}
		rect.width -= 24f;
		if (things.Count == 1)
		{
			Widgets.InfoCardButton(rect.width - 24f, curY, things[0]);
		}
		else
		{
			Widgets.InfoCardButton(rect.width - 24f, curY, thingDef);
		}
		rect.width -= 24f;
		if (Mouse.IsOver(rect))
		{
			GUI.color = ThingHighlightColor;
			GUI.DrawTexture(rect, TexUI.HighlightTex);
		}
		if (thingDef.DrawMatSingle != null && thingDef.DrawMatSingle.mainTexture != null)
		{
			Rect rect2 = new Rect(4f, curY, 28f, 28f);
			if (things.Count == 1)
			{
				Widgets.ThingIcon(rect2, things[0]);
			}
			else
			{
				Widgets.ThingIcon(rect2, thingDef);
			}
		}
		Text.Anchor = TextAnchor.MiddleLeft;
		GUI.color = ThingLabelColor;
		Rect rect3 = new Rect(36f, curY, rect.width - 36f, rect.height);
		string text2 = ((things.Count == 1 && count == things[0].stackCount) ? things[0].LabelCap : ((thingDef != ThingDefOf.MinifiedThing) ? GenLabel.ThingLabel(thingDef, null, count).CapitalizeFirst() : GenLabel.ThingLabel(things[0].GetInnerIfMinified(), count).CapitalizeFirst()));
		Text.WordWrap = false;
		Widgets.Label(rect3, text2.Truncate(rect3.width));
		Text.WordWrap = true;
		Text.Anchor = TextAnchor.UpperLeft;
		TooltipHandler.TipRegion(rect, text2);
		if (Widgets.ButtonInvisible(rect))
		{
			SelectLater(things);
		}
		if (Mouse.IsOver(rect))
		{
			for (int num = 0; num < things.Count; num++)
			{
				TargetHighlighter.Highlight(things[num]);
			}
		}
		curY += 28f;
		void DoDropThing(int count2)
		{
			LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan = (LordJob_FormAndSendCaravan)SelPawn.GetLord().LordJob;
			Thing resultingThing;
			if (singleThingForClosure != null && singleThingForClosure.ParentHolder is Pawn_CarryTracker pawn_CarryTracker)
			{
				pawn_CarryTracker.TryDropCarriedThing(pawn_CarryTracker.pawn.PositionHeld, count2, ThingPlaceMode.Near, out resultingThing);
			}
			else if (singleThingForClosure != null && singleThingForClosure.ParentHolder is Pawn_InventoryTracker pawn_InventoryTracker)
			{
				pawn_InventoryTracker.innerContainer.TryDrop(singleThingForClosure, pawn_InventoryTracker.pawn.PositionHeld, pawn_InventoryTracker.pawn.MapHeld, ThingPlaceMode.Near, count2, out resultingThing);
			}
			else
			{
				List<TransferableOneWay> transferables = lordJob_FormAndSendCaravan.transferables;
				TransferableOneWay transferableOneWay = null;
				for (int i = 0; i < transferables.Count; i++)
				{
					if (transferables[i].CountToTransfer > 0 && transferables[i].AnyThing.def == thingDef)
					{
						transferableOneWay = transferables[i];
						break;
					}
				}
				if (transferableOneWay != null)
				{
					DropThingToLoad(transferableOneWay, count2);
				}
			}
		}
	}
}
