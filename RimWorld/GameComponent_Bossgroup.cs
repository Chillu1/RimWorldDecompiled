using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GameComponent_Bossgroup : GameComponent
	{
		public int lastBossgroupCalled = -9999999;

		private Dictionary<BossgroupDef, int> timesCalledBossgroups = new Dictionary<BossgroupDef, int>();

		private List<BossDef> allBosses;

		private List<BossDef> killedBosses = new List<BossDef>();

		private List<BossgroupDef> bossgroupTmp;

		private List<int> calledTmp;

		public List<BossDef> AllBosses
		{
			get
			{
				if (allBosses.NullOrEmpty())
				{
					allBosses = new List<BossDef>();
					allBosses.AddRange(DefDatabase<BossDef>.AllDefs);
				}
				return allBosses;
			}
		}

		public GameComponent_Bossgroup(Game game)
		{
		}

		public bool ReservedByBossgroup(PawnKindDef bossgroupKidDef)
		{
			BossDef bossForKind = GetBossForKind(bossgroupKidDef);
			if (bossForKind != null)
			{
				if (bossForKind.appearAfterTicks < Find.TickManager.TicksGame)
				{
					return false;
				}
				if (!IsDefeated(bossForKind))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsDefeated(BossDef bossDef)
		{
			return killedBosses.Contains(bossDef);
		}

		private BossDef GetBossForKind(PawnKindDef kindDef)
		{
			return AllBosses.FirstOrDefault((BossDef b) => b.kindDef == kindDef);
		}

		public int NumTimesCalledBossgroup(BossgroupDef bossgroupDef)
		{
			if (!timesCalledBossgroups.ContainsKey(bossgroupDef))
			{
				timesCalledBossgroups.Add(bossgroupDef, 0);
			}
			return timesCalledBossgroups[bossgroupDef];
		}

		public void ResetProgress()
		{
			timesCalledBossgroups.Clear();
		}

		public void Notify_BossgroupCalled(BossgroupDef bossgroupDef)
		{
			lastBossgroupCalled = Find.TickManager.TicksGame;
			if (timesCalledBossgroups.ContainsKey(bossgroupDef))
			{
				timesCalledBossgroups[bossgroupDef]++;
			}
			else
			{
				timesCalledBossgroups.Add(bossgroupDef, 0);
			}
		}

		public virtual void Notify_PawnKilled(Pawn pawn)
		{
			BossDef bossForKind = GetBossForKind(pawn.kindDef);
			if (bossForKind != null && !killedBosses.Contains(bossForKind))
			{
				killedBosses.Add(bossForKind);
			}
		}

		public void DebugResetDefeatedPawns()
		{
			killedBosses.Clear();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref lastBossgroupCalled, "lastBossgroupCalled", -9999999);
			Scribe_Collections.Look(ref killedBosses, "killedBosses", LookMode.Def);
			Scribe_Collections.Look(ref timesCalledBossgroups, "timesCalledBossgroups", LookMode.Def, LookMode.Value, ref bossgroupTmp, ref calledTmp);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				HashSet<PawnKindDef> valueHashSet = null;
				Scribe_Collections.Look(ref valueHashSet, "killedBossgroupMechs");
				if (valueHashSet != null)
				{
					if (killedBosses == null)
					{
						killedBosses = new List<BossDef>();
					}
					foreach (PawnKindDef item in valueHashSet)
					{
						BossDef bossForKind = GetBossForKind(item);
						if (bossForKind != null && !killedBosses.Contains(bossForKind))
						{
							killedBosses.Add(bossForKind);
						}
					}
				}
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (timesCalledBossgroups == null)
				{
					timesCalledBossgroups = new Dictionary<BossgroupDef, int>();
				}
				if (killedBosses == null)
				{
					killedBosses = new List<BossDef>();
				}
			}
		}
	}
}
