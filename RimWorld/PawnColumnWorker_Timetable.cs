using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class PawnColumnWorker_Timetable : PawnColumnWorker
{
	public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
	{
		if (pawn.timetable != null && !pawn.IsSubhuman)
		{
			float num = rect.x;
			float num2 = rect.width / 24f;
			for (int i = 0; i < 24; i++)
			{
				Rect rect2 = new Rect(num, rect.y, num2, rect.height);
				DoTimeAssignment(rect2, pawn, i);
				num += num2;
			}
			GUI.color = Color.white;
			if (TimeAssignmentSelector.selectedAssignment != null)
			{
				UIHighlighter.HighlightOpportunity(rect, "TimeAssignmentTableRow-If" + TimeAssignmentSelector.selectedAssignment.defName + "Selected");
			}
		}
	}

	public override void DoHeader(Rect rect, PawnTable table)
	{
		float num = rect.x;
		Text.Font = GameFont.Tiny;
		Text.Anchor = TextAnchor.LowerCenter;
		float num2 = rect.width / 24f;
		for (int i = 0; i < 24; i++)
		{
			Widgets.Label(new Rect(num, rect.y, num2, rect.height + 3f), i.ToString());
			num += num2;
		}
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public override int GetMinWidth(PawnTable table)
	{
		return Mathf.Max(base.GetMinWidth(table), 360);
	}

	public override int GetOptimalWidth(PawnTable table)
	{
		return Mathf.Clamp(504, GetMinWidth(table), GetMaxWidth(table));
	}

	public override int GetMaxWidth(PawnTable table)
	{
		return Mathf.Min(base.GetMaxWidth(table), 600);
	}

	public override int GetMinHeaderHeight(PawnTable table)
	{
		return Mathf.Max(base.GetMinHeaderHeight(table), 15);
	}

	public override int Compare(Pawn a, Pawn b)
	{
		return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
	}

	private int GetValueToCompare(Pawn pawn)
	{
		if (pawn.timetable == null)
		{
			return int.MinValue;
		}
		return pawn.timetable.times.FirstIndexOf((TimeAssignmentDef x) => x == TimeAssignmentDefOf.Work);
	}

	private void DoTimeAssignment(Rect rect, Pawn p, int hour)
	{
		rect = rect.ContractedBy(1f);
		bool mouseButton = Input.GetMouseButton(0);
		TimeAssignmentDef assignment = p.timetable.GetAssignment(hour);
		GUI.DrawTexture(rect, assignment.ColorTexture);
		if (!mouseButton)
		{
			MouseoverSounds.DoRegion(rect);
		}
		if (!Mouse.IsOver(rect))
		{
			return;
		}
		Widgets.DrawBox(rect, 2);
		if (mouseButton && assignment != TimeAssignmentSelector.selectedAssignment && TimeAssignmentSelector.selectedAssignment != null)
		{
			SoundDefOf.Designate_DragStandard_Changed_NoCam.PlayOneShotOnCamera();
			p.timetable.SetAssignment(hour, TimeAssignmentSelector.selectedAssignment);
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeAssignments, KnowledgeAmount.SmallInteraction);
			if (TimeAssignmentSelector.selectedAssignment == TimeAssignmentDefOf.Meditate)
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.MeditationSchedule, KnowledgeAmount.Total);
			}
		}
	}
}
