using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class EnterCooldownComp : WorldObjectComp
{
	private int endTick;

	public WorldObjectCompProperties_EnterCooldown Props => (WorldObjectCompProperties_EnterCooldown)props;

	public bool Active => endTick > GenTicks.TicksGame;

	public bool BlocksEntering
	{
		get
		{
			if (Active)
			{
				return !base.ParentHasMap;
			}
			return false;
		}
	}

	public int TicksLeft
	{
		get
		{
			if (!Active)
			{
				return 0;
			}
			return endTick - GenTicks.TicksGame;
		}
	}

	public float DaysLeft => (float)TicksLeft / 60000f;

	public void Start(float? durationDays = null)
	{
		float num = durationDays ?? Props.durationDays;
		endTick = GenTicks.TicksGame + Mathf.RoundToInt(num * 60000f);
	}

	public void Stop()
	{
		endTick = 0;
	}

	public override void PostMapGenerate()
	{
		base.PostMapGenerate();
		if (Active)
		{
			Stop();
		}
	}

	public override void PostMyMapRemoved()
	{
		base.PostMyMapRemoved();
		if (Props.autoStartOnMapRemoved)
		{
			Start();
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		int value = 0;
		Scribe_Values.Look(ref value, "ticksLeft", 0);
		Scribe_Values.Look(ref endTick, "endTick", 0);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && value > 0 && endTick == 0)
		{
			endTick = GenTicks.TicksGame + value;
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Set enter cooldown to 1 hour",
				action = delegate
				{
					endTick = GenTicks.TicksGame + 2500;
				}
			};
			yield return new Command_Action
			{
				defaultLabel = "DEV: Reset enter cooldown",
				action = delegate
				{
					endTick = GenTicks.TicksGame;
				}
			};
		}
	}
}
