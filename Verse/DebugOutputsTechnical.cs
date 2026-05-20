using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public static class DebugOutputsTechnical
{
	[DebugOutput]
	public static void KeyStrings()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
		{
			stringBuilder.AppendLine(value.ToString() + " - " + value.ToStringReadable());
		}
		Log.Message(stringBuilder.ToString());
	}

	[DebugOutput]
	public static void DefNames()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Type type in from def in GenDefDatabase.AllDefTypesWithDatabases()
			orderby def.Name
			select def)
		{
			DebugMenuOption item = new DebugMenuOption(type.Name, DebugMenuOptionMode.Action, delegate
			{
				IEnumerable source = (IEnumerable)GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), type, "AllDefs");
				int num = 0;
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Def item2 in source.Cast<Def>())
				{
					stringBuilder.AppendLine(item2.defName);
					num++;
					if (num >= 500)
					{
						Log.Message(stringBuilder.ToString());
						stringBuilder = new StringBuilder();
						num = 0;
					}
				}
				Log.Message(stringBuilder.ToString());
			});
			list.Add(item);
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugOutput]
	public static void DefNamesAll()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (Type item in from def in GenDefDatabase.AllDefTypesWithDatabases()
			orderby def.Name
			select def)
		{
			IEnumerable source = (IEnumerable)GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), item, "AllDefs");
			stringBuilder.AppendLine("--    " + item.ToString());
			foreach (Def item2 in from Def def in source
				orderby def.defName
				select def)
			{
				stringBuilder.AppendLine(item2.defName);
				num++;
				if (num >= 500)
				{
					Log.Message(stringBuilder.ToString());
					stringBuilder = new StringBuilder();
					num = 0;
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
		}
		Log.Message(stringBuilder.ToString());
	}

	[DebugOutput]
	public static void DefLabels()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Type type in from def in GenDefDatabase.AllDefTypesWithDatabases()
			orderby def.Name
			select def)
		{
			DebugMenuOption item = new DebugMenuOption(type.Name, DebugMenuOptionMode.Action, delegate
			{
				IEnumerable source = (IEnumerable)GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), type, "AllDefs");
				int num = 0;
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Def item2 in source.Cast<Def>())
				{
					stringBuilder.AppendLine(item2.label);
					num++;
					if (num >= 500)
					{
						Log.Message(stringBuilder.ToString());
						stringBuilder = new StringBuilder();
						num = 0;
					}
				}
				Log.Message(stringBuilder.ToString());
			});
			list.Add(item);
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugOutput]
	public static void BestThingRequestGroup()
	{
		DebugTables.MakeTablesDialog(from x in DefDatabase<ThingDef>.AllDefs
			where ListerThings.EverListable(x, ListerThingsUse.Global) || ListerThings.EverListable(x, ListerThingsUse.Region)
			orderby x.label
			select x, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("best local", delegate(ThingDef d)
		{
			IEnumerable<ThingRequestGroup> source = (ListerThings.EverListable(d, ListerThingsUse.Region) ? ((ThingRequestGroup[])Enum.GetValues(typeof(ThingRequestGroup))).Where((ThingRequestGroup x) => x.StoreInRegion() && x.Includes(d)) : Enumerable.Empty<ThingRequestGroup>());
			if (!source.Any())
			{
				return "-";
			}
			ThingRequestGroup best = source.MinBy((ThingRequestGroup x) => DefDatabase<ThingDef>.AllDefs.Count((ThingDef y) => ListerThings.EverListable(y, ListerThingsUse.Region) && x.Includes(y)));
			return best.ToString() + " (defs: " + DefDatabase<ThingDef>.AllDefs.Count((ThingDef x) => ListerThings.EverListable(x, ListerThingsUse.Region) && best.Includes(x)) + ")";
		}), new TableDataGetter<ThingDef>("best global", delegate(ThingDef d)
		{
			IEnumerable<ThingRequestGroup> source = (ListerThings.EverListable(d, ListerThingsUse.Global) ? ((ThingRequestGroup[])Enum.GetValues(typeof(ThingRequestGroup))).Where((ThingRequestGroup x) => x.Includes(d)) : Enumerable.Empty<ThingRequestGroup>());
			if (!source.Any())
			{
				return "-";
			}
			ThingRequestGroup best = source.MinBy((ThingRequestGroup x) => DefDatabase<ThingDef>.AllDefs.Count((ThingDef y) => ListerThings.EverListable(y, ListerThingsUse.Global) && x.Includes(y)));
			return best.ToString() + " (defs: " + DefDatabase<ThingDef>.AllDefs.Count((ThingDef x) => ListerThings.EverListable(x, ListerThingsUse.Global) && best.Includes(x)) + ")";
		}));
	}

	[DebugOutput]
	public static void DamageTest()
	{
		ThingDef thingDef = ThingDef.Named("Bullet_BoltActionRifle");
		PawnKindDef slave = PawnKindDefOf.Slave;
		Faction faction = FactionUtility.DefaultFactionFrom(slave.defaultFactionDef);
		DamageInfo dinfo = new DamageInfo(thingDef.projectile.damageDef, thingDef.projectile.GetDamageAmount(null), thingDef.projectile.GetArmorPenetration());
		dinfo.SetIgnoreInstantKillProtection(ignore: true);
		int num = 0;
		int num2 = 0;
		DefMap<BodyPartDef, int> defMap = new DefMap<BodyPartDef, int>();
		for (int i = 0; i < 500; i++)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(slave, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true));
			List<BodyPartDef> list = (from hd in pawn.health.hediffSet.GetMissingPartsCommonAncestors()
				select hd.Part.def).ToList();
			for (int num3 = 0; num3 < 2; num3++)
			{
				pawn.TakeDamage(dinfo);
				if (pawn.Dead)
				{
					num++;
					break;
				}
			}
			List<BodyPartDef> list2 = (from hd in pawn.health.hediffSet.GetMissingPartsCommonAncestors()
				select hd.Part.def).ToList();
			if (list2.Count > list.Count)
			{
				num2++;
				foreach (BodyPartDef item in list2)
				{
					defMap[item]++;
				}
				foreach (BodyPartDef item2 in list)
				{
					defMap[item2]--;
				}
			}
			Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Damage test");
		stringBuilder.AppendLine("Hit " + 500 + " " + slave.label + "s with " + 2 + "x " + thingDef.label + " (" + thingDef.projectile.GetDamageAmount(null) + " damage) each. Results:");
		stringBuilder.AppendLine("Killed: " + num + " / " + 500 + " (" + ((float)num / 500f).ToStringPercent() + ")");
		stringBuilder.AppendLine("Part losers: " + num2 + " / " + 500 + " (" + ((float)num2 / 500f).ToStringPercent() + ")");
		stringBuilder.AppendLine("Parts lost:");
		foreach (BodyPartDef allDef in DefDatabase<BodyPartDef>.AllDefs)
		{
			if (defMap[allDef] > 0)
			{
				stringBuilder.AppendLine("   " + allDef.label + ": " + defMap[allDef]);
			}
		}
		Log.Message(stringBuilder.ToString());
	}

	[DebugOutput]
	public static void PlayerHasGravEngineTest()
	{
		Log.Message("PlayerHasGravEngine: " + GravshipUtility.PlayerHasGravEngine());
	}
}
