using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnColumnWorker_ControlGroup : PawnColumnWorker
{
	private const int Width = 160;

	private static List<MechanitorControlGroup> tmpControlGroups = new List<MechanitorControlGroup>();

	public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
	{
		if (pawn.IsGestating())
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, "Gestating".Translate().Colorize(ColoredText.SubtleGrayColor));
			Text.Anchor = TextAnchor.UpperLeft;
		}
		else if (pawn.GetOverseer() != null)
		{
			Widgets.Dropdown(rect, pawn, (Pawn p) => p.GetMechControlGroup(), Button_GenerateMenu, pawn.GetMechControlGroup().Index.ToString(), null, null, null, null, def.paintable);
		}
	}

	private IEnumerable<Widgets.DropdownMenuElement<MechanitorControlGroup>> Button_GenerateMenu(Pawn pawn)
	{
		Pawn overseer = pawn.GetOverseer();
		tmpControlGroups.Clear();
		tmpControlGroups.AddRange(overseer.mechanitor.controlGroups);
		MechanitorControlGroup currentControlGroup = pawn.GetMechControlGroup();
		for (int i = 0; i < tmpControlGroups.Count; i++)
		{
			MechanitorControlGroup localControlGroup = tmpControlGroups[i];
			if (currentControlGroup == localControlGroup)
			{
				yield return new Widgets.DropdownMenuElement<MechanitorControlGroup>
				{
					option = new FloatMenuOption("CannotAssignMechToControlGroup".Translate(localControlGroup.LabelIndexWithWorkMode) + ": " + "AssignMechAlreadyAssigned".Translate(), null),
					payload = localControlGroup
				};
				continue;
			}
			yield return new Widgets.DropdownMenuElement<MechanitorControlGroup>
			{
				option = new FloatMenuOption("AssignMechToControlGroup".Translate(localControlGroup.LabelIndexWithWorkMode), delegate
				{
					localControlGroup.Assign(pawn);
				}),
				payload = localControlGroup
			};
		}
		tmpControlGroups.Clear();
	}

	public override int GetMinWidth(PawnTable table)
	{
		return Mathf.Max(base.GetMinWidth(table), 160);
	}

	public override int GetMaxWidth(PawnTable table)
	{
		return Mathf.Min(base.GetMaxWidth(table), GetMinWidth(table));
	}
}
