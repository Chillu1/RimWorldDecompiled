using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnBreathMoteMaker
{
	private Pawn pawn;

	private bool doThisBreath;

	private const int BreathDuration = 80;

	private const int BreathInterval = 320;

	private const int MoteInterval = 8;

	private const float MaxBreathTemperature = 0f;

	private const float BreathRotationOffsetDist = 0.21f;

	private static readonly Vector3 BreathOffset = new Vector3(0f, 0f, -0.04f);

	public PawnBreathMoteMaker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void ProcessPostTickVisuals(int ticksPassed)
	{
		if (pawn.RaceProps.Humanlike && !pawn.RaceProps.IsMechanoid && !pawn.IsShambler && (!ModsConfig.OdysseyActive || !(pawn.Position.GetVacuum(pawn.MapHeld) > 0f)))
		{
			int num = Mathf.Abs(Find.TickManager.TicksGame + pawn.HashOffset()) % 320;
			if (num < ticksPassed)
			{
				doThisBreath = pawn.AmbientTemperature < 0f && pawn.GetPosture() == PawnPosture.Standing;
			}
			if (doThisBreath && num < 80 && num % 8 < ticksPassed)
			{
				TryMakeBreathMote();
			}
		}
	}

	private void TryMakeBreathMote()
	{
		FleckMaker.ThrowBreathPuff(pawn.Drawer.DrawPos + pawn.Drawer.renderer.BaseHeadOffsetAt(pawn.Rotation) + pawn.Rotation.FacingCell.ToVector3() * 0.21f + BreathOffset, inheritVelocity: pawn.Drawer.tweener.LastTickTweenedVelocity, map: pawn.Map, throwAngle: pawn.Rotation.AsAngle);
	}
}
