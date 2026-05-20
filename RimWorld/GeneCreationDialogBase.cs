using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public abstract class GeneCreationDialogBase : Window
{
	protected int gcx;

	protected int met;

	protected int arc;

	protected string xenotypeName;

	protected bool xenotypeNameLocked;

	protected float scrollHeight;

	protected Vector2 scrollPosition;

	protected float selectedHeight;

	protected float unselectedHeight;

	protected bool ignoreRestrictions;

	protected float postXenotypeHeight;

	protected bool alwaysUseFullBiostatsTableHeight;

	protected int maxGCX = -1;

	protected float searchWidgetOffsetX;

	protected QuickSearchWidget quickSearchWidget = new QuickSearchWidget();

	protected HashSet<GeneDef> matchingGenes = new HashSet<GeneDef>();

	protected Dictionary<GeneDef, List<GeneDef>> randomChosenGroups = new Dictionary<GeneDef, List<GeneDef>>();

	protected List<GeneLeftChosenGroup> leftChosenGroups = new List<GeneLeftChosenGroup>();

	protected List<GeneDef> cachedOverriddenGenes = new List<GeneDef>();

	protected List<GeneDef> cachedUnoverriddenGenes = new List<GeneDef>();

	protected List<GeneDefWithType> tmpGenesWithType = new List<GeneDefWithType>();

	protected XenotypeIconDef iconDef;

	protected static readonly Vector2 ButSize = new Vector2(150f, 38f);

	protected const float HeaderHeight = 35f;

	protected const float GeneGap = 4f;

	private const int MaxNameLength = 40;

	private const int NumCharsTypedBeforeAutoLockingName = 3;

	private const int MaxTriesToGenerateUniqueXenotypeNames = 1000;

	private const float TextFieldWidthPct = 0.25f;

	private static readonly Regex ValidSymbolRegex = new Regex("^[\\p{L}0-9 '\\-]*$");

	public static readonly Texture2D UnlockedTex = ContentFinder<Texture2D>.Get("UI/Overlays/LockedMonochrome");

	public static readonly Texture2D LockedTex = ContentFinder<Texture2D>.Get("UI/Overlays/Locked");

	protected const float BiostatsWidth = 38f;

	public static readonly Vector2 GeneSize = new Vector2(87f, 68f);

	protected static readonly Color OutlineColorUnselected = new Color(1f, 1f, 1f, 0.1f);

	protected const float GenepackGap = 14f;

	protected const float QuickSearchFilterWidth = 300f;

	protected abstract List<GeneDef> SelectedGenes { get; }

	protected abstract string Header { get; }

	protected abstract string AcceptButtonLabel { get; }

	public override void PreOpen()
	{
		base.PreOpen();
		iconDef = XenotypeIconDefOf.Basic;
		UpdateSearchResults();
		OnGenesChanged();
	}

	public override void DoWindowContents(Rect rect)
	{
		Rect rect2 = rect;
		rect2.yMax -= ButSize.y + 4f;
		Rect rect3 = new Rect(rect2.x, rect2.y, rect2.width, 35f);
		Text.Font = GameFont.Medium;
		Widgets.Label(rect3, Header);
		Text.Font = GameFont.Small;
		DrawSearchRect(rect);
		rect2.yMin += 39f;
		float num = rect.width * 0.25f - Margin - 10f;
		float num2 = num - 24f - 10f;
		float num3 = Mathf.Max(BiostatsTable.HeightForBiostats(alwaysUseFullBiostatsTableHeight ? 1 : arc), postXenotypeHeight);
		Rect rect4 = new Rect(rect2.x + Margin, rect2.y, rect2.width - Margin * 2f, rect2.height - num3 - 8f);
		DrawGenes(rect4);
		float num4 = rect4.yMax + 4f;
		Rect rect5 = new Rect(rect2.x + Margin + 10f, num4, rect.width * 0.75f - Margin * 3f - 10f, num3);
		rect5.yMax = rect4.yMax + num3 + 4f;
		BiostatsTable.Draw(rect5, gcx, met, arc, drawMax: true, ignoreRestrictions, maxGCX);
		string text = "XenotypeName".Translate().CapitalizeFirst() + ":";
		Rect rect6 = new Rect(rect5.xMax + Margin, num4, Text.CalcSize(text).x, Text.LineHeight);
		Widgets.Label(rect6, text);
		Rect rect7 = new Rect(rect6.xMin, rect6.y + Text.LineHeight, num, Text.LineHeight);
		rect7.xMax = rect2.xMax - Margin - 17f - num2 * 0.25f;
		string text2 = xenotypeName;
		xenotypeName = Widgets.TextField(rect7, xenotypeName, 40, ValidSymbolRegex);
		if (text2 != xenotypeName)
		{
			if (xenotypeName.Length > text2.Length && xenotypeName.Length > 3)
			{
				xenotypeNameLocked = true;
			}
			else if (xenotypeName.Length == 0)
			{
				xenotypeNameLocked = false;
			}
		}
		Rect rect8 = new Rect(rect7.xMax + 4f, rect7.yMax - 35f, 35f, 35f);
		DrawIconSelector(rect8);
		Rect rect9 = new Rect(rect7.x, rect7.yMax + 4f, num2 * 0.75f - 4f, 24f);
		if (Widgets.ButtonText(rect9, "Randomize".Translate()))
		{
			if (SelectedGenes.Count == 0)
			{
				Messages.Message("SelectAGeneToRandomizeName".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				GUI.FocusControl(null);
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				xenotypeName = GeneUtility.GenerateXenotypeNameFromGenes(SelectedGenes);
			}
		}
		Rect rect10 = new Rect(rect9.xMax + 4f, rect9.y, num2 * 0.25f, 24f);
		if (Widgets.ButtonText(rect10, "..."))
		{
			if (SelectedGenes.Count > 0)
			{
				List<string> list = new List<string>();
				int num5 = 0;
				while (list.Count < 20)
				{
					string text3 = GeneUtility.GenerateXenotypeNameFromGenes(SelectedGenes);
					if (text3.NullOrEmpty())
					{
						break;
					}
					if (list.Contains(text3) || text3 == xenotypeName)
					{
						num5++;
						if (num5 >= 1000)
						{
							break;
						}
					}
					else
					{
						list.Add(text3);
					}
				}
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				for (int i = 0; i < list.Count; i++)
				{
					string n = list[i];
					list2.Add(new FloatMenuOption(n, delegate
					{
						xenotypeName = n;
					}));
				}
				if (list2.Any())
				{
					Find.WindowStack.Add(new FloatMenu(list2));
				}
			}
			else
			{
				Messages.Message("SelectAGeneToChooseAName".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			}
		}
		Rect rect11 = new Rect(rect10.xMax + 10f, rect9.y, 24f, 24f);
		if (Widgets.ButtonImage(rect11, xenotypeNameLocked ? LockedTex : UnlockedTex))
		{
			xenotypeNameLocked = !xenotypeNameLocked;
			if (xenotypeNameLocked)
			{
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
			}
		}
		if (Mouse.IsOver(rect11))
		{
			string text4 = "LockNameButtonDesc".Translate() + "\n\n" + (xenotypeNameLocked ? "LockNameOn" : "LockNameOff").Translate();
			TooltipHandler.TipRegion(rect11, text4);
		}
		postXenotypeHeight = rect11.yMax - num4;
		PostXenotypeOnGUI(rect6.xMin, rect9.y + 24f);
		Rect rect12 = rect;
		rect12.yMin = rect12.yMax - ButSize.y;
		DoBottomButtons(rect12);
	}

	protected virtual void DrawSearchRect(Rect rect)
	{
		Rect rect2 = new Rect(rect.width - 300f - searchWidgetOffsetX, 11f, 300f, 24f);
		quickSearchWidget.OnGUI(rect2, UpdateSearchResults);
	}

	protected virtual bool WithinAcceptableBiostatLimits(bool showMessage)
	{
		if (ignoreRestrictions)
		{
			return true;
		}
		if (met < GeneTuning.BiostatRange.TrueMin)
		{
			if (showMessage)
			{
				Messages.Message("MetabolismTooLowToCreateXenogerm".Translate(met.Named("AMOUNT"), GeneTuning.BiostatRange.TrueMin.Named("MIN")), null, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return true;
	}

	protected virtual bool CanAccept()
	{
		string text = xenotypeName;
		if (text != null && text.Trim().Length == 0)
		{
			Messages.Message("XenotypeNameCannotBeEmpty".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		if (!WithinAcceptableBiostatLimits(showMessage: true))
		{
			return false;
		}
		List<GeneDef> selectedGenes = SelectedGenes;
		foreach (GeneDef selectedGene in SelectedGenes)
		{
			if (selectedGene.prerequisite != null && !selectedGenes.Contains(selectedGene.prerequisite))
			{
				Messages.Message("MessageGeneMissingPrerequisite".Translate(selectedGene.label).CapitalizeFirst() + ": " + selectedGene.prerequisite.LabelCap, null, MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
		}
		return true;
	}

	private void DrawIconSelector(Rect rect)
	{
		Widgets.DrawHighlight(rect);
		if (Widgets.ButtonImage(rect, iconDef.Icon, XenotypeDef.IconColor))
		{
			Find.WindowStack.Add(new Dialog_SelectXenotypeIcon(iconDef, delegate(XenotypeIconDef i)
			{
				iconDef = i;
			}));
		}
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegion(rect, "SelectIconDesc".Translate() + "\n\n" + "ClickToEdit".Translate().Colorize(ColoredText.SubtleGrayColor));
		}
	}

	protected virtual void PostXenotypeOnGUI(float curX, float curY)
	{
	}

	protected abstract void Accept();

	protected abstract void DrawGenes(Rect rect);

	protected virtual void OnGenesChanged()
	{
		randomChosenGroups.Clear();
		leftChosenGroups.Clear();
		cachedOverriddenGenes.Clear();
		cachedUnoverriddenGenes.Clear();
		tmpGenesWithType.Clear();
		gcx = 0;
		met = 0;
		arc = 0;
		List<GeneDef> selectedGenes = SelectedGenes;
		for (int i = 0; i < selectedGenes.Count; i++)
		{
			if (!selectedGenes[i].RandomChosen)
			{
				continue;
			}
			for (int j = i + 1; j < selectedGenes.Count; j++)
			{
				if (selectedGenes[i].ConflictsWith(selectedGenes[j]))
				{
					if (!randomChosenGroups.ContainsKey(selectedGenes[i]))
					{
						randomChosenGroups.Add(selectedGenes[i], new List<GeneDef> { selectedGenes[i] });
					}
					randomChosenGroups[selectedGenes[i]].Add(selectedGenes[j]);
				}
			}
		}
		for (int k = 0; k < selectedGenes.Count; k++)
		{
			if (selectedGenes[k].RandomChosen)
			{
				continue;
			}
			for (int l = k + 1; l < selectedGenes.Count; l++)
			{
				if (selectedGenes[l].RandomChosen || !selectedGenes[k].ConflictsWith(selectedGenes[l]))
				{
					continue;
				}
				int num = GeneUtility.GenesInOrder.IndexOf(selectedGenes[k]);
				int num2 = GeneUtility.GenesInOrder.IndexOf(selectedGenes[l]);
				GeneDef leftMost = ((num < num2) ? selectedGenes[k] : selectedGenes[l]);
				GeneDef rightMost = ((num >= num2) ? selectedGenes[k] : selectedGenes[l]);
				GeneLeftChosenGroup geneLeftChosenGroup = leftChosenGroups.FirstOrDefault((GeneLeftChosenGroup x) => x.leftChosen == leftMost);
				GeneLeftChosenGroup geneLeftChosenGroup2 = leftChosenGroups.FirstOrDefault((GeneLeftChosenGroup x) => x.leftChosen == rightMost);
				if (geneLeftChosenGroup == null)
				{
					geneLeftChosenGroup = new GeneLeftChosenGroup(leftMost);
					leftChosenGroups.Add(geneLeftChosenGroup);
				}
				if (geneLeftChosenGroup2 != null)
				{
					foreach (GeneDef overriddenGene in geneLeftChosenGroup2.overriddenGenes)
					{
						if (!geneLeftChosenGroup.overriddenGenes.Contains(overriddenGene))
						{
							geneLeftChosenGroup.overriddenGenes.Add(overriddenGene);
						}
						if (!cachedOverriddenGenes.Contains(overriddenGene))
						{
							cachedOverriddenGenes.Add(overriddenGene);
						}
					}
					leftChosenGroups.Remove(geneLeftChosenGroup2);
				}
				if (!geneLeftChosenGroup.overriddenGenes.Contains(rightMost))
				{
					geneLeftChosenGroup.overriddenGenes.Add(rightMost);
				}
				if (!cachedOverriddenGenes.Contains(rightMost))
				{
					cachedOverriddenGenes.Add(rightMost);
				}
			}
		}
		foreach (GeneLeftChosenGroup leftChosenGroup in leftChosenGroups)
		{
			leftChosenGroup.overriddenGenes.SortBy((GeneDef x) => selectedGenes.IndexOf(x));
		}
		cachedUnoverriddenGenes.AddRange(SelectedGenes);
		foreach (GeneDef cachedOverriddenGene in cachedOverriddenGenes)
		{
			cachedUnoverriddenGenes.Remove(cachedOverriddenGene);
		}
		for (int num3 = 0; num3 < selectedGenes.Count; num3++)
		{
			tmpGenesWithType.Add(new GeneDefWithType(selectedGenes[num3], xenogene: true));
		}
		foreach (GeneDef item in tmpGenesWithType.NonOverriddenGenes().Distinct())
		{
			gcx += item.biostatCpx;
			met += item.biostatMet;
			arc += item.biostatArc;
		}
	}

	protected abstract void UpdateSearchResults();

	protected virtual void DoBottomButtons(Rect rect)
	{
		if (Widgets.ButtonText(new Rect(rect.xMax - ButSize.x, rect.y, ButSize.x, ButSize.y), AcceptButtonLabel) && CanAccept())
		{
			Accept();
		}
		if (Widgets.ButtonText(new Rect(rect.x, rect.y, ButSize.x, ButSize.y), "Close".Translate()))
		{
			Close();
		}
	}
}
