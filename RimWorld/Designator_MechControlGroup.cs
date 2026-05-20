using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_MechControlGroup : Designator
{
	private static readonly HashSet<MechanitorControlGroup> tmpControlGroups = new HashSet<MechanitorControlGroup>();

	private static List<MechanitorControlGroup> tmpSelectedControlGroups = new List<MechanitorControlGroup>();

	public static IEnumerable<MechanitorControlGroup> SelectedControlGroups
	{
		get
		{
			tmpControlGroups.Clear();
			foreach (Pawn selectedPawn in Find.Selector.SelectedPawns)
			{
				if (selectedPawn == null)
				{
					continue;
				}
				Pawn pawn = selectedPawn;
				if (!pawn.IsColonyMech)
				{
					continue;
				}
				Pawn overseer = selectedPawn.GetOverseer();
				foreach (MechanitorControlGroup controlGroup in overseer.mechanitor.controlGroups)
				{
					if (controlGroup != null && tmpControlGroups.Add(controlGroup))
					{
						yield return controlGroup;
					}
				}
			}
		}
	}

	public Designator_MechControlGroup()
	{
		defaultLabel = "DesignatorMechControlGroup".Translate() + "...";
		defaultDesc = "DesignatorMechControlGroupDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Gizmos/MechControlGroup");
		showReverseDesignatorDisabledReason = true;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		foreach (Thing thing in c.GetThingList(base.Map))
		{
			if (CanDesignateThing(thing).Accepted)
			{
				return true;
			}
		}
		return false;
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (!(t is Pawn { IsColonyMech: not false } pawn) || pawn.GetOverseer() == null)
		{
			return false;
		}
		Pawn pawn2 = null;
		foreach (Pawn selectedPawn in Find.Selector.SelectedPawns)
		{
			Pawn overseer = selectedPawn.GetOverseer();
			if (pawn2 != null && pawn2 != overseer)
			{
				return false;
			}
			pawn2 = overseer;
		}
		AcceptanceReport canControlMechs = pawn2.mechanitor.CanControlMechs;
		if (!canControlMechs)
		{
			if (!canControlMechs.Reason.NullOrEmpty())
			{
				return ("DisabledCommand".Translate() + ": " + canControlMechs.Reason).Colorize(ColorLibrary.RedReadable);
			}
			return false;
		}
		return true;
	}

	public override void DesignateThing(Thing t)
	{
		ProcessInput(null);
	}

	private bool CanAssignToControlGroup(Pawn pawn, MechanitorControlGroup group)
	{
		Pawn overseer = pawn.GetOverseer();
		if (overseer != null)
		{
			return group.Tracker.Pawn == overseer;
		}
		return false;
	}

	public override void ProcessInput(Event ev)
	{
		tmpSelectedControlGroups.Clear();
		tmpSelectedControlGroups.AddRange(SelectedControlGroups);
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		for (int i = 0; i < tmpSelectedControlGroups.Count; i++)
		{
			MechanitorControlGroup selected = tmpSelectedControlGroups[i];
			if (Find.Selector.SelectedPawns.All((Pawn p) => p.GetMechControlGroup() == selected))
			{
				FloatMenuOption item = new FloatMenuOption("CannotAssignMechToControlGroup".Translate(selected.Index) + " (" + selected.WorkMode.LabelCap.ToString() + ")" + ": " + "AssignMechAlreadyAssigned".Translate(), null);
				list.Add(item);
				continue;
			}
			FloatMenuOption item2 = new FloatMenuOption("AssignMechToControlGroup".Translate(selected.Index) + " (" + selected.WorkMode.LabelCap.ToString() + ")", delegate
			{
				foreach (Pawn selectedPawn in Find.Selector.SelectedPawns)
				{
					if (CanAssignToControlGroup(selectedPawn, selected))
					{
						selected.Assign(selectedPawn);
					}
				}
			});
			list.Add(item2);
		}
		Find.WindowStack.Add(new FloatMenu(list));
		tmpSelectedControlGroups.Clear();
	}

	public override void SelectedUpdate()
	{
	}
}
