using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class CompAbilityEffect_WithDest : CompAbilityEffect, ITargetingSource
	{
		protected LocalTargetInfo selectedTarget = LocalTargetInfo.Invalid;

		private List<IntVec3> cells = new List<IntVec3>();

		public new CompProperties_EffectWithDest Props => (CompProperties_EffectWithDest)props;

		public virtual TargetingParameters targetParams => new TargetingParameters
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

		public bool CanPlaceSelectedTargetAt(LocalTargetInfo target)
		{
			if (selectedTarget.Pawn != null)
			{
				if (!target.Cell.Impassable(parent.pawn.Map))
				{
					return target.Cell.Walkable(parent.pawn.Map);
				}
				return false;
			}
			return CanTeleportThingTo(target, parent.pawn.Map);
		}

		public static bool CanTeleportThingTo(LocalTargetInfo target, Map map)
		{
			Building edifice = target.Cell.GetEdifice(map);
			Building_Door building_Door;
			if (edifice != null && edifice.def.surfaceType != SurfaceType.Item && edifice.def.surfaceType != SurfaceType.Eat && ((building_Door = edifice as Building_Door) == null || !building_Door.Open))
			{
				return false;
			}
			List<Thing> thingList = target.Cell.GetThingList(map);
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
			if (Props.range > 0f && target.Cell.DistanceTo(selectedTarget.Cell) > Props.range)
			{
				return false;
			}
			if (Props.requiresLineOfSight && !GenSight.LineOfSight(selectedTarget.Cell, target.Cell, parent.pawn.Map))
			{
				return false;
			}
			return true;
		}

		public virtual bool ValidateTarget(LocalTargetInfo target)
		{
			return CanHitTarget(target);
		}

		public void DrawHighlight(LocalTargetInfo target)
		{
			if (Props.range > 0f)
			{
				if (Props.requiresLineOfSight)
				{
					GenDraw.DrawRadiusRing(selectedTarget.Cell, Props.range, Color.white, (IntVec3 c) => GenSight.LineOfSight(selectedTarget.Cell, c, parent.pawn.Map) && CanPlaceSelectedTargetAt(c));
				}
				else
				{
					GenDraw.DrawRadiusRing(selectedTarget.Cell, Props.range);
				}
			}
			if (target.IsValid)
			{
				GenDraw.DrawTargetHighlight(target);
			}
		}

		public void OnGUI(LocalTargetInfo target)
		{
			Texture2D icon = ((!target.IsValid) ? TexCommand.CannotShoot : parent.def.uiIcon);
			GenUI.DrawMouseAttachment(icon);
			string text = ExtraLabel(target);
			if (!text.NullOrEmpty())
			{
				Widgets.MouseAttachedLabel(text);
			}
		}

		public void OrderForceTarget(LocalTargetInfo target)
		{
			parent.QueueCastingJob(selectedTarget, target);
			selectedTarget = LocalTargetInfo.Invalid;
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
