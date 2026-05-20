using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public sealed class FleckManager : IFleckCreator, IExposable
{
	public readonly Map parent;

	private Dictionary<Type, FleckSystem> systems = new Dictionary<Type, FleckSystem>();

	private DrawBatch drawBatch = new DrawBatch();

	public IEnumerable<FleckSystem> Systems => systems.Values;

	public FleckManager()
	{
		foreach (FleckDef item in DefDatabase<FleckDef>.AllDefsListForReading)
		{
			if (!systems.TryGetValue(item.fleckSystemClass, out var value))
			{
				value = CreateFleckSystemFor(item);
				systems.Add(item.fleckSystemClass, value);
			}
			value.handledDefs.Add(item);
		}
	}

	public FleckManager(Map parent)
		: this()
	{
		this.parent = parent;
	}

	public FleckSystem CreateFleckSystemFor(FleckDef def)
	{
		return (FleckSystem)Activator.CreateInstance(def.fleckSystemClass, this);
	}

	public void CreateFleck(FleckCreationData fleckData)
	{
		if (!systems.TryGetValue(fleckData.def.fleckSystemClass, out var value))
		{
			throw new Exception("No system to handle MoteDef " + fleckData.def?.ToString() + " found!?");
		}
		fleckData.spawnPosition.y = fleckData.def.altitudeLayer.AltitudeFor(fleckData.def.altitudeLayerIncOffset);
		value.CreateFleck(fleckData);
	}

	public void FleckManagerUpdate()
	{
		float deltaTime = Time.deltaTime;
		foreach (FleckSystem value in systems.Values)
		{
			value.Update(deltaTime);
		}
	}

	public void FleckManagerTick()
	{
		foreach (FleckSystem value in systems.Values)
		{
			value.Tick();
		}
	}

	public void FleckManagerDraw()
	{
		try
		{
			foreach (FleckSystem value in systems.Values)
			{
				value.Draw(drawBatch);
			}
		}
		finally
		{
			drawBatch.Flush();
		}
	}

	public void HandOverSystem(FleckSystem system)
	{
		Type type = system.GetType();
		if (systems.TryGetValue(type, out var value))
		{
			value.MergeWith(system);
		}
		else
		{
			systems.Add(type, system);
		}
	}

	public void FleckManagerOnGUI()
	{
		foreach (FleckSystem value in systems.Values)
		{
			value.OnGUI();
		}
	}

	public void ExposeData()
	{
	}
}
