using System.Linq;
using LudeonTK;
using RimWorld;

namespace Verse;

public static class DebugOutputsJoy
{
	[DebugOutput]
	public static void JoyGivers()
	{
		DebugTables.MakeTablesDialog(DefDatabase<JoyGiverDef>.AllDefs, new TableDataGetter<JoyGiverDef>("defName", (JoyGiverDef d) => d.defName), new TableDataGetter<JoyGiverDef>("joyKind", (JoyGiverDef d) => (d.joyKind != null) ? d.joyKind.defName : "null"), new TableDataGetter<JoyGiverDef>("baseChance", (JoyGiverDef d) => d.baseChance.ToString()), new TableDataGetter<JoyGiverDef>("canDoWhileInBed", (JoyGiverDef d) => d.canDoWhileInBed.ToStringCheckBlank()), new TableDataGetter<JoyGiverDef>("desireSit", (JoyGiverDef d) => d.desireSit.ToStringCheckBlank()), new TableDataGetter<JoyGiverDef>("unroofedOnly", (JoyGiverDef d) => d.unroofedOnly.ToStringCheckBlank()), new TableDataGetter<JoyGiverDef>("jobDef", (JoyGiverDef d) => (d.jobDef != null) ? d.jobDef.defName : "null"), new TableDataGetter<JoyGiverDef>("pctPawnsEverDo", (JoyGiverDef d) => d.pctPawnsEverDo.ToStringPercent()), new TableDataGetter<JoyGiverDef>("requiredCapacities", (JoyGiverDef d) => (d.requiredCapacities != null) ? d.requiredCapacities.Select((PawnCapacityDef c) => c.defName).ToCommaList() : ""), new TableDataGetter<JoyGiverDef>("thingDefs", (JoyGiverDef d) => (d.thingDefs != null) ? d.thingDefs.Select((ThingDef c) => c.defName).ToCommaList() : ""), new TableDataGetter<JoyGiverDef>("JoyGainFactors", (JoyGiverDef d) => (d.thingDefs != null) ? d.thingDefs.Select((ThingDef c) => c.GetStatValueAbstract(StatDefOf.JoyGainFactor).ToString("F2")).ToCommaList() : ""));
	}

	[DebugOutput]
	public static void JoyKinds()
	{
		DebugTables.MakeTablesDialog(DefDatabase<JoyKindDef>.AllDefs, new TableDataGetter<JoyKindDef>("defName", (JoyKindDef d) => d.defName), new TableDataGetter<JoyKindDef>("titleRequiredAny", (JoyKindDef d) => (d.titleRequiredAny != null) ? string.Join(",", d.titleRequiredAny.Select((RoyalTitleDef t) => t.defName).ToArray()) : "NULL"));
	}

	[DebugOutput]
	public static void JoyJobs()
	{
		DebugTables.MakeTablesDialog(DefDatabase<JobDef>.AllDefs.Where((JobDef j) => j.joyKind != null), new TableDataGetter<JobDef>("defName", (JobDef d) => d.defName), new TableDataGetter<JobDef>("joyKind", (JobDef d) => d.joyKind.defName), new TableDataGetter<JobDef>("joyDuration", (JobDef d) => d.joyDuration.ToString()), new TableDataGetter<JobDef>("joyGainRate", (JobDef d) => d.joyGainRate.ToString()), new TableDataGetter<JobDef>("joyMaxParticipants", (JobDef d) => d.joyMaxParticipants.ToString()), new TableDataGetter<JobDef>("joySkill", (JobDef d) => (d.joySkill == null) ? "" : d.joySkill.defName), new TableDataGetter<JobDef>("joyXpPerTick", (JobDef d) => d.joyXpPerTick.ToString()));
	}
}
