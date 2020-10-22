using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
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

		public List<Thing> sendAwayIfAllDespawned;

		public Faction sendAwayIfAllPawnsLeftToLoadAreNotOfFaction;

		public Quest sendAwayIfQuestFinished;

		public int requiredColonistCount;

		public int maxColonistCount = -1;

		public bool acceptColonists;

		public bool onlyAcceptColonists;

		public bool onlyAcceptHealthy;

		public bool dropEverythingIfUnsatisfied;

		public bool dropNonRequiredIfUnsatisfied = true;

		public bool leaveImmediatelyWhenSatisfied;

		public bool dropEverythingOnArrival;

		public bool stayAfterDroppedEverythingOnArrival;

		public bool permitShuttle;

		public WorldObject missionShuttleTarget;

		public WorldObject missionShuttleHome;

		public bool hideControls;

		private bool autoload;

		public bool leaveASAP;

		public int leaveAfterTicks;

		private List<Thing> droppedOnArrival = new List<Thing>();

		private CompTransporter cachedCompTransporter;

		private List<CompTransporter> cachedTransporterList;

		private bool sending;

		private const int CheckAutoloadIntervalTicks = 120;

		private const int DropInterval = 60;

		private static readonly Texture2D AutoloadToggleTex = ContentFinder<Texture2D>.Get("UI/Commands/Autoload");

		private static readonly Texture2D SendCommandTex = CompLaunchable.LaunchCommandTex;

		public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment");

		public static readonly IntVec3 DropoffSpotOffset = IntVec3.South * 2;

		private static List<ThingDefCount> tmpRequiredItemsWithoutDuplicates = new List<ThingDefCount>();

		private static List<Pawn> tmpAllowedPawns = new List<Pawn>();

		private static List<string> tmpRequiredLabels = new List<string>();

		private static List<ThingDefCount> tmpRequiredItems = new List<ThingDefCount>();

		private static List<Pawn> tmpRequiredPawns = new List<Pawn>();

		private static List<Pawn> tmpAllSendablePawns = new List<Pawn>();

		private static List<Thing> tmpAllSendableItems = new List<Thing>();

		private static List<Pawn> tmpRequiredPawnsPossibleToSend = new List<Pawn>();

		public bool Autoload => autoload;

		public bool LoadingInProgressOrReadyToLaunch => Transporter.LoadingInProgressOrReadyToLaunch;

		public List<CompTransporter> TransportersInGroup => Transporter.TransportersInGroup(parent.Map);

		public bool CanAutoLoot
		{
			get
			{
				if ((permitShuttle || IsMissionShuttle) && !parent.Map.IsPlayerHome)
				{
					return !GenHostility.AnyHostileActiveThreatToPlayer(parent.Map, countDormantPawnsAsHostile: true);
				}
				return false;
			}
		}

		public bool ShowLoadingGizmos
		{
			get
			{
				if (hideControls)
				{
					return false;
				}
				if (parent.Faction == null || parent.Faction == Faction.OfPlayer)
				{
					return true;
				}
				return false;
			}
		}

		public CompTransporter Transporter
		{
			get
			{
				if (cachedCompTransporter == null)
				{
					cachedCompTransporter = parent.GetComp<CompTransporter>();
				}
				return cachedCompTransporter;
			}
		}

		public bool AnyInGroupIsUnderRoof
		{
			get
			{
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					if (transportersInGroup[i].parent.Position.Roofed(parent.Map))
					{
						return true;
					}
				}
				return false;
			}
		}

		private bool Autoloadable
		{
			get
			{
				if (cachedTransporterList == null)
				{
					cachedTransporterList = new List<CompTransporter>
					{
						Transporter
					};
				}
				foreach (Pawn item in TransporterUtility.AllSendablePawns_NewTmp(cachedTransporterList, parent.Map, autoLoot: false))
				{
					if (!IsRequired(item))
					{
						return false;
					}
				}
				foreach (Thing item2 in TransporterUtility.AllSendableItems_NewTmp(cachedTransporterList, parent.Map, autoLoot: false))
				{
					if (!IsRequired(item2))
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool AllRequiredThingsLoaded
		{
			get
			{
				ThingOwner innerContainer = Transporter.innerContainer;
				for (int i = 0; i < requiredPawns.Count; i++)
				{
					if (!requiredPawns[i].Dead && !innerContainer.Contains(requiredPawns[i]))
					{
						return false;
					}
				}
				if (requiredColonistCount > 0)
				{
					int num = 0;
					for (int j = 0; j < innerContainer.Count; j++)
					{
						Pawn pawn = innerContainer[j] as Pawn;
						if (pawn != null && pawn.IsFreeColonist)
						{
							num++;
						}
					}
					if (num < requiredColonistCount)
					{
						return false;
					}
				}
				tmpRequiredItemsWithoutDuplicates.Clear();
				for (int k = 0; k < requiredItems.Count; k++)
				{
					bool flag = false;
					for (int l = 0; l < tmpRequiredItemsWithoutDuplicates.Count; l++)
					{
						if (tmpRequiredItemsWithoutDuplicates[l].ThingDef == requiredItems[k].ThingDef)
						{
							tmpRequiredItemsWithoutDuplicates[l] = tmpRequiredItemsWithoutDuplicates[l].WithCount(tmpRequiredItemsWithoutDuplicates[l].Count + requiredItems[k].Count);
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						tmpRequiredItemsWithoutDuplicates.Add(requiredItems[k]);
					}
				}
				for (int m = 0; m < tmpRequiredItemsWithoutDuplicates.Count; m++)
				{
					int num2 = 0;
					for (int n = 0; n < innerContainer.Count; n++)
					{
						if (innerContainer[n].def == tmpRequiredItemsWithoutDuplicates[m].ThingDef)
						{
							num2 += innerContainer[n].stackCount;
						}
					}
					if (num2 < tmpRequiredItemsWithoutDuplicates[m].Count)
					{
						return false;
					}
				}
				return true;
			}
		}

		public TaggedString RequiredThingsLabel
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < requiredPawns.Count; i++)
				{
					if (!requiredPawns[i].Dead)
					{
						stringBuilder.AppendLine("  - " + requiredPawns[i].NameShortColored.Resolve());
					}
				}
				for (int j = 0; j < requiredItems.Count; j++)
				{
					stringBuilder.AppendLine("  - " + requiredItems[j].LabelCap);
				}
				return stringBuilder.ToString().TrimEndNewlines();
			}
		}

		public bool IsMissionShuttle
		{
			get
			{
				if (missionShuttleTarget == null)
				{
					return missionShuttleHome != null;
				}
				return true;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Shuttle is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 8811221);
			}
			else
			{
				base.PostSpawnSetup(respawningAfterLoad);
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (ShowLoadingGizmos)
			{
				if (Autoloadable)
				{
					Command_Toggle command_Toggle = new Command_Toggle();
					command_Toggle.defaultLabel = "CommandAutoloadTransporters".Translate();
					command_Toggle.defaultDesc = "CommandAutoloadTransportersDesc".Translate();
					command_Toggle.icon = AutoloadToggleTex;
					command_Toggle.isActive = () => autoload;
					command_Toggle.toggleAction = delegate
					{
						autoload = !autoload;
						if (autoload && !LoadingInProgressOrReadyToLaunch)
						{
							TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
						}
						CheckAutoload();
					};
					yield return command_Toggle;
				}
				if (!IsMissionShuttle)
				{
					Command_Action command_Action = new Command_Action();
					command_Action.defaultLabel = "CommandSendShuttle".Translate();
					command_Action.defaultDesc = "CommandSendShuttleDesc".Translate();
					command_Action.icon = SendCommandTex;
					command_Action.alsoClickIfOtherInGroupClicked = false;
					command_Action.action = delegate
					{
						Send();
					};
					if (!LoadingInProgressOrReadyToLaunch || !AllRequiredThingsLoaded)
					{
						command_Action.Disable("CommandSendShuttleFailMissingRequiredThing".Translate());
					}
					yield return command_Action;
				}
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
				if (!LoadingInProgressOrReadyToLaunch)
				{
					TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
				}
				Job job = JobMaker.MakeJob(JobDefOf.EnterTransporter, parent);
				selPawn.jobs.TryTakeOrderedJob(job);
			});
		}

		public override IEnumerable<FloatMenuOption> CompMultiSelectFloatMenuOptions(List<Pawn> selPawns)
		{
			tmpAllowedPawns.Clear();
			string text = "EnterShuttle".Translate();
			for (int i = 0; i < selPawns.Count; i++)
			{
				if (selPawns[i].CanReach(parent, PathEndMode.Touch, Danger.Deadly))
				{
					tmpAllowedPawns.Add(selPawns[i]);
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
				if (!LoadingInProgressOrReadyToLaunch)
				{
					TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
				}
				for (int j = 0; j < tmpAllowedPawns.Count; j++)
				{
					Pawn pawn = tmpAllowedPawns[j];
					if (pawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly) && !pawn.Downed && !pawn.Dead && pawn.Spawned)
					{
						Job job = JobMaker.MakeJob(JobDefOf.EnterTransporter, parent);
						tmpAllowedPawns[j].jobs.TryTakeOrderedJob(job);
					}
				}
			});
		}

		public override void CompTick()
		{
			base.CompTick();
			if (parent.IsHashIntervalTick(120))
			{
				CheckAutoload();
			}
			if (parent.Spawned && (dropEverythingOnArrival || (sendAwayIfQuestFinished != null && sendAwayIfQuestFinished.Historical)) && parent.IsHashIntervalTick(60))
			{
				OffloadShuttleOrSend();
			}
			if (leaveASAP && parent.Spawned)
			{
				if (!LoadingInProgressOrReadyToLaunch)
				{
					TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
				}
				Send();
			}
			if (leaveAfterTicks > 0 && parent.Spawned && !IsMissionShuttle)
			{
				leaveAfterTicks--;
				if (leaveAfterTicks == 0)
				{
					if (!LoadingInProgressOrReadyToLaunch)
					{
						TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
					}
					Send();
				}
			}
			Pawn pawn;
			if (!IsMissionShuttle && !dropEverythingOnArrival && ((leaveImmediatelyWhenSatisfied && AllRequiredThingsLoaded) || (!sendAwayIfAllDespawned.NullOrEmpty() && sendAwayIfAllDespawned.All((Thing p) => !p.Spawned && ((pawn = p as Pawn) == null || pawn.CarriedBy == null))) || (sendAwayIfAllPawnsLeftToLoadAreNotOfFaction != null && requiredPawns.All((Pawn p) => Transporter.innerContainer.Contains(p) || p.Faction != sendAwayIfAllPawnsLeftToLoadAreNotOfFaction))))
			{
				Send();
			}
		}

		private void OffloadShuttleOrSend()
		{
			Thing thingToDrop = null;
			float num = 0f;
			for (int i = 0; i < Transporter.innerContainer.Count; i++)
			{
				Thing thing = Transporter.innerContainer[i];
				float num2 = GetDropPriority(thing);
				if (num2 > num)
				{
					thingToDrop = thing;
					num = num2;
				}
			}
			if (thingToDrop != null)
			{
				IntVec3 dropLoc = parent.Position + DropoffSpotOffset;
				if (!Transporter.innerContainer.TryDrop_NewTmp(thingToDrop, dropLoc, parent.Map, ThingPlaceMode.Near, out var _, null, delegate(IntVec3 c)
				{
					if (c.Fogged(parent.Map))
					{
						return false;
					}
					Pawn pawn2;
					return ((pawn2 = thingToDrop as Pawn) == null || !pawn2.Downed || c.GetFirstPawn(parent.Map) == null) ? true : false;
				}, playDropSound: false))
				{
					return;
				}
				Transporter.Notify_ThingRemoved(thingToDrop);
				droppedOnArrival.Add(thingToDrop);
				Pawn pawn;
				if ((pawn = thingToDrop as Pawn) != null)
				{
					if (pawn.IsColonist && pawn.Spawned && !parent.Map.IsPlayerHome)
					{
						pawn.drafter.Drafted = true;
					}
					if (pawn.guest != null && pawn.guest.IsPrisoner)
					{
						pawn.guest.WaitInsteadOfEscapingForDefaultTicks();
					}
				}
			}
			else
			{
				if (!Transporter.LoadingInProgressOrReadyToLaunch)
				{
					TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
				}
				if (!stayAfterDroppedEverythingOnArrival || (sendAwayIfQuestFinished != null && sendAwayIfQuestFinished.Historical))
				{
					Send();
				}
				else
				{
					dropEverythingOnArrival = false;
				}
			}
			float GetDropPriority(Thing t)
			{
				if (droppedOnArrival.Contains(t))
				{
					return 0f;
				}
				Pawn p;
				if ((p = t as Pawn) != null)
				{
					Lord lord = p.GetLord();
					LordToil_EnterShuttleOrLeave lordToil_EnterShuttleOrLeave;
					if (lord?.CurLordToil != null && (lordToil_EnterShuttleOrLeave = lord.CurLordToil as LordToil_EnterShuttleOrLeave) != null && lordToil_EnterShuttleOrLeave.shuttle == parent)
					{
						return 0f;
					}
					LordToil_LoadAndEnterTransporters lordToil_LoadAndEnterTransporters;
					if (lord?.CurLordToil != null && (lordToil_LoadAndEnterTransporters = lord.CurLordToil as LordToil_LoadAndEnterTransporters) != null && lordToil_LoadAndEnterTransporters.transportersGroup == parent.TryGetComp<CompTransporter>().groupID)
					{
						return 0f;
					}
					if (!p.AnimalOrWildMan())
					{
						return 1f;
					}
					return 0.5f;
				}
				return 0.25f;
			}
		}

		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			tmpRequiredLabels.Clear();
			if (requiredColonistCount > 0)
			{
				tmpRequiredLabels.Add(requiredColonistCount + " " + ((requiredColonistCount > 1) ? Faction.OfPlayer.def.pawnsPlural : Faction.OfPlayer.def.pawnSingular));
			}
			for (int i = 0; i < requiredPawns.Count; i++)
			{
				if (!requiredPawns[i].Dead && !Transporter.innerContainer.Contains(requiredPawns[i]))
				{
					tmpRequiredLabels.Add(requiredPawns[i].LabelShort);
				}
			}
			for (int j = 0; j < requiredItems.Count; j++)
			{
				if (Transporter.innerContainer.TotalStackCountOfDef(requiredItems[j].ThingDef) < requiredItems[j].Count)
				{
					tmpRequiredLabels.Add(requiredItems[j].Label);
				}
			}
			if (tmpRequiredLabels.Any())
			{
				stringBuilder.Append("Required".Translate() + ": " + tmpRequiredLabels.ToCommaList().CapitalizeFirst());
			}
			return stringBuilder.ToString();
		}

		public void Send()
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Shuttle is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 8811221);
			}
			else
			{
				if (sending)
				{
					return;
				}
				if (!parent.Spawned)
				{
					Log.Error(string.Concat("Tried to send ", parent, ", but it's unspawned."));
					return;
				}
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				if (transportersInGroup == null)
				{
					Log.Error(string.Concat("Tried to send ", parent, ", but it's not in any group."));
				}
				else
				{
					if (!LoadingInProgressOrReadyToLaunch)
					{
						return;
					}
					if (!AllRequiredThingsLoaded)
					{
						if (dropEverythingIfUnsatisfied)
						{
							Transporter.CancelLoad();
						}
						else if (dropNonRequiredIfUnsatisfied)
						{
							for (int i = 0; i < transportersInGroup.Count; i++)
							{
								for (int num = transportersInGroup[i].innerContainer.Count - 1; num >= 0; num--)
								{
									Thing thing = transportersInGroup[i].innerContainer[num];
									Pawn pawn;
									if (!IsRequired(thing) && (requiredColonistCount <= 0 || (pawn = thing as Pawn) == null || !pawn.IsColonist))
									{
										transportersInGroup[i].innerContainer.TryDrop(thing, ThingPlaceMode.Near, out var _);
									}
								}
							}
						}
					}
					sending = true;
					Map map = parent.Map;
					Transporter.TryRemoveLord(map);
					SendLaunchedSignals(transportersInGroup);
					List<Pawn> list = new List<Pawn>();
					for (int j = 0; j < transportersInGroup.Count; j++)
					{
						CompTransporter compTransporter = transportersInGroup[j];
						for (int num2 = transportersInGroup[j].innerContainer.Count - 1; num2 >= 0; num2--)
						{
							Pawn pawn2 = transportersInGroup[j].innerContainer[num2] as Pawn;
							if (pawn2 != null)
							{
								if (pawn2.IsColonist && !requiredPawns.Contains(pawn2))
								{
									list.Add(pawn2);
								}
								pawn2.ExitMap(allowedToJoinOrCreateCaravan: false, Rot4.Invalid);
							}
						}
						compTransporter.innerContainer.ClearAndDestroyContentsOrPassToWorld();
						DropPodLeaving obj = (DropPodLeaving)ThingMaker.MakeThing(ThingDefOf.ShuttleLeaving);
						obj.createWorldObject = permitShuttle && compTransporter.innerContainer.Any();
						obj.worldObjectDef = WorldObjectDefOf.TravelingShuttle;
						compTransporter.CleanUpLoadingVars(map);
						compTransporter.parent.Destroy(DestroyMode.QuestLogic);
						GenSpawn.Spawn(obj, compTransporter.parent.Position, map);
					}
					if (list.Count != 0)
					{
						for (int k = 0; k < transportersInGroup.Count; k++)
						{
							QuestUtility.SendQuestTargetSignals(transportersInGroup[k].parent.questTags, "SentWithExtraColonists", transportersInGroup[k].parent.Named("SUBJECT"), list.Named("SENTCOLONISTS"));
						}
					}
					sending = false;
				}
			}
		}

		public void SendLaunchedSignals(List<CompTransporter> transporters)
		{
			string signalPart = (AllRequiredThingsLoaded ? "SentSatisfied" : "SentUnsatisfied");
			for (int i = 0; i < transporters.Count; i++)
			{
				QuestUtility.SendQuestTargetSignals(transporters[i].parent.questTags, signalPart, transporters[i].parent.Named("SUBJECT"), transporters[i].innerContainer.ToList().Named("SENT"));
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref requiredItems, "requiredItems", LookMode.Deep);
			Scribe_Collections.Look(ref requiredPawns, "requiredPawns", LookMode.Reference);
			Scribe_Collections.Look(ref sendAwayIfAllDespawned, "sendAwayIfAllDespawned", LookMode.Reference);
			Scribe_Collections.Look(ref droppedOnArrival, "droppedOnArrival", LookMode.Reference);
			Scribe_Values.Look(ref requiredColonistCount, "requiredColonistCount", 0);
			Scribe_Values.Look(ref acceptColonists, "acceptColonists", defaultValue: false);
			Scribe_Values.Look(ref onlyAcceptColonists, "onlyAcceptColonists", defaultValue: false);
			Scribe_Values.Look(ref leaveImmediatelyWhenSatisfied, "leaveImmediatelyWhenSatisfied", defaultValue: false);
			Scribe_Values.Look(ref autoload, "autoload", defaultValue: false);
			Scribe_Values.Look(ref dropEverythingIfUnsatisfied, "dropEverythingIfUnsatisfied", defaultValue: false);
			Scribe_Values.Look(ref dropNonRequiredIfUnsatisfied, "dropNonRequiredIfUnsatisfied", defaultValue: false);
			Scribe_Values.Look(ref leaveASAP, "leaveASAP", defaultValue: false);
			Scribe_Values.Look(ref leaveAfterTicks, "leaveAfterTicks", 0);
			Scribe_Values.Look(ref dropEverythingOnArrival, "dropEverythingOnArrival", defaultValue: false);
			Scribe_Values.Look(ref stayAfterDroppedEverythingOnArrival, "stayAfterDroppedEverythingOnArrival", defaultValue: false);
			Scribe_Values.Look(ref permitShuttle, "permitShuttle", defaultValue: false);
			Scribe_References.Look(ref missionShuttleTarget, "missionShuttleTarget");
			Scribe_References.Look(ref missionShuttleHome, "missionShuttleHome");
			Scribe_Values.Look(ref hideControls, "hideControls", defaultValue: false);
			Scribe_Values.Look(ref maxColonistCount, "maxColonistCount", -1);
			Scribe_References.Look(ref sendAwayIfAllPawnsLeftToLoadAreNotOfFaction, "sendAwayIfAllPawnsLeftToLoadAreNotOfFaction");
			Scribe_References.Look(ref sendAwayIfQuestFinished, "sendAwayIfQuestFinished");
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
			if (!autoload || !LoadingInProgressOrReadyToLaunch || !parent.Spawned)
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
				Pawn pawn = innerContainer[i] as Pawn;
				if (pawn != null)
				{
					tmpRequiredPawns.Remove(pawn);
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
				tmpAllSendablePawns.AddRange(TransporterUtility.AllSendablePawns_NewTmp(TransportersInGroup, parent.Map, autoLoot: false));
				tmpAllSendableItems.Clear();
				tmpAllSendableItems.AddRange(TransporterUtility.AllSendableItems_NewTmp(TransportersInGroup, parent.Map, autoLoot: false));
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

		public bool IsRequired(Thing thing)
		{
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				return requiredPawns.Contains(pawn);
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

		public bool IsAllowed(Thing t)
		{
			if (IsRequired(t))
			{
				return true;
			}
			if (acceptColonists)
			{
				Pawn pawn = t as Pawn;
				if (pawn != null && (pawn.IsColonist || (!onlyAcceptColonists && pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)) && (!onlyAcceptColonists || !pawn.IsQuestLodger()) && (!onlyAcceptHealthy || PawnIsHealthyEnoughForShuttle(pawn)))
				{
					return true;
				}
			}
			if (permitShuttle)
			{
				Pawn pawn2;
				if ((pawn2 = t as Pawn) == null)
				{
					return true;
				}
				if (pawn2.Faction == Faction.OfPlayer && !pawn2.IsQuestLodger())
				{
					return true;
				}
			}
			if (IsMissionShuttle && !(t is Pawn) && CanAutoLoot)
			{
				return true;
			}
			return false;
		}

		public bool IsAllowedNow(Thing t)
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
				if (item != t)
				{
					Pawn pawn = item as Pawn;
					if (pawn != null && pawn.IsColonist)
					{
						num++;
					}
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

		public void CleanUpLoadingVars()
		{
			autoload = false;
		}
	}
}
