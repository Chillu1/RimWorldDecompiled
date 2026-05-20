using Verse;

namespace RimWorld;

public class CompUseEffect_DestroySelf : CompUseEffect
{
	private int delayTicks = -1;

	private CompProperties_UseEffectDestroySelf Props => (CompProperties_UseEffectDestroySelf)props;

	public override float OrderPriority => Props.orderPriority;

	public override void DoEffect(Pawn usedBy)
	{
		base.DoEffect(usedBy);
		if (Props.delayTicks <= 0)
		{
			DoDestroy();
		}
		else
		{
			delayTicks = Props.delayTicks;
		}
	}

	private void DoDestroy()
	{
		if (Props.effecterDef != null)
		{
			Effecter obj = new Effecter(Props.effecterDef);
			obj.Trigger(new TargetInfo(parent.Position, parent.Map), TargetInfo.Invalid);
			obj.Cleanup();
		}
		if (Props.spawnKilledLeavings)
		{
			GenLeaving.DoLeavingsFor(parent, parent.MapHeld, DestroyMode.KillFinalizeLeavingsOnly);
		}
		if (!Props.leavings.NullOrEmpty())
		{
			Rand.PushState(parent.thingIDNumber);
			for (int i = 0; i < Props.leavings.Count; i++)
			{
				ThingDefCountClass thingDefCountClass = Props.leavings[i];
				if (!thingDefCountClass.IsChanceBased || Rand.Chance(thingDefCountClass.DropChance))
				{
					Thing thing = ThingMaker.MakeThing(Props.leavings[i].thingDef);
					thing.stackCount = Props.leavings[i].count;
					GenDrop.TryDropSpawn(thing, parent.PositionHeld, parent.MapHeld, ThingPlaceMode.Near, out var _);
				}
			}
			Rand.PopState();
		}
		parent.SplitOff(1).Destroy();
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref delayTicks, "delayTicks", -1);
	}

	public override void CompTick()
	{
		base.CompTick();
		if (delayTicks > 0)
		{
			delayTicks--;
		}
		if (delayTicks == 0)
		{
			DoDestroy();
			delayTicks = -1;
		}
	}
}
