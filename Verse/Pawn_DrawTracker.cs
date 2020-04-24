using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Pawn_DrawTracker
	{
		private Pawn pawn;

		public PawnTweener tweener;

		private JitterHandler jitterer;

		public PawnLeaner leaner;

		public PawnRenderer renderer;

		public PawnUIOverlay ui;

		private PawnFootprintMaker footprintMaker;

		private PawnBreathMoteMaker breathMoteMaker;

		private const float MeleeJitterDistance = 0.5f;

		public Vector3 DrawPos
		{
			get
			{
				tweener.PreDrawPosCalculation();
				Vector3 tweenedPos = tweener.TweenedPos;
				tweenedPos += jitterer.CurrentOffset;
				tweenedPos += leaner.LeanOffset;
				tweenedPos.y = pawn.def.Altitude;
				return tweenedPos;
			}
		}

		public Pawn_DrawTracker(Pawn pawn)
		{
			this.pawn = pawn;
			tweener = new PawnTweener(pawn);
			jitterer = new JitterHandler();
			leaner = new PawnLeaner(pawn);
			renderer = new PawnRenderer(pawn);
			ui = new PawnUIOverlay(pawn);
			footprintMaker = new PawnFootprintMaker(pawn);
			breathMoteMaker = new PawnBreathMoteMaker(pawn);
		}

		public void DrawTrackerTick()
		{
			if (pawn.Spawned && (Current.ProgramState != ProgramState.Playing || Find.CameraDriver.CurrentViewRect.ExpandedBy(3).Contains(pawn.Position)))
			{
				jitterer.JitterHandlerTick();
				footprintMaker.FootprintMakerTick();
				breathMoteMaker.BreathMoteMakerTick();
				leaner.LeanerTick();
				renderer.RendererTick();
			}
		}

		public void DrawAt(Vector3 loc)
		{
			renderer.RenderPawnAt(loc);
		}

		public void Notify_Spawned()
		{
			tweener.ResetTweenedPosToRoot();
		}

		public void Notify_WarmingCastAlongLine(ShootLine newShootLine, IntVec3 ShootPosition)
		{
			leaner.Notify_WarmingCastAlongLine(newShootLine, ShootPosition);
		}

		public void Notify_DamageApplied(DamageInfo dinfo)
		{
			if (!pawn.Destroyed)
			{
				jitterer.Notify_DamageApplied(dinfo);
				renderer.Notify_DamageApplied(dinfo);
			}
		}

		public void Notify_DamageDeflected(DamageInfo dinfo)
		{
			if (!pawn.Destroyed)
			{
				jitterer.Notify_DamageDeflected(dinfo);
			}
		}

		public void Notify_MeleeAttackOn(Thing Target)
		{
			if (Target.Position != pawn.Position)
			{
				jitterer.AddOffset(0.5f, (Target.Position - pawn.Position).AngleFlat);
			}
			else if (Target.DrawPos != pawn.DrawPos)
			{
				jitterer.AddOffset(0.25f, (Target.DrawPos - pawn.DrawPos).AngleFlat());
			}
		}

		public void Notify_DebugAffected()
		{
			for (int i = 0; i < 10; i++)
			{
				MoteMaker.ThrowAirPuffUp(pawn.DrawPos, pawn.Map);
			}
			jitterer.AddOffset(0.05f, Rand.Range(0, 360));
		}
	}
}
