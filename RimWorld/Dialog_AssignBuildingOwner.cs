using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_AssignBuildingOwner : Window
	{
		private CompAssignableToPawn assignable;

		private Vector2 scrollPosition;

		private const float EntryHeight = 35f;

		private const float LineSpacing = 8f;

		public override Vector2 InitialSize => new Vector2(620f, 500f);

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
			outRect.width -= 16f;
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)assignable.AssigningCandidates.Count() * 35f + 100f);
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			try
			{
				float num = 0f;
				bool flag = false;
				foreach (Pawn assignedPawn in assignable.AssignedPawns)
				{
					flag = true;
					Rect rect = new Rect(0f, num, viewRect.width * 0.7f, 32f);
					Widgets.Label(rect, assignedPawn.LabelCap);
					rect.x = rect.xMax;
					rect.width = viewRect.width * 0.3f;
					if (Widgets.ButtonText(rect, "BuildingUnassign".Translate()))
					{
						assignable.TryUnassignPawn(assignedPawn);
						SoundDefOf.Click.PlayOneShotOnCamera();
						return;
					}
					num += 35f;
				}
				if (flag)
				{
					num += 15f;
				}
				foreach (Pawn assigningCandidate in assignable.AssigningCandidates)
				{
					if (assignable.AssignedPawns.Contains(assigningCandidate))
					{
						continue;
					}
					AcceptanceReport acceptanceReport = assignable.CanAssignTo(assigningCandidate);
					bool accepted = acceptanceReport.Accepted;
					string text = assigningCandidate.LabelCap + (accepted ? "" : (" (" + acceptanceReport.Reason.StripTags() + ")"));
					float width = viewRect.width * 0.7f;
					float num2 = Text.CalcHeight(text, width);
					float num3 = ((35f > num2) ? 35f : num2);
					Rect rect2 = new Rect(0f, num, width, num3);
					if (!accepted)
					{
						GUI.color = Color.gray;
					}
					Widgets.Label(rect2, text);
					rect2.x = rect2.xMax;
					rect2.width = viewRect.width * 0.3f;
					rect2.height = 35f;
					TaggedString taggedString = (assignable.AssignedAnything(assigningCandidate) ? "BuildingReassign".Translate() : "BuildingAssign".Translate());
					if (Widgets.ButtonText(rect2, taggedString, drawBackground: true, doMouseoverSound: true, accepted))
					{
						assignable.TryAssignPawn(assigningCandidate);
						if (assignable.MaxAssignedPawnsCount == 1)
						{
							Close();
						}
						else
						{
							SoundDefOf.Click.PlayOneShotOnCamera();
						}
						break;
					}
					GUI.color = Color.white;
					num += num3;
				}
			}
			finally
			{
				Widgets.EndScrollView();
			}
		}
	}
}
