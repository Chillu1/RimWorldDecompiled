using System;
using System.Collections.Generic;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class GeneUIUtility
{
	private static List<GeneDef> geneDefs = new List<GeneDef>();

	private static List<Gene> xenogenes = new List<Gene>();

	private static List<Gene> endogenes = new List<Gene>();

	private static float xenogenesHeight;

	private static float endogenesHeight;

	private static float scrollHeight;

	private static int gcx;

	private static int met;

	private static int arc;

	private static readonly Color CapsuleBoxColor = new Color(0.25f, 0.25f, 0.25f);

	private static readonly Color CapsuleBoxColorOverridden = new Color(0.15f, 0.15f, 0.15f);

	private static readonly CachedTexture GeneBackground_Archite = new CachedTexture("UI/Icons/Genes/GeneBackground_ArchiteGene");

	private static readonly CachedTexture GeneBackground_Xenogene = new CachedTexture("UI/Icons/Genes/GeneBackground_Xenogene");

	private static readonly CachedTexture GeneBackground_Endogene = new CachedTexture("UI/Icons/Genes/GeneBackground_Endogene");

	private const float OverriddenGeneIconAlpha = 0.75f;

	private const float XenogermIconSize = 34f;

	private const float XenotypeLabelWidth = 140f;

	private const float GeneGap = 6f;

	private const float GeneSize = 90f;

	public const float BiostatsWidth = 38f;

	public static void DrawGenesInfo(Rect rect, Thing target, float initialHeight, ref Vector2 size, ref Vector2 scrollPosition, GeneSet pregnancyGenes = null)
	{
		Rect rect2 = rect;
		Rect position = rect2.ContractedBy(10f);
		if (Prefs.DevMode)
		{
			DoDebugButton(new Rect(rect2.xMax - 18f - 125f, 5f, 115f, Text.LineHeight), target, pregnancyGenes);
		}
		GUI.BeginGroup(position);
		float num = BiostatsTable.HeightForBiostats(arc);
		Rect rect3 = new Rect(0f, 0f, position.width, position.height - num - 12f);
		DrawGeneSections(rect3, target, pregnancyGenes, ref scrollPosition);
		Rect rect4 = new Rect(0f, rect3.yMax + 6f, position.width - 140f - 4f, num);
		rect4.yMax = rect3.yMax + num + 6f;
		if (!(target is Pawn))
		{
			rect4.width = position.width;
		}
		BiostatsTable.Draw(rect4, gcx, met, arc, drawMax: false, ignoreLimits: false);
		TryDrawXenotype(target, rect4.xMax + 4f, rect4.y + Text.LineHeight / 2f);
		if (Event.current.type == EventType.Layout)
		{
			float num2 = endogenesHeight + xenogenesHeight + num + 12f + 70f;
			if (num2 > initialHeight)
			{
				size.y = Mathf.Min(num2, (float)(UI.screenHeight - 35) - 165f - 30f);
			}
			else
			{
				size.y = initialHeight;
			}
			xenogenesHeight = 0f;
			endogenesHeight = 0f;
		}
		GUI.EndGroup();
	}

	private static void DrawGeneSections(Rect rect, Thing target, GeneSet genesOverride, ref Vector2 scrollPosition)
	{
		RecacheGenes(target, genesOverride);
		GUI.BeginGroup(rect);
		Rect rect2 = new Rect(0f, 0f, rect.width - 16f, scrollHeight);
		float curY = 0f;
		Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, rect2);
		Rect containingRect = rect2;
		containingRect.y = scrollPosition.y;
		containingRect.height = rect.height;
		if (target is Pawn)
		{
			if (endogenes.Any())
			{
				DrawSection(rect, xeno: false, endogenes.Count, ref curY, ref endogenesHeight, delegate(int i, Rect r)
				{
					DrawGene(endogenes[i], r, GeneType.Endogene);
				}, containingRect);
				curY += 12f;
			}
			DrawSection(rect, xeno: true, xenogenes.Count, ref curY, ref xenogenesHeight, delegate(int i, Rect r)
			{
				DrawGene(xenogenes[i], r, GeneType.Xenogene);
			}, containingRect);
		}
		else
		{
			GeneType geneType = ((genesOverride == null && !(target is HumanEmbryo)) ? GeneType.Xenogene : GeneType.Endogene);
			DrawSection(rect, geneType == GeneType.Xenogene, geneDefs.Count, ref curY, ref xenogenesHeight, delegate(int i, Rect r)
			{
				DrawGeneDef(geneDefs[i], r, geneType);
			}, containingRect);
		}
		if (Event.current.type == EventType.Layout)
		{
			scrollHeight = curY;
		}
		Widgets.EndScrollView();
		GUI.EndGroup();
	}

	private static void RecacheGenes(Thing target, GeneSet genesOverride)
	{
		geneDefs.Clear();
		xenogenes.Clear();
		endogenes.Clear();
		gcx = 0;
		met = 0;
		arc = 0;
		Pawn pawn = target as Pawn;
		GeneSet geneSet = (target as GeneSetHolderBase)?.GeneSet ?? genesOverride;
		if (pawn != null)
		{
			foreach (Gene xenogene in pawn.genes.Xenogenes)
			{
				if (!xenogene.Overridden)
				{
					AddBiostats(xenogene.def);
				}
				xenogenes.Add(xenogene);
			}
			foreach (Gene endogene in pawn.genes.Endogenes)
			{
				if (endogene.def.endogeneCategory != EndogeneCategory.Melanin || !pawn.genes.Endogenes.Any((Gene x) => x.def.skinColorOverride.HasValue))
				{
					if (!endogene.Overridden)
					{
						AddBiostats(endogene.def);
					}
					endogenes.Add(endogene);
				}
			}
			xenogenes.SortGenes();
			endogenes.SortGenes();
		}
		else
		{
			if (geneSet == null)
			{
				return;
			}
			foreach (GeneDef item in geneSet.GenesListForReading)
			{
				geneDefs.Add(item);
			}
			gcx = geneSet.ComplexityTotal;
			met = geneSet.MetabolismTotal;
			arc = geneSet.ArchitesTotal;
			geneDefs.SortGeneDefs();
		}
		static void AddBiostats(GeneDef gene)
		{
			gcx += gene.biostatCpx;
			met += gene.biostatMet;
			arc += gene.biostatArc;
		}
	}

	private static void DrawSection(Rect rect, bool xeno, int count, ref float curY, ref float sectionHeight, Action<int, Rect> drawer, Rect containingRect)
	{
		Widgets.Label(10f, ref curY, rect.width, (xeno ? "Xenogenes" : "Endogenes").Translate().CapitalizeFirst(), (xeno ? "XenogenesDesc" : "EndogenesDesc").Translate());
		float num = curY;
		Rect rect2 = new Rect(rect.x, curY, rect.width, sectionHeight);
		if (xeno && count == 0)
		{
			Text.Anchor = TextAnchor.UpperCenter;
			GUI.color = ColoredText.SubtleGrayColor;
			rect2.height = Text.LineHeight;
			Widgets.Label(rect2, "(" + "NoXenogermImplanted".Translate() + ")");
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			curY += 90f;
		}
		else
		{
			Widgets.DrawMenuSection(rect2);
			float num2 = (rect.width - 12f - 630f - 36f) / 2f;
			curY += num2;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < count; i++)
			{
				if (num4 >= 6)
				{
					num4 = 0;
					num3++;
				}
				else if (i > 0)
				{
					num4++;
				}
				Rect rect3 = new Rect(num2 + (float)num4 * 90f + (float)num4 * 6f, curY + (float)num3 * 90f + (float)num3 * 6f, 90f, 90f);
				if (containingRect.Overlaps(rect3))
				{
					drawer(i, rect3);
				}
			}
			curY += (float)(num3 + 1) * 90f + (float)num3 * 6f + num2;
		}
		if (Event.current.type == EventType.Layout)
		{
			sectionHeight = curY - num;
		}
	}

	private static void TryDrawXenotype(Thing target, float x, float y)
	{
		Pawn sourcePawn = target as Pawn;
		if (sourcePawn == null)
		{
			return;
		}
		Rect rect = new Rect(x, y, 140f, Text.LineHeight);
		Text.Anchor = TextAnchor.UpperCenter;
		Widgets.Label(rect, sourcePawn.genes.XenotypeLabelCap);
		Text.Anchor = TextAnchor.UpperLeft;
		Rect position = new Rect(rect.center.x - 17f, rect.yMax + 4f, 34f, 34f);
		GUI.color = XenotypeDef.IconColor;
		GUI.DrawTexture(position, sourcePawn.genes.XenotypeIcon);
		GUI.color = Color.white;
		rect.yMax = position.yMax;
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegion(rect, () => ("Xenotype".Translate() + ": " + sourcePawn.genes.XenotypeLabelCap).Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + sourcePawn.genes.XenotypeDescShort, 883938493);
		}
		if (Widgets.ButtonInvisible(rect) && !sourcePawn.genes.UniqueXenotype)
		{
			Find.WindowStack.Add(new Dialog_InfoCard(sourcePawn.genes.Xenotype));
		}
	}

	private static void DoDebugButton(Rect buttonRect, Thing target, GeneSet genesOverride)
	{
		Pawn sourcePawn = target as Pawn;
		GeneSet geneSet = (target as GeneSetHolderBase)?.GeneSet ?? genesOverride;
		if (!Widgets.ButtonText(buttonRect, "Devtool..."))
		{
			return;
		}
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		string label = ((genesOverride != null || target is HumanEmbryo) ? "Add gene" : "Add xenogene");
		list.Add(new FloatMenuOption(label, delegate
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugToolsPawns.Options_AddGene(delegate(GeneDef geneDef)
			{
				AddGene(geneDef, xeno: true);
			})));
		}));
		if (sourcePawn != null)
		{
			list.Add(new FloatMenuOption("Add endogene", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugToolsPawns.Options_AddGene(delegate(GeneDef geneDef)
				{
					AddGene(geneDef, xeno: false);
				})));
			}));
			if (xenogenes.Any() || endogenes.Any())
			{
				list.Add(new FloatMenuOption("Remove gene", delegate
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					List<Gene> list3 = new List<Gene>();
					list3.AddRange(endogenes);
					list3.AddRange(xenogenes);
					foreach (Gene item in list3)
					{
						Gene gene = item;
						list2.Add(new DebugMenuOption(gene.LabelCap, DebugMenuOptionMode.Action, delegate
						{
							sourcePawn.genes.RemoveGene(gene);
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}));
			}
			list.Add(new FloatMenuOption("Add all genes (xenogene)", delegate
			{
				sourcePawn.genes.Debug_AddAllGenes(xenogene: true);
			}));
			list.Add(new FloatMenuOption("Add all genes (endogene)", delegate
			{
				sourcePawn.genes.Debug_AddAllGenes(xenogene: false);
			}));
			list.Add(new FloatMenuOption("Apply xenotype", delegate
			{
				List<DebugMenuOption> list2 = new List<DebugMenuOption>();
				foreach (XenotypeDef allDef in DefDatabase<XenotypeDef>.AllDefs)
				{
					XenotypeDef xenotype = allDef;
					list2.Add(new DebugMenuOption(xenotype.LabelCap, DebugMenuOptionMode.Action, delegate
					{
						sourcePawn.genes.SetXenotype(xenotype);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
			}));
			if (!sourcePawn.genes.UniqueXenotype)
			{
				list.Add(new FloatMenuOption("Reset genes to base xenotype", delegate
				{
					List<Gene> list2 = sourcePawn.genes.Endogenes;
					for (int num = list2.Count - 1; num >= 0; num--)
					{
						Gene gene = list2[num];
						if (gene.def.endogeneCategory != EndogeneCategory.Melanin && gene.def.endogeneCategory != EndogeneCategory.HairColor)
						{
							sourcePawn.genes.RemoveGene(gene);
						}
					}
					sourcePawn.genes.SetXenotype(sourcePawn.genes.Xenotype);
				}));
			}
		}
		else if (geneDefs.Any() && geneSet != null)
		{
			list.Add(new FloatMenuOption("Remove gene", delegate
			{
				List<DebugMenuOption> list2 = new List<DebugMenuOption>();
				foreach (GeneDef geneDef in geneDefs)
				{
					GeneDef gene = geneDef;
					list2.Add(new DebugMenuOption(gene.LabelCap, DebugMenuOptionMode.Action, delegate
					{
						geneSet.Debug_RemoveGene(gene);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
		void AddGene(GeneDef geneDef, bool xeno)
		{
			if (sourcePawn != null)
			{
				sourcePawn.genes.AddGene(geneDef, xeno);
			}
			else if (geneSet != null)
			{
				geneSet.AddGene(geneDef);
			}
		}
	}

	public static void DrawGene(Gene gene, Rect geneRect, GeneType geneType, bool doBackground = true, bool clickable = true)
	{
		DrawGeneBasics(gene.def, geneRect, geneType, doBackground, clickable, !gene.Active);
		if (Mouse.IsOver(geneRect))
		{
			string text = gene.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + gene.def.DescriptionFull;
			if (gene.Overridden)
			{
				text += "\n\n";
				text = ((gene.overriddenByGene.def != gene.def) ? (text + ("OverriddenByGene".Translate() + ": " + gene.overriddenByGene.LabelCap).Colorize(ColorLibrary.RedReadable)) : (text + ("OverriddenByIdenticalGene".Translate() + ": " + gene.overriddenByGene.LabelCap).Colorize(ColorLibrary.RedReadable)));
			}
			if (clickable)
			{
				text = text + "\n\n" + "ClickForMoreInfo".Translate().ToString().Colorize(ColoredText.SubtleGrayColor);
			}
			TooltipHandler.TipRegion(geneRect, text);
		}
	}

	public static void DrawGeneDef(GeneDef gene, Rect geneRect, GeneType geneType, Func<string> extraTooltip = null, bool doBackground = true, bool clickable = true, bool overridden = false)
	{
		DrawGeneBasics(gene, geneRect, geneType, doBackground, clickable, overridden);
		if (!Mouse.IsOver(geneRect))
		{
			return;
		}
		TooltipHandler.TipRegion(geneRect, delegate
		{
			string text = gene.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + gene.DescriptionFull;
			if (extraTooltip != null)
			{
				string text2 = extraTooltip();
				if (!text2.NullOrEmpty())
				{
					text = text + "\n\n" + text2.Colorize(ColorLibrary.RedReadable);
				}
			}
			if (clickable)
			{
				text = text + "\n\n" + "ClickForMoreInfo".Translate().ToString().Colorize(ColoredText.SubtleGrayColor);
			}
			return text;
		}, 316238373);
	}

	private static void DrawGeneBasics(GeneDef gene, Rect geneRect, GeneType geneType, bool doBackground, bool clickable, bool overridden)
	{
		GUI.BeginGroup(geneRect);
		Rect rect = geneRect.AtZero();
		if (doBackground)
		{
			Widgets.DrawHighlight(rect);
			GUI.color = new Color(1f, 1f, 1f, 0.05f);
			Widgets.DrawBox(rect);
			GUI.color = Color.white;
		}
		float num = rect.width - Text.LineHeight;
		Rect rect2 = new Rect(geneRect.width / 2f - num / 2f, 0f, num, num);
		Color iconColor = gene.IconColor;
		if (overridden)
		{
			iconColor.a = 0.75f;
			GUI.color = ColoredText.SubtleGrayColor;
		}
		CachedTexture cachedTexture = GeneBackground_Archite;
		if (gene.biostatArc == 0)
		{
			switch (geneType)
			{
			case GeneType.Endogene:
				cachedTexture = GeneBackground_Endogene;
				break;
			case GeneType.Xenogene:
				cachedTexture = GeneBackground_Xenogene;
				break;
			}
		}
		GUI.DrawTexture(rect2, cachedTexture.Texture);
		Widgets.DefIcon(rect2, gene, null, 0.9f, null, drawPlaceholder: false, iconColor);
		Text.Font = GameFont.Tiny;
		float num2 = Text.CalcHeight(gene.LabelCap, rect.width);
		Rect rect3 = new Rect(0f, rect.yMax - num2, rect.width, num2);
		GUI.DrawTexture(new Rect(rect3.x, rect3.yMax - num2, rect3.width, num2), TexUI.GrayTextBG);
		Text.Anchor = TextAnchor.LowerCenter;
		if (overridden)
		{
			GUI.color = ColoredText.SubtleGrayColor;
		}
		if (doBackground && num2 < (Text.LineHeight - 2f) * 2f)
		{
			rect3.y -= 3f;
		}
		Widgets.Label(rect3, gene.LabelCap);
		GUI.color = Color.white;
		Text.Anchor = TextAnchor.UpperLeft;
		Text.Font = GameFont.Small;
		if (clickable)
		{
			if (Widgets.ButtonInvisible(rect))
			{
				Find.WindowStack.Add(new Dialog_InfoCard(gene));
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
		}
		GUI.EndGroup();
	}

	private static void DrawStat(Rect iconRect, CachedTexture icon, string stat, float iconWidth)
	{
		GUI.DrawTexture(iconRect, icon.Texture);
		Text.Anchor = TextAnchor.MiddleRight;
		Widgets.LabelFit(new Rect(iconRect.xMax, iconRect.y, 38f - iconWidth, iconWidth), stat);
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public static void DrawBiostats(int gcx, int met, int arc, ref float curX, float curY, float margin = 6f)
	{
		float num = GeneCreationDialogBase.GeneSize.y / 3f;
		float num2 = 0f;
		float num3 = Text.LineHeightOf(GameFont.Small);
		Rect iconRect = new Rect(curX, curY + margin + num2, num3, num3);
		DrawStat(iconRect, GeneUtility.GCXTex, gcx.ToString(), num3);
		Rect rect = new Rect(curX, iconRect.y, 38f, num3);
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegion(rect, "Complexity".Translate().CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "ComplexityDesc".Translate());
		}
		num2 += num;
		if (met != 0)
		{
			Rect iconRect2 = new Rect(curX, curY + margin + num2, num3, num3);
			DrawStat(iconRect2, GeneUtility.METTex, met.ToStringWithSign(), num3);
			Rect rect2 = new Rect(curX, iconRect2.y, 38f, num3);
			if (Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
				TooltipHandler.TipRegion(rect2, "Metabolism".Translate().CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "MetabolismDesc".Translate());
			}
			num2 += num;
		}
		if (arc > 0)
		{
			Rect iconRect3 = new Rect(curX, curY + margin + num2, num3, num3);
			DrawStat(iconRect3, GeneUtility.ARCTex, arc.ToString(), num3);
			Rect rect3 = new Rect(curX, iconRect3.y, 38f, num3);
			if (Mouse.IsOver(rect3))
			{
				Widgets.DrawHighlight(rect3);
				TooltipHandler.TipRegion(rect3, "ArchitesRequired".Translate().CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "ArchitesRequiredDesc".Translate());
			}
		}
		curX += 34f;
	}
}
