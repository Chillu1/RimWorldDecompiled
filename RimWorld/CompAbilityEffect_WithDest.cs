using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class CompAbilityEffect_WithDest : CompAbilityEffect, ITargetingSource
	{
		protected LocalTargetInfo selectedTarget;

		private List<IntVec3> cells = new List<IntVec3>();

		public new CompProperties_EffectWithDest Props => (CompProperties_EffectWithDest)props;

		public TargetingParameters targetParams => new TargetingParameters
		{
			canTargetLocations = true
		};

		public bool MultiSelect => false;

		public Thing Caster => parent.pawn;

		public Pawn CasterPawn => parent.pawn;

		public Verb GetVerb => null;

		public bool CasterIsPawn => true;

		public bool IsMeleeAttack => false;

		public bool Targetable => true;

		public Texture2D UIIcon => BaseContent.BadTex;

		public ITargetingSource DestinationSelector => null;

		public LocalTargetInfo GetDestination(LocalTargetInfo target)
		{
			Map map = parent.pawn.Map;
			switch (Props.destination)
			{
			case AbilityEffectDestination.Caster:
				return new LocalTargetInfo(parent.pawn.InteractionCell);
			case AbilityEffectDestination.RandomInRange:
			{
				cells.Clear();
				int num = GenRadial.NumCellsInRadius(Props.randomRange.max);
				for (int i = 0; i < num; i++)
				{
					IntVec3 intVec = GenRadial.RadialPattern[i];
					if (!(intVec.DistanceTo(IntVec3.Zero) < Props.randomRange.min))
					{
						IntVec3 intVec2 = target.Cell + intVec;
						if (intVec2.Standable(map) && (!Props.requiresLineOfSight || GenSight.LineOfSight(target.Cell, intVec2, map)))
						{
							cells.Add(intVec2);
						}
					}
				}
				if (cells.Any())
				{
					return new LocalTargetInfo(cells.RandomElement());
				}
				Messages.Message("NoValidDestinationFound".Translate(parent.def.LabelCap), MessageTypeDefOf.RejectInput);
				return LocalTargetInfo.Invalid;
			}
			case AbilityEffectDestination.Selected:
				return target;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		protected bool CanPlaceSelectedTargetAt(LocalTargetInfo target)
		{
			if (selectedTarget.Pawn != null)
			{
				return !target.Cell.Impassable(parent.pawn.Map);
			}
			Building edifice = target.Cell.GetEdifice(parent.pawn.Map);
			Building_Door building_Door;
			if (edifice != null && edifice.def.surfaceType != SurfaceType.Item && edifice.def.surfaceType != SurfaceType.Eat && ((building_Door = (edifice as Building_Door)) == null || !building_Door.Open))
			{
				return false;
			}
			List<Thing> thingList = target.Cell.GetThingList(parent.pawn.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.category == ThingCategory.Item)
				{
					return false;
				}
			}
			return true;
		}

		public virtual bool CanHitTarget(LocalTargetInfo target)
		{
			if (!target.IsValid)
			{
				return false;
			}
			if (target.Cell.DistanceTo(target.Cell) > Props.range)
			{
				return false;
			}
			if (!CanPlaceSelectedTargetAt(target))
			{
				return false;
			}
			if (Props.requiresLineOfSight && !GenSight.LineOfSight(selectedTarget.Cell, target.Cell, parent.pawn.Map))
			{
				return false;
			}
			return true;
		}

		public bool ValidateTarget(LocalTargetInfo target)
		{
			return CanHitTarget(target);
		}

		public void DrawHighlight(LocalTargetInfo target)
		{
			if (Props.requiresLineOfSight)
			{
				GenDraw.DrawRadiusRing(selectedTarget.Cell, Props.range, Color.white, (IntVec3 c) => GenSight.LineOfSight(selectedTarget.Cell, c, parent.pawn.Map) && CanPlaceSelectedTargetAt(c));
			}
			else
			{
				GenDraw.DrawRadiusRing(selectedTarget.Cell, Props.range);
			}
			if (target.IsValid)
			{
				GenDraw.DrawTargetHighlight(target);
			}
		}

		public void OnGUI(LocalTargetInfo target)
		{
			Texture2D icon = (!target.IsValid) ? TexCommand.CannotShoot : parent.def.uiIcon;
			GenUI.DrawMouseAttachment(icon);
		}

		public void OrderForceTarget(LocalTargetInfo target)
		{
			parent.QueueCastingJob(selectedTarget, target);
		}

		public void SetTarget(LocalTargetInfo target)
		{
			selectedTarget = target;
		}

		public virtual void SelectDestination()
		{
			Find.Targeter.BeginTargeting(this);
		}
	}
}
