using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class VerbUtility
	{
		public struct VerbPropertiesWithSource
		{
			public VerbProperties verbProps;

			public Tool tool;

			public ManeuverDef maneuver;

			public ToolCapacityDef ToolCapacity
			{
				get
				{
					if (maneuver == null)
					{
						return null;
					}
					return maneuver.requiredCapacity;
				}
			}

			public VerbPropertiesWithSource(VerbProperties verbProps)
			{
				this.verbProps = verbProps;
				tool = null;
				maneuver = null;
			}

			public VerbPropertiesWithSource(VerbProperties verbProps, Tool tool, ManeuverDef maneuver)
			{
				this.verbProps = verbProps;
				this.tool = tool;
				this.maneuver = maneuver;
			}
		}

		public static ThingDef GetProjectile(this Verb verb)
		{
			return (verb as Verb_LaunchProjectile)?.Projectile;
		}

		public static DamageDef GetDamageDef(this Verb verb)
		{
			if (verb.verbProps.LaunchesProjectile)
			{
				return verb.GetProjectile()?.projectile.damageDef;
			}
			return verb.verbProps.meleeDamageDef;
		}

		public static bool IsIncendiary(this Verb verb)
		{
			return verb.GetProjectile()?.projectile.ai_IsIncendiary ?? false;
		}

		public static bool ProjectileFliesOverhead(this Verb verb)
		{
			return verb.GetProjectile()?.projectile.flyOverhead ?? false;
		}

		public static bool HarmsHealth(this Verb verb)
		{
			return verb.GetDamageDef()?.harmsHealth ?? false;
		}

		public static bool IsEMP(this Verb verb)
		{
			return verb.GetDamageDef() == DamageDefOf.EMP;
		}

		public static bool UsesExplosiveProjectiles(this Verb verb)
		{
			ThingDef projectile = verb.GetProjectile();
			if (projectile != null)
			{
				return projectile.projectile.explosionRadius > 0f;
			}
			return false;
		}

		public static List<Verb> GetConcreteExampleVerbs(Def def, ThingDef stuff = null)
		{
			List<Verb> result = null;
			ThingDef thingDef = def as ThingDef;
			if (thingDef != null)
			{
				Thing concreteExample = thingDef.GetConcreteExample(stuff);
				result = ((concreteExample is Pawn) ? ((Pawn)concreteExample).VerbTracker.AllVerbs : ((!(concreteExample is ThingWithComps)) ? null : ((ThingWithComps)concreteExample).GetComp<CompEquippable>().AllVerbs));
			}
			HediffDef hediffDef = def as HediffDef;
			if (hediffDef != null)
			{
				result = hediffDef.ConcreteExample.TryGetComp<HediffComp_VerbGiver>().VerbTracker.AllVerbs;
			}
			return result;
		}

		public static float CalculateAdjustedForcedMiss(float forcedMiss, IntVec3 vector)
		{
			float num = vector.LengthHorizontalSquared;
			if (num < 9f)
			{
				return 0f;
			}
			if (num < 25f)
			{
				return forcedMiss * 0.5f;
			}
			if (num < 49f)
			{
				return forcedMiss * 0.8f;
			}
			return forcedMiss;
		}

		public static float InterceptChanceFactorFromDistance(Vector3 origin, IntVec3 c)
		{
			float num = (c.ToVector3Shifted() - origin).MagnitudeHorizontalSquared();
			if (num <= 25f)
			{
				return 0f;
			}
			if (num >= 144f)
			{
				return 1f;
			}
			return Mathf.InverseLerp(25f, 144f, num);
		}

		public static IEnumerable<VerbPropertiesWithSource> GetAllVerbProperties(List<VerbProperties> verbProps, List<Tool> tools)
		{
			if (verbProps != null)
			{
				for (int j = 0; j < verbProps.Count; j++)
				{
					yield return new VerbPropertiesWithSource(verbProps[j]);
				}
			}
			if (tools != null)
			{
				for (int j = 0; j < tools.Count; j++)
				{
					foreach (ManeuverDef maneuver in tools[j].Maneuvers)
					{
						yield return new VerbPropertiesWithSource(maneuver.verb, tools[j], maneuver);
					}
				}
			}
		}

		public static bool AllowAdjacentShot(LocalTargetInfo target, Thing caster)
		{
			if (!(caster is Pawn))
			{
				return true;
			}
			Pawn pawn = target.Thing as Pawn;
			if (pawn != null && pawn.HostileTo(caster))
			{
				return pawn.Downed;
			}
			return true;
		}

		public static VerbSelectionCategory GetSelectionCategory(this Verb v, Pawn p, float highestWeight)
		{
			float num = InitialVerbWeight(v, p);
			if (num >= highestWeight * 0.95f)
			{
				return VerbSelectionCategory.Best;
			}
			if (num < highestWeight * 0.25f)
			{
				return VerbSelectionCategory.Worst;
			}
			return VerbSelectionCategory.Mid;
		}

		public static float InitialVerbWeight(Verb v, Pawn p)
		{
			return DPS(v, p) * AdditionalSelectionFactor(v);
		}

		public static float DPS(Verb v, Pawn p)
		{
			return v.verbProps.AdjustedMeleeDamageAmount(v, p) * (1f + v.verbProps.AdjustedArmorPenetration(v, p)) * v.verbProps.accuracyTouch / v.verbProps.AdjustedFullCycleTime(v, p);
		}

		private static float AdditionalSelectionFactor(Verb v)
		{
			float num = (v.tool != null) ? v.tool.chanceFactor : 1f;
			if (v.verbProps.meleeDamageDef != null && !v.verbProps.meleeDamageDef.additionalHediffs.NullOrEmpty())
			{
				foreach (DamageDefAdditionalHediff additionalHediff in v.verbProps.meleeDamageDef.additionalHediffs)
				{
					_ = additionalHediff;
					num += 0.1f;
				}
				return num;
			}
			return num;
		}

		public static float FinalSelectionWeight(Verb verb, Pawn p, List<Verb> allMeleeVerbs, float highestWeight)
		{
			VerbSelectionCategory selectionCategory = verb.GetSelectionCategory(p, highestWeight);
			if (selectionCategory == VerbSelectionCategory.Worst)
			{
				return 0f;
			}
			int num = 0;
			foreach (Verb allMeleeVerb in allMeleeVerbs)
			{
				if (allMeleeVerb.GetSelectionCategory(p, highestWeight) == selectionCategory)
				{
					num++;
				}
			}
			return 1f / (float)num * ((selectionCategory == VerbSelectionCategory.Mid) ? 0.25f : 0.75f);
		}
	}
}
