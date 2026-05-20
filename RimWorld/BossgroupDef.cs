using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class BossgroupDef : Def
{
	public BossDef boss;

	public List<BossGroupWave> waves = new List<BossGroupWave>();

	public int repeatWaveStartIndex;

	public Type workerClass = typeof(BossgroupWorker);

	[MustTranslate]
	public string leaderDescription;

	public QuestScriptDef quest;

	public ThingDef rewardDef;

	private BossgroupWorker workerInt;

	private List<string> tmpEntries = new List<string>();

	public BossgroupWorker Worker
	{
		get
		{
			if (workerInt == null && workerClass != null)
			{
				workerInt = (BossgroupWorker)Activator.CreateInstance(workerClass);
				workerInt.def = this;
			}
			return workerInt;
		}
	}

	public string LeaderDescription => leaderDescription.Formatted(NamedArgumentUtility.Named(boss.kindDef, "LEADERKIND"));

	public int GetWaveIndex(int timesSummoned)
	{
		int num = timesSummoned;
		if (num >= waves.Count)
		{
			Rand.PushState(Gen.HashCombine(Find.World.info.Seed, timesSummoned));
			num = Rand.Range(repeatWaveStartIndex, waves.Count);
			Rand.PopState();
		}
		return num;
	}

	public BossGroupWave GetWave(int index)
	{
		return waves[GetWaveIndex(index)];
	}

	public string GetWaveDescription(int waveIndex)
	{
		BossGroupWave wave = GetWave(GetWaveIndex(waveIndex));
		tmpEntries.Clear();
		string text = GenLabel.BestKindLabel(boss.kindDef, Gender.None).CapitalizeFirst();
		if (!wave.bossApparel.NullOrEmpty())
		{
			text = "BossWithApparel".Translate(text.Named("BOSS"), wave.bossApparel.Select((ThingDef a) => a.label).ToCommaList(useAnd: true).Named("APPAREL"));
		}
		tmpEntries.Add(text + " x" + wave.bossCount);
		foreach (PawnKindDefCount escort in wave.escorts)
		{
			tmpEntries.Add(GenLabel.BestKindLabel(escort.kindDef, Gender.None).CapitalizeFirst() + " x" + escort.count);
		}
		return tmpEntries.ToLineList("  - ");
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (boss == null)
		{
			yield return "boss required for all bossgroups";
		}
		if (waves.NullOrEmpty())
		{
			yield return "no waves defined.";
		}
		if (repeatWaveStartIndex >= waves.Count)
		{
			yield return "repeatWaveStartIndex must be lower than wave count.";
		}
	}
}
