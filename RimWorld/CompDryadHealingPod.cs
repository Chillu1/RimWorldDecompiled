using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompDryadHealingPod : CompDryadHolder
{
	private int tickExpire = -1;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!respawningAfterLoad)
		{
			innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
			tickExpire = Find.TickManager.TicksGame + 600;
		}
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		if (mode == DestroyMode.WillReplace)
		{
			return;
		}
		innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near, delegate(Thing t, int c)
		{
			if (t is Pawn { mindState: not null } pawn)
			{
				pawn.mindState.returnToHealingPod = false;
			}
			t.Rotation = Rot4.South;
			SoundDefOf.Pawn_Dryad_Spawn.PlayOneShot(parent);
		}, null, playDropSound: false);
	}

	public override void CompTick()
	{
		base.CompTick();
		if (!parent.Destroyed)
		{
			if (innerContainer.Count > 0 && base.TreeComp.ShouldReturnToTree((Pawn)innerContainer[0]))
			{
				parent.Destroy();
			}
			else if (tickExpire >= 0 && Find.TickManager.TicksGame >= tickExpire)
			{
				tickExpire = -1;
				parent.Destroy();
			}
		}
	}

	public override void TryAcceptPawn(Pawn p)
	{
		base.TryAcceptPawn(p);
		p.Rotation = Rot4.South;
		tickComplete = Find.TickManager.TicksGame + (int)(60000f * base.Props.daysToComplete);
		tickExpire = -1;
	}

	protected override void Complete()
	{
		tickComplete = Find.TickManager.TicksGame;
		EffecterDefOf.DryadEmergeFromCocoon.Spawn(parent.Position, parent.Map).Cleanup();
		foreach (Thing item in (IEnumerable<Thing>)innerContainer)
		{
			if (!(item is Pawn pawn))
			{
				continue;
			}
			pawn.mindState.returnToHealingPod = false;
			int num = 1000;
			Hediff hediff;
			BodyPartRecord part;
			while (num > 0 && HealthUtility.TryGetWorstHealthCondition(pawn, out hediff, out part))
			{
				if (hediff != null)
				{
					pawn.health.RemoveHediff(hediff);
				}
				else if (part != null)
				{
					pawn.health.RestorePart(part);
				}
				num--;
			}
		}
		parent.Destroy();
	}
}
