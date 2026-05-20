using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class Command_VerbTarget : Command
{
	public Verb verb;

	private List<Verb> groupedVerbs;

	public Thing ownerThing;

	public bool drawRadius = true;

	public bool requiresAvailableVerb = true;

	public override Color IconDrawColor
	{
		get
		{
			if (verb.EquipmentSource != null)
			{
				return verb.EquipmentSource.DrawColor;
			}
			return base.IconDrawColor;
		}
	}

	public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
	{
		if (ownerThing != null)
		{
			Thing thing = ownerThing;
			float scale = iconDrawScale * 0.85f;
			bool lowLight = parms.lowLight;
			Widgets.ThingIcon(rect, thing, 1f, null, stackOfOne: false, scale, lowLight);
		}
		else
		{
			base.DrawIcon(rect, buttonMat, parms);
		}
	}

	public override void GizmoUpdateOnMouseover()
	{
		if (!drawRadius)
		{
			return;
		}
		verb.verbProps.DrawRadiusRing(verb.caster.Position, verb);
		if (groupedVerbs.NullOrEmpty())
		{
			return;
		}
		foreach (Verb groupedVerb in groupedVerbs)
		{
			groupedVerb.verbProps.DrawRadiusRing(groupedVerb.caster.Position, groupedVerb);
		}
	}

	public override bool GroupsWith(Gizmo other)
	{
		if (!(other is Command_VerbTarget command_VerbTarget))
		{
			return false;
		}
		if (ownerThing != null && command_VerbTarget.ownerThing != null)
		{
			return ownerThing.def == command_VerbTarget.ownerThing.def;
		}
		return base.GroupsWith(other);
	}

	public override void MergeWith(Gizmo other)
	{
		base.MergeWith(other);
		if (!(other is Command_VerbTarget command_VerbTarget))
		{
			Log.ErrorOnce("Tried to merge Command_VerbTarget with unexpected type", 73406263);
			return;
		}
		if (groupedVerbs == null)
		{
			groupedVerbs = new List<Verb>();
		}
		groupedVerbs.Add(command_VerbTarget.verb);
		if (command_VerbTarget.groupedVerbs != null)
		{
			groupedVerbs.AddRange(command_VerbTarget.groupedVerbs);
		}
	}

	public override void ProcessInput(Event ev)
	{
		base.ProcessInput(ev);
		SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
		Targeter targeter = Find.Targeter;
		if (verb.CasterIsPawn && targeter.targetingSource != null && targeter.targetingSource.GetVerb.verbProps == verb.verbProps)
		{
			Pawn casterPawn = verb.CasterPawn;
			if (!targeter.IsPawnTargeting(casterPawn))
			{
				targeter.targetingSourceAdditionalPawns.Add(casterPawn);
			}
		}
		else
		{
			Find.Targeter.BeginTargeting(verb, null, allowNonSelectedTargetingSource: false, null, null, requiresAvailableVerb);
		}
	}
}
