using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI.Group;

public class Trigger_ThingsDamageTaken : Trigger
{
	private List<Thing> things;

	private float damageFraction = 0.5f;

	public Trigger_ThingsDamageTaken(List<Thing> things, float damageFraction)
	{
		this.things = things;
		this.damageFraction = damageFraction;
	}

	public override bool ActivateOn(Lord lord, TriggerSignal signal)
	{
		if (signal.type == TriggerSignalType.Tick)
		{
			if (things.Count == 0)
			{
				return true;
			}
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < things.Count; i++)
			{
				if (things[i] != null && things[i].Spawned)
				{
					if (things[i] is Pawn)
					{
						Pawn t = (Pawn)things[i];
						num += (t.DestroyedOrNull() ? 0f : 1f);
					}
					else
					{
						num += (float)things[i].HitPoints / (float)things[i].MaxHitPoints;
					}
					num2++;
				}
			}
			if (Mathf.Approximately(num, 0f) || num2 == 0)
			{
				return true;
			}
			num /= (float)num2;
			return num < 1f - damageFraction;
		}
		return false;
	}
}
