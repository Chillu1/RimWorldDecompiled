using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Dance : JobDriver
{
	private bool jumping;

	private int moveChangeInterval = 240;

	public int AgeTicks => Find.TickManager.TicksGame - startTick;

	public override Vector3 ForcedBodyOffset
	{
		get
		{
			float num = Mathf.Sin((float)AgeTicks / 60f * 8f);
			if (jumping)
			{
				float z = Mathf.Max(Mathf.Pow((num + 1f) * 0.5f, 2f) * 0.2f - 0.06f, 0f);
				return new Vector3(0f, 0f, z);
			}
			float num2 = Mathf.Sign(num);
			return new Vector3(EasingFunctions.EaseInOutQuad(Mathf.Abs(num) * 0.6f) * 0.09f * num2, 0f, 0f);
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		pawn.Rotation = Rot4.Random;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (!ModLister.CheckIdeology("Dance job"))
		{
			yield break;
		}
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		jumping = Rand.Bool;
		toil.tickIntervalAction = delegate
		{
			if (AgeTicks % moveChangeInterval == 0)
			{
				jumping = !jumping;
			}
			if (AgeTicks % 120 == 0 && !jumping)
			{
				pawn.Rotation = Rot4.Random;
			}
		};
		toil.socialMode = RandomSocialMode.SuperActive;
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.handlingFacing = true;
		yield return toil;
	}
}
