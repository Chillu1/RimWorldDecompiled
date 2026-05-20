using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse.AI.Group;

namespace Verse;

public class Corpse : ThingWithComps, IThingHolder, IStrippable, IBillGiver, IObservedThoughtGiver
{
	protected ThingOwner<Pawn> innerContainer;

	public int timeOfDeath = -1;

	private int vanishAfterTimestamp = -1;

	private BillStack operationsBillStack;

	public bool everBuriedInSarcophagus;

	[Unsaved(false)]
	private string cachedLabel;

	private const int DontCauseObservedCorpseThoughtAfterRitualExecutionTicks = 60000;

	private static readonly IntRange ExplodeFilthCountRange = new IntRange(2, 5);

	public Pawn InnerPawn
	{
		get
		{
			if (innerContainer.Count <= 0)
			{
				return null;
			}
			return innerContainer[0];
		}
		set
		{
			if (value == null)
			{
				innerContainer.Clear();
				return;
			}
			if (innerContainer.Count > 0)
			{
				Log.Error("Setting InnerPawn in corpse that already has one.");
				innerContainer.Clear();
			}
			innerContainer.TryAdd(value);
		}
	}

	public int Age
	{
		get
		{
			return Find.TickManager.TicksGame - timeOfDeath;
		}
		set
		{
			timeOfDeath = Find.TickManager.TicksGame - value;
		}
	}

	public override string LabelNoCount
	{
		get
		{
			if (cachedLabel == null)
			{
				if (Bugged)
				{
					Log.Error("LabelNoCount on Corpse while Bugged.");
					return string.Empty;
				}
				cachedLabel = "DeadLabel".Translate(InnerPawn.Label, InnerPawn);
			}
			return cachedLabel;
		}
	}

	public override bool IngestibleNow
	{
		get
		{
			if (Bugged)
			{
				Log.Error("IngestibleNow on Corpse while Bugged.");
				return false;
			}
			if (!base.IngestibleNow)
			{
				return false;
			}
			if (!InnerPawn.RaceProps.IsFlesh)
			{
				return false;
			}
			if (this.GetRotStage() != RotStage.Fresh)
			{
				return false;
			}
			return true;
		}
	}

	public RotDrawMode CurRotDrawMode
	{
		get
		{
			CompRottable comp = GetComp<CompRottable>();
			if (comp != null)
			{
				return comp.Stage switch
				{
					RotStage.Dessicated => RotDrawMode.Dessicated, 
					RotStage.Rotting => RotDrawMode.Rotting, 
					_ => RotDrawMode.Fresh, 
				};
			}
			return RotDrawMode.Fresh;
		}
	}

	private bool ShouldVanish
	{
		get
		{
			if (InnerPawn.RaceProps.Animal && vanishAfterTimestamp > 0 && Age >= vanishAfterTimestamp && base.Spawned && this.GetRoom() != null && this.GetRoom().TouchesMapEdge)
			{
				return !base.Map.roofGrid.Roofed(base.Position);
			}
			return false;
		}
	}

	public BillStack BillStack => operationsBillStack;

	public IEnumerable<IntVec3> IngredientStackCells
	{
		get
		{
			yield return InteractionCell;
		}
	}

	public bool Bugged
	{
		get
		{
			if (innerContainer.Count != 0 && innerContainer[0]?.def != null)
			{
				return innerContainer[0].kindDef == null;
			}
			return true;
		}
	}

	public Corpse()
	{
		operationsBillStack = new BillStack(this);
		innerContainer = new ThingOwner<Pawn>(this, oneStackOnly: true, LookMode.Reference, removeContentsIfDestroyed: false);
	}

	public bool CurrentlyUsableForBills()
	{
		return InteractionCell.IsValid;
	}

	public bool UsableForBillsAfterFueling()
	{
		return CurrentlyUsableForBills();
	}

	public bool AnythingToStrip()
	{
		return InnerPawn.AnythingToStrip();
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public override void PostMake()
	{
		base.PostMake();
		timeOfDeath = Find.TickManager.TicksGame;
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (Bugged)
		{
			Log.Error(this?.ToString() + " spawned in bugged state.");
			return;
		}
		base.SpawnSetup(map, respawningAfterLoad);
		InnerPawn.Rotation = Rot4.South;
		List<Hediff> hediffs = InnerPawn.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			hediffs[i].Notify_PawnCorpseSpawned();
		}
		NotifyColonistBar();
	}

