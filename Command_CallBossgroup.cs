using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

public class Command_CallBossgroup : Command
{
	private static readonly CachedTexture BossgroupTex = new CachedTexture("UI/Icons/SummonMechThreat");

	private Pawn_MechanitorTracker mechanitor;

	private IEnumerable<FloatMenuOption> FloatMenuOptions
	{
		get
		{
			IEnumerable<BossgroupDef> allDefs = DefDatabase<BossgroupDef>.AllDefs;
			foreach (BossgroupDef bg in allDefs)
			{
				AcceptanceReport acceptanceReport = CallBossgroupUtility.BossgroupEverCallable(mechanitor.Pawn, bg);
				if (!acceptanceReport)
				{
					yield return new FloatMenuOption("CannotSummon".Translate(bg.boss.kindDef.label) + ": " + acceptanceReport.Reason, null);
					continue;
				}
				yield return new FloatMenuOption("Summon".Translate(bg.boss.kindDef.label), delegate
				{
					CallBossgroupUtility.TryStartSummonBossgroupJob(bg, mechanitor.Pawn);
				});
			}
		}
	}

	public override string DescPostfix
	{
		get
		{
			string text = "";
			Dictionary<BossgroupDef, AcceptanceReport> source = DefDatabase<BossgroupDef>.AllDefs.ToDictionary((BossgroupDef b) => b, (BossgroupDef b) => CallBossgroupUtility.BossgroupEverCallable(mechanitor.Pawn, b));
			foreach (KeyValuePair<BossgroupDef, AcceptanceReport> item in source.Where((KeyValuePair<BossgroupDef, AcceptanceReport> b) => b.Value))
			{
				text = text + "\n\n" + "ReadyToSummonThreat".Translate(item.Key.boss.kindDef.label).Colorize(ColorLibrary.Green).CapitalizeFirst();
			}
			foreach (KeyValuePair<BossgroupDef, AcceptanceReport> item2 in source.Where((KeyValuePair<BossgroupDef, AcceptanceReport> b) => !b.Value))
			{
				text = text + "\n\n" + ("CannotSummon".Translate(item2.Key.boss.kindDef.label) + ": " + item2.Value.Reason).Colorize(ColorLibrary.RedReadable);
			}
			return text;
		}
	}

	public Command_CallBossgroup(Pawn_MechanitorTracker mechanitor)
	{
		this.mechanitor = mechanitor;
		defaultLabel = "CommandCallBossgroup".Translate();
		defaultDesc = "CommandCallBossgroupDesc".Translate();
		icon = BossgroupTex.Texture;
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		disabled = IsDisabled(out disabledReason);
		return base.GizmoOnGUI(topLeft, maxWidth, parms);
	}

	private bool IsDisabled(out string reason)
	{
		int lastBossgroupCalled = Find.BossgroupManager.lastBossgroupCalled;
		int num = Find.TickManager.TicksGame - lastBossgroupCalled;
		if (Faction.OfMechanoids == null || Faction.OfMechanoids.deactivated)
		{
			reason = "MechsDisabled".Translate();
			return true;
		}
		if (!mechanitor.Pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			reason = "Incapable".Translate().CapitalizeFirst();
			return true;
		}
		if (num < 120000)
		{
			reason = "BossgroupAvailableIn".Translate((120000 - num).ToStringTicksToPeriod()).CapitalizeFirst();
			return true;
		}
		PawnKindDef pendingBossgroup = CallBossgroupUtility.GetPendingBossgroup();
		if (pendingBossgroup != null)
		{
			reason = "BossgroupIncoming".Translate(pendingBossgroup.label).CapitalizeFirst();
			return true;
		}
		if (FloatMenuOptions.All((FloatMenuOption f) => f.action == null))
		{
			reason = null;
			return true;
		}
		reason = null;
		return false;
	}

	public override void ProcessInput(Event ev)
	{
		base.ProcessInput(ev);
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		list.AddRange(FloatMenuOptions);
		Find.WindowStack.Add(new FloatMenu(list));
	}
}
