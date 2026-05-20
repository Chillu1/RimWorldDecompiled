using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

public class Hediff_MetalhorrorImplant : HediffWithComps
{
	private ImplantSource implantSource = new ImplantSource();

	private bool emerging;

	private bool knowsDetected;

	private bool sympatheticEmergence;

	private bool sentMaturingMessage;

	private string emergeReason;

	private int emergeAt;

	private bool liedAboutInspection;

	private int filthTimer;

	private int spawnFilthAt;

	private IntVec3 filthPos = IntVec3.Invalid;

	private Map filthMap;

	public bool debugDiscoverNextInteraction;

	private static readonly IntRange TicksToEmerge = new IntRange(60, 180);

	private static readonly IntRange TicksToEmergeSympathetic = new IntRange(60, 600);

	private static readonly FloatRange InitialFilthDelayDays = new FloatRange(1.5f, 2.5f);

	private static readonly FloatRange FilthDelayDays = new FloatRange(3f, 10f);

	private static readonly FloatRange SpawnFilthDelayHours = new FloatRange(1f, 12f);

	private static readonly List<(Hediff_MetalhorrorImplant hediff, string message)> TmpEmerging = new List<(Hediff_MetalhorrorImplant, string)>();

	public ImplantSource ImplantSource
	{
		get
		{
			return implantSource;
		}
		set
		{
			implantSource = value;
		}
	}

	public int Biosignature => implantSource.Biosignature;

	public string BiosignatureName => implantSource.BiosignatureName;

	public bool Emerging => emerging;

	public bool KnowsDetected => knowsDetected;

	public bool LiedAboutInspection
	{
		get
		{
			return liedAboutInspection;
		}
		set
		{
			liedAboutInspection = value || liedAboutInspection;
		}
	}

