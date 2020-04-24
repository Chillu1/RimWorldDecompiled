using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Jetter : Thing
	{
		private enum JetterState
		{
			Resting,
			WickBurning,
			Jetting
		}

		private JetterState JState;

		private int WickTicksLeft;

		private int TicksUntilMove;

		protected Sustainer wickSoundSustainer;

		protected Sustainer jetSoundSustainer;

		private const int TicksBeforeBeginAccelerate = 25;

		private const int TicksBetweenMoves = 3;

		public override void Tick()
		{
			if (JState == JetterState.WickBurning)
			{
				base.Map.overlayDrawer.DrawOverlay(this, OverlayTypes.BurningWick);
				WickTicksLeft--;
				if (WickTicksLeft == 0)
				{
					StartJetting();
				}
			}
			else if (JState == JetterState.Jetting)
			{
				TicksUntilMove--;
				if (TicksUntilMove <= 0)
				{
					MoveJetter();
					TicksUntilMove = 3;
				}
			}
		}

		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			base.PostApplyDamage(dinfo, totalDamageDealt);
			if (!base.Destroyed && dinfo.Def.harmsHealth && JState == JetterState.Resting)
			{
				StartWick();
			}
		}

		protected void StartWick()
		{
			JState = JetterState.WickBurning;
			WickTicksLeft = 25;
			SoundDefOf.MetalHitImportant.PlayOneShot(this);
			wickSoundSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(this);
		}

		protected void StartJetting()
		{
			JState = JetterState.Jetting;
			TicksUntilMove = 3;
			wickSoundSustainer.End();
			wickSoundSustainer = null;
			wickSoundSustainer = SoundDefOf.HissJet.TrySpawnSustainer(this);
		}

		protected void MoveJetter()
		{
			IntVec3 intVec = base.Position + base.Rotation.FacingCell;
			if (!intVec.Walkable(base.Map) || base.Map.thingGrid.CellContains(intVec, ThingCategory.Pawn) || intVec.GetEdifice(base.Map) != null)
			{
				Destroy();
				GenExplosion.DoExplosion(base.Position, base.Map, 2.9f, DamageDefOf.Bomb, null);
			}
			else
			{
				base.Position = intVec;
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Destroy(mode);
			if (wickSoundSustainer != null)
			{
				wickSoundSustainer.End();
				wickSoundSustainer = null;
			}
			if (jetSoundSustainer != null)
			{
				jetSoundSustainer.End();
				jetSoundSustainer = null;
			}
		}
	}
}
