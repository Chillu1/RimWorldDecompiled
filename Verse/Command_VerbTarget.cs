using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class Command_VerbTarget : Command
	{
		public Verb verb;

		private List<Verb> groupedVerbs;

		public bool drawRadius = true;

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

		public override void GizmoUpdateOnMouseover()
		{
			if (drawRadius)
			{
				verb.verbProps.DrawRadiusRing(verb.caster.Position);
				if (!groupedVerbs.NullOrEmpty())
				{
					foreach (Verb groupedVerb in groupedVerbs)
					{
						groupedVerb.verbProps.DrawRadiusRing(groupedVerb.caster.Position);
					}
				}
			}
		}

		public override void MergeWith(Gizmo other)
		{
			base.MergeWith(other);
			Command_VerbTarget command_VerbTarget = other as Command_VerbTarget;
			if (command_VerbTarget == null)
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
				Find.Targeter.BeginTargeting(verb);
			}
		}
	}
}
