using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class Effecter
{
	public EffecterDef def;

	public List<SubEffecter> children = new List<SubEffecter>();

	public int ticksLeft = -1;

	public Vector3 offset;

	public float scale = 1f;

	public int spawnTick;

	public Effecter(EffecterDef def)
	{
		this.def = def;
		spawnTick = Find.TickManager.TicksGame;
		for (int i = 0; i < def.children.Count; i++)
		{
			SubEffecterDef subEffecterDef = def.children[i];
			if (DebugSettings.anomalyDarkeningFX)
			{
				children.Add(subEffecterDef.Spawn(this));
			}
		}
	}

	public void EffectTick(TargetInfo A, TargetInfo B)
	{
		for (int i = 0; i < children.Count; i++)
		{
			children[i].SubEffectTick(A, B);
		}
	}

	public Effecter Trigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1)
	{
		for (int i = 0; i < children.Count; i++)
		{
			children[i].SubTrigger(A, B, overrideSpawnTick);
		}
		return this;
	}

	public void ForceEnd()
	{
		ticksLeft = 0;
		Cleanup();
	}

	public void Cleanup()
	{
		for (int i = 0; i < children.Count; i++)
		{
			children[i].SubCleanup();
		}
	}
}