	public override string TipStringExtra => implantSource.GetSourceDesc().CapitalizeFirst();

	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModLister.CheckAnomaly("Metalhorror implant"))
		{
			pawn.health.RemoveHediff(this);
			return;
		}
		base.PostAdd(dinfo);
		filthTimer = Mathf.CeilToInt(InitialFilthDelayDays.RandomInRange * 60000f);
	}

	public override void PostTickInterval(int delta)
	{
		base.PostTickInterval(delta);
		if (pawn.SpawnedOrAnyParentSpawned)
		{
			DoPassiveTransmission();
			FilthTick(delta);
			if (Emerging && GenTicks.TicksGame >= emergeAt)
			{
				MetalhorrorUtility.SpawnMetalhorror(pawn, this);
			}
			if (knowsDetected)
			{
				Emerge("");
			}
			else if (MetalhorrorUtility.ShouldRandomEmerge(pawn, delta))
			{
				Emerge("MetalhorrorReasonRandom".Translate(pawn.Named("INFECTED")));
			}
		}
	}

	public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
		Emerge("MetalhorrorReasonHostDied".Translate(pawn.Named("INFECTED")));
	}

	public void Emerge(string reason, bool isSympathetic = false)
	{
		if (emerging || !pawn.SpawnedOrAnyParentSpawned)
		{
			return;
		}
		Find.Anomaly.emergedBiosignatures.Add(Biosignature);
		SetVisible();
		emerging = true;
		knowsDetected = true;
		sympatheticEmergence = isSympathetic || sympatheticEmergence;
		if (string.IsNullOrEmpty(emergeReason))
		{
			emergeReason = reason;
		}
		if (sympatheticEmergence)
		{
			emergeAt = GenTicks.TicksGame + TicksToEmergeSympathetic.RandomInRange;
		}
		else
		{
			emergeAt = GenTicks.TicksGame + TicksToEmerge.RandomInRange;
		}
		if (!pawn.DeadOrDowned)
		{
			HealthUtility.DamageUntilDowned(pawn, allowBleedingWounds: true, DamageDefOf.Cut, ThingDefOf.Metalhorror, BodyPartGroupDefOf.LeftBlade);
		}
		else if (!pawn.Dead)
		{
			DamageWorker.DamageResult damageResult = pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 20f));
			if (damageResult.hediffs != null)
			{
				foreach (Hediff hediff in damageResult.hediffs)
				{
					hediff.sourceDef = ThingDefOf.Metalhorror;
					hediff.sourceBodyPartGroup = BodyPartGroupDefOf.LeftBlade;
				}
			}
		}
		if (!sympatheticEmergence)
		{
			TmpEmerging.Clear();
			foreach (Pawn item in pawn.MapHeld.mapPawns.AllHumanlike)
			{
				TryTriggerSympatheticEmergence(item, TmpEmerging);
			}
			foreach (Thing item2 in pawn.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.Corpse))
			{
				if (item2 is Corpse { InnerPawn: not null } corpse)
				{
					TryTriggerSympatheticEmergence(corpse.InnerPawn, TmpEmerging);
				}
			}
			TmpEmerging.SortByDescending(((Hediff_MetalhorrorImplant hediff, string message) tuple2) => tuple2.hediff.ageTicks);
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (var (hediff_MetalhorrorImplant, value) in TmpEmerging)
			{
				stringBuilder.AppendLine(value);
				if (hediff_MetalhorrorImplant.LiedAboutInspection)
				{
					if (stringBuilder2.Length == 0)
					{
						stringBuilder2.AppendLine(string.Format("{0}:\n", "MetalhorrorSurgicalInspectionDetails".Translate()));
					}
					stringBuilder2.AppendLine($" - {hediff_MetalhorrorImplant.pawn.NameShortColored}");
				}
			}
			TaggedString taggedString = ((TmpEmerging.Count != 1) ? "MetalhorrorEmergingPluralDesc".Translate() : "MetalhorrorEmergingDesc".Translate());
			taggedString += "\n\n" + stringBuilder;
			if (stringBuilder2.Length > 0)
			{
				taggedString += "\n" + stringBuilder2;
			}
			taggedString += "\n" + emergeReason;
			taggedString += "\n\n" + "MetalhorrorEmergingDescAppended".Translate();
			Find.LetterStack.ReceiveLetter("MetalhorrorEmergingLabel".Translate(), taggedString.Resolve(), LetterDefOf.ThreatBig, TmpEmerging.Select(((Hediff_MetalhorrorImplant hediff, string message) hd) => hd.hediff.pawn).ToList());
			Find.AnalysisManager.RemoveAnalysisDetails(Biosignature);
			TmpEmerging.Clear();
		}
		DoEmergingEffects();
		if (pawn.Dead)
		{
			MetalhorrorUtility.SpawnMetalhorror(pawn, this);
		}
	}

	private void TryTriggerSympatheticEmergence(Pawn target, List<(Hediff_MetalhorrorImplant hediff, string message)> list)
	{
		Hediff_MetalhorrorImplant firstHediff = target.health.hediffSet.GetFirstHediff<Hediff_MetalhorrorImplant>();
		if (firstHediff != null && firstHediff.Biosignature == Biosignature)
		{
			if (target != pawn)
			{
				MetalhorrorUtility.TryEmerge(target, null, sympathetic: true);
			}
			string item = $"  - {target.NameShortColored} ({firstHediff.ImplantSource.GetSourceDesc()})";
			list.Add((firstHediff, item));
		}
	}

	private void DoEmergingEffects()
	{
		EffecterDefOf.MetalhorrorEmerging.Spawn(pawn.PositionHeld, pawn.MapHeld).Cleanup();
		IntVec3 positionHeld = pawn.PositionHeld;
		Map mapHeld = pawn.MapHeld;
		CellRect cellRect = new CellRect(positionHeld.x, positionHeld.z, 3, 3);
		for (int i = 0; i < 5; i++)
		{
			IntVec3 randomCell = cellRect.RandomCell;
			if (randomCell.InBounds(mapHeld) && GenSight.LineOfSight(randomCell, positionHeld, mapHeld))
			{
				FilthMaker.TryMakeFilth(randomCell, mapHeld, (i % 2 == 0) ? ThingDefOf.Filth_Blood : ThingDefOf.Filth_GrayFlesh);
			}
		}
	}

	public void Detect(string reason, bool noticed)
	{
		SetVisible();
		knowsDetected = noticed;
		if (knowsDetected)
		{
			Emerge(reason);
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (!DebugSettings.godMode || emerging)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Emerge",
			action = delegate
			{
				Emerge("MetalhorrorReasonRandom".Translate(pawn.Named("INFECTED")));
			}
		};
		if (!filthPos.IsValid)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Mark for flesh drop",
				action = delegate
				{
					filthTimer = 0;
				}
			};
		}
		if (!debugDiscoverNextInteraction)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Discover next interaction",
				action = delegate
				{
					debugDiscoverNextInteraction = true;
				}
			};
		}
		int hours = ageTicks / 2500;
		if (hours < 72)
		{
			string text = "Larva";
			if (hours > 24)
			{
				text = "Juvenile";
			}
			yield return new Command_Action
			{
				defaultLabel = "DEV: Inc Lifestage (" + text + ")",
				action = delegate
				{
					if (hours < 24)
					{
						ageTicks = 62500;
					}
					else
					{
						ageTicks = 182500;
					}
				}
			};
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Change biosignature",
			action = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (Pawn item in pawn.MapHeld.mapPawns.AllHumanlike)
				{
					if (MetalhorrorUtility.IsInfected(item, out var implant) && !list.ContainsAny((FloatMenuOption x) => x.Label == implant.BiosignatureName))
					{
						list.Add(new FloatMenuOption(implant.BiosignatureName, delegate
						{
							implantSource.DebugSetBiosignature(implant.Biosignature);
						}));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		};
	}

	private void DoPassiveTransmission()
	{
		if (!pawn.InBed())
		{
			return;
		}
		foreach (Pawn curOccupant in pawn.CurrentBed().CurOccupants)
		{
			if (curOccupant != pawn)
			{
				MetalhorrorUtility.Infect(curOccupant, pawn, "SleepImplant");
			}
		}
	}

	private void FilthTick(int delta)
	{
		UpdateFilthTimer(delta);
		if (filthTimer <= 0)
		{
			if (!FilthMaker.CanMakeFilth(pawn.PositionHeld, pawn.MapHeld, ThingDefOf.Filth_GrayFleshNoticeable))
			{
				filthTimer = 600;
			}
			else
			{
				filthTimer = Mathf.CeilToInt(FilthDelayDays.RandomInRange * 60000f);
				spawnFilthAt = GenTicks.TicksGame + Mathf.CeilToInt(SpawnFilthDelayHours.RandomInRange * 2500f);
				filthMap = pawn.MapHeld;
				filthPos = pawn.PositionHeld;
			}
		}
		TryPlaceFilth();
	}

	private void TryPlaceFilth()
	{
		if (!filthPos.IsValid)
		{
			return;
		}
		if (filthMap == null || filthMap.Disposed)
		{
			if (pawn.SpawnedOrAnyParentSpawned)
			{
				filthMap = pawn.MapHeld;
				filthPos = pawn.PositionHeld;
				spawnFilthAt = GenTicks.TicksGame + 2500;
			}
		}
		else if (GenTicks.TicksGame >= spawnFilthAt)
		{
			if (FilthMaker.TryMakeFilth(filthPos, filthMap, ThingDefOf.Filth_GrayFleshNoticeable, out var outFilth))
			{
				((FilthGrayFleshNoticeable)outFilth).biosignature = Biosignature;
			}
			filthPos = IntVec3.Invalid;
			filthMap = null;
		}
	}

	public override string GetInspectString()
	{
		if (Prefs.DevMode && DebugSettings.godMode)
		{
			return $"DEV: Bio: {BiosignatureName}, mark filth: {filthTimer.TicksToSeconds():0}s " + ((spawnFilthAt < GenTicks.TicksGame) ? "" : $" (spawns: {(spawnFilthAt - GenTicks.TicksGame).TicksToSeconds():0}s)");
		}
		return base.GetInspectString();
	}

	private void UpdateFilthTimer(int delta)
	{
		if (!pawn.InBed() && !pawn.GetPosture().Laying() && !pawn.Downed)
		{
			filthTimer -= delta;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref emerging, "emerging", defaultValue: false);
		Scribe_Values.Look(ref emergeAt, "emergeAt", 0);
		Scribe_Values.Look(ref filthTimer, "filthTimer", 0);
		Scribe_Values.Look(ref knowsDetected, "knowsDetected", defaultValue: false);
		Scribe_Values.Look(ref sympatheticEmergence, "sympatheticEmergence", defaultValue: false);
		Scribe_Values.Look(ref sentMaturingMessage, "sentMaturingMessage", defaultValue: false);
		Scribe_Values.Look(ref spawnFilthAt, "spawnFilthAt", 0);
		Scribe_Values.Look(ref filthPos, "filthPos");
		Scribe_Values.Look(ref liedAboutInspection, "liedAboutInspection", defaultValue: false);
		Scribe_References.Look(ref filthMap, "filthMap");
		Scribe_Deep.Look(ref implantSource, "implantSource");
	}
}
