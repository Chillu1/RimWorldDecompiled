using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Verb_Jump : Verb
	{
		private float cachedEffectiveRange = -1f;

		protected override float EffectiveRange
		{
			get
			{
				if (cachedEffectiveRange < 0f)
				{
					cachedEffectiveRange = base.EquipmentSource.GetStatValue(StatDefOf.JumpRange);
				}
				return cachedEffectiveRange;
			}
		}

		public override bool MultiSelect => true;

		protected override bool TryCastShot()
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Items with jump capability are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 550187797);
				return false;
			}
			CompReloadable reloadableCompSource = base.ReloadableCompSource;
			Pawn casterPawn = CasterPawn;
			if (casterPawn == null || reloadableCompSource == null || !reloadableCompSource.CanBeUsed)
			{
				return false;
			}
			IntVec3 cell = currentTarget.Cell;
			Map map = casterPawn.Map;
			reloadableCompSource.UsedOnce();
			PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnJumper, casterPawn, cell);
			if (pawnFlyer != null)
			{
				GenSpawn.Spawn(pawnFlyer, cell, map);
				return true;
			}
			return false;
		}

		public override void OrderForceTarget(LocalTargetInfo target)
		{
			Map map = CasterPawn.Map;
			IntVec3 intVec = RCellFinder.BestOrderedGotoDestNear_NewTemp(target.Cell, CasterPawn, AcceptableDestination);
			Job job = JobMaker.MakeJob(JobDefOf.CastJump, intVec);
			job.verbToUse = this;
			if (CasterPawn.jobs.TryTakeOrderedJob(job))
			{
				MoteMaker.MakeStaticMote(intVec, map, ThingDefOf.Mote_FeedbackGoto);
			}
			bool AcceptableDestination(IntVec3 c)
			{
				if (ValidJumpTarget(map, c))
				{
					return CanHitTargetFrom(caster.Position, c);
				}
				return false;
			}
		}

		public override bool ValidateTarget(LocalTargetInfo target)
		{
			if (caster == null)
			{
				return false;
			}
			if (!CanHitTarget(target) || !ValidJumpTarget(caster.Map, target.Cell))
			{
				return false;
			}
			if (!ReloadableUtility.CanUseConsideringQueuedJobs(CasterPawn, base.EquipmentSource))
			{
				return false;
			}
			return true;
		}

		public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
		{
			float num = EffectiveRange * EffectiveRange;
			IntVec3 cell = targ.Cell;
			if ((float)caster.Position.DistanceToSquared(cell) <= num)
			{
				return GenSight.LineOfSight(root, cell, caster.Map);
			}
			return false;
		}

		public override void OnGUI(LocalTargetInfo target)
		{
			if (CanHitTarget(target) && ValidJumpTarget(caster.Map, target.Cell))
			{
				base.OnGUI(target);
			}
			else
			{
				GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
			}
		}

		public override void DrawHighlight(LocalTargetInfo target)
		{
			if (target.IsValid && ValidJumpTarget(caster.Map, target.Cell))
			{
				GenDraw.DrawTargetHighlightWithLayer(target.CenterVector3, AltitudeLayer.MetaOverlays);
			}
			GenDraw.DrawRadiusRing(caster.Position, EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(caster.Position, c, caster.Map) && ValidJumpTarget(caster.Map, c));
		}

		public static bool ValidJumpTarget(Map map, IntVec3 cell)
		{
			if (!cell.IsValid || !cell.InBounds(map))
			{
				return false;
			}
			if (cell.Impassable(map) || !cell.Walkable(map) || cell.Fogged(map))
			{
				return false;
			}
			Building edifice = cell.GetEdifice(map);
			Building_Door building_Door;
			if (edifice != null && (building_Door = edifice as Building_Door) != null && !building_Door.Open)
			{
				return false;
			}
			return true;
		}
	}
}
