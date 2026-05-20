using UnityEngine;
using Verse;

namespace RimWorld;

public class Command_VerbOwner : Command_VerbTarget
{
	private readonly CompApparelVerbOwner comp;

	public Color? overrideColor;

	public override string Label
	{
		get
		{
			if (comp.parent.def.mergeVerbGizmos || Find.Selector.SelectedPawns.Count < 2)
			{
				return base.Label;
			}
			return base.Label + " (" + verb.caster.LabelShortCap + ")";
		}
	}

	public override string TopRightLabel => comp.GizmoExtraLabel;

	public override Color IconDrawColor => overrideColor ?? base.IconDrawColor;

	public Command_VerbOwner(CompApparelVerbOwner comp)
	{
		this.comp = comp;
	}

	public override void GizmoUpdateOnMouseover()
	{
		verb.DrawHighlight(LocalTargetInfo.Invalid);
	}

	public override bool GroupsWith(Gizmo other)
	{
		if (!base.GroupsWith(other))
		{
			return false;
		}
		if (!(other is Command_VerbOwner command_VerbOwner))
		{
			return false;
		}
		if (comp.parent.def == command_VerbOwner.comp.parent.def)
		{
			return comp.parent.def.mergeVerbGizmos;
		}
		return false;
	}
}
