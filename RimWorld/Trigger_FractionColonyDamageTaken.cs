using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class Trigger_FractionColonyDamageTaken : Trigger
	{
		private float desiredColonyDamageFraction;

		private float minDamage;

		private TriggerData_FractionColonyDamageTaken Data => (TriggerData_FractionColonyDamageTaken)data;

		public Trigger_FractionColonyDamageTaken(float desiredColonyDamageFraction, float minDamage = float.MaxValue)
		{
			data = new TriggerData_FractionColonyDamageTaken();
			this.desiredColonyDamageFraction = desiredColonyDamageFraction;
			this.minDamage = minDamage;
		}

		public override void SourceToilBecameActive(Transition transition, LordToil previousToil)
		{
			if (!transition.sources.Contains(previousToil))
			{
				Data.startColonyDamage = transition.Map.damageWatcher.DamageTakenEver;
			}
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick)
			{
				if (data == null || !(data is TriggerData_FractionColonyDamageTaken))
				{
					BackCompatibility.TriggerDataFractionColonyDamageTakenNull(this, lord.Map);
				}
				float num = Mathf.Max((float)lord.initialColonyHealthTotal * desiredColonyDamageFraction, minDamage);
				return lord.Map.damageWatcher.DamageTakenEver > Data.startColonyDamage + num;
			}
			return false;
		}
	}
}
