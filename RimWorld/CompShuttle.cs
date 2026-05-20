using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompShuttle : ThingComp
	{
		public List<ThingDefCount> requiredItems = new List<ThingDefCount>();

		public List<Pawn> requiredPawns = new List<Pawn>();

		public List<Pawn> pawnsToIgnoreIfDownedOfNotOnTheMap = new List<Pawn>();

		public int requiredColonistCount;

		public bool requireAllColonistsOnMap;

		public int maxColonistCount = -1;

		public bool acceptColonists;

		public bool acceptChildren;

		public bool acceptColonyPrisoners;

		public bool onlyAcceptColonists;

		public bool onlyAcceptHealthy;

		public bool permitShuttle;

		public bool allowSlaves;

		public float minAge;

		private bool autoload;

		public TransportShip shipParent;

		private CompRefuelable cachedCompRefuelable;

		private CompTransporter cachedCompTransporter;

		private List<CompTransporter> cachedTransporterList;

		private const int CheckAutoloadIntervalTicks = 120;

		private const int CheckUnloadPawnIntervalTicks = 600;

		private static readonly Texture2D AutoloadToggleTex = ContentFinder<Texture2D>.Get("UI/Commands/Autoload");

		private const float MinPilotingSkill = 0.1f;

		private static readonly List<ThingDefCount> tmpRequiredItemsWithoutDuplicates = new List<ThingDefCount>();

		private static List<Pawn> tmpAllowedPawns = new List<Pawn>();

		private static List<string> tmpRequiredLabels = new List<string>();

		private static List<ThingDefCount> tmpRequiredItems = new List<ThingDefCount>();

		private static List<Pawn> tmpRequiredPawns = new List<Pawn>();

		private static List<Pawn> tmpAllSendablePawns = new List<Pawn>();

		private static List<Thing> tmpAllSendableItems = new List<Thing>();

		private static List<Pawn> tmpRequiredPawnsPossibleToSend = new List<Pawn>();

		public bool IsPlayerShuttle
		{
			get
			{
				if (ModLister.OdysseyInstalled)
				{
					return Props.shipDef?.playerShuttle ?? false;
				}
				return false;
			}
		}

		public bool Autoload => autoload;

		public List<CompTransporter> TransportersInGroup => Transporter.TransportersInGroup(parent.Map);

		public CompProperties_Shuttle Props => (CompProperties_Shuttle)props;

		public bool ShowLoadingGizmos
		{
			get
			{
				if (IsPlayerShuttle)
				{
					return true;
				}
				if (shipParent != null && !shipParent.ShowGizmos)
				{
					return false;
				}
				if (parent.Faction != null)
				{
					return parent.Faction == Faction.OfPlayer;
				}
				return true;
			}
		}

		public CompTransporter Transporter => cachedCompTransporter ?? (cachedCompTransporter = parent.GetComp<CompTransporter>());

		private bool Autoloadable
		{
			get
			{
				if (IsPlayerShuttle)
				{
					return false;
				}
				if (cachedTransporterList == null)
				{
					cachedTransporterList = new List<CompTransporter> { Transporter };
				}
				foreach (Pawn item in TransporterUtility.AllSendablePawns(cachedTransporterList, parent.Map))
				{
					if (!IsRequired(item))
					{
						return false;
					}
				}
				foreach (Thing item2 in TransporterUtility.AllSendableItems(cachedTransporterList, parent.Map))
				{
					if (!IsRequired(item2))
					{
						return false;
					}
				}
				return true;
			}
		}

		private int ContainedColonistCount
		{
			get
			{
				int num = 0;
				ThingOwner innerContainer = Transporter.innerContainer;
				for (int i = 0; i < innerContainer.Count; i++)
				{
					if (innerContainer[i] is Pawn { IsFreeColonist: not false })
					{
						num++;
					}
				}
				return num;
			}
		}

		private IEnumerable<Pawn> ContainedPawns
		{
			get
			{
				ThingOwner things = Transporter.innerContainer;
				for (int i = 0; i < things.Count; i++)
				{
					if (things[i] is Pawn pawn)
					{
						yield return pawn;
					}
				}
			}
		}

		public bool AllRequiredThingsLoaded
		{
			get
			{
				ThingOwner innerContainer = Transporter.innerContainer;
				foreach (Pawn requiredPawn in RequiredPawns)
				{
					if (!innerContainer.Contains(requiredPawn))
					{
						return false;
					}
				}
				if (requireAllColonistsOnMap || requiredColonistCount > 0)
				{
					int containedColonistCount = ContainedColonistCount;
					int num = parent.Map.mapPawns.FreeColonistsCount;
					foreach (Map childPocketMap in parent.Map.ChildPocketMaps)
					{
						num += childPocketMap.mapPawns.FreeColonistsCount;
					}
					if (requireAllColonistsOnMap && containedColonistCount < num)
					{
						return false;
					}
					if (requiredColonistCount > 0 && containedColonistCount < requiredColonistCount)
					{
						return false;
					}
				}
				tmpRequiredItemsWithoutDuplicates.Clear();
				for (int i = 0; i < requiredItems.Count; i++)
				{
					bool flag = false;
					for (int j = 0; j < tmpRequiredItemsWithoutDuplicates.Count; j++)
					{
						if (tmpRequiredItemsWithoutDuplicates[j].ThingDef == requiredItems[i].ThingDef)
						{
							tmpRequiredItemsWithoutDuplicates[j] = tmpRequiredItemsWithoutDuplicates[j].WithCount(tmpRequiredItemsWithoutDuplicates[j].Count + requiredItems[i].Count);
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						tmpRequiredItemsWithoutDuplicates.Add(requiredItems[i]);
					}
				}
				for (int k = 0; k < tmpRequiredItemsWithoutDuplicates.Count; k++)
				{
					int num2 = 0;
					for (int l = 0; l < innerContainer.Count; l++)
					{
						if (innerContainer[l].def == tmpRequiredItemsWithoutDuplicates[k].ThingDef)
						{
							num2 += innerContainer[l].stackCount;
						}
					}
					if (num2 < tmpRequiredItemsWithoutDuplicates[k].Count)
					{
						return false;
					}
				}
				return true;
			}
		}

		public IEnumerable<Pawn> RequiredPawns
		{
			get
			{
				foreach (Pawn requiredPawn in requiredPawns)
				{
					if (!requiredPawn.Dead && ((!requiredPawn.Downed && requiredPawn.MapHeld == parent.MapHeld) || !pawnsToIgnoreIfDownedOfNotOnTheMap.Contains(requiredPawn)))
					{
						yield return requiredPawn;
					}
				}
			}
		}

		public TaggedString RequiredThingsLabel
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Pawn requiredPawn in RequiredPawns)
				{
					stringBuilder.AppendLine("  - " + requiredPawn.NameShortColored.Resolve());
				}
				for (int i = 0; i < requiredItems.Count; i++)
				{
					stringBuilder.AppendLine("  - " + requiredItems[i].LabelCap);
				}
				return stringBuilder.ToString().TrimEndNewlines();
			}
		}

		public bool HasPilot
		{
			get
			{
				ThingOwner innerContainer = Transporter.innerContainer;
				for (int i = 0; i < innerContainer.Count; i++)
				{
					if (innerContainer[i] is Pawn { IsFreeColonist: not false } pawn && !StatDefOf.PilotingAbility.Worker.IsDisabledFor(pawn) && pawn.GetStatValue(StatDefOf.PilotingAbility) > 0.1f)
					{
						return true;
					}
				}
				return false;
			}
		}

		public AcceptanceReport CanLaunch
		{
			get
			{
				TransportShipDef shipDef = Props.shipDef;
				if (shipDef != null && !shipDef.playerShuttle)
				{
					if (!Transporter.LoadingInProgressOrReadyToLaunch)
					{
						return "CommandLaunchGroupFailNotLoaded".Translate();
					}
					return true;
				}
				if (parent.Spawned && !HasPilot)
				{
					return "CommandShuttleNoPilot".Translate();
				}
				if (parent.Spawned)
				{
					WeatherDef curWeatherPerceived = parent.Map.weatherManager.CurWeatherPerceived;
					if (curWeatherPerceived.preventsShuttleLaunch)
					{
						return curWeatherPerceived.LabelCap;
					}
					foreach (GameCondition activeCondition in parent.Map.GameConditionManager.ActiveConditions)
					{
						if (activeCondition.def.preventShuttleLaunch)
						{
							return activeCondition.LabelCap;
						}
					}
				}
				return true;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!ModLister.CheckAnyExpansion("Shuttle"))
			{
				return;
			}
			if (!respawningAfterLoad && Props.shipDef != null)
			{
				TransportShipDef shipDef = Props.shipDef;
				if (shipDef != null && shipDef.playerShuttle)
				{
					acceptColonists = true;
					acceptChildren = true;
					acceptColonyPrisoners = true;
					allowSlaves = true;
				}
				if (shipParent == null)
				{
					shipParent = TransportShipMaker.MakeTransportShip(Props.shipDef, null, parent);
				}
			}
			base.PostSpawnSetup(respawningAfterLoad);
		}

		public void SetPawnToLeaveBehind(Func<Pawn, bool> predicate)
		{
			foreach (Pawn requiredPawn in requiredPawns)
			{
				if (predicate(requiredPawn))
				{
					pawnsToIgnoreIfDownedOfNotOnTheMap.AddUnique(requiredPawn);
				}
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (shipParent != null && shipParent.ShowGizmos)
			{
				IEnumerable<Gizmo> jobGizmos = shipParent.curJob.GetJobGizmos();
				if (jobGizmos != null)
				{
					foreach (Gizmo item2 in jobGizmos)
					{
						yield return item2;
					}
				}
			}
			if (ShowLoadingGizmos && Autoloadable)
			{
				Command_Toggle command_Toggle = new Command_Toggle();
				command_Toggle.defaultLabel = "CommandAutoloadTransporters".Translate();
				command_Toggle.defaultDesc = "CommandAutoloadTransportersDesc".Translate();
				command_Toggle.icon = AutoloadToggleTex;
				command_Toggle.isActive = () => autoload;
				command_Toggle.toggleAction = delegate
				{
					autoload = !autoload;
					if (autoload && !Transporter.LoadingInProgressOrReadyToLaunch)
					{
						TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
					}
					CheckAutoload();
				};
				yield return command_Toggle;
			}
			foreach (Gizmo questRelatedGizmo in QuestUtility.GetQuestRelatedGizmos(parent))
			{
				yield return questRelatedGizmo;
			}
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
			{
				yield break;
			}
			string text = "EnterShuttle".Translate();
			if (!IsAllowedNow(selPawn))
			{
				yield return new FloatMenuOption(text + " (" + "NotAllowed".Translate() + ")", null);
				yield break;
			}
			yield return new FloatMenuOption(text, delegate
			{
				if (!Transporter.LoadingInProgressOrReadyToLaunch)
				{
					TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
				}
				Job job = JobMaker.MakeJob(JobDefOf.EnterTransporter, parent);
				selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			});
		}

		public override IEnumerable<FloatMenuOption> CompMultiSelectFloatMenuOptions(IEnumerable<Pawn> selPawns)
		{
			tmpAllowedPawns.Clear();
			string text = "EnterShuttle".Translate();
			foreach (Pawn selPawn in selPawns)
			{
				if (selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
				{
					tmpAllowedPawns.Add(selPawn);
				}
			}
			if (!tmpAllowedPawns.Any())
			{
				yield return new FloatMenuOption(text + " (" + "NoPath".Translate() + ")", null);
				yield break;
			}
			for (int num = tmpAllowedPawns.Count - 1; num >= 0; num--)
			{
				if (!IsAllowedNow(tmpAllowedPawns[num]))
				{
					tmpAllowedPawns.RemoveAt(num);
				}
			}
			if (!tmpAllowedPawns.Any())
			{
				yield return new FloatMenuOption(text + " (" + "NotAllowed".Translate() + ")", null);
				yield break;
			}
			yield return new FloatMenuOption(text, delegate
			{
				if (!Transporter.LoadingInProgressOrReadyToLaunch)
				{
					TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
				}
				for (int i = 0; i < tmpAllowedPawns.Count; i++)
				{
					Pawn pawn = tmpAllowedPawns[i];
					if (pawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly) && !pawn.Downed && !pawn.Dead && pawn.Spawned)
					{
						Job job = JobMaker.MakeJob(JobDefOf.EnterTransporter, parent);
						tmpAllowedPawns[i].jobs.TryTakeOrderedJob(job, JobTag.Misc);
					}
				}
			});
		}

		public override void CompTick()
		{
			if (!parent.Spawned)
			{
				return;
			}
			if (parent.IsHashIntervalTick(120))
			{
				CheckAutoload();
			}
			if (parent.IsHashIntervalTick(600))
			{
				foreach (Pawn containedPawn in ContainedPawns)
				{
					if (containedPawn.TickDeSpawned > 0 && Find.TickManager.TicksGame - containedPawn.TickDeSpawned > 60000)
					{
						ShipJob_Unload.UnloadThingFromShuttle(shipParent, containedPawn);
						containedPawn.GetLord()?.Notify_PawnLost(containedPawn, PawnLostCondition.LeftVoluntarily);
					}
				}
			}
			TransportShipDef shipDef = Props.shipDef;
			if (shipDef != null && shipDef.playerShuttle && !Transporter.LoadingInProgressOrReadyToLaunch && ContainedColonistCount > 0)
			{
				TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
			}
		}

		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (shipParent != null && shipParent.curJob != null)
			{
				string text = shipParent.curJob.ShipThingExtraInspectString();
				if (!text.NullOrEmpty())
				{
					stringBuilder.Append(text);
				}
			}
			tmpRequiredLabels.Clear();
			if (requireAllColonistsOnMap)
			{
				int freeColonistsCount = parent.Map.mapPawns.FreeColonistsCount;
				tmpRequiredLabels.Add(freeColonistsCount + " " + ((freeColonistsCount > 1) ? Faction.OfPlayer.def.pawnsPlural : Faction.OfPlayer.def.pawnSingular));
			}
			else if (requiredColonistCount > 0)
			{
				tmpRequiredLabels.Add(requiredColonistCount + " " + ((requiredColonistCount > 1) ? Faction.OfPlayer.def.pawnsPlural : Faction.OfPlayer.def.pawnSingular));
			}
			foreach (Pawn requiredPawn in RequiredPawns)
			{
				if (!Transporter.innerContainer.Contains(requiredPawn))
				{
					tmpRequiredLabels.Add(requiredPawn.LabelShort);
				}
			}
			for (int i = 0; i < requiredItems.Count; i++)
			{
				if (Transporter.innerContainer.TotalStackCountOfDef(requiredItems[i].ThingDef) < requiredItems[i].Count)
				{
					tmpRequiredLabels.Add(requiredItems[i].Label);
				}
			}
			if (tmpRequiredLabels.Any())
			{
				stringBuilder.AppendInNewLine("Required".Translate() + ": " + tmpRequiredLabels.ToCommaList().CapitalizeFirst());
			}
			return stringBuilder.ToString();
		}

		public void SendLaunchedSignals()
		{
			List<CompTransporter> transportersInGroup = TransportersInGroup;
			List<Pawn> list = new List<Pawn>();
			for (int i = 0; i < transportersInGroup.Count; i++)
			{
				for (int j = 0; j < transportersInGroup[i].innerContainer.Count; j++)
				{
					if (transportersInGroup[i].innerContainer[j] is Pawn { IsColonist: not false } pawn && !requiredPawns.Contains(pawn))
					{
						list.Add(pawn);
					}
				}
			}
			if (list.Count != 0)
			{
				for (int k = 0; k < transportersInGroup.Count; k++)
				{
					QuestUtility.SendQuestTargetSignals(transportersInGroup[k].parent.questTags, "SentWithExtraColonists", transportersInGroup[k].parent.Named("SUBJECT"), list.Named("SENTCOLONISTS"));
				}
			}
			string signalPart = (AllRequiredThingsLoaded ? "SentSatisfied" : "SentUnsatisfied");
			for (int l = 0; l < transportersInGroup.Count; l++)
			{
				QuestUtility.SendQuestTargetSignals(transportersInGroup[l].parent.questTags, signalPart, transportersInGroup[l].parent.Named("SUBJECT"), transportersInGroup[l].innerContainer.ToList().Named("SENT"));
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref requiredItems, "requiredItems", LookMode.Deep);
			Scribe_Collections.Look(ref requiredPawns, "requiredPawns", LookMode.Reference);
			Scribe_Collections.Look(ref pawnsToIgnoreIfDownedOfNotOnTheMap, "pawnsToIgnoreIfDownedOfNotOnTheMap", LookMode.Reference);
			Scribe_Values.Look(ref requiredColonistCount, "requiredColonistCount", 0);
			Scribe_Values.Look(ref requireAllColonistsOnMap, "requireAllColonistsOnMap", defaultValue: false);
			Scribe_Values.Look(ref acceptColonists, "acceptColonists", defaultValue: false);
			Scribe_Values.Look(ref acceptChildren, "acceptChildren", defaultValue: true);
			Scribe_Values.Look(ref onlyAcceptColonists, "onlyAcceptColonists", defaultValue: false);
			Scribe_Values.Look(ref allowSlaves, "allowSlaves", defaultValue: false);
			Scribe_Values.Look(ref autoload, "autoload", defaultValue: false);
			Scribe_Values.Look(ref permitShuttle, "permitShuttle", defaultValue: false);
			Scribe_Values.Look(ref acceptColonyPrisoners, "acceptColonyPrisoners", defaultValue: false);
			Scribe_Values.Look(ref maxColonistCount, "maxColonistCount", -1);
			Scribe_References.Look(ref shipParent, "shipParent");
			Scribe_Values.Look(ref minAge, "minAge", 0f);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				requiredPawns.RemoveAll((Pawn x) => x == null);
			}
			if (maxColonistCount == 0)
			{
				maxColonistCount = -1;
			}
		}

		private void CheckAutoload()
		{
			if (!autoload || !Transporter.LoadingInProgressOrReadyToLaunch || !parent.Spawned)
			{
				return;
			}
			tmpRequiredItems.Clear();
			tmpRequiredItems.AddRange(requiredItems);
			tmpRequiredPawns.Clear();
			tmpRequiredPawns.AddRange(requiredPawns);
			ThingOwner innerContainer = Transporter.innerContainer;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				if (innerContainer[i] is Pawn item)
				{
					tmpRequiredPawns.Remove(item);
					continue;
				}
				int num = innerContainer[i].stackCount;
				for (int j = 0; j < tmpRequiredItems.Count; j++)
				{
					if (tmpRequiredItems[j].ThingDef == innerContainer[i].def)
					{
						int num2 = Mathf.Min(tmpRequiredItems[j].Count, num);
						if (num2 > 0)
						{
							tmpRequiredItems[j] = tmpRequiredItems[j].WithCount(tmpRequiredItems[j].Count - num2);
							num -= num2;
						}
					}
				}
			}
			for (int num3 = tmpRequiredItems.Count - 1; num3 >= 0; num3--)
			{
				if (tmpRequiredItems[num3].Count <= 0)
				{
					tmpRequiredItems.RemoveAt(num3);
				}
			}
			if (tmpRequiredItems.Any() || tmpRequiredPawns.Any())
			{
				if (Transporter.leftToLoad != null)
				{
					Transporter.leftToLoad.Clear();
				}
				tmpAllSendablePawns.Clear();
				tmpAllSendablePawns.AddRange(TransporterUtility.AllSendablePawns(TransportersInGroup, parent.Map));
				tmpAllSendableItems.Clear();
				tmpAllSendableItems.AddRange(TransporterUtility.AllSendableItems(TransportersInGroup, parent.Map));
				tmpAllSendableItems.AddRange(TransporterUtility.ThingsBeingHauledTo(TransportersInGroup, parent.Map));
				tmpRequiredPawnsPossibleToSend.Clear();
				for (int k = 0; k < tmpRequiredPawns.Count; k++)
				{
					if (tmpAllSendablePawns.Contains(tmpRequiredPawns[k]))
					{
						TransferableOneWay transferableOneWay = new TransferableOneWay();
						transferableOneWay.things.Add(tmpRequiredPawns[k]);
						Transporter.AddToTheToLoadList(transferableOneWay, 1);
						tmpRequiredPawnsPossibleToSend.Add(tmpRequiredPawns[k]);
					}
				}
				for (int l = 0; l < tmpRequiredItems.Count; l++)
				{
					if (tmpRequiredItems[l].Count <= 0)
					{
						continue;
					}
					int num4 = 0;
					for (int m = 0; m < tmpAllSendableItems.Count; m++)
					{
						if (tmpAllSendableItems[m].def == tmpRequiredItems[l].ThingDef)
						{
							num4 += tmpAllSendableItems[m].stackCount;
						}
					}
					if (num4 <= 0)
					{
						continue;
					}
					TransferableOneWay transferableOneWay2 = new TransferableOneWay();
					for (int n = 0; n < tmpAllSendableItems.Count; n++)
					{
						if (tmpAllSendableItems[n].def == tmpRequiredItems[l].ThingDef)
						{
							transferableOneWay2.things.Add(tmpAllSendableItems[n]);
						}
					}
					int count = Mathf.Min(tmpRequiredItems[l].Count, num4);
					Transporter.AddToTheToLoadList(transferableOneWay2, count);
				}
				TransporterUtility.MakeLordsAsAppropriate(tmpRequiredPawnsPossibleToSend, TransportersInGroup, parent.Map);
				tmpAllSendablePawns.Clear();
				tmpAllSendableItems.Clear();
				tmpRequiredItems.Clear();
				tmpRequiredPawns.Clear();
				tmpRequiredPawnsPossibleToSend.Clear();
			}
			else
			{
				if (Transporter.leftToLoad != null)
				{
					Transporter.leftToLoad.Clear();
				}
				TransporterUtility.MakeLordsAsAppropriate(tmpRequiredPawnsPossibleToSend, TransportersInGroup, parent.Map);
			}
		}

		public virtual bool IsRequired(Thing thing)
		{
			if (thing is Pawn item)
			{
				return requiredPawns.Contains(item);
			}
			for (int i = 0; i < requiredItems.Count; i++)
			{
				if (requiredItems[i].ThingDef == thing.def && requiredItems[i].Count != 0)
				{
					return true;
				}
			}
			return false;
		}

		public virtual bool IsAllowed(Thing t)
		{
			TransportShipDef shipDef = Props.shipDef;
			if (shipDef != null && shipDef.playerShuttle)
			{
				return true;
			}
			if (IsRequired(t))
			{
				return true;
			}
			if (acceptColonists && t is Pawn pawn && (pawn.IsColonist || (pawn.IsPrisonerOfColony && acceptColonyPrisoners) || (!onlyAcceptColonists && pawn.IsAnimal && pawn.Faction == Faction.OfPlayer)) && (!pawn.IsSlave || allowSlaves) && (!onlyAcceptColonists || !pawn.IsQuestLodger()) && (!onlyAcceptHealthy || PawnIsHealthyEnoughForShuttle(pawn)) && pawn.ageTracker.AgeBiologicalYearsFloat >= minAge)
			{
				return true;
			}
			if (!acceptChildren && t is Pawn pawn2 && pawn2.RaceProps.Humanlike && !pawn2.DevelopmentalStage.Adult())
			{
				return false;
			}
			if (permitShuttle)
			{
				if (!(t is Pawn pawn3))
				{
					return true;
				}
				if ((pawn3.Faction == Faction.OfPlayer && !pawn3.IsQuestLodger()) || pawn3.IsPrisonerOfColony)
				{
					return true;
				}
			}
			if (!(t is Pawn) && !parent.Map.IsPlayerHome && !GenHostility.AnyHostileActiveThreatToPlayer(parent.Map, countDormantPawnsAsHostile: true))
			{
				return true;
			}
			return false;
		}

		public virtual bool IsAllowedNow(Thing t)
		{
			if (!IsAllowed(t))
			{
				return false;
			}
			if (maxColonistCount == -1)
			{
				return true;
			}
			int num = 0;
			foreach (Thing item in (IEnumerable<Thing>)Transporter.innerContainer)
			{
				if (item != t && item is Pawn { IsColonist: not false })
				{
					num++;
				}
			}
			foreach (Pawn allPawn in parent.Map.mapPawns.AllPawns)
			{
				if (allPawn.jobs == null || allPawn.jobs.curDriver == null || allPawn == t)
				{
					continue;
				}
				foreach (QueuedJob item2 in allPawn.jobs.jobQueue)
				{
					if (CheckJob(item2.job))
					{
						num++;
					}
				}
				if (CheckJob(allPawn.jobs.curJob))
				{
					num++;
				}
			}
			return num < maxColonistCount;
			bool CheckJob(Job job)
			{
				if (typeof(JobDriver_EnterTransporter).IsAssignableFrom(job.def.driverClass) && job.GetTarget(TargetIndex.A).Thing == parent)
				{
					return true;
				}
				return false;
			}
		}

		private bool PawnIsHealthyEnoughForShuttle(Pawn p)
		{
			if (p.Downed || p.InMentalState || !p.health.capacities.CanBeAwake || !p.health.capacities.CapableOf(PawnCapacityDefOf.Moving) || !p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				return false;
			}
			return true;
		}

		public virtual void CleanUpLoadingVars()
		{
			autoload = false;
		}
	}
}
