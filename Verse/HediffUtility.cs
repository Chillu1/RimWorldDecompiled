using System.Collections.Generic;

namespace Verse;

public static class HediffUtility
{
	public static T TryGetComp<T>(this Hediff hd) where T : HediffComp
	{
		if (!(hd is HediffWithComps hediffWithComps))
		{
			return null;
		}
		if (hediffWithComps.comps != null)
		{
			for (int i = 0; i < hediffWithComps.comps.Count; i++)
			{
				if (hediffWithComps.comps[i] is T result)
				{
					return result;
				}
			}
		}
		return null;
	}

	public static bool TryGetComp<T>(this Hediff hd, out T comp) where T : HediffComp
	{
		comp = hd.TryGetComp<T>();
		return comp != null;
	}

	public static bool IsTended(this Hediff hd)
	{
		if (!(hd is HediffWithComps hd2))
		{
			return false;
		}
		return hd2.TryGetComp<HediffComp_TendDuration>()?.IsTended ?? false;
	}

	public static bool IsPermanent(this Hediff hd)
	{
		if (!(hd is HediffWithComps hd2))
		{
			return false;
		}
		return hd2.TryGetComp<HediffComp_GetsPermanent>()?.IsPermanent ?? false;
	}

	public static bool FullyImmune(this Hediff hd)
	{
		if (!(hd is HediffWithComps hd2))
		{
			return false;
		}
		return hd2.TryGetComp<HediffComp_Immunizable>()?.FullyImmune ?? false;
	}

	public static bool CanHealFromTending(this Hediff_Injury hd)
	{
		if (hd.IsTended())
		{
			return !hd.IsPermanent();
		}
		return false;
	}

	public static bool CanHealNaturally(this Hediff_Injury hd)
	{
		return !hd.IsPermanent();
	}

	public static int CountAddedAndImplantedParts(this HediffSet hs)
	{
		int num = 0;
		List<Hediff> hediffs = hs.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def.countsAsAddedPartOrImplant)
			{
				num++;
			}
		}
		return num;
	}
}