	public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
	{
		if (dinfo.HasValue && dinfo.Value.Def == DamageDefOf.Bomb)
		{
			CompRottable comp = GetComp<CompRottable>();
			ThingDef thingDef = ((comp == null || comp.Stage != RotStage.Rotting) ? InnerPawn.RaceProps.BloodDef : ThingDefOf.Filth_CorpseBile);
			if (thingDef != null)
			{
				int randomInRange = ExplodeFilthCountRange.RandomInRange;
				for (int i = 0; i < randomInRange; i++)
				{
					FilthMaker.TryMakeFilth(base.PositionHeld, base.MapHeld, thingDef);
				}
			}
		}
		base.Kill(dinfo, exactCulprit);
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		base.DeSpawn(mode);
		if (!Bugged)
		{
			NotifyColonistBar();
		}
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		Pawn pawn = null;
		if (!Bugged)
		{
			pawn = InnerPawn;
			NotifyColonistBar();
			innerContainer.Clear();
		}
		this.GetLord()?.Notify_CorpseLost(this);
		base.Destroy(mode);
		if (pawn != null)
		{
			PostCorpseDestroy(pawn);
		}
	}

	public static void PostCorpseDestroy(Pawn pawn, bool discarded = false)
	{
		pawn.ownership?.UnclaimAll();
		pawn.equipment?.DestroyAllEquipment();
		pawn.inventory.DestroyAll();
		pawn.health.Notify_PawnCorpseDestroyed();
		pawn.apparel?.DestroyAll();
		if (!PawnGenerator.IsBeingGenerated(pawn) && !discarded)
		{
			pawn.Ideo?.Notify_MemberCorpseDestroyed(pawn);
		}
	}

	public override void TickRare()
	{
		TickRareInt();
		if (base.Destroyed)
		{
			return;
		}
		if (Bugged)
		{
			Log.Error(this?.ToString() + " has null innerPawn. Destroying.");
			Destroy();
			return;
		}
		Hediff_DeathRefusal firstHediff = InnerPawn.health.hediffSet.GetFirstHediff<Hediff_DeathRefusal>();
		if (firstHediff != null)
		{
			firstHediff.TickRare();
			if (base.Destroyed)
			{
				return;
			}
		}
		if (ModsConfig.AnomalyActive && InnerPawn.kindDef.IsFleshBeast() && this.GetRotStage() == RotStage.Dessicated)
		{
			FilthMaker.TryMakeFilth(base.PositionHeld, base.MapHeld, ThingDefOf.Filth_TwistedFlesh);
			Destroy();
		}
		else if (ShouldVanish)
		{
			Destroy();
		}
	}

	protected void TickRareInt()
	{
		if (base.AllComps != null)
		{
			int i = 0;
			for (int count = base.AllComps.Count; i < count; i++)
			{
				base.AllComps[i].CompTickRare();
			}
		}
		if (!base.Destroyed)
		{
			InnerPawn.TickRare();
			GasUtility.CorpseGasEffectsTickRare(this);
		}
	}

