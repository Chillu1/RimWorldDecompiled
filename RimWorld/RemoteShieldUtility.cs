using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class RemoteShieldUtility
{
	private static MechShield GetActiveMechShield(Pawn mech)
	{
		if (!mech.Spawned)
		{
			return null;
		}
		List<Thing> thingList = mech.Position.GetThingList(mech.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i] is MechShield mechShield && mechShield.IsTargeting(mech))
			{
				return mechShield;
			}
		}
		return null;
	}

	public static Gizmo GetActiveMechShieldGizmo(Pawn mech)
	{
		if (!Find.Selector.IsSelected(mech))
		{
			return null;
		}
		MechShield activeMechShield = GetActiveMechShield(mech);
		if (activeMechShield != null)
		{
			return new Gizmo_ProjectileInterceptorHitPoints
			{
				interceptor = activeMechShield.TryGetComp<CompProjectileInterceptor>()
			};
		}
		return null;
	}
}
