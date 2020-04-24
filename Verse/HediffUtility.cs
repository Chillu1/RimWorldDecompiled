using System.Collections.Generic;

namespace Verse
{
	public static class HediffUtility
	{
		public static T TryGetComp<T>(this Hediff hd) where T : HediffComp
		{
			HediffWithComps hediffWithComps = hd as HediffWithComps;
			if (hediffWithComps == null)
			{
				return null;
			}
			if (hediffWithComps.comps != null)
			{
				for (int i = 0; i < hediffWithComps.comps.Count; i++)
				{
					T val = hediffWithComps.comps[i] as T;
					if (val != null)
					{
						return val;
					}
				}
			}
			return null;
		}

		public static bool IsTended(this Hediff hd)
		{
			HediffWithComps hediffWithComps = hd as HediffWithComps;
			if (hediffWithComps == null)
			{
				return false;
			}
			return hediffWithComps.TryGetComp<HediffComp_TendDuration>()?.IsTended ?? false;
		}

		public static bool IsPermanent(this Hediff hd)
		{
			HediffWithComps hediffWithComps = hd as HediffWithComps;
			if (hediffWithComps == null)
			{
				return false;
			}
			return hediffWithComps.TryGetComp<HediffComp_GetsPermanent>()?.IsPermanent ?? false;
		}

		public static bool FullyImmune(this Hediff hd)
		{
			HediffWithComps hediffWithComps = hd as HediffWithComps;
			if (hediffWithComps == null)
			{
				return false;
			}
			return hediffWithComps.TryGetComp<HediffComp_Immunizable>()?.FullyImmune ?? false;
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
				if (hediffs[i] is Hediff_Implant)
				{
					num++;
				}
			}
			return num;
		}
	}
}
