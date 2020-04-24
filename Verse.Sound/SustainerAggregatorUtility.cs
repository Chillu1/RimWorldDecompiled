namespace Verse.Sound
{
	public static class SustainerAggregatorUtility
	{
		private static float AggregateRadius = 12f;

		public static Sustainer AggregateOrSpawnSustainerFor(ISizeReporter reporter, SoundDef def, SoundInfo info)
		{
			Sustainer sustainer = null;
			foreach (Sustainer allSustainer in Find.SoundRoot.sustainerManager.AllSustainers)
			{
				if (allSustainer.def == def && allSustainer.info.Maker.Map == info.Maker.Map && allSustainer.info.Maker.Cell.InHorDistOf(info.Maker.Cell, AggregateRadius))
				{
					sustainer = allSustainer;
					break;
				}
			}
			if (sustainer == null)
			{
				sustainer = def.TrySpawnSustainer(info);
			}
			else
			{
				sustainer.Maintain();
			}
			if (sustainer.externalParams.sizeAggregator == null)
			{
				sustainer.externalParams.sizeAggregator = new SoundSizeAggregator();
			}
			sustainer.externalParams.sizeAggregator.RegisterReporter(reporter);
			return sustainer;
		}
	}
}
