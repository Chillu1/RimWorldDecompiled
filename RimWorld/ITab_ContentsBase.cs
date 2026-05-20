using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class ITab_ContentsBase : ITab
{
	private Vector2 scrollPosition;

	private float lastDrawnHeight;

	private List<Thing> thingsToSelect = new List<Thing>();

	public bool canRemoveThings = true;

	protected static List<Thing> tmpSingleThing = new List<Thing>();

	protected const float TopPadding = 20f;

	protected const float SpaceBetweenItemsLists = 10f;

	protected const float ThingRowHeight = 28f;

	protected const float ThingIconSize = 28f;

	protected const float ThingLeftX = 36f;

	protected static readonly Color ThingLabelColor = ITab_Pawn_Gear.ThingLabelColor;

	protected static readonly Color ThingHighlightColor = ITab_Pawn_Gear.HighlightColor;

	public string containedItemsKey;

	public abstract IList<Thing> container { get; }

	public override bool IsVisible => base.SelThing.Faction == Faction.OfPlayer;

	public virtual IntVec3 DropOffset => IntVec3.Zero;

	public virtual bool UseDiscardMessage => true;

	public ITab_ContentsBase()
	{
		size = new Vector2(460f, 450f);
	}

	protected override void FillTab()
	{
		thingsToSelect.Clear();
		Rect outRect = new Rect(default(Vector2), size).ContractedBy(10f);
		outRect.yMin += 20f;
		Rect rect = new Rect(0f, 0f, outRect.width - 16f, Mathf.Max(lastDrawnHeight, outRect.height));
		Text.Font = GameFont.Small;
		Widgets.BeginScrollView(outRect, ref scrollPosition, rect);
		float curY = 0f;
		DoItemsLists(rect, ref curY);
		lastDrawnHeight = curY;
		Widgets.EndScrollView();
		if (thingsToSelect.Any())
		{
			ITab_Pawn_FormingCaravan.SelectNow(thingsToSelect);
			thingsToSelect.Clear();
		}
	}

	protected virtual void DoItemsLists(Rect inRect, ref float curY)
	{
		Widgets.BeginGroup(inRect);
		Widgets.ListSeparator(ref curY, inRect.width, containedItemsKey.Translate());
		IList<Thing> list = container;
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			Thing t = list[i];
			if (t != null)
			{
				flag = true;
				tmpSingleThing.Clear();
				tmpSingleThing.Add(t);
				DoThingRow(t.def, t.stackCount, tmpSingleThing, inRect.width, ref curY, delegate(int x)
				{
					OnDropThing(t, x);
				});
				tmpSingleThing.Clear();
			}
		}
		if (!flag)
		{
			Widgets.NoneLabel(ref curY, inRect.width);
		}
		Widgets.EndGroup();
	}

	protected virtual void OnDropThing(Thing t, int count)
	{
		GenDrop.TryDropSpawn(t.SplitOff(count), base.SelThing.Position + DropOffset, base.SelThing.Map, ThingPlaceMode.Near, out var _);
	}

	protected void DoThingRow(ThingDef thingDef, int count, List<Thing> things, float width, ref float curY, Action<int> discardAction)
	{
		Rect rect = new Rect(0f, curY, width, 28f);
		if (canRemoveThings)
		{
			if (count != 1 && Widgets.ButtonImage(new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f), CaravanThingsTabUtility.AbandonSpecificCountButtonTex))
			{
				Find.WindowStack.Add(new Dialog_Slider("RemoveSliderText".Translate(thingDef.label), 1, count, discardAction));
			}
			rect.width -= 24f;
			if (Widgets.ButtonImage(new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f), CaravanThingsTabUtility.AbandonButtonTex))
			{
				if (UseDiscardMessage)
				{
					string text = thingDef.label;
					if (things.Count == 1 && things[0] is Pawn)
					{
						text = ((Pawn)things[0]).LabelShortCap;
					}
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmRemoveItemDialog".Translate(text), delegate
					{
						discardAction(count);
					}));
				}
				else
				{
					discardAction(count);
				}
			}
			rect.width -= 24f;
		}
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
		Widgets.Label(rect3, text2.StripTags().Truncate(rect3.width));
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
	}

	private void SelectLater(List<Thing> things)
	{
		thingsToSelect.Clear();
		thingsToSelect.AddRange(things);
	}
}
