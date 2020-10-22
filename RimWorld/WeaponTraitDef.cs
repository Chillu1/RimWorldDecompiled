using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class WeaponTraitDef : Def
	{
		public List<StatModifier> equippedStatOffsets;

		public List<HediffDef> equippedHediffs;

		public List<HediffDef> bondedHediffs;

		public ThoughtDef bondedThought;

		public ThoughtDef killThought;

		public Type workerClass = typeof(WeaponTraitWorker);

		public List<string> exclusionTags;

		public float commonality;

		public float marketValueOffset;

		public bool neverBond;

		private WeaponTraitWorker worker;

		public WeaponTraitWorker Worker
		{
			get
			{
				if (worker == null)
				{
					worker = (WeaponTraitWorker)Activator.CreateInstance(workerClass);
					worker.def = this;
				}
				return worker;
			}
		}

		public bool Overlaps(WeaponTraitDef other)
		{
			if (other == this)
			{
				return true;
			}
			if (exclusionTags.NullOrEmpty() || other.exclusionTags.NullOrEmpty())
			{
				return false;
			}
			return exclusionTags.Any((string x) => other.exclusionTags.Contains(x));
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (!typeof(WeaponTraitWorker).IsAssignableFrom(workerClass))
			{
				yield return $"WeaponTraitDef {defName} has worker class {workerClass}, which is not deriving from {typeof(WeaponTraitWorker).FullName}";
			}
			if (commonality <= 0f)
			{
				yield return $"WeaponTraitDef {defName} has a commonality <= 0.";
			}
		}
	}
}
