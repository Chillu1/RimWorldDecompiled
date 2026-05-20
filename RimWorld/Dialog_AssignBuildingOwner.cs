using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Dialog_AssignBuildingOwner : Window
{
	private CompAssignableToPawn assignable;

	private Vector2 scrollPosition;

	private const float EntryHeight = 35f;

	private const int AssignButtonWidth = 165;

	private const int SeparatorHeight = 7;

	private static readonly List<Pawn> tmpPawnSorted = new List<Pawn>(16);

	public override Vector2 InitialSize => new Vector2(520f, 500f);

	public Dialog_AssignBuildingOwner(CompAssignableToPawn assignable)
	{
		this.assignable = assignable;
		doCloseButton = true;
		doCloseX = true;
		closeOnClickedOutside = true;
		absorbInputAroundWindow = true;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Small;
		Rect outRect = new Rect(inRect);
		outRect.yMin += 20f;
		outRect.yMax -= 40f;
		float num = 0f;
		num += (float)assignable.AssignedPawnsForReading.Count * 35f;
		num += (float)assignable.AssigningCandidates.Count() * 35f;
		num += 7f;
		Rect viewRect = new Rect(0f, 0f, outRect.width, num);
		Widgets.AdjustRectsForScrollView(inRect, ref outRect, ref viewRect);
		Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
		SortTmpList(assignable.AssignedPawnsForReading);
		float y = 0f;
		for (int i = 0; i < tmpPawnSorted.Count; i++)
		{
			Pawn pawn = tmpPawnSorted[i];
			DrawAssignedRow(pawn, ref y, viewRect, i);
		}
		if (assignable.AssignedPawnsForReading.Count > 0)
		{
			Rect rect = new Rect(0f, y, viewRect.width, 7f);
			y += 7f;
			using (new TextBlock(Widgets.SeparatorLineColor))
			{
				Widgets.DrawLineHorizontal(rect.x, rect.y + rect.height / 2f, rect.width);
			}
		}
		SortTmpList(assignable.AssigningCandidates);
		for (int j = 0; j < tmpPawnSorted.Count; j++)
		{
			Pawn pawn2 = tmpPawnSorted[j];
			DrawUnassignedRow(pawn2, ref y, viewRect, j);
		}
		tmpPawnSorted.Clear();
		Widgets.EndScrollView();
	}

	private void SortTmpList(IEnumerable<Pawn> collection)
	{
		tmpPawnSorted.Clear();
		tmpPawnSorted.AddRange(collection);
		tmpPawnSorted.SortBy((Pawn x) => x.LabelShort);
	}

	private void DrawAssignedRow(Pawn pawn, ref float y, Rect viewRect, int i)
	{
		Rect rect = new Rect(0f, y, viewRect.width, 35f);
		y += 35f;
		if (i % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect);
		}
		Rect rect2 = rect;
		rect2.width = rect.height;
		Widgets.ThingIcon(rect2, pawn);
		Rect rect3 = rect;
		rect3.xMin = rect.xMax - 165f - 10f;
		rect3 = rect3.ContractedBy(2f);
		if (Widgets.ButtonText(rect3, "BuildingUnassign".Translate()))
		{
			assignable.TryUnassignPawn(pawn);
			SoundDefOf.Click.PlayOneShotOnCamera();
		}
		Rect rect4 = rect;
		rect4.xMin = rect2.xMax + 10f;
		rect4.xMax = rect3.xMin - 10f;
		using (new TextBlock(TextAnchor.MiddleLeft))
		{
			Widgets.LabelEllipses(rect4, pawn.LabelCap);
		}
	}

	private void DrawUnassignedRow(Pawn pawn, ref float y, Rect viewRect, int i)
	{
		if (assignable.AssignedPawnsForReading.Contains(pawn))
		{
			return;
		}
		AcceptanceReport acceptanceReport = assignable.CanAssignTo(pawn);
		bool accepted = acceptanceReport.Accepted;
		Rect rect = new Rect(0f, y, viewRect.width, 35f);
		y += 35f;
		if (i % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect);
		}
		if (!accepted)
		{
			GUI.color = Color.gray;
		}
		Rect rect2 = rect;
		rect2.width = rect.height;
		Widgets.ThingIcon(rect2, pawn);
		Rect rect3 = rect;
		rect3.xMin = rect.xMax - 165f - 10f;
		rect3 = rect3.ContractedBy(2f);
		if (!Find.IdeoManager.classicMode && accepted && assignable.IdeoligionForbids(pawn))
		{
			Rect rect4 = rect;
			rect4.width = rect.height;
			rect4.x = rect.xMax - rect.height;
			rect4 = rect4.ContractedBy(2f);
			using (new TextBlock(TextAnchor.MiddleLeft))
			{
				Widgets.Label(rect3, "IdeoligionForbids".Translate());
			}
			IdeoUIUtility.DoIdeoIcon(rect4, pawn.ideo.Ideo, doTooltip: true, delegate
			{
				IdeoUIUtility.OpenIdeoInfo(pawn.ideo.Ideo);
				Close();
			});
		}
		else if (accepted)
		{
			TaggedString taggedString = (assignable.AssignedAnything(pawn) ? "BuildingReassign".Translate() : "BuildingAssign".Translate());
			if (Widgets.ButtonText(rect3, taggedString))
			{
				assignable.TryAssignPawn(pawn);
				if (assignable.MaxAssignedPawnsCount == 1)
				{
					Close();
				}
				else
				{
					SoundDefOf.Click.PlayOneShotOnCamera();
				}
			}
		}
		Rect rect5 = rect;
		rect5.xMin = rect2.xMax + 10f;
		rect5.xMax = rect3.xMin - 10f;
		string label = pawn.LabelCap + (accepted ? "" : (" (" + acceptanceReport.Reason.StripTags() + ")"));
		using (new TextBlock(TextAnchor.MiddleLeft))
		{
			Widgets.LabelEllipses(rect5, label);
		}
	}
}
