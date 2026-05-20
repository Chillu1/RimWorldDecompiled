using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class TemporaryThingDrawer : IExposable
{
	private List<TemporaryThingDrawable> drawables = new List<TemporaryThingDrawable>();

	public void Tick()
	{
		for (int num = drawables.Count - 1; num >= 0; num--)
		{
			TemporaryThingDrawable temporaryThingDrawable = drawables[num];
			if (temporaryThingDrawable.ticksLeft >= 0 && temporaryThingDrawable.thing != null)
			{
				temporaryThingDrawable.ticksLeft--;
			}
			else
			{
				drawables.RemoveAt(num);
			}
		}
	}

	public void Draw()
	{
		foreach (TemporaryThingDrawable drawable in drawables)
		{
			drawable.thing.DrawNowAt(drawable.position);
		}
	}

	public void AddThing(Thing t, Vector3 position, int ticks)
	{
		drawables.Add(new TemporaryThingDrawable
		{
			thing = t,
			position = position,
			ticksLeft = ticks
		});
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref drawables, "drawables", LookMode.Undefined);
	}
}
