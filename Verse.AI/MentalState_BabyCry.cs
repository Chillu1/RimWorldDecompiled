using RimWorld;
using UnityEngine;

namespace Verse.AI;

public class MentalState_BabyCry : MentalState_BabyFit
{
	private int ticksUntilLeftTear;

	private int ticksUntilRightTear;

	private const int TicksBetweenTearDots = 35;

	private static readonly IntRange TicksBetweenTears = new IntRange(25, 40);

	private const float speed = 0.66f;

	private static readonly FloatRange randAngle = new FloatRange(10f, 30f);

	private static readonly FloatRange randScale = new FloatRange(0.6f, 1f);

	public override void MentalStateTick(int delta)
	{
		base.MentalStateTick(delta);
		float num = base.pawn.Drawer.renderer.BodyAngle(PawnRenderFlags.None);
		if (base.pawn.SpawnedParentOrMe is Pawn pawn && !pawn.Position.Fogged(pawn.Map))
		{
			if (--ticksUntilLeftTear <= 0)
			{
				pawn.Map.flecks.CreateFleck(new FleckCreationData
				{
					spawnPosition = base.pawn.DrawPosHeld.Value + new Vector3(-0.15f, 0f, 0.066f).RotatedBy(num),
					velocitySpeed = -0.66f,
					velocityAngle = 90f + num - randAngle.RandomInRange,
					def = FleckDefOf.FleckBabyCrying,
					scale = randScale.RandomInRange
				});
				ticksUntilLeftTear = TicksBetweenTears.RandomInRange;
			}
			if (--ticksUntilRightTear <= 0)
			{
				pawn.Map.flecks.CreateFleck(new FleckCreationData
				{
					spawnPosition = base.pawn.DrawPosHeld.Value + new Vector3(0.15f, 0f, 0.066f).RotatedBy(num),
					velocitySpeed = 0.66f,
					velocityAngle = 90f + num + randAngle.RandomInRange,
					def = FleckDefOf.FleckBabyCrying,
					scale = randScale.RandomInRange,
					exactScale = new Vector3(-1f, 1f, 1f)
				});
				ticksUntilRightTear = TicksBetweenTears.RandomInRange;
			}
			if (base.pawn.IsHashIntervalTick(35, delta))
			{
				MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_BabyCryingDots, new Vector3(0.27f, 0f, 0.066f).RotatedBy(num)).exactRotation = Rand.Value * 180f;
				MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_BabyCryingDots, new Vector3(-0.27f, 0f, 0.066f).RotatedBy(num)).exactRotation = Rand.Value * 180f;
			}
		}
	}

	protected override void AuraEffect(Thing source, Pawn hearer)
	{
		hearer.HearClamor(source, ClamorDefOf.BabyCry);
		if (source is Pawn otherPawn && hearer.needs.mood != null)
		{
			if (hearer == otherPawn.GetMother() || hearer == otherPawn.GetFather())
			{
				hearer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.MyCryingBaby, otherPawn);
			}
			else
			{
				hearer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.CryingBaby, otherPawn);
			}
			hearer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BabyCriedSocial, otherPawn);
		}
	}
}