	protected override void IngestedCalculateAmounts(Pawn ingester, float nutritionWanted, out int numTaken, out float nutritionIngested)
	{
		BodyPartRecord bodyPartRecord = GetBestBodyPartToEat(nutritionWanted);
		if (bodyPartRecord == null)
		{
			Log.Error(ingester?.ToString() + " ate " + this?.ToString() + " but no body part was found. Replacing with core part.");
			bodyPartRecord = InnerPawn.RaceProps.body.corePart;
		}
		float bodyPartNutrition = FoodUtility.GetBodyPartNutrition(this, bodyPartRecord);
		if (bodyPartRecord == InnerPawn.RaceProps.body.corePart)
		{
			if (ingester != null && PawnUtility.ShouldSendNotificationAbout(InnerPawn) && InnerPawn.RaceProps.Humanlike)
			{
				Messages.Message("MessageEatenByPredator".Translate(InnerPawn.LabelShort, ingester.Named("PREDATOR"), InnerPawn.Named("EATEN")).CapitalizeFirst(), ingester, MessageTypeDefOf.NegativeEvent);
			}
			numTaken = 1;
		}
		else
		{
			Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, InnerPawn, bodyPartRecord);
			if (ingester != null)
			{
				hediff_MissingPart.lastInjury = HediffDefOf.Bite;
			}
			hediff_MissingPart.IsFresh = true;
			InnerPawn.health.AddHediff(hediff_MissingPart);
			numTaken = 0;
		}
		nutritionIngested = bodyPartNutrition;
	}

	public override IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
	{
		foreach (Thing item in InnerPawn.ButcherProducts(butcher, efficiency))
		{
			yield return item;
		}
		if (InnerPawn.RaceProps.BloodDef != null)
		{
			FilthMaker.TryMakeFilth(butcher.Position, butcher.Map, InnerPawn.RaceProps.BloodDef, InnerPawn.LabelIndefinite());
		}
		if (InnerPawn.RaceProps.Humanlike)
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ButcheredHuman, new SignalArgs(butcher.Named(HistoryEventArgsNames.Doer), InnerPawn.Named(HistoryEventArgsNames.Victim))));
			TaleRecorder.RecordTale(TaleDefOf.ButcheredHumanlikeCorpse, butcher);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref timeOfDeath, "timeOfDeath", 0);
		Scribe_Values.Look(ref vanishAfterTimestamp, "vanishAfterTimestamp", 0);
		Scribe_Values.Look(ref everBuriedInSarcophagus, "everBuriedInSarcophagus", defaultValue: false);
		Scribe_Deep.Look(ref operationsBillStack, "operationsBillStack", this);
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && innerContainer.removeContentsIfDestroyed)
		{
			innerContainer.removeContentsIfDestroyed = false;
		}
	}

	public void Strip(bool notifyFaction = true)
	{
		InnerPawn.Strip(notifyFaction);
	}

	public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
	{
		InnerPawn.DynamicDrawPhaseAt(phase, drawLoc.WithYOffset(InnerPawn.Drawer.SeededYOffset));
	}

	public Thought_Memory GiveObservedThought(Pawn observer)
	{
		return null;
	}

	public HistoryEventDef GiveObservedHistoryEvent(Pawn observer)
	{
		if (!InnerPawn.RaceProps.Humanlike)
		{
			return null;
		}
		if (InnerPawn.health.killedByRitual && Find.TickManager.TicksGame - timeOfDeath < 60000)
		{
			return null;
		}
		if (this.StoringThing() == null)
		{
			if (this.IsNotFresh())
			{
				return HistoryEventDefOf.ObservedLayingRottingCorpse;
			}
			return HistoryEventDefOf.ObservedLayingCorpse;
		}
		return null;
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (InnerPawn.Faction != null && !InnerPawn.Faction.Hidden)
		{
			stringBuilder.AppendLineTagged("Faction".Translate() + ": " + InnerPawn.Faction.NameColored);
		}
		stringBuilder.AppendLine("DeadTime".Translate(Age.ToStringTicksToPeriodVague(vagueMin: true, vagueMax: false)));
		float num = 1f - InnerPawn.health.hediffSet.GetCoverageOfNotMissingNaturalParts(InnerPawn.RaceProps.body.corePart);
		if (num >= 0.01f)
		{
			stringBuilder.AppendLine("CorpsePercentMissing".Translate() + ": " + num.ToStringPercent());
		}
		Hediff_DeathRefusal firstHediff = InnerPawn.health.hediffSet.GetFirstHediff<Hediff_DeathRefusal>();
		if (firstHediff != null && firstHediff.InProgress)
		{
			stringBuilder.AppendLine("SelfResurrecting".Translate());
		}
		stringBuilder.AppendLine(base.GetInspectString());
		return stringBuilder.ToString().TrimEndNewlines();
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats())
		{
			yield return item;
		}
		yield return new StatDrawEntry(StatCategoryDefOf.Basics, "BodySize".Translate(), InnerPawn.BodySize.ToString("F2"), "Stat_Race_BodySize_Desc".Translate(), 4195);
		if (this.GetRotStage() == RotStage.Fresh)
		{
			StatDef meatAmount = StatDefOf.MeatAmount;
			yield return new StatDrawEntry(meatAmount.category, meatAmount, InnerPawn.GetStatValue(meatAmount), StatRequest.For(InnerPawn));
			StatDef leatherAmount = StatDefOf.LeatherAmount;
			yield return new StatDrawEntry(leatherAmount.category, leatherAmount, InnerPawn.GetStatValue(leatherAmount), StatRequest.For(InnerPawn));
		}
	}

	public void RotStageChanged()
	{
		InnerPawn.Drawer.renderer.SetAllGraphicsDirty();
		InnerPawn.Drawer.renderer.WoundOverlays.ClearCache();
		NotifyColonistBar();
	}

	public BodyPartRecord GetBestBodyPartToEat(float nutritionWanted)
	{
		IEnumerable<BodyPartRecord> source = from x in InnerPawn.health.hediffSet.GetNotMissingParts()
			where x.depth == BodyPartDepth.Outside && FoodUtility.GetBodyPartNutrition(this, x) > 0.001f
			select x;
		if (!source.Any())
		{
			return null;
		}
		return source.MinBy((BodyPartRecord x) => Mathf.Abs(FoodUtility.GetBodyPartNutrition(this, x) - nutritionWanted));
	}

	private void NotifyColonistBar()
	{
		if (InnerPawn.Faction == Faction.OfPlayer && Current.ProgramState == ProgramState.Playing)
		{
			Find.ColonistBar.MarkColonistsDirty();
		}
	}

	public void Notify_BillDeleted(Bill bill)
	{
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (InnerPawn.HasShowGizmosOnCorpseHediff)
		{
			foreach (Gizmo gizmo2 in InnerPawn.GetGizmos())
			{
				yield return gizmo2;
			}
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Resurrect",
			action = delegate
			{
				ResurrectionUtility.TryResurrect(InnerPawn);
			}
		};
		if (ModsConfig.AnomalyActive)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Resurrect as shambler",
				action = delegate
				{
					MutantUtility.ResurrectAsShambler(InnerPawn);
				}
			};
		}
	}
}
