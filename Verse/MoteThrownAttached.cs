using UnityEngine;

namespace Verse;

internal class MoteThrownAttached : MoteThrown
{
	private Vector3 attacheeLastPosition = new Vector3(-1000f, -1000f, -1000f);

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (link1.Linked)
		{
			attacheeLastPosition = link1.LastDrawPos;
		}
		exactPosition += def.mote.attachedDrawOffset;
	}

	protected override Vector3 NextExactPosition(float deltaTime)
	{
		Vector3 result = base.NextExactPosition(deltaTime);
		if (link1.Linked)
		{
			bool flag = detachAfterTicks == -1 || Find.TickManager.TicksGame - spawnTick < detachAfterTicks;
			if (!link1.Target.ThingDestroyed && flag)
			{
				link1.UpdateDrawPos();
			}
			Vector3 vector = link1.LastDrawPos - attacheeLastPosition;
			result += vector;
			result.y = AltitudeLayer.MoteOverhead.AltitudeFor();
			attacheeLastPosition = link1.LastDrawPos;
		}
		return result;
	}
}
