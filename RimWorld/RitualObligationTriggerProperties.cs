using System;

namespace RimWorld
{
	public abstract class RitualObligationTriggerProperties
	{
		public Type triggerClass;

		public bool mustBePlayerIdeo;

		public virtual RitualObligationTrigger GetInstance(Precept_Ritual parent)
		{
			RitualObligationTrigger obj = (RitualObligationTrigger)Activator.CreateInstance(triggerClass);
			obj.ritual = parent;
			return obj;
		}
	}
}
