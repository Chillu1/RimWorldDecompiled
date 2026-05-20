using System.Collections.Generic;
using System.Text;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public class Building_VoidMonolith : Building, ITargetingSource, IThingGlower
{
	private static readonly FloatRange DisturbingVisionRangeDaysRange = new FloatRange(13f, 16f);

	private const int DisturbingVisionRetryTicks = 15000;

	public const int ActivateLetterDelayTicks = 360;

	private const int AutoActivateAlertTicks = 300000;

	private const int MonolithFragmentCount = 3;

	private const int MonolithShardCount = 10;

	private const int CollapseScatterRadius = 5;

	private const int HintEffecterInterval = 30;

	public Quest quest;

	private int disturbingVisionTick = -99999;

	private int autoActivateTick = -99999;

	private List<Thing> monolithAttachments = new List<Thing>();

	private Thing pyramidThing;

	private int activatedDialogTick = -99999;

	private Pawn activatorPawn;

	private Effecter gleamingEffecter;

	private Effecter gleamingVoidNodeEffecter;

	private Effecter autoActivateEffecter;

	private Effecter level0HintEffecter;

	private static readonly TargetingParameters targetParmsInt = new TargetingParameters
	{
		canTargetBuildings = false,
		canTargetAnimals = false,
		canTargetMechs = false,
		onlyTargetColonists = true
	};

	public override int? OverrideGraphicIndex => Find.Anomaly.LevelDef.graphicIndex;

	public bool IsAutoActivating
	{
		get
		{
			if (autoActivateTick > 0)
			{
				return Find.TickManager.TicksGame > autoActivateTick - 300000;
			}
			return false;
		}
	}

	public int TicksUntilAutoActivate => autoActivateTick - Find.TickManager.TicksGame;

	private TargetInfo EffecterInfo => new TargetInfo(base.Position, base.Map);

	public override CellRect? CustomRectForSelector => GenAdj.OccupiedRect(base.Position, Rot4.North, Find.Anomaly.LevelDef.sizeIncludingAttachments ?? def.Size);

	public override Texture UIIconOverride => Find.Anomaly.LevelDef.UIIcon;

	public override string LabelNoCount
	{
		get
		{
			if (Find.Anomaly.LevelDef.monolithLabel != null)
			{
				return Find.Anomaly.LevelDef.monolithLabel;
			}
			return base.LabelNoCount;
		}
	}

	public override string DescriptionFlavor
	{
		get
		{
			if (Find.Anomaly.LevelDef.monolithDescription != null)
			{
				return Find.Anomaly.LevelDef.monolithDescription;
			}
			return base.DescriptionFlavor;
		}
	}

	public bool CasterIsPawn => true;

	public bool IsMeleeAttack => false;

	public bool Targetable => true;

	public bool MultiSelect => false;

	public bool HidePawnTooltips => false;

	public Thing Caster => this;

	public Pawn CasterPawn => null;

	public Verb GetVerb => null;

	public TargetingParameters targetParams => targetParmsInt;

	public virtual ITargetingSource DestinationSelector => null;

	public Texture2D UIIcon
	{
		get
		{
			if (Find.Anomaly.Level != 0)
			{
				return ContentFinder<Texture2D>.Get("UI/Commands/ActivateMonolith");
			}
			return ContentFinder<Texture2D>.Get("UI/Commands/ActivateMonolithInitial");
		}
	}

	public bool ShouldBeLitNow()
	{
		return Find.Anomaly.LevelDef.monolithGlows;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref quest, "quest");
		Scribe_Values.Look(ref disturbingVisionTick, "disturbingVisionTick", 0);
		Scribe_Values.Look(ref autoActivateTick, "autoActivateTick", 0);
		Scribe_Collections.Look(ref monolithAttachments, "monolithAttachments", LookMode.Reference);
		Scribe_Values.Look(ref activatedDialogTick, "activatedDialogTick", 0);
		Scribe_References.Look(ref activatorPawn, "activatorPawn");
		Scribe_References.Look(ref pyramidThing, "pyramidThing");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && monolithAttachments == null)
		{
			monolithAttachments = new List<Thing>();
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (!ModLister.CheckAnomaly("Void monolith"))
		{
			Destroy();
			return;
		}
		if (!respawningAfterLoad)
		{
			if (Find.Anomaly.Level > 0)
			{
				this.TryGetComp<CompProximityLetter>().letterSent = true;
			}
			if (Find.Anomaly.monolith == null)
			{
				Find.Anomaly.monolith = this;
			}
			disturbingVisionTick = Find.TickManager.TicksGame + Mathf.RoundToInt(DisturbingVisionRangeDaysRange.RandomInRange * 60000f);
		}
		base.SpawnSetup(map, respawningAfterLoad);
		UpdateAttachments();
	}

	protected override void Tick()
	{
		base.Tick();
		if (this.IsHashIntervalTick(30))
		{
			if (Find.Anomaly.Level == 0 && !base.Map.reservationManager.IsReserved(this))
			{
				if (level0HintEffecter == null)
				{
					level0HintEffecter = EffecterDefOf.MonolithL0Glow.Spawn(EffecterInfo, EffecterInfo);
				}
			}
			else
			{
				level0HintEffecter?.Cleanup();
				level0HintEffecter = null;
			}
		}
		level0HintEffecter?.EffectTick(EffecterInfo, EffecterInfo);
		if (Find.Anomaly.Level == 0 && disturbingVisionTick > 0 && Find.TickManager.TicksGame > disturbingVisionTick)
		{
			if (base.Map.mapPawns.FreeColonistsSpawned.TryRandomElement(out var result))
			{
				Find.LetterStack.ReceiveLetter("VoidMonolithVisionLabel".Translate(), "VoidMonolithVisionText".Translate(result.Named("PAWN")), LetterDefOf.NeutralEvent, this);
				disturbingVisionTick = -99999;
			}
			else
			{
				disturbingVisionTick += 15000;
			}
		}
		if (autoActivateTick > 0)
		{
			if (Find.TickManager.TicksGame == autoActivateTick - 300000)
			{
				Find.LetterStack.ReceiveLetter("MonolithAutoActivatingLabel".Translate(), "MonolithAutoActivatingText".Translate(), LetterDefOf.NegativeEvent, this);
			}
			if (Find.TickManager.TicksGame == autoActivateTick - 60)
			{
				Find.LetterStack.ReceiveLetter("MonolithAutoActivatedLabel".Translate(), "MonolithAutoActivatedText".Translate(), LetterDefOf.ThreatBig, this);
			}
			if (Find.TickManager.TicksGame > autoActivateTick && base.Map.mapPawns.FreeColonists.TryRandomElement(out var result2))
			{
				Activate(result2);
			}
		}
		if (IsAutoActivating)
		{
			if (autoActivateEffecter == null)
			{
				autoActivateEffecter = EffecterDefOf.MonolithAutoActivating.Spawn();
			}
			autoActivateEffecter.EffectTick(EffecterInfo, EffecterInfo);
		}
		else if (autoActivateEffecter != null)
		{
			autoActivateEffecter.Cleanup();
			autoActivateEffecter = null;
		}
		if (Find.Anomaly.Level == MonolithLevelDefOf.Gleaming.level && Find.CurrentMap == base.MapHeld)
		{
			if (gleamingEffecter == null)
			{
				gleamingEffecter = EffecterDefOf.MonolithGleaming_Sustained.Spawn();
			}
			gleamingEffecter.EffectTick(EffecterInfo, EffecterInfo);
			if (gleamingVoidNodeEffecter == null)
			{
				gleamingVoidNodeEffecter = EffecterDefOf.MonolithGleamingVoidNode.Spawn(this, base.Map);
			}
			gleamingVoidNodeEffecter.EffectTick(EffecterInfo, EffecterInfo);
		}
		else if (gleamingEffecter != null || Find.CurrentMap != base.MapHeld)
		{
			gleamingEffecter?.Cleanup();
			gleamingEffecter = null;
			gleamingVoidNodeEffecter?.Cleanup();
			gleamingVoidNodeEffecter = null;
		}
		if (activatedDialogTick > 0 && Find.TickManager.TicksGame > activatedDialogTick)
		{
			OpenActivatedDialog();
		}
	}

	public bool CanActivate(out string reason, out string reasonShort)
	{
		reason = "";
		reasonShort = "";
		if (!Find.Anomaly.LevelDef.advanceThroughActivation)
		{
			return false;
		}
		MonolithLevelDef nextLevelDef = Find.Anomaly.NextLevelDef;
		if (nextLevelDef == null)
		{
			return false;
		}
		if (nextLevelDef.entityCatagoryCompletionRequired != null && Find.EntityCodex.DiscoveredCount(nextLevelDef.entityCatagoryCompletionRequired) < nextLevelDef.entityCountCompletionRequired)
		{
			int num = nextLevelDef.entityCountCompletionRequired - Find.EntityCodex.DiscoveredCount(nextLevelDef.entityCatagoryCompletionRequired);
			reason = string.Format("{0}:\n  - {1}", "VoidMonolithRequiresDiscovery".Translate(), "VoidMonolithRequiresCategory".Translate(num, nextLevelDef.entityCatagoryCompletionRequired.label));
			reasonShort = "VoidMonolithRequiresDiscoveryShort".Translate();
			return false;
		}
		foreach (GameCondition activeCondition in base.Map.GameConditionManager.ActiveConditions)
		{
			List<GameConditionDef> unreachableDuringConditions = nextLevelDef.unreachableDuringConditions;
			if (unreachableDuringConditions != null && unreachableDuringConditions.Contains(activeCondition.def))
			{
				reason = activeCondition.def.LabelCap;
				reasonShort = activeCondition.def.LabelCap;
				return false;
			}
		}
		return true;
	}

	public void Activate(Pawn pawn)
	{
		CheckAndGenerateQuest();
		Find.Anomaly.IncrementLevel();
		EffecterDefOf.MonolithLevelChanged.Spawn().Trigger(EffecterInfo, EffecterInfo);
		activatedDialogTick = Find.TickManager.TicksGame + 360;
		activatorPawn = pawn;
		if (Find.Anomaly.Level == 1)
		{
			GetComp<CompStudiable>()?.SetStudyEnabled(enabled: true);
		}
		autoActivateTick = -99999;
	}

	public void CheckAndGenerateQuest()
	{
		if (Find.Anomaly.monolith == null)
		{
			Find.Anomaly.monolith = this;
		}
		if (quest == null && !Find.Anomaly.LevelDef.postEndgame && Find.Anomaly.GenerateMonolith)
		{
			Slate slate = new Slate();
			slate.Set("map", base.Map);
			slate.Set("monolith", this);
			quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.EndGame_VoidMonolith, slate);
			Find.SignalManager.SendSignal(new Signal("MonolithLevelChanged", global: true));
		}
	}

	private void OpenActivatedDialog()
	{
		DiaNode diaNode = new DiaNode(Find.Anomaly.LevelDef.activatedLetterText.Formatted(activatorPawn.Named("PAWN")));
		diaNode.options.Add(new DiaOption("VoidMonolithViewQuest".Translate())
		{
			action = delegate
			{
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
				((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
			},
			resolveTree = true
		});
		if (Find.Anomaly.LevelDef != MonolithLevelDefOf.VoidAwakened)
		{
			diaNode.options.Add(new DiaOption("VoidMonolithViewResearch".Translate())
			{
				action = delegate
				{
					Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
					((MainTabWindow_Research)MainButtonDefOf.Research.TabWindow).CurTab = ResearchTabDefOf.Anomaly;
				},
				resolveTree = true
			});
		}
		if (Find.Anomaly.Level == 1)
		{
			diaNode.options.Add(new DiaOption("ViewEntityCodex".Translate())
			{
				action = delegate
				{
					Find.WindowStack.Add(new Dialog_EntityCodex());
				},
				resolveTree = true
			});
		}
		diaNode.options.Add(new DiaOption("Close".Translate())
		{
			resolveTree = true
		});
		Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode);
		dialog_NodeTree.forcePause = true;
		Find.WindowStack.Add(dialog_NodeTree);
		activatedDialogTick = -99999;
		activatorPawn = null;
	}

	public void Investigate(Pawn pawn)
	{
		Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(new DiaNode("VoidMonolithInvestigatedText".Translate(pawn.Named("PAWN")))
		{
			options = 
			{
				new DiaOption("VoidMonolithInvestigate".Translate())
				{
					action = delegate
					{
						if (pawn != null)
						{
							Job job = JobMaker.MakeJob(JobDefOf.ActivateMonolith, this);
							pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
						}
					},
					resolveTree = true
				},
				new DiaOption("VoidMonolithWalkAway".Translate())
				{
					resolveTree = true
				}
			}
		});
		dialog_NodeTree.forcePause = true;
		Find.WindowStack.Add(dialog_NodeTree);
	}

	public void SetLevel(MonolithLevelDef levelDef)
	{
		CompGlower comp = GetComp<CompGlower>();
		int monolithGlowRadiusOverride = levelDef.monolithGlowRadiusOverride;
		if (monolithGlowRadiusOverride != -1)
		{
			comp.GlowRadius = monolithGlowRadiusOverride;
		}
		comp.UpdateLit(base.Map);
		if (comp.Glows)
		{
			comp.ForceRegister(base.Map);
		}
		if (base.Spawned)
		{
			UpdateAttachments();
			DirtyMapMesh(base.Map);
		}
		if (Find.Anomaly.LevelDef.activatedSound != null)
		{
			Find.Anomaly.LevelDef.activatedSound.PlayOneShot(this);
		}
		if (levelDef.level == MonolithLevelDefOf.Gleaming.level)
		{
			EffecterDefOf.MonolithGleaming_Transition.SpawnMaintained(base.PositionHeld, base.Map);
		}
	}

	private void UpdateAttachments()
	{
		TerrainDef newTerr = base.Map.Biome.TerrainForAffordance(def.terrainAffordanceNeeded);
		Thing.allowDestroyNonDestroyable = true;
		foreach (Thing monolithAttachment in monolithAttachments)
		{
			if (!monolithAttachment.Destroyed)
			{
				monolithAttachment.Destroy();
			}
		}
		Thing.allowDestroyNonDestroyable = false;
		monolithAttachments.Clear();
		if (Find.Anomaly.LevelDef.attachments == null)
		{
			return;
		}
		foreach (MonolithAttachment attachment in Find.Anomaly.LevelDef.attachments)
		{
			IntVec3 intVec = base.Position + attachment.offset.ToIntVec3;
			foreach (IntVec3 item in GenAdj.OccupiedRect(intVec, Rot4.North, attachment.def.Size))
			{
				if (!item.GetAffordances(base.Map).Contains(def.terrainAffordanceNeeded))
				{
					base.Map.terrainGrid.RemoveTopLayer(item, doLeavings: false);
					base.Map.terrainGrid.SetTerrain(item, newTerr);
				}
			}
			Thing thing = ThingMaker.MakeThing(attachment.def);
			thing.TryGetComp<CompSelectProxy>().thingToSelect = this;
			GenSpawn.Spawn(thing, intVec, base.Map, Rot4.North, WipeMode.FullRefund, respawningAfterLoad: false, forbidLeavings: true);
			thing.overrideGraphicIndex = attachment.graphicIndex;
			thing.DirtyMapMesh(base.Map);
			monolithAttachments.Add(thing);
		}
	}

	public void Collapse()
	{
		Map map = Find.Anomaly.monolith.Map;
		for (int i = 0; i < 3; i++)
		{
			if (CellFinder.TryFindRandomCellNear(Find.Anomaly.monolith.Position, map, 5, (IntVec3 c) => c.Standable(map), out var result))
			{
				GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.MonolithFragment), result, Find.Anomaly.monolith.Map);
			}
		}
		int num = 10;
		while (num > 0)
		{
			if (CellFinder.TryFindRandomCellNear(Find.Anomaly.monolith.Position, map, 5, (IntVec3 c) => c.Standable(map), out var result2))
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Shard);
				thing.stackCount = Mathf.Min(Rand.RangeInclusive(1, 2), num);
				num -= thing.stackCount;
				GenSpawn.Spawn(thing, result2, Find.Anomaly.monolith.Map);
			}
		}
	}

	public void Reset()
	{
		quest = null;
		GetComp<CompVoidStructure>().Reset();
		SetLevel(Find.Anomaly.LevelDef);
	}

	public bool CanHitTarget(LocalTargetInfo target)
	{
		return ValidateTarget(target, showMessages: false);
	}

	public bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (!target.IsValid || target.Pawn == null)
		{
			return false;
		}
		if (target.Pawn.Downed)
		{
			if (showMessages)
			{
				Messages.Message("VoidMonolithActivatorDowned".Translate(), target.Pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		if (target.Pawn.InMentalState)
		{
			if (showMessages)
			{
				Messages.Message("VoidMonolithActivatorMentalState".Translate(), target.Pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		if (!target.Pawn.CanCasuallyInteractNow(twoWayInteraction: false, canInteractWhileSleeping: false, canInteractWhileRoaming: false, canInteractWhileDrafted: true))
		{
			if (showMessages)
			{
				Messages.Message("VoidMonolithActivatorBusy".Translate(), target.Pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		if (!target.Pawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
		{
			if (showMessages)
			{
				Messages.Message("NoPath".Translate(), target.Pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return true;
	}

	public void DrawHighlight(LocalTargetInfo target)
	{
		if (target.IsValid)
		{
			GenDraw.DrawTargetHighlight(target);
		}
	}

	public void OrderForceTarget(LocalTargetInfo target)
	{
		if (ValidateTarget(target, showMessages: false))
		{
			Pawn pawn = target.Pawn;
			if (Find.Anomaly.Level == 0)
			{
				Job job = JobMaker.MakeJob(JobDefOf.InvestigateMonolith, this);
				pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				Messages.Message(Find.Anomaly.LevelDef.pawnSentToActivateMessage.Formatted(pawn.Named("PAWN")), this, MessageTypeDefOf.NeutralEvent);
				this.TryGetComp<CompProximityLetter>().letterSent = true;
			}
			else
			{
				OrderActivation(pawn, sendMessage: true);
			}
		}
	}

	private void OrderActivation(Pawn pawn, bool sendMessage)
	{
		Job job = JobMaker.MakeJob(JobDefOf.ActivateMonolith, this);
		pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		if (sendMessage)
		{
			Messages.Message(Find.Anomaly.LevelDef.pawnSentToActivateMessage.Formatted(pawn.Named("PAWN")), this, MessageTypeDefOf.NeutralEvent);
		}
	}

	public void AutoActivate(int tick)
	{
		autoActivateTick = tick;
		this.TryGetComp<CompProximityLetter>().letterSent = true;
		disturbingVisionTick = -99999;
	}

	public void OnGUI(LocalTargetInfo target)
	{
		Widgets.MouseAttachedLabel("VoidMonolithChooseActivator".Translate());
		if (ValidateTarget(target, showMessages: false) && targetParams.CanTarget(target.Pawn, this))
		{
			GenUI.DrawMouseAttachment(UIIcon);
		}
		else
		{
			GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
		}
	}

	public override string GetInspectString()
	{
		string inspectString = base.GetInspectString();
		StringBuilder stringBuilder = new StringBuilder();
		if (!Find.Anomaly.LevelDef.levelInspectText.NullOrEmpty())
		{
			stringBuilder.Append(Find.Anomaly.LevelDef.levelInspectText);
		}
		if (Find.Anomaly.Level > 0)
		{
			if (Find.Anomaly.LevelDef.advanceThroughActivation)
			{
				if (CanActivate(out var reason, out var _))
				{
					stringBuilder.AppendLineIfNotEmpty().Append(Find.Anomaly.LevelDef.monolithCanBeActivatedText);
				}
				else if (!reason.NullOrEmpty())
				{
					stringBuilder.AppendLineIfNotEmpty().Append(reason);
				}
			}
		}
		else
		{
			stringBuilder.AppendLineIfNotEmpty().Append("VoidMonolithUndiscovered".Translate());
		}
		if (!inspectString.NullOrEmpty())
		{
			stringBuilder.AppendLineIfNotEmpty().Append(inspectString);
		}
		return stringBuilder.ToString();
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		foreach (Gizmo questRelatedGizmo in QuestUtility.GetQuestRelatedGizmos(this))
		{
			yield return questRelatedGizmo;
		}
		if (!Find.Anomaly.LevelDef.advanceThroughActivation)
		{
			yield break;
		}
		string text = null;
		if (!CanActivate(out var reason, out var _))
		{
			text = reason;
		}
		yield return new Command_Action
		{
			defaultLabel = Find.Anomaly.LevelDef.activateGizmoText.CapitalizeFirst() + "...",
			defaultDesc = Find.Anomaly.LevelDef.activateGizmoDescription,
			icon = UIIcon,
			Disabled = !text.NullOrEmpty(),
			disabledReason = text,
			action = delegate
			{
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
				Find.Targeter.BeginTargeting(this);
			}
		};
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		if (base.Map.mapPawns.AnyFreeColonistSpawned)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Activate",
				action = delegate
				{
					if (base.Map.mapPawns.FreeColonists.TryRandomElement(out var result))
					{
						Activate(result);
					}
				}
			};
		}
		if (!Find.Anomaly.MonolithSpawned)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Relink Monolith",
				action = delegate
				{
					Find.Anomaly.monolith = this;
				}
			};
		}
	}

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		foreach (FloatMenuOption floatMenuOption2 in base.GetFloatMenuOptions(selPawn))
		{
			yield return floatMenuOption2;
		}
		string reason;
		string reasonShort;
		if (Find.Anomaly.Level == 0)
		{
			TaggedString taggedString = Find.Anomaly.LevelDef.activateFloatMenuText.Formatted(Label);
			if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
			{
				yield return new FloatMenuOption("CannotGenericWorkCustom".Translate(taggedString).CapitalizeFirst() + ": " + "NoPath".Translate().CapitalizeFirst(), null);
				yield break;
			}
			FloatMenuOption floatMenuOption = new FloatMenuOption(taggedString.CapitalizeFirst(), delegate
			{
				Job job = JobMaker.MakeJob(JobDefOf.InvestigateMonolith, this);
				selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				this.TryGetComp<CompProximityLetter>().letterSent = true;
			});
			floatMenuOption.tutorTag = "Investigate-" + def.defName;
			yield return floatMenuOption;
		}
		else if (CanActivate(out reason, out reasonShort))
		{
			if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
			{
				yield return new FloatMenuOption("CantActivateMonolith".Translate(Find.Anomaly.LevelDef.activateGizmoText) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
				yield break;
			}
			yield return new FloatMenuOption(Find.Anomaly.LevelDef.activateFloatMenuText.Formatted(Label).CapitalizeFirst(), delegate
			{
				OrderActivation(selPawn, sendMessage: false);
			});
		}
		else if (Find.Anomaly.LevelDef.advanceThroughActivation)
		{
			yield return new FloatMenuOption("CantActivateMonolith".Translate(Find.Anomaly.LevelDef.activateGizmoText) + ": " + reasonShort, null);
		}
	}
}
