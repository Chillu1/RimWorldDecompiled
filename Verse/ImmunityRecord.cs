using RimWorld;
using UnityEngine;

namespace Verse
{
	public class ImmunityRecord : IExposable
	{
		public HediffDef hediffDef;

		public HediffDef source;

		public float immunity;

		public void ExposeData()
		{
			Scribe_Defs.Look(ref hediffDef, "hediffDef");
			Scribe_Defs.Look(ref source, "source");
			Scribe_Values.Look(ref immunity, "immunity", 0f);
		}

		public void ImmunityTick(Pawn pawn, bool sick, Hediff diseaseInstance)
		{
			immunity += ImmunityChangePerTick(pawn, sick, diseaseInstance);
			immunity = Mathf.Clamp01(immunity);
		}

		public float ImmunityChangePerTick(Pawn pawn, bool sick, Hediff diseaseInstance)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				return 0f;
			}
			HediffCompProperties_Immunizable hediffCompProperties_Immunizable = hediffDef.CompProps<HediffCompProperties_Immunizable>();
			if (sick)
			{
				float immunityPerDaySick = hediffCompProperties_Immunizable.immunityPerDaySick;
				immunityPerDaySick *= pawn.GetStatValue(StatDefOf.ImmunityGainSpeed);
				if (diseaseInstance != null)
				{
					Rand.PushState();
					Rand.Seed = Gen.HashCombineInt(diseaseInstance.loadID ^ Find.World.info.persistentRandomValue, 156482735);
					immunityPerDaySick *= Mathf.Lerp(0.8f, 1.2f, Rand.Value);
					Rand.PopState();
				}
				return immunityPerDaySick / 60000f;
			}
			return hediffCompProperties_Immunizable.immunityPerDayNotSick / 60000f;
		}
	}
}
