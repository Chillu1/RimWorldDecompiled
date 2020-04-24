namespace Verse.Sound
{
	public class ResolvedGrain_Silence : ResolvedGrain
	{
		public AudioGrain_Silence sourceGrain;

		public ResolvedGrain_Silence(AudioGrain_Silence sourceGrain)
		{
			this.sourceGrain = sourceGrain;
			duration = sourceGrain.durationRange.RandomInRange;
		}

		public override string ToString()
		{
			return "Silence";
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			ResolvedGrain_Silence resolvedGrain_Silence = obj as ResolvedGrain_Silence;
			if (resolvedGrain_Silence == null)
			{
				return false;
			}
			return resolvedGrain_Silence.sourceGrain == sourceGrain;
		}

		public override int GetHashCode()
		{
			return sourceGrain.GetHashCode();
		}
	}
}
