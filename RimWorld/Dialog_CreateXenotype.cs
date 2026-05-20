using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Dialog_CreateXenotype : GeneCreationDialogBase
{
	private int generationRequestIndex;

	private Action callback;

	private List<GeneDef> selectedGenes = new List<GeneDef>();

	private bool inheritable;

	private bool? selectedCollapsed = false;

	private HashSet<GeneCategoryDef> matchingCategories = new HashSet<GeneCategoryDef>();

	private Dictionary<GeneCategoryDef, bool> collapsedCategories = new Dictionary<GeneCategoryDef, bool>();

	private bool hoveredAnyGene;

	private GeneDef hoveredGene;

	private static bool ignoreRestrictionsConfirmationSent;

	private Vector2 selectedScrollPosition;

	private const int MaxCustomXenotypes = 200;

	private static readonly Color OutlineColorSelected = new Color(1f, 1f, 0.7f, 1f);

	public override Vector2 InitialSize => new Vector2(Mathf.Min(UI.screenWidth, 1036), UI.screenHeight - 4);

	protected override List<GeneDef> SelectedGenes => selectedGenes;

	protected override string Header => "CreateXenotype".Translate().CapitalizeFirst();

	protected override string AcceptButtonLabel => "SaveAndApply".Translate().CapitalizeFirst();

	public Dialog_CreateXenotype(int index, Action callback)
	{
		generationRequestIndex = index;
		this.callback = callback;
		xenotypeName = string.Empty;
		closeOnAccept = false;
		absorbInputAroundWindow = true;
		alwaysUseFullBiostatsTableHeight = true;
		searchWidgetOffsetX = GeneCreationDialogBase.ButSize.x * 2f + 4f;
		foreach (GeneCategoryDef allDef in DefDatabase<GeneCategoryDef>.AllDefs)
		{
			collapsedCategories.Add(allDef, value: false);
		}
		OnGenesChanged();
	}

	public override void PostOpen()
	{
		if (!ModLister.CheckBiotech("xenotype creation"))
		{
			Close(doCloseSound: false);
		}
		else
		{
			base.PostOpen();
		}
	}

	protected override void DrawGenes(Rect rect)
	{
		hoveredAnyGene = false;
		GUI.BeginGroup(rect);
		float curY = 0f;
		DrawSection(new Rect(0f, 0f, rect.width, selectedHeight), selectedGenes, "SelectedGenes".Translate(), ref curY, ref selectedHeight, adding: false, rect, ref selectedCollapsed);
		if (!selectedCollapsed.Value)
		{
			curY += 10f;
		}
		float num = curY;
		Widgets.Label(0f, ref curY, rect.width, "Genes".Translate().CapitalizeFirst());
		curY += 10f;
		float height = curY - num - 4f;
		if (Widgets.ButtonText(new Rect(rect.width - 150f - 16f, num, 150f, height), "CollapseAllCategories".Translate()))
		{
			SoundDefOf.TabClose.PlayOneShotOnCamera();
			foreach (GeneCategoryDef allDef in DefDatabase<GeneCategoryDef>.AllDefs)
			{
				collapsedCategories[allDef] = true;
			}
		}
		if (Widgets.ButtonText(new Rect(rect.width - 300f - 4f - 16f, num, 150f, height), "ExpandAllCategories".Translate()))
		{
			SoundDefOf.TabOpen.PlayOneShotOnCamera();
			foreach (GeneCategoryDef allDef2 in DefDatabase<GeneCategoryDef>.AllDefs)
			{
				collapsedCategories[allDef2] = false;
			}
		}
		float num2 = curY;
		Rect rect2 = new Rect(0f, curY, rect.width - 16f, scrollHeight);
		Widgets.BeginScrollView(new Rect(0f, curY, rect.width, rect.height - curY), ref scrollPosition, rect2);
		Rect containingRect = rect2;
		containingRect.y = curY + scrollPosition.y;
		containingRect.height = rect.height;
		bool? collapsed = null;
		DrawSection(rect, GeneUtility.GenesInOrder, null, ref curY, ref unselectedHeight, adding: true, containingRect, ref collapsed);
		if (Event.current.type == EventType.Layout)
		{
			scrollHeight = curY - num2;
		}
		Widgets.EndScrollView();
		GUI.EndGroup();
		if (!hoveredAnyGene)
		{
			hoveredGene = null;
		}
	}

	private void DrawSection(Rect rect, List<GeneDef> genes, string label, ref float curY, ref float sectionHeight, bool adding, Rect containingRect, ref bool? collapsed)
	{
		float curX = 4f;
		if (!label.NullOrEmpty())
		{
			Rect rect2 = new Rect(0f, curY, rect.width, Text.LineHeight);
			rect2.xMax -= (adding ? 16f : (Text.CalcSize("ClickToAddOrRemove".Translate()).x + 4f));
			if (collapsed.HasValue)
			{
				Rect position = new Rect(rect2.x, rect2.y + (rect2.height - 18f) / 2f, 18f, 18f);
				GUI.DrawTexture(position, collapsed.Value ? TexButton.Reveal : TexButton.Collapse);
				if (Widgets.ButtonInvisible(rect2))
				{
					collapsed = !collapsed;
					if (collapsed.Value)
					{
						SoundDefOf.TabClose.PlayOneShotOnCamera();
					}
					else
					{
						SoundDefOf.TabOpen.PlayOneShotOnCamera();
					}
				}
				if (Mouse.IsOver(rect2))
				{
					Widgets.DrawHighlight(rect2);
				}
				rect2.xMin += position.width;
			}
			Widgets.Label(rect2, label);
			if (!adding)
			{
				Text.Anchor = TextAnchor.UpperRight;
				GUI.color = ColoredText.SubtleGrayColor;
				Widgets.Label(new Rect(rect2.xMax - 18f, curY, rect.width - rect2.width, Text.LineHeight), "ClickToAddOrRemove".Translate());
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
			}
			curY += Text.LineHeight + 3f;
		}
		if (collapsed == true)
		{
			if (Event.current.type == EventType.Layout)
			{
				sectionHeight = 0f;
			}
			return;
		}
		float num = curY;
		bool flag = false;
		float num2 = 34f + GeneCreationDialogBase.GeneSize.x + 8f;
		float num3 = GeneCreationDialogBase.GeneSize.y + 8f + 4f;
		float num4 = rect.width - 16f;
		float num5 = num2 + 4f;
		float b = (num4 - num5 * Mathf.Floor(num4 / num5)) / 2f;
		Rect rect3 = new Rect(0f, curY, rect.width, sectionHeight);
		if (!adding)
		{
			Widgets.DrawRectFast(rect3, Widgets.MenuSectionBGFillColor);
		}
		curY += 4f;
		if (!genes.Any())
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			GUI.color = ColoredText.SubtleGrayColor;
			Widgets.Label(rect3, "(" + "NoneLower".Translate() + ")");
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			curY += num3;
		}
		else
		{
			float num6 = rect.width - 16f;
			int num7 = Mathf.FloorToInt(num6 / (num2 + 1f));
			int num8 = Mathf.CeilToInt((float)genes.Count / (float)num7);
			float num9 = Mathf.Max(num3 * (float)Mathf.Min(num8, 3));
			float num10 = num3 * (float)num8;
			bool flag2 = !adding && collapsed == false && genes.Count > 0 && num10 > num9;
			float num11 = curY;
			if (flag2)
			{
				num11 = 0f;
				Widgets.BeginScrollView(new Rect(0f, curY, rect.width, num9), viewRect: new Rect(0f, 0f, num6, num10), scrollPosition: ref selectedScrollPosition);
			}
			GeneCategoryDef geneCategoryDef = null;
			int num12 = 0;
			for (int i = 0; i < genes.Count; i++)
			{
				GeneDef geneDef = genes[i];
				if ((adding && quickSearchWidget.filter.Active && (!matchingGenes.Contains(geneDef) || selectedGenes.Contains(geneDef)) && !matchingCategories.Contains(geneDef.displayCategory)) || (!ignoreRestrictions && geneDef.biostatArc > 0))
				{
					continue;
				}
				bool flag3 = false;
				if (curX + num2 > num4)
				{
					curX = 4f;
					num11 += GeneCreationDialogBase.GeneSize.y + 8f + 4f;
					flag3 = true;
				}
				bool flag4 = quickSearchWidget.filter.Active && (matchingGenes.Contains(geneDef) || matchingCategories.Contains(geneDef.displayCategory));
				bool flag5 = collapsedCategories[geneDef.displayCategory] && !flag4;
				if (adding && geneCategoryDef != geneDef.displayCategory)
				{
					if (!flag3 && flag)
					{
						curX = 4f;
						num11 += GeneCreationDialogBase.GeneSize.y + 8f + 4f;
					}
					geneCategoryDef = geneDef.displayCategory;
					Rect rect4 = new Rect(curX, num11, rect.width - 8f, Text.LineHeight);
					if (!flag4)
					{
						Rect position2 = new Rect(rect4.x, rect4.y + (rect4.height - 18f) / 2f, 18f, 18f);
						GUI.DrawTexture(position2, flag5 ? TexButton.Reveal : TexButton.Collapse);
						if (Widgets.ButtonInvisible(rect4))
						{
							collapsedCategories[geneDef.displayCategory] = !collapsedCategories[geneDef.displayCategory];
							if (collapsedCategories[geneDef.displayCategory])
							{
								SoundDefOf.TabClose.PlayOneShotOnCamera();
							}
							else
							{
								SoundDefOf.TabOpen.PlayOneShotOnCamera();
							}
						}
						if (num12 % 2 == 1)
						{
							Widgets.DrawLightHighlight(rect4);
						}
						if (Mouse.IsOver(rect4))
						{
							Widgets.DrawHighlight(rect4);
						}
						rect4.xMin += position2.width;
					}
					Widgets.Label(rect4, geneCategoryDef.LabelCap);
					num11 += rect4.height;
					if (!flag5)
					{
						GUI.color = Color.grey;
						Widgets.DrawLineHorizontal(curX, num11, rect.width - 8f);
						GUI.color = Color.white;
						num11 += 10f;
					}
					num12++;
				}
				if (adding && flag5)
				{
					flag = false;
					if (Event.current.type == EventType.Layout)
					{
						sectionHeight = num11 - num;
					}
					continue;
				}
				curX = Mathf.Max(curX, b);
				flag = true;
				if (DrawGene(geneDef, !adding, ref curX, num11, num2, containingRect, flag4))
				{
					if (selectedGenes.Contains(geneDef))
					{
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
						selectedGenes.Remove(geneDef);
					}
					else
					{
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
						selectedGenes.Add(geneDef);
					}
					if (!xenotypeNameLocked)
					{
						xenotypeName = GeneUtility.GenerateXenotypeNameFromGenes(SelectedGenes);
					}
					OnGenesChanged();
					break;
				}
			}
			num11 += num3;
			if (flag2)
			{
				Widgets.EndScrollView();
				curY += num9;
			}
			else
			{
				curY = num11;
			}
		}
		if (Event.current.type == EventType.Layout)
		{
			sectionHeight = curY - num;
		}
	}

	private bool DrawGene(GeneDef geneDef, bool selectedSection, ref float curX, float curY, float packWidth, Rect containingRect, bool isMatch)
	{
		bool result = false;
		Rect rect = new Rect(curX, curY, packWidth, GeneCreationDialogBase.GeneSize.y + 8f);
		if (!containingRect.Overlaps(rect))
		{
			curX = rect.xMax + 4f;
			return false;
		}
		bool selected = !selectedSection && selectedGenes.Contains(geneDef);
		bool overridden = leftChosenGroups.Any((GeneLeftChosenGroup x) => x.overriddenGenes.Contains(geneDef));
		Widgets.DrawOptionBackground(rect, selected);
		curX += 4f;
		GeneUIUtility.DrawBiostats(geneDef.biostatCpx, geneDef.biostatMet, geneDef.biostatArc, ref curX, curY, 4f);
		GeneUIUtility.DrawGeneDef(geneRect: new Rect(curX, curY + 4f, GeneCreationDialogBase.GeneSize.x, GeneCreationDialogBase.GeneSize.y), gene: geneDef, geneType: (!inheritable) ? GeneType.Xenogene : GeneType.Endogene, extraTooltip: () => GeneTip(geneDef, selectedSection), doBackground: false, clickable: false, overridden: overridden);
		curX += GeneCreationDialogBase.GeneSize.x + 4f;
		if (Mouse.IsOver(rect))
		{
			hoveredGene = geneDef;
			hoveredAnyGene = true;
		}
		else if (hoveredGene != null && geneDef.ConflictsWith(hoveredGene))
		{
			Widgets.DrawLightHighlight(rect);
		}
		if (Widgets.ButtonInvisible(rect))
		{
			result = true;
		}
		curX = Mathf.Max(curX, rect.xMax + 4f);
		return result;
	}

	private string GeneTip(GeneDef geneDef, bool selectedSection)
	{
		string text = null;
		if (selectedSection)
		{
			if (leftChosenGroups.Any((GeneLeftChosenGroup x) => x.leftChosen == geneDef))
			{
				text = GroupInfo(leftChosenGroups.FirstOrDefault((GeneLeftChosenGroup x) => x.leftChosen == geneDef));
			}
			else if (cachedOverriddenGenes.Contains(geneDef))
			{
				text = GroupInfo(leftChosenGroups.FirstOrDefault((GeneLeftChosenGroup x) => x.overriddenGenes.Contains(geneDef)));
			}
			else if (randomChosenGroups.ContainsKey(geneDef))
			{
				text = ("GeneWillBeRandomChosen".Translate() + ":\n" + randomChosenGroups[geneDef].Select((GeneDef x) => x.label).ToLineList("  - ", capitalizeItems: true)).Colorize(ColoredText.TipSectionTitleColor);
			}
		}
		if (selectedGenes.Contains(geneDef) && geneDef.prerequisite != null && !selectedGenes.Contains(geneDef.prerequisite))
		{
			if (!text.NullOrEmpty())
			{
				text += "\n\n";
			}
			text += ("MessageGeneMissingPrerequisite".Translate(geneDef.label).CapitalizeFirst() + ": " + geneDef.prerequisite.LabelCap).Colorize(ColorLibrary.RedReadable);
		}
		if (!text.NullOrEmpty())
		{
			text += "\n\n";
		}
		return text + (selectedGenes.Contains(geneDef) ? "ClickToRemove" : "ClickToAdd").Translate().Colorize(ColoredText.SubtleGrayColor);
		static string GroupInfo(GeneLeftChosenGroup group)
		{
			if (group == null)
			{
				return null;
			}
			return ("GeneLeftmostActive".Translate() + ":\n  - " + group.leftChosen.LabelCap + " (" + "Active".Translate() + ")" + "\n" + group.overriddenGenes.Select((GeneDef x) => (x.label + " (" + "Suppressed".Translate() + ")").Colorize(ColorLibrary.RedReadable)).ToLineList("  - ", capitalizeItems: true)).Colorize(ColoredText.TipSectionTitleColor);
		}
	}

	protected override void PostXenotypeOnGUI(float curX, float curY)
	{
		TaggedString taggedString = "GenesAreInheritable".Translate();
		TaggedString taggedString2 = "IgnoreRestrictions".Translate();
		float width = Mathf.Max(Text.CalcSize(taggedString).x, Text.CalcSize(taggedString2).x) + 4f + 24f;
		Rect rect = new Rect(curX, curY, width, Text.LineHeight);
		Widgets.CheckboxLabeled(rect, taggedString, ref inheritable);
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegion(rect, "GenesAreInheritableDesc".Translate());
		}
		rect.y += Text.LineHeight;
		bool num = ignoreRestrictions;
		Widgets.CheckboxLabeled(rect, taggedString2, ref ignoreRestrictions);
		if (num != ignoreRestrictions)
		{
			if (ignoreRestrictions)
			{
				if (!ignoreRestrictionsConfirmationSent)
				{
					ignoreRestrictionsConfirmationSent = true;
					Find.WindowStack.Add(new Dialog_MessageBox("IgnoreRestrictionsConfirmation".Translate(), "Yes".Translate(), delegate
					{
					}, "No".Translate(), delegate
					{
						ignoreRestrictions = false;
					}));
				}
			}
			else
			{
				selectedGenes.RemoveAll((GeneDef x) => x.biostatArc > 0);
				OnGenesChanged();
			}
		}
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegion(rect, "IgnoreRestrictionsDesc".Translate());
		}
		postXenotypeHeight += rect.yMax - curY;
	}

	protected override void OnGenesChanged()
	{
		selectedGenes.SortGeneDefs();
		base.OnGenesChanged();
	}

	protected override void DrawSearchRect(Rect rect)
	{
		base.DrawSearchRect(rect);
		if (Widgets.ButtonText(new Rect(rect.xMax - GeneCreationDialogBase.ButSize.x, rect.y, GeneCreationDialogBase.ButSize.x, GeneCreationDialogBase.ButSize.y), "LoadCustom".Translate()))
		{
			Find.WindowStack.Add(new Dialog_XenotypeList_Load(delegate(CustomXenotype customXenotype)
			{
				xenotypeName = customXenotype.name;
				xenotypeNameLocked = true;
				selectedGenes.Clear();
				selectedGenes.AddRange(customXenotype.genes);
				inheritable = customXenotype.inheritable;
				iconDef = customXenotype.IconDef;
				OnGenesChanged();
				ignoreRestrictions = selectedGenes.Any((GeneDef x) => x.biostatArc > 0) || !WithinAcceptableBiostatLimits(showMessage: false);
			}));
		}
		if (!Widgets.ButtonText(new Rect(rect.xMax - GeneCreationDialogBase.ButSize.x * 2f - 4f, rect.y, GeneCreationDialogBase.ButSize.x, GeneCreationDialogBase.ButSize.y), "LoadPremade".Translate()))
		{
			return;
		}
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (XenotypeDef item in DefDatabase<XenotypeDef>.AllDefs.OrderBy((XenotypeDef c) => 0f - c.displayPriority))
		{
			XenotypeDef xenotype = item;
			list.Add(new FloatMenuOption(xenotype.LabelCap, delegate
			{
				xenotypeName = xenotype.label;
				selectedGenes.Clear();
				selectedGenes.AddRange(xenotype.genes);
				inheritable = xenotype.inheritable;
				OnGenesChanged();
				ignoreRestrictions = selectedGenes.Any((GeneDef g) => g.biostatArc > 0) || !WithinAcceptableBiostatLimits(showMessage: false);
			}, xenotype.Icon, XenotypeDef.IconColor, MenuOptionPriority.Default, delegate(Rect r)
			{
				TooltipHandler.TipRegion(r, xenotype.descriptionShort ?? xenotype.description);
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	protected override void DoBottomButtons(Rect rect)
	{
		base.DoBottomButtons(rect);
		if (leftChosenGroups.Any())
		{
			int num = leftChosenGroups.Sum((GeneLeftChosenGroup geneLeftChosenGroup2) => geneLeftChosenGroup2.overriddenGenes.Count);
			GeneLeftChosenGroup geneLeftChosenGroup = leftChosenGroups[0];
			string text = "GenesConflict".Translate() + ": " + "GenesConflictDesc".Translate(geneLeftChosenGroup.leftChosen.Named("FIRST"), geneLeftChosenGroup.overriddenGenes[0].Named("SECOND")).CapitalizeFirst() + ((num > 1) ? (" +" + (num - 1)) : string.Empty);
			float x = Text.CalcSize(text).x;
			GUI.color = ColorLibrary.RedReadable;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(new Rect(rect.xMax - GeneCreationDialogBase.ButSize.x - x - 4f, rect.y, x, rect.height), text);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
		}
	}

	protected override bool CanAccept()
	{
		if (!base.CanAccept())
		{
			return false;
		}
		if (!selectedGenes.Any())
		{
			Messages.Message("MessageNoSelectedGenes".Translate(), null, MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		if (GenFilePaths.AllCustomXenotypeFiles.EnumerableCount() >= 200)
		{
			Messages.Message("MessageTooManyCustomXenotypes", null, MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		if (!ignoreRestrictions && leftChosenGroups.Any())
		{
			Messages.Message("MessageConflictingGenesPresent".Translate(), null, MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		if (generationRequestIndex >= 0)
		{
			PawnGenerationRequest generationRequest = StartingPawnUtility.GetGenerationRequest(generationRequestIndex);
			if (TutorSystem.TutorialMode && generationRequest.MustBeCapableOfViolence && selectedGenes.Any((GeneDef g) => g.disabledWorkTags.HasFlag(WorkTags.Violent)))
			{
				Messages.Message("MessageStartingPawnCapableOfViolence".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
		}
		return true;
	}

	protected override void Accept()
	{
		IEnumerable<string> warnings = GetWarnings();
		if (warnings.Any())
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("XenotypeBreaksLimits".Translate() + ":\n" + warnings.ToLineList("  - ", capitalizeItems: true) + "\n\n" + "SaveAnyway".Translate(), AcceptInner));
		}
		else
		{
			AcceptInner();
		}
	}

	private void AcceptInner()
	{
		CustomXenotype customXenotype = new CustomXenotype();
		customXenotype.name = xenotypeName?.Trim();
		customXenotype.genes.AddRange(selectedGenes);
		customXenotype.inheritable = inheritable;
		customXenotype.iconDef = iconDef;
		string text = GenFile.SanitizedFileName(customXenotype.name);
		string absPath = GenFilePaths.AbsFilePathForXenotype(text);
		LongEventHandler.QueueLongEvent(delegate
		{
			GameDataSaveLoader.SaveXenotype(customXenotype, absPath);
		}, "SavingLongEvent", doAsynchronously: false, null);
		if (generationRequestIndex >= 0)
		{
			PawnGenerationRequest generationRequest = StartingPawnUtility.GetGenerationRequest(generationRequestIndex);
			generationRequest.ForcedXenotype = null;
			generationRequest.ForcedCustomXenotype = customXenotype;
			StartingPawnUtility.SetGenerationRequest(generationRequestIndex, generationRequest);
		}
		callback?.Invoke();
		Close();
	}

	private IEnumerable<string> GetWarnings()
	{
		if (ignoreRestrictions)
		{
			if (arc > 0 && inheritable)
			{
				yield return "XenotypeBreaksLimits_Archites".Translate();
			}
			if (met > GeneTuning.BiostatRange.TrueMax)
			{
				yield return "XenotypeBreaksLimits_Exceeds".Translate("Metabolism".Translate().Named("STAT"), met.Named("VALUE"), GeneTuning.BiostatRange.TrueMax.Named("MAX")).CapitalizeFirst();
			}
			else if (met < GeneTuning.BiostatRange.TrueMin)
			{
				yield return "XenotypeBreaksLimits_Below".Translate("Metabolism".Translate().Named("STAT"), met.Named("VALUE"), GeneTuning.BiostatRange.TrueMin.Named("MIN")).CapitalizeFirst();
			}
		}
	}

	protected override void UpdateSearchResults()
	{
		quickSearchWidget.noResultsMatched = false;
		matchingGenes.Clear();
		matchingCategories.Clear();
		if (!quickSearchWidget.filter.Active)
		{
			return;
		}
		foreach (GeneDef item in GeneUtility.GenesInOrder)
		{
			if (!selectedGenes.Contains(item))
			{
				if (quickSearchWidget.filter.Matches(item.label))
				{
					matchingGenes.Add(item);
				}
				if (quickSearchWidget.filter.Matches(item.displayCategory.label))
				{
					matchingCategories.Add(item.displayCategory);
				}
			}
		}
		quickSearchWidget.noResultsMatched = !matchingGenes.Any() && !matchingCategories.Any();
	}
}
