using System;
using RimWorld;

namespace Verse
{
	public class HediffComp
	{
		public HediffWithComps parent;

		public HediffCompProperties props;

		public Pawn Pawn => parent.pawn;

		public HediffDef Def => parent.def;

		public virtual string CompLabelInBracketsExtra => null;

		public virtual string CompTipStringExtra => null;

		public virtual TextureAndColor CompStateIcon => TextureAndColor.None;

		public virtual bool CompShouldRemove => false;

		public virtual void CompPostMake()
		{
		}

		public virtual void CompPostTick(ref float severityAdjustment)
		{
		}

		public virtual void CompExposeData()
		{
		}

		public virtual void CompPostPostAdd(DamageInfo? dinfo)
		{
		}

		public virtual void CompPostPostRemoved()
		{
		}

		public virtual void CompPostMerged(Hediff other)
		{
		}

		public virtual bool CompDisallowVisible()
		{
			return false;
		}

		public virtual void CompModifyChemicalEffect(ChemicalDef chem, ref float effect)
		{
		}

		public virtual void CompPostInjuryHeal(float amount)
		{
		}

		[Obsolete("Only need this overload to not break mod compatibility.")]
		public virtual void CompTended(float quality, int batchPosition = 0)
		{
		}

		public virtual void CompTended_NewTemp(float quality, float maxQuality, int batchPosition = 0)
		{
		}

		public virtual void Notify_ImplantUsed(string violationSourceName, float detectionChance, int violationSourceLevel = -1)
		{
		}

		public virtual void Notify_EntropyGained(float baseAmount, float finalAmount, Thing source = null)
		{
		}

		public virtual void Notify_PawnUsedVerb(Verb verb, LocalTargetInfo target)
		{
		}

		public virtual void Notify_PawnDied()
		{
		}

		public virtual void Notify_PawnKilled()
		{
		}

		public virtual void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
		}

		public virtual string CompDebugString()
		{
			return null;
		}
	}
}
