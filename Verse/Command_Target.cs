using System;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class Command_Target : Command
{
	public Action<LocalTargetInfo> action;

	public Action<LocalTargetInfo> onUpdate;

	public TargetingParameters targetingParams;

	private int lastUpdate;

	public override void ProcessInput(Event ev)
	{
		base.ProcessInput(ev);
		SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
		Find.DesignatorManager.Deselect();
		Find.Targeter.BeginTargeting(targetingParams, delegate(LocalTargetInfo target)
		{
			action(target);
		}, null, null, null, null, null, playSoundOnAction: true, null, Update);
	}

	public override void GizmoUpdateOnMouseover()
	{
		Update(null);
		base.GizmoUpdateOnMouseover();
	}

	private void Update(LocalTargetInfo target)
	{
		if (lastUpdate != Time.frameCount)
		{
			lastUpdate = Time.frameCount;
			onUpdate?.Invoke(target);
		}
	}

	public override bool InheritInteractionsFrom(Gizmo other)
	{
		return false;
	}
}
