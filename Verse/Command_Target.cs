using RimWorld;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class Command_Target : Command
	{
		public Action<Thing> action;

		public TargetingParameters targetingParams;

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			Find.Targeter.BeginTargeting(targetingParams, delegate(LocalTargetInfo target)
			{
				action(target.Thing);
			});
		}

		public override bool InheritInteractionsFrom(Gizmo other)
		{
			return false;
		}
	}
}
