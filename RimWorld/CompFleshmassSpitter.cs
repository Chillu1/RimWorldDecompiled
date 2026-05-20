using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class CompFleshmassSpitter : CompFleshmassHeartChild, IAttackTargetSearcher, IVerbOwner
{
	private static readonly IntRange SpitIntervalRangeTicks = new IntRange(5000, 7500);

	private const int SpitCheckIntervalTicks = 180;

	private const int ReadySpriteIndex = 0;

	private const int CooldownSpriteIndex = 1;

	private int lastSpitTick = -99999;

	private int nextSpitDelay = -99999;

	private LocalTargetInfo lastAttackedTarget;

	private VerbTracker verbTracker;

	private Effecter progressBarEffecter;

	private Verb AttackVerb => AllVerbs[0];

	private int TicksToNextSpit => lastSpitTick + nextSpitDelay - Find.TickManager.TicksGame;

	private bool OnCooldown => TicksToNextSpit > 0;

	public Thing Thing => parent;

	public Verb CurrentEffectiveVerb => AttackVerb;

	public LocalTargetInfo LastAttackedTarget => lastAttackedTarget;

	public int LastAttackTargetTick => lastSpitTick;

	public VerbTracker VerbTracker => verbTracker ?? (verbTracker = new VerbTracker(this));

	public List<VerbProperties> VerbProperties => parent.def.Verbs;

	public List<Tool> Tools => parent.def.tools;

	public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

	public Thing ConstantCaster => parent;

	public List<Verb> AllVerbs => VerbTracker.AllVerbs;

	public string UniqueVerbOwnerID()
	{
		return "CompFleshmassSpitter_" + parent.ThingID;
	}

	public bool VerbsStillUsableBy(Pawn p)
	{
		return false;
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		SetupVerbs();
		if (!respawningAfterLoad)
		{
			lastSpitTick = Find.TickManager.TicksGame;
			nextSpitDelay = SpitIntervalRangeTicks.RandomInRange;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref nextSpitDelay, "nextSpitDelay", 0);
		Scribe_Values.Look(ref lastSpitTick, "lastSpitTick", 0);
		Scribe_TargetInfo.Look(ref lastAttackedTarget, "lastAttackedTarget");
		Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultDesc = "Dev: Cooldown Spit";
			command_Action.defaultLabel = "Dev: Cooldown Spit";
			command_Action.action = delegate
			{
				lastSpitTick = -999999;
				nextSpitDelay = -999999;
			};
			yield return command_Action;
		}
	}

	private void SetupVerbs()
	{
		foreach (Verb allVerb in AllVerbs)
		{
			allVerb.caster = parent;
		}
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		progressBarEffecter?.ForceEnd();
		progressBarEffecter = null;
	}

	public override void CompTick()
	{
		base.CompTick();
		if (!parent.Spawned)
		{
			return;
		}
		if (OnCooldown)
		{
			if (progressBarEffecter == null)
			{
				progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
			}
			progressBarEffecter.EffectTick(parent, TargetInfo.Invalid);
			MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
			mote.progress = 1f - (float)Mathf.Max(TicksToNextSpit, 0) / (float)nextSpitDelay;
			mote.offsetZ = -0.8f;
			parent.overrideGraphicIndex = 1;
		}
		else
		{
			parent.overrideGraphicIndex = 0;
		}
		if (TicksToNextSpit <= 0 && parent.IsHashIntervalTick(180))
		{
			Thing thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(this, TargetScanFlags.NeedThreat, (Thing t) => !t.Position.Roofed(t.Map));
			if (thing != null)
			{
				AttackVerb.TryStartCastOn(thing);
				lastSpitTick = Find.TickManager.TicksGame;
				nextSpitDelay = SpitIntervalRangeTicks.RandomInRange;
				Messages.Message("SpitterAttacking".Translate(), parent, MessageTypeDefOf.NegativeEvent);
			}
		}
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		AttackVerb.DrawHighlight(LocalTargetInfo.Invalid);
	}

	public override string CompInspectStringExtra()
	{
		if (TicksToNextSpit > 0)
		{
			return "SpitterGatheringSpit".Translate() + ": " + TicksToNextSpit.ToStringTicksToPeriod();
		}
		return "SpitterReadyToSpit".Translate();
	}
}
