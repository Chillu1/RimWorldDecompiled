using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ITab_ContentsGenepackHolder : ITab_ContentsBase
{
	private static readonly CachedTexture DropTex = new CachedTexture("UI/Buttons/Drop");

	private const float MiniGeneIconSize = 22f;

	public override IList<Thing> container => ContainerThing.innerContainer;

	public CompGenepackContainer ContainerThing => base.SelThing.TryGetComp<CompGenepackContainer>();

	public ITab_ContentsGenepackHolder()
	{
		labelKey = "TabCasketContents";
		containedItemsKey = "TabCasketContents";
	}

	public override void OnOpen()
	{
		if (!ModLister.CheckBiotech("genepack container"))
		{
			CloseTab();
		}
	}

	protected override void DoItemsLists(Rect inRect, ref float curY)
	{
		CompGenepackContainer containerThing = ContainerThing;
		bool autoLoad = containerThing.autoLoad;
		Rect rect = new Rect(inRect.x, inRect.y, inRect.width, 24f);
		Widgets.CheckboxLabeled(rect, "AllowAllGenepacks".Translate(), ref containerThing.autoLoad);
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegionByKey(rect, "AllowAllGenepacksDesc");
		}
		if (autoLoad != containerThing.autoLoad)
		{
			containerThing.leftToLoad.Clear();
		}
		curY += 28f;
		ListContainedGenepacks(inRect, containerThing, ref curY);
		if (!containerThing.autoLoad)
		{
			ListGenepacksToLoad(inRect, containerThing, ref curY);
			ListGenepacksOnMap(inRect, containerThing, ref curY);
		}
	}

	private void ListContainedGenepacks(Rect inRect, CompGenepackContainer container, ref float curY)
	{
		GUI.BeginGroup(inRect);
		float num = curY;
		Widgets.ListSeparator(ref curY, inRect.width, containedItemsKey.Translate());
		Rect rect = new Rect(0f, num, inRect.width, curY - num - 3f);
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegionByKey(rect, "ContainedGenepacksDesc");
		}
		List<Genepack> containedGenepacks = container.ContainedGenepacks;
		bool flag = false;
		for (int i = 0; i < containedGenepacks.Count; i++)
		{
			Genepack genepack = containedGenepacks[i];
			if (genepack != null)
			{
				flag = true;
				DoRow(genepack, container, inRect.width, ref curY, insideContainer: true);
			}
		}
		if (!flag)
		{
			Widgets.NoneLabel(ref curY, inRect.width);
		}
		GUI.EndGroup();
	}

	private void ListGenepacksToLoad(Rect inRect, CompGenepackContainer container, ref float curY)
	{
		bool flag = false;
		GUI.BeginGroup(inRect);
		float num = curY;
		Widgets.ListSeparator(ref curY, inRect.width, "GenepacksToLoad".Translate());
		Rect rect = new Rect(0f, num, inRect.width, curY - num - 3f);
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegionByKey(rect, "GenepacksToLoadDesc");
		}
		if (container.leftToLoad != null)
		{
			for (int num2 = container.leftToLoad.Count - 1; num2 >= 0; num2--)
			{
				if (!(container.leftToLoad[num2] is Genepack { Destroyed: false } genepack) || genepack.MapHeld != container.parent.Map || !genepack.AutoLoad)
				{
					container.leftToLoad.RemoveAt(num2);
				}
				else
				{
					DoRow(genepack, container, inRect.width, ref curY, insideContainer: false);
					flag = true;
				}
			}
		}
		if (!flag)
		{
			Widgets.NoneLabel(ref curY, inRect.width);
		}
		GUI.EndGroup();
	}

	private void ListGenepacksOnMap(Rect inRect, CompGenepackContainer container, ref float curY)
	{
		bool flag = false;
		GUI.BeginGroup(inRect);
		float num = curY;
		Widgets.ListSeparator(ref curY, inRect.width, "GenepacksToIgnore".Translate());
		Rect rect = new Rect(0f, num, inRect.width, curY - num - 3f);
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegionByKey(rect, "GenepacksIgnoredDesc");
		}
		List<Thing> list = container.parent.Map.listerThings.ThingsOfDef(ThingDefOf.Genepack);
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			Thing thing = list[num2];
			if (thing != null)
			{
				Genepack genepack = (Genepack)thing;
				if (genepack.targetContainer == null && genepack.AutoLoad)
				{
					DoRow((Genepack)thing, container, inRect.width, ref curY, insideContainer: false);
					flag = true;
				}
			}
		}
		if (!flag)
		{
			Widgets.NoneLabel(ref curY, inRect.width);
		}
		GUI.EndGroup();
	}

	private void DoRow(Genepack genepack, CompGenepackContainer container, float width, ref float curY, bool insideContainer)
	{
		bool checkOn = container.leftToLoad.Contains(genepack);
		bool flag = checkOn;
		Rect rect = new Rect(0f, curY, width, 28f);
		Rect rect2 = new Rect(rect.width - 24f, curY, 24f, 24f);
		if (insideContainer)
		{
			if (Widgets.ButtonImage(rect2, DropTex.Texture))
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmRemoveGenepack".Translate(genepack.LabelNoCount), delegate
				{
					OnDropThing(genepack, genepack.stackCount);
				}));
			}
			TooltipHandler.TipRegionByKey(rect2, "EjectGenepackDesc");
		}
		else
		{
			Widgets.Checkbox(rect.width - 24f, curY, ref checkOn);
			string key = (checkOn ? "RemoveFromLoadingListDesc" : "AddToLoadingListDesc");
			TooltipHandler.TipRegionByKey(rect2, key);
		}
		rect.width -= 24f;
		for (int num = Mathf.Min(genepack.GeneSet.GenesListForReading.Count, 5) - 1; num >= 0; num--)
		{
			GeneDef geneDef = genepack.GeneSet.GenesListForReading[num];
			Rect rect3 = new Rect(rect.xMax - 22f, rect.yMax - rect.height / 2f - 11f, 22f, 22f);
			Widgets.DefIcon(rect3, geneDef, null, 0.75f);
			Rect rect4 = rect3;
			rect4.yMin = rect.yMin;
			rect4.yMax = rect.yMax;
			if (Mouse.IsOver(rect4))
			{
				Widgets.DrawHighlight(rect4);
				TooltipHandler.TipRegion(rect4, geneDef.LabelCap + "\n\n" + geneDef.DescriptionFull);
			}
			rect.xMax -= 22f;
		}
		Widgets.InfoCardButton(0f, curY, genepack);
		if (Mouse.IsOver(rect))
		{
			GUI.color = ITab_ContentsBase.ThingHighlightColor;
			GUI.DrawTexture(rect, TexUI.HighlightTex);
		}
		Widgets.ThingIcon(new Rect(24f, curY, 28f, 28f), genepack);
		Rect rect5 = new Rect(60f, curY, rect.width - 36f, rect.height);
		Text.Anchor = TextAnchor.MiddleLeft;
		Widgets.Label(rect5, genepack.LabelCap.Truncate(rect5.width));
		Text.Anchor = TextAnchor.UpperLeft;
		if (Mouse.IsOver(rect))
		{
			TargetHighlighter.Highlight(genepack, arrow: true, colonistBar: false);
			TooltipHandler.TipRegion(rect, genepack.LabelCap);
		}
		curY += 28f;
		if (flag != checkOn)
		{
			if (!checkOn)
			{
				genepack.targetContainer = null;
				container.leftToLoad.Remove(genepack);
			}
			else if (!container.CanLoadMore)
			{
				Messages.Message("CanOnlyStoreNumGenepacks".Translate(container.parent, container.Props.maxCapacity).CapitalizeFirst(), container.parent, MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				genepack.targetContainer = container.parent;
				container.leftToLoad.Add(genepack);
			}
		}
	}
}
