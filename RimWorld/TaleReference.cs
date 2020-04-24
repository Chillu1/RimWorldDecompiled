using Verse;

namespace RimWorld
{
	public class TaleReference : IExposable
	{
		private Tale tale;

		private int seed;

		public static TaleReference Taleless => new TaleReference(null);

		public TaleReference()
		{
		}

		public TaleReference(Tale tale)
		{
			this.tale = tale;
			seed = Rand.Range(0, int.MaxValue);
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref seed, "seed", 0);
			Scribe_References.Look(ref tale, "tale");
		}

		public void ReferenceDestroyed()
		{
			if (tale != null)
			{
				tale.Notify_ReferenceDestroyed();
				tale = null;
			}
		}

		public TaggedString GenerateText(TextGenerationPurpose purpose, RulePackDef extraInclude)
		{
			return TaleTextGenerator.GenerateTextFromTale(purpose, tale, seed, extraInclude);
		}

		public override string ToString()
		{
			return "TaleReference(tale=" + ((tale == null) ? "null" : tale.ToString()) + ", seed=" + seed + ")";
		}
	}
}
