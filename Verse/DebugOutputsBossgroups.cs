using System.Linq;
using LudeonTK;
using RimWorld;

namespace Verse;

public static class DebugOutputsBossgroups
{
	[DebugOutput("Bossgroups", true)]
	public static void Bossgroups()
	{
		DebugTables.MakeTablesDialog(DefDatabase<BossgroupDef>.AllDefs.OrderBy((BossgroupDef d) => d.defName), new TableDataGetter<BossgroupDef>("defName", (BossgroupDef d) => d.defName), new TableDataGetter<BossgroupDef>("boss", (BossgroupDef d) => d.boss.kindDef.label), new TableDataGetter<BossgroupDef>("available after", (BossgroupDef d) => d.boss.appearAfterTicks.ToStringTicksToPeriod()), new TableDataGetter<BossgroupDef>("available in", (BossgroupDef d) => (d.boss.appearAfterTicks <= Find.TickManager.TicksGame) ? "now" : (d.boss.appearAfterTicks - Find.TickManager.TicksGame).ToStringTicksToPeriod()), new TableDataGetter<BossgroupDef>("defeated in combat", (BossgroupDef d) => Find.BossgroupManager.IsDefeated(d.boss)), new TableDataGetter<BossgroupDef>("reserved by bossgroup", (BossgroupDef d) => Find.BossgroupManager.ReservedByBossgroup(d.boss.kindDef)), new TableDataGetter<BossgroupDef>("reward", (BossgroupDef d) => d.rewardDef?.label));
	}
}
