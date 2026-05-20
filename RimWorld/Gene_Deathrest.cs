using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Gene_Deathrest : Gene
{
	private int deathrestCapacity;

	private List<Thing> boundBuildings = new List<Thing>();

	public float adjustedDeathrestTicks;

	public int deathrestTicks;

	public bool autoWake;

	public int chamberThoughtIndex = -1;

	private bool notifiedWakeOK;

	[Unsaved(false)]
	private Gene_Hemogen cachedHemogenGene;

	[Unsaved(false)]
	private Need_Deathrest cachedDeathrestNeed;

	[Unsaved(false)]
	private List<CompDeathrestBindable> cachedBoundComps;

	[Unsaved(false)]
	private GeneGizmo_DeathrestCapacity gizmo;

	private const int InitialDeathrestCapacity = 1;

	public const int BaseDeathrestTicksWithoutInterruptedHediff = 240000;

	public const float PresencePercentRequiredToApply = 0.75f;

	private static readonly CachedTexture WakeCommandTex = new CachedTexture("UI/Gizmos/Wake");

	private static readonly CachedTexture AutoWakeCommandTex = new CachedTexture("UI/Gizmos/DeathrestAutoWake");

	private const int LessonDeathrestTicks = 200;

	private const int SunlightCheckInterval = 150;

	public Need_Deathrest DeathrestNeed
	{
		get
		{
			if (cachedDeathrestNeed == null)
			{
				pawn.needs.TryGetNeed(out cachedDeathrestNeed);
			}
			return cachedDeathrestNeed;
		}
	}

	public List<Thing> BoundBuildings => boundBuildings;

	public List<CompDeathrestBindable> BoundComps
	{
		get
		{
			if (cachedBoundComps == null)
			{
				cachedBoundComps = new List<CompDeathrestBindable>();
				foreach (Thing boundBuilding in boundBuildings)
				{
					if (CanUseBindableNow(boundBuilding))
					{
						cachedBoundComps.Add(boundBuilding.TryGetComp<CompDeathrestBindable>());
					}
				}
			}
			return cachedBoundComps;
		}
	}

	public float DeathrestEfficiency
	{
		get
		{
			float num = 1f;
			foreach (CompDeathrestBindable boundComp in BoundComps)
			{
				if (boundComp.Props.deathrestEffectivenessFactor > 0f && boundComp.CanIncreasePresence)
				{
					num *= boundComp.Props.deathrestEffectivenessFactor;
				}
			}
			return num;
		}
	}

	public int MinDeathrestTicks => Mathf.RoundToInt(240000f / DeathrestEfficiency);

	public int DeathrestCapacity => deathrestCapacity;

	public int CurrentCapacity
	{
		get
		{
			int num = 0;
			for (int i = 0; i < boundBuildings.Count; i++)
			{
				CompDeathrestBindable compDeathrestBindable = boundBuildings[i].TryGetComp<CompDeathrestBindable>();
				if (compDeathrestBindable != null && compDeathrestBindable.Props.countsTowardsBuildingLimit)
				{
					num++;
				}
			}
			return num;
		}
	}

	public bool AtBuildingCapacityLimit => CurrentCapacity >= deathrestCapacity;

	public float DeathrestPercent => Mathf.Clamp01((float)deathrestTicks / (float)MinDeathrestTicks);

	public bool ShowWakeAlert
	{
		get
		{
			if (DeathrestPercent >= 1f)
			{
				return !autoWake;
			}
			return false;
		}
	}

	public override void PostAdd()
	{
		if (ModLister.CheckBiotech("Deathrest"))
		{
			base.PostAdd();
			Reset();
		}
	}

	public override void PostRemove()
	{
		base.PostRemove();
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Deathrest);
		if (firstHediffOfDef != null)
		{
			pawn.health.RemoveHediff(firstHediffOfDef);
		}
		Hediff firstHediffOfDef2 = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DeathrestExhaustion);
		if (firstHediffOfDef2 != null)
		{
			pawn.health.RemoveHediff(firstHediffOfDef2);
		}
		RemoveOldDeathrestBonuses();
		Reset();
	}

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (pawn.IsHashIntervalTick(200, delta) && pawn.IsColonistPlayerControlled)
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.Deathrest, OpportunityType.Important);
		}
	}

	public void OffsetCapacity(int offset, bool sendNotification = true)
	{
		int num = deathrestCapacity;
		deathrestCapacity += offset;
		if (sendNotification && PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			Messages.Message("MessageDeathrestCapacityChanged".Translate(pawn.Named("PAWN"), num.Named("OLD"), deathrestCapacity.Named("NEW")), pawn, MessageTypeDefOf.PositiveEvent);
		}
	}

	public void TickDeathresting(bool paused, int delta)
	{
		if (paused)
		{
			return;
		}
		if (DeathrestNeed != null)
		{
			DeathrestNeed.lastDeathrestTick = Find.TickManager.TicksGame;
		}
		deathrestTicks += delta;
		adjustedDeathrestTicks += DeathrestEfficiency * (float)delta;
		foreach (CompDeathrestBindable boundComp in BoundComps)
		{
			boundComp.TryIncreasePresence(delta);
		}
		if (DeathrestPercent >= 1f && !notifiedWakeOK)
		{
			notifiedWakeOK = true;
			if (autoWake)
			{
				Wake();
				return;
			}
			if (PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				Messages.Message("MessageDeathrestingPawnCanWakeSafely".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
			}
		}
		if (pawn.Spawned && pawn.IsHashIntervalTick(150, delta) && PawnOrBedTouchingSunlight())
		{
			if (PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				Messages.Message("MessagePawnWokenFromSunlight".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
			}
			Wake();
		}
	}

	private bool PawnOrBedTouchingSunlight()
	{
		Building_Bed building_Bed = pawn.CurrentBed();
		if (building_Bed != null)
		{
			foreach (IntVec3 item in building_Bed.OccupiedRect())
			{
				if (item.InSunlight(building_Bed.Map))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Notify_DeathrestStarted()
	{
		RemoveOldDeathrestBonuses();
		TryLinkToNearbyDeathrestBuildings();
		notifiedWakeOK = false;
	}

	public void Notify_DeathrestEnded()
	{
		foreach (Thing boundBuilding in boundBuildings)
		{
			boundBuilding.TryGetComp<CompDeathrestBindable>().Notify_DeathrestEnded();
		}
	}

	public void Notify_BoundBuildingDeSpawned(Thing building)
	{
		boundBuildings.Remove(building);
		cachedBoundComps = null;
	}

	public void TryLinkToNearbyDeathrestBuildings()
	{
		if (!ModsConfig.BiotechActive || !pawn.Spawned)
		{
			return;
		}
		cachedBoundComps = null;
		Room room = pawn.GetRoom();
		if (room == null)
		{
			return;
		}
		foreach (Region region in room.Regions)
		{
			foreach (Thing item in region.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
			{
				CompDeathrestBindable bindComp = item.TryGetComp<CompDeathrestBindable>();
				if (CanBindToBindable(bindComp))
				{
					BindTo(bindComp);
				}
			}
		}
	}

	public void RemoveOldDeathrestBonuses()
	{
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		for (int num = hediffs.Count - 1; num >= 0; num--)
		{
			if (hediffs[num].def.removeOnDeathrestStart)
			{
				pawn.health.RemoveHediff(hediffs[num]);
			}
		}
		pawn.genes.GetFirstGeneOfType<Gene_Hemogen>()?.ResetMax();
	}

	private void ApplyDeathrestBuildingBonuses()
	{
		if (deathrestTicks == 0)
		{
			return;
		}
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.InterruptedDeathrest);
		if (firstHediffOfDef != null)
		{
			pawn.health.RemoveHediff(firstHediffOfDef);
		}
		foreach (CompDeathrestBindable boundComp in BoundComps)
		{
			if ((float)boundComp.presenceTicks / (float)deathrestTicks >= 0.75f)
			{
				boundComp.Apply();
			}
		}
	}

	public bool CanBindToBindable(CompDeathrestBindable bindComp)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (bindComp == null)
		{
			return false;
		}
		if (bindComp.parent.Faction != pawn.Faction)
		{
			return false;
		}
		if (boundBuildings.Contains(bindComp.parent))
		{
			return false;
		}
		if (bindComp.Props.countsTowardsBuildingLimit && AtBuildingCapacityLimit)
		{
			return false;
		}
		if (!bindComp.CanBindTo(pawn))
		{
			return false;
		}
		if (!CanUseBindableNow(bindComp.parent))
		{
			return false;
		}
		if (BindingWillExceedStackLimit(bindComp))
		{
			return false;
		}
		return true;
	}

	public bool BindingWillExceedStackLimit(CompDeathrestBindable bindComp)
	{
		if (boundBuildings.Contains(bindComp.parent))
		{
			return false;
		}
		int stackLimit = bindComp.Props.stackLimit;
		if (stackLimit > 0)
		{
			int num = 0;
			for (int i = 0; i < boundBuildings.Count; i++)
			{
				if (boundBuildings[i].def == bindComp.parent.def)
				{
					num++;
					if (num >= stackLimit)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool CanUseBindableNow(Thing building)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		CompDeathrestBindable compDeathrestBindable = building.TryGetComp<CompDeathrestBindable>();
		if (compDeathrestBindable == null)
		{
			return false;
		}
		Building_Bed building_Bed = pawn.CurrentBed();
		if (building_Bed == null)
		{
			return false;
		}
		if (compDeathrestBindable.Props.mustBeLayingInToBind)
		{
			if (building_Bed != building)
			{
				return false;
			}
		}
		else if (!building_Bed.def.HasComp(typeof(CompDeathrestBindable)))
		{
			return false;
		}
		if (!building.Spawned || building.Map != pawn.MapHeld || building.GetRoom() != pawn.GetRoom())
		{
			return false;
		}
		if (!GenSight.LineOfSight(pawn.Position, building.OccupiedRect().CenterCell, pawn.Map))
		{
			return false;
		}
		return true;
	}

	public void BindTo(CompDeathrestBindable bindComp)
	{
		bindComp.BindTo(pawn);
		boundBuildings.Add(bindComp.parent);
		cachedBoundComps = null;
		if (PawnUtility.ShouldSendNotificationAbout(pawn) && bindComp.Props.countsTowardsBuildingLimit)
		{
			Messages.Message("MessageDeathrestBuildingBound".Translate(bindComp.parent.Named("BUILDING"), pawn.Named("PAWN"), CurrentCapacity.Named("CUR"), DeathrestCapacity.Named("MAX")), new LookTargets(bindComp.parent, pawn), MessageTypeDefOf.NeutralEvent, historical: false);
		}
	}

	public override void Reset()
	{
		deathrestCapacity = 1;
		foreach (CompDeathrestBindable boundComp in BoundComps)
		{
			boundComp.Notify_DeathrestGeneRemoved();
		}
		cachedBoundComps = null;
	}

	public override void Notify_NewColony()
	{
		if (!pawn.BeingTransportedOnGravship)
		{
			boundBuildings.Clear();
			cachedBoundComps = null;
		}
	}

	public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
		base.Notify_PawnDied(dinfo, culprit);
		boundBuildings.Clear();
		cachedBoundComps = null;
		cachedDeathrestNeed = null;
	}

	public void Wake()
	{
		if (DeathrestPercent < 1f)
		{
			pawn.health.AddHediff(HediffDefOf.InterruptedDeathrest);
		}
		else
		{
			ApplyDeathrestBuildingBonuses();
		}
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Deathrest);
		if (firstHediffOfDef != null)
		{
			pawn.health.RemoveHediff(firstHediffOfDef);
		}
		deathrestTicks = 0;
		adjustedDeathrestTicks = 0f;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		if (!Active)
		{
			yield break;
		}
		if (gizmo == null)
		{
			gizmo = new GeneGizmo_DeathrestCapacity(this);
		}
		if (Find.Selector.SelectedPawns.Count == 1)
		{
			yield return gizmo;
		}
		if (pawn.Deathresting)
		{
			if (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony)
			{
				string text = "WakeDesc".Translate(pawn.Named("PAWN"), deathrestTicks.ToStringTicksToPeriod().Named("DURATION")).Resolve() + "\n\n";
				text = ((!(DeathrestPercent < 1f)) ? (text + "WakeExtraDesc_Safe".Translate(pawn.Named("PAWN")).Resolve()) : (text + "WakeExtraDesc_Exhaustion".Translate(pawn.Named("PAWN"), MinDeathrestTicks.ToStringTicksToPeriod().Named("TOTAL")).Resolve()));
				Command_Action command_Action = new Command_Action
				{
					defaultLabel = "Wake".Translate().CapitalizeFirst(),
					defaultDesc = text,
					icon = WakeCommandTex.Texture,
					action = delegate
					{
						if (DeathrestPercent < 1f)
						{
							Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("WarningWakingInterruptsDeathrest".Translate(pawn.Named("PAWN"), MinDeathrestTicks.ToStringTicksToPeriod().Named("MINDURATION"), deathrestTicks.ToStringTicksToPeriod().Named("CURDURATION")), Wake, destructive: true);
							Find.WindowStack.Add(window);
						}
						else
						{
							Wake();
						}
					}
				};
				if (SanguophageUtility.ShouldBeDeathrestingOrInComaInsteadOfDead(pawn))
				{
					command_Action.Disable("WakeDisabledCriticalCondition".Translate(pawn.Named("PAWN")));
				}
				yield return command_Action;
				if (DeathrestPercent < 1f)
				{
					yield return new Command_Toggle
					{
						defaultLabel = "AutoWake".Translate().CapitalizeFirst(),
						defaultDesc = "AutoWakeDesc".Translate(pawn.Named("PAWN")).Resolve(),
						icon = AutoWakeCommandTex.Texture,
						isActive = () => autoWake,
						toggleAction = delegate
						{
							autoWake = !autoWake;
						}
					};
				}
			}
			if (DebugSettings.ShowDevGizmos)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Wake and apply bonuses",
					action = delegate
					{
						deathrestTicks = MinDeathrestTicks + 1;
						adjustedDeathrestTicks = 240001f;
						foreach (CompDeathrestBindable boundComp in BoundComps)
						{
							boundComp.presenceTicks = deathrestTicks;
						}
						Wake();
					}
				};
			}
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Set capacity",
			action = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				for (int i = 1; i <= 20; i++)
				{
					int newCap = i;
					list.Add(new FloatMenuOption(newCap.ToString(), delegate
					{
						OffsetCapacity(newCap - deathrestCapacity, sendNotification: false);
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		};
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "DeathrestCapacity".Translate().CapitalizeFirst(), deathrestCapacity.ToString(), "DeathrestCapacityDesc".Translate(), 1010);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref deathrestCapacity, "deathrestCapacity", 1);
		Scribe_Values.Look(ref adjustedDeathrestTicks, "adjustedDeathrestTicks", 0f);
		Scribe_Values.Look(ref deathrestTicks, "deathrestTicks", 0);
		Scribe_Values.Look(ref autoWake, "autoWake", defaultValue: false);
		Scribe_Values.Look(ref chamberThoughtIndex, "chamberThoughtIndex", -1);
		Scribe_Values.Look(ref notifiedWakeOK, "notifiedWakeOK", defaultValue: false);
		Scribe_Collections.Look(ref boundBuildings, "boundBuildings", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			boundBuildings.RemoveAll((Thing x) => x == null);
		}
	}
}
