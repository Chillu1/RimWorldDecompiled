using UnityEngine;

namespace Verse;

public class TemporaryThingDrawable : IExposable
{
	public Thing thing;

	public Vector3 position;

	public int ticksLeft;

	public void ExposeData()
	{
		Scribe_References.Look(ref thing, "thing");
		Scribe_Values.Look(ref position, "position");
		Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
	}
}
