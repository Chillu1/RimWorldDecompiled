using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class EffecterDef : Def
{
	public enum OffsetMode
	{
		TrueCenterCompensated,
		Free
	}

	public List<SubEffecterDef> children;

	public float positionRadius;

	public FloatRange offsetTowardsTarget;

	public int maintainTicks;

	public float randomWeight = 1f;

	public OffsetMode offsetMode;

	public Effecter Spawn()
	{
		return new Effecter(this);
	}

	public Effecter SpawnMaintained(IntVec3 target, Map map, float scale = 1f)
	{
		Effecter effecter = Spawn(target, map, scale);
		if (maintainTicks != 0)
		{
			map.effecterMaintainer.AddEffecterToMaintain(effecter, target, maintainTicks);
		}
		else
		{
			effecter.Cleanup();
		}
		return effecter;
	}

	public Effecter SpawnMaintained(Thing target, Map map, float scale = 1f)
	{
		Effecter effecter = Spawn(target, map, scale);
		if (maintainTicks != 0)
		{
			map.effecterMaintainer.AddEffecterToMaintain(effecter, target, maintainTicks);
		}
		else
		{
			effecter.Cleanup();
		}
		return effecter;
	}

	public Effecter SpawnMaintained(TargetInfo A, TargetInfo B, float scale = 1f)
	{
		Effecter effecter = Spawn(A, B, scale);
		if (maintainTicks != 0)
		{
			A.Map.effecterMaintainer.AddEffecterToMaintain(effecter, A, B, maintainTicks);
		}
		else
		{
			effecter.Cleanup();
		}
		return effecter;
	}

	public Effecter Spawn(IntVec3 target, Map map, float scale = 1f)
	{
		Effecter effecter = new Effecter(this);
		TargetInfo targetInfo = new TargetInfo(target, map);
		effecter.scale = scale;
		effecter.Trigger(targetInfo, targetInfo);
		return effecter;
	}

	public Effecter Spawn(IntVec3 targetA, IntVec3 targetB, Map map, float scale = 1f)
	{
		Effecter effecter = new Effecter(this);
		TargetInfo a = new TargetInfo(targetA, map);
		effecter.scale = scale;
		effecter.Trigger(a, new TargetInfo(targetB, map));
		return effecter;
	}

	public Effecter Spawn(IntVec3 target, Map map, Vector3 offset, float scale = 1f)
	{
		Effecter effecter = new Effecter(this);
		TargetInfo targetInfo = new TargetInfo(target, map);
		effecter.scale = scale;
		effecter.offset = offset;
		effecter.Trigger(targetInfo, targetInfo);
		return effecter;
	}

	public Effecter Spawn(Thing target, Map map, float scale = 1f)
	{
		Effecter effecter = new Effecter(this);
		effecter.offset = ((offsetMode == OffsetMode.TrueCenterCompensated) ? (target.TrueCenter() - target.Position.ToVector3Shifted()) : Vector3.zero);
		effecter.scale = scale;
		TargetInfo targetInfo = new TargetInfo(target.Position, map);
		effecter.Trigger(targetInfo, targetInfo);
		return effecter;
	}

	public Effecter Spawn(TargetInfo targetA, TargetInfo targetB, float scale = 1f)
	{
		Effecter effecter = new Effecter(this);
		effecter.scale = scale;
		effecter.Trigger(targetA, targetB);
		return effecter;
	}

	public Effecter SpawnAttached(Thing target, Map map, float scale = 1f)
	{
		Effecter effecter = new Effecter(this);
		effecter.offset = ((offsetMode == OffsetMode.TrueCenterCompensated) ? (target.TrueCenter() - target.Position.ToVector3Shifted()) : Vector3.zero);
		effecter.scale = scale;
		effecter.Trigger(target, target);
		return effecter;
	}

	public Effecter Spawn(Thing target, Map map, Vector3 offset)
	{
		Effecter effecter = new Effecter(this);
		effecter.offset = offset;
		TargetInfo targetInfo = new TargetInfo(target.Position, map);
		effecter.Trigger(targetInfo, targetInfo);
		return effecter;
	}
}
