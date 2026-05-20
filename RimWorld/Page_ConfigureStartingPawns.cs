using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Page_ConfigureStartingPawns : Page
{
	private int curPawnIndex;

	private bool renderClothes;

	private bool renderHeadgear;

	private int reorderableGroupID;

	private Vector2 scroll;

	private const float TabAreaWidth = 140f;

	private const float RightRectLeftPadding = 5f;

	private const float PawnEntryHeight = 60f;

	private const float SkillSummaryHeight = 127f;

	public static readonly Vector2 PawnPortraitSize = new Vector2(92f, 128f);

	private static readonly Vector2 PawnSelectorPortraitSize = new Vector2(70f, 110f);

	public override string PageTitle => "CreateCharacters".Translate();

	private bool StartingPawnsAllBabies
	{
		get
		{
			List<Pawn> startingAndOptionalPawns = Find.GameInitData.startingAndOptionalPawns;
			int num = 0;
			for (int i = 0; i < Find.GameInitData.startingPawnCount; i++)
			{
				if (startingAndOptionalPawns[i].DevelopmentalStage.Baby())
				{
					num++;
				}
			}
			return num >= Find.GameInitData.startingPawnCount;
		}
	}

	private AcceptanceReport ExtraCanDoNextReport
	{
		get
		{
			if (ModsConfig.BiotechActive && StartingPawnsAllBabies)
			{
				return "ChooseChildOrAdult".Translate();
			}
			IEnumerable<Pawn> source = Find.GameInitData.startingAndOptionalPawns.Take(Find.GameInitData.startingPawnCount);
			if (source.Any((Pawn p) => (p.DevelopmentalStage & Find.GameInitData.allowedDevelopmentalStages) == 0))
			{
				return "SelectedCharactersMustBeOfAllowedDevelopmentalStages".Translate(Find.GameInitData.allowedDevelopmentalStages.ToCommaListOr());
			}
			if (!Find.GameInitData.startingSkillsRequired.NullOrEmpty())
			{
				foreach (SkillDef skill in Find.GameInitData.startingSkillsRequired)
				{
					if (!source.Any((Pawn p) => !p.skills.GetSkill(skill).TotallyDisabled))
					{
						return "SelectedCharacterMustBeCapableOf".Translate(skill.skillLabel);
					}
				}
			}
			if (!Find.GameInitData.startingPawnsRequired.NullOrEmpty())
			{
				for (int num = 0; num < Find.GameInitData.startingPawnsRequired.Count; num++)
				{
					PawnKindCount required = Find.GameInitData.startingPawnsRequired[num];
					int num2 = source.Count((Pawn p) => p.kindDef == required.kindDef);
					if (required.count > num2)
					{
						if (required.count <= 1 || required.kindDef.labelPlural.NullOrEmpty())
						{
							_ = required.kindDef.label;
						}
						else
						{
							_ = required.kindDef.labelPlural;
						}
						return "SelectedCharactersMustInclude".Translate(required.Summary.Named("SUMMARY"));
					}
				}
			}
			if (!Find.GameInitData.startingXenotypesRequired.NullOrEmpty())
			{
				for (int num3 = 0; num3 < Find.GameInitData.startingXenotypesRequired.Count; num3++)
				{
					XenotypeCount required2 = Find.GameInitData.startingXenotypesRequired[num3];
					if (source.Count((Pawn p) => p.genes.Xenotype == required2.xenotype && required2.allowedDevelopmentalStages.Has(p.DevelopmentalStage)) != required2.count)
					{
						return "SelectedCharactersMustInclude".Translate(required2.Summary.Named("SUMMARY"));
					}
				}
			}
			if (!Find.GameInitData.startingMutantsRequired.NullOrEmpty())
			{
				for (int num4 = 0; num4 < Find.GameInitData.startingMutantsRequired.Count; num4++)
				{
					MutantCount required3 = Find.GameInitData.startingMutantsRequired[num4];
					if (source.Count((Pawn p) => p.IsMutant && p.mutant.Def == required3.mutant && required3.allowedDevelopmentalStages.Has(p.DevelopmentalStage)) != required3.count)
					{
						return "SelectedCharactersMustInclude".Translate(required3.Summary.Named("SUMMARY"));
					}
				}
			}
			return true;
		}
	}

	public override void PreOpen()
	{
		base.PreOpen();
		if (Find.GameInitData.startingAndOptionalPawns.Count > 0)
		{
			curPawnIndex = 0;
		}
		renderClothes = true;
		renderHeadgear = false;
	}

	public override void PostOpen()
	{
		base.PostOpen();
		TutorSystem.Notify_Event("PageStart-ConfigureStartingPawns");
	}

	public override void DoWindowContents(Rect rect)
	{
		DrawPageTitle(rect);
		DrawApparelOptions(rect);
		rect.yMin += 45f;
		DoBottomButtons(rect, "Start".Translate(), null, null, showNext: true, doNextOnKeypress: false);
		DrawXenotypeEditorButton(rect);
		AcceptanceReport extraCanDoNextReport = ExtraCanDoNextReport;
		if (!extraCanDoNextReport.Accepted && !extraCanDoNextReport.Reason.NullOrEmpty())
		{
			Rect rect2 = new Rect(rect.center.x + Page.BottomButSize.x / 2f + 4f, rect.y + rect.height - Page.BottomButSize.y, Page.BottomButSize.x, Page.BottomButSize.y);
			rect2.xMax = rect.xMax - Page.BottomButSize.x - 4f;
			string text = ExtraCanDoNextReport.Reason.TruncateHeight(rect2.width, rect2.height);
			using (new TextBlock(GameFont.Tiny, Color.red))
			{
				Widgets.Label(rect2, text);
			}
			if (ExtraCanDoNextReport.Reason != text && Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
				TooltipHandler.TipRegion(rect2, ExtraCanDoNextReport.Reason);
			}
		}
		rect.yMax -= 48f;
		Rect rect3 = rect;
		rect3.width = 140f;
		DrawPawnList(rect3);
		UIHighlighter.HighlightOpportunity(rect3, "ReorderPawn");
		Rect rect4 = rect;
		rect4.xMin += 140f;
		Rect rect5 = rect4.BottomPartPixels(127f);
		rect4.yMax = rect5.yMin;
		rect4 = rect4.ContractedBy(4f);
		rect5 = rect5.ContractedBy(4f);
		StartingPawnUtility.DrawPortraitArea(rect4, curPawnIndex, renderClothes, renderHeadgear);
		StartingPawnUtility.DrawSkillSummaries(rect5);
	}

	private void DrawPawnList(Rect rect)
	{
		Rect position = rect;
		position.yMax -= 22f;
		float num = 0f;
		if (Find.GameInitData.startingPawnCount < Find.GameInitData.startingAndOptionalPawns.Count)
		{
			num = 22f;
		}
		float num2 = (float)Find.GameInitData.startingAndOptionalPawns.Count * 60f + 22f + num;
		float num3 = ((num2 > position.height) ? 16f : 0f);
		Rect rect2 = new Rect(0f, 0f, position.width - num3, num2);
		Rect rect3 = rect2;
		rect3.height = 60f;
		rect3 = rect3.ContractedBy(4f);
		scroll = GUI.BeginScrollView(position, scroll, rect2);
		if (Event.current.type == EventType.Repaint)
		{
			reorderableGroupID = ReorderableWidget.NewGroup(delegate(int from, int to)
			{
				if (TutorSystem.AllowAction("ReorderPawn"))
				{
					Pawn item = Find.GameInitData.startingAndOptionalPawns[from];
					Find.GameInitData.startingAndOptionalPawns.Insert(to, item);
					Find.GameInitData.startingAndOptionalPawns.RemoveAt((from < to) ? from : (from + 1));
					StartingPawnUtility.ReorderRequests(from, to);
					TutorSystem.Notify_Event("ReorderPawn");
					if (to < Find.GameInitData.startingPawnCount && from >= Find.GameInitData.startingPawnCount)
					{
						TutorSystem.Notify_Event("ReorderPawnOptionalToStarting");
					}
					curPawnIndex = ((from < to) ? (to - 1) : (curPawnIndex = to));
				}
			}, ReorderableDirection.Vertical, rect, -1f, null, playSoundOnStartReorder: false);
		}
		DrawPawnListLabel(ref rect3, "StartingPawnsSelected".Translate());
		for (int num4 = 0; num4 < Find.GameInitData.startingAndOptionalPawns.Count; num4++)
		{
			if (num4 == Find.GameInitData.startingPawnCount)
			{
				DrawPawnListLabel(ref rect3, "StartingPawnsLeftBehind".Translate());
			}
			Pawn pawn = Find.GameInitData.startingAndOptionalPawns[num4];
			Widgets.BeginGroup(rect3.ExpandedBy(4f));
			Rect rect4 = new Rect(new Vector2(4f, 4f), rect3.size);
			Widgets.DrawOptionBackground(rect4, curPawnIndex == num4);
			MouseoverSounds.DoRegion(rect4);
			Widgets.BeginGroup(rect4);
			GUI.color = new Color(1f, 1f, 1f, 0.2f);
			Rect position2 = new Rect(110f - PawnSelectorPortraitSize.x / 2f, 40f - PawnSelectorPortraitSize.y / 2f, PawnSelectorPortraitSize.x, PawnSelectorPortraitSize.y);
			Vector2 pawnSelectorPortraitSize = PawnSelectorPortraitSize;
			Rot4 south = Rot4.South;
			bool flag = renderClothes;
			bool flag2 = renderHeadgear;
			GUI.DrawTexture(position2, PortraitsCache.Get(pawn, pawnSelectorPortraitSize, south, default(Vector3), 1f, supersample: true, compensateForUIScale: true, flag2, flag));
			GUI.color = Color.white;
			Widgets.Label(label: (!(pawn.Name is NameTriple nameTriple)) ? pawn.LabelShort : (string.IsNullOrEmpty(nameTriple.Nick) ? nameTriple.First : nameTriple.Nick), rect: rect4.TopPart(0.5f).Rounded());
			if (Text.CalcSize(pawn.story.TitleCap).x > rect4.width)
			{
				Widgets.Label(rect4.BottomPart(0.5f).Rounded(), pawn.story.TitleShortCap);
			}
			else
			{
				Widgets.Label(rect4.BottomPart(0.5f).Rounded(), pawn.story.TitleCap);
			}
			if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect4))
			{
				curPawnIndex = num4;
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			}
			Widgets.EndGroup();
			Widgets.EndGroup();
			if (ReorderableWidget.Reorderable(reorderableGroupID, rect3.ExpandedBy(4f)))
			{
				Widgets.DrawRectFast(rect3, Widgets.WindowBGFillColor * new Color(1f, 1f, 1f, 0.5f));
			}
			if (Mouse.IsOver(rect3))
			{
				TooltipHandler.TipRegion(rect3, new TipSignal("DragToReorder".Translate(), pawn.GetHashCode() * 3499));
			}
			rect3.y += 60f;
		}
		GUI.EndScrollView();
		Rect rect5 = rect;
		rect5.yMin = position.yMax;
		using (new TextBlock(Color.gray))
		{
			Widgets.Label(rect5, "DragToReorder".Translate());
		}
	}

	private void DrawPawnListLabel(ref Rect rect, string label, bool isGray = false)
	{
		Rect rect2 = rect;
		rect2.xMin -= 4f;
		rect2.height = 22f;
		rect.y += 22f;
		string text = label.Truncate(rect2.width);
		TextBlock textBlock = new TextBlock(isGray ? Color.gray : Color.white);
		try
		{
			Widgets.Label(rect2, text);
		}
		finally
		{
			((IDisposable)textBlock/*cast due to .constrained prefix*/).Dispose();
		}
		if (label != text)
		{
			TooltipHandler.TipRegion(rect2, label);
		}
	}

	private void DrawApparelOptions(Rect rect)
	{
		if (ModsConfig.IdeologyActive)
		{
			string text = "ShowHeadgear".Translate();
			string text2 = "ShowApparel".Translate();
			float num = Mathf.Max(Text.CalcSize(text).x, Text.CalcSize(text2).x) + 4f + 24f;
			Rect rect2 = new Rect(rect.xMax - num, rect.y, num, Text.LineHeight * 2f);
			Widgets.CheckboxLabeled(new Rect(rect2.x, rect2.y, rect2.width, rect2.height / 2f), text, ref renderHeadgear);
			Widgets.CheckboxLabeled(new Rect(rect2.x, rect2.y + rect2.height / 2f, rect2.width, rect2.height / 2f), text2, ref renderClothes);
		}
	}

	private void DrawXenotypeEditorButton(Rect rect)
	{
		if (!ModsConfig.BiotechActive)
		{
			return;
		}
		Text.Font = GameFont.Small;
		float x = (rect.width - Page.BottomButSize.x) / 2f;
		float y = rect.y + rect.height - 38f;
		if (Widgets.ButtonText(new Rect(x, y, Page.BottomButSize.x, Page.BottomButSize.y), "XenotypeEditor".Translate()))
		{
			Find.WindowStack.Add(new Dialog_CreateXenotype(curPawnIndex, delegate
			{
				CharacterCardUtility.cachedCustomXenotypes = null;
				StartingPawnUtility.RandomizePawn(curPawnIndex);
			}));
		}
	}

	protected override bool CanDoNext()
	{
		if (!base.CanDoNext())
		{
			return false;
		}
		if (TutorSystem.TutorialMode)
		{
			WorkTypeDef workTypeDef = StartingPawnUtility.RequiredWorkTypesDisabledForEveryone().FirstOrDefault();
			if (workTypeDef != null)
			{
				Messages.Message("RequiredWorkTypeDisabledForEveryone".Translate() + ": " + workTypeDef.gerundLabel.CapitalizeFirst() + ".", MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
		}
		foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
		{
			if (!startingAndOptionalPawn.Name.IsValid)
			{
				Messages.Message("EveryoneNeedsValidName".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
		}
		AcceptanceReport extraCanDoNextReport = ExtraCanDoNextReport;
		if (!extraCanDoNextReport.Reason.NullOrEmpty())
		{
			Messages.Message(extraCanDoNextReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		PortraitsCache.Clear();
		return true;
	}

	protected override void DoNext()
	{
		CheckWarnRequiredWorkTypesDisabledForEveryone(delegate
		{
			foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
			{
				if (startingAndOptionalPawn.Name is NameTriple nameTriple && string.IsNullOrEmpty(nameTriple.Nick))
				{
					startingAndOptionalPawn.Name = new NameTriple(nameTriple.First, nameTriple.First, nameTriple.Last);
				}
			}
			base.DoNext();
		});
	}

	private void CheckWarnRequiredWorkTypesDisabledForEveryone(Action nextAction)
	{
		IEnumerable<WorkTypeDef> enumerable = StartingPawnUtility.RequiredWorkTypesDisabledForEveryone();
		if (enumerable.Any())
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (WorkTypeDef item in enumerable)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("  - " + item.gerundLabel.CapitalizeFirst());
			}
			TaggedString text = "ConfirmRequiredWorkTypeDisabledForEveryone".Translate(stringBuilder.ToString());
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, nextAction));
		}
		else
		{
			nextAction();
		}
	}

	public void SelectPawn(Pawn c)
	{
		int num = StartingPawnUtility.PawnIndex(c);
		if (num != -1)
		{
			curPawnIndex = num;
		}
	}
}
