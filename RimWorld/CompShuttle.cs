using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompShuttle : ThingComp
	{
		public List<ThingDefCount> requiredItems = new List<ThingDefCount>();

		public List<Pawn> requiredPawns = new List<Pawn>();

		public int requiredColonistCount;

		public bool acceptColonists;

		public bool onlyAcceptColonists;

		public bool onlyAcceptHealthy;

		public bool dropEverythingIfUnsatisfied;

		public bool dropNonRequiredIfUnsatisfied = true;

		public bool leaveImmediatelyWhenSatisfied;

		public bool dropEverythingOnArrival;

		private bool autoload;

		public bool leaveASAP;

		private CompTransporter cachedCompTransporter;

		private List<CompTransporter> cachedTransporterList;

		private bool sending;

		private const int CheckAutoloadIntervalTicks = 120;

		private const int DropInterval = 60;

		private static readonly Texture2D AutoloadToggleTex = ContentFinder<Texture2D>.Get("UI/Commands/Autoload");

		private static readonly Texture2D SendCommandTex = CompLaunchable.LaunchCommandTex;

		private static List<ThingDefCount> tmpRequiredItemsWithoutDuplicates = new List<ThingDefCount>();

		private static List<string> tmpRequiredLabels = new List<string>();

		private static List<ThingDefCount> tmpRequiredItems = new List<ThingDefCount>();

		private static List<Pawn> tmpRequiredPawns = new List<Pawn>();

		private static List<Pawn> tmpAllSendablePawns = new List<Pawn>();

		private static List<Thing> tmpAllSendableItems = new List<Thing>();

		private static List<Pawn> tmpRequiredPawnsPossibleToSend = new List<Pawn>();

		public bool Autoload => autoload;

		public bool LoadingInProgressOrReadyToLaunch => Transporter.LoadingInProgressOrReadyToLaunch;

		public List<CompTransporter> TransportersInGroup => Transporter.TransportersInGroup(parent.Map);

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

		public bool ShowLoadingGizmos
		{
			get
			{
				if (parent.Faction != null)
				{
					return parent.Faction == Faction.OfPlayer;
				}
				return true;
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

		public bool AllRequiredThingsLoaded
		{
			get
			{
				ThingOwner innerContainer = Transporter.innerContainer;
				for (int i = 0; i < requiredPawns.Count; i++)
				{
					if (!innerContainer.Contains(requiredPawns[i]))
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
					stringBuilder.AppendLine("  - " + requiredPawns[i].NameShortColored.Resolve());
				}
				for (int j = 0; j < requiredItems.Count; j++)
				{
					stringBuilder.AppendLine("  - " + requiredItems[j].LabelCap);
				}
				return stringBuilder.ToString().TrimEndNewlines();
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
					command_Toggle.isActive = (() => autoload);
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
			foreach (Gizmo questRelatedGizmo in QuestUtility.GetQuestRelatedGizmos(parent))
			{
				yield return questRelatedGizmo;
			}
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			CompShuttle compShuttle = this;
			Pawn selPawn2 = selPawn;
			if (!selPawn2.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
			{
				yield break;
			}
			string text = "EnterShuttle".Translate();
			if (!IsAllowed(selPawn2))
			{
				yield return new FloatMenuOption(text + " (" + "NotAllowed".Translate() + ")", null);
				yield break;
			}
			yield return new FloatMenuOption(text, delegate
			{
				if (!compShuttle.LoadingInProgressOrReadyToLaunch)
				{
					TransporterUtility.InitiateLoading(Gen.YieldSingle(compShuttle.Transporter));
				}
				Job job = JobMaker.MakeJob(JobDefOf.EnterTransporter, compShuttle.parent);
				selPawn2.jobs.TryTakeOrderedJob(job);
			});
		}

		public override void CompTick()
		{
			base.CompTick();
			if (parent.IsHashIntervalTick(120))
			{
				CheckAutoload();
			}
			if (parent.Spawned && dropEverythingOnArrival && parent.IsHashIntervalTick(60))
			{
				if (Transporter.innerContainer.Any())
				{
					Thing thing = Transporter.innerContainer[0];
					IntVec3 dropLoc = parent.Position + IntVec3.South;
					Pawn pawn;
					if (Transporter.innerContainer.TryDrop_NewTmp(thing, dropLoc, parent.Map, ThingPlaceMode.Near, out Thing _, null, (IntVec3 c) => ((pawn = (Transporter.innerContainer[0] as Pawn)) == null || !pawn.Downed || c.GetFirstPawn(parent.Map) == null) ? true : false, playDropSound: false))
					{
						Transporter.Notify_ThingRemoved(thing);
					}
				}
				else
				{
					TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
					Send();
				}
			}
			if (leaveASAP && parent.Spawned)
			{
				if (!LoadingInProgressOrReadyToLaunch)
				{
					TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
				}
				Send();
			}
			if (leaveImmediatelyWhenSatisfied && AllRequiredThingsLoaded)
			{
				Send();
			}
		}

		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Required".Translate() + ": ");
			tmpRequiredLabels.Clear();
			if (requiredColonistCount > 0)
			{
				tmpRequiredLabels.Add(requiredColonistCount + " " + ((requiredColonistCount > 1) ? Faction.OfPlayer.def.pawnsPlural : Faction.OfPlayer.def.pawnSingular));
			}
			for (int i = 0; i < requiredPawns.Count; i++)
			{
				tmpRequiredLabels.Add(requiredPawns[i].LabelShort);
			}
			for (int j = 0; j < requiredItems.Count; j++)
			{
				tmpRequiredLabels.Add(requiredItems[j].Label);
			}
			if (tmpRequiredLabels.Any())
			{
				stringBuilder.Append(tmpRequiredLabels.ToCommaList(useAnd: true).CapitalizeFirst());
			}
			else
			{
				stringBuilder.Append("Nothing".Translate());
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
									if (!IsRequired(thing) && (requiredColonistCount <= 0 || (pawn = (thing as Pawn)) == null || !pawn.IsColonist))
									{
										transportersInGroup[i].innerContainer.TryDrop(thing, ThingPlaceMode.Near, out Thing _);
									}
								}
							}
						}
					}
					sending = true;
					bool allRequiredThingsLoaded = AllRequiredThingsLoaded;
					Map map = parent.Map;
					Transporter.TryRemoveLord(map);
					string signalPart = allRequiredThingsLoaded ? "SentSatisfied" : "SentUnsatisfied";
					for (int j = 0; j < transportersInGroup.Count; j++)
					{
						QuestUtility.SendQuestTargetSignals(transportersInGroup[j].parent.questTags, signalPart, transportersInGroup[j].parent.Named("SUBJECT"), transportersInGroup[j].innerContainer.ToList().Named("SENT"));
					}
					List<Pawn> list = new List<Pawn>();
					for (int k = 0; k < transportersInGroup.Count; k++)
					{
						CompTransporter compTransporter = transportersInGroup[k];
						for (int num2 = transportersInGroup[k].innerContainer.Count - 1; num2 >= 0; num2--)
						{
							Pawn pawn2 = transportersInGroup[k].innerContainer[num2] as Pawn;
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
						Thing newThing = ThingMaker.MakeThing(ThingDefOf.ShuttleLeaving);
						compTransporter.CleanUpLoadingVars(map);
						compTransporter.parent.Destroy(DestroyMode.QuestLogic);
						GenSpawn.Spawn(newThing, compTransporter.parent.Position, map);
					}
					if (list.Count != 0)
					{
						for (int l = 0; l < transportersInGroup.Count; l++)
						{
							QuestUtility.SendQuestTargetSignals(transportersInGroup[l].parent.questTags, "SentWithExtraColonists", transportersInGroup[l].parent.Named("SUBJECT"), list.Named("SENTCOLONISTS"));
						}
					}
					sending = false;
				}
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref requiredItems, "requiredItems", LookMode.Deep);
			Scribe_Collections.Look(ref requiredPawns, "requiredPawns", LookMode.Reference);
			Scribe_Values.Look(ref requiredColonistCount, "requiredColonistCount", 0);
			Scribe_Values.Look(ref acceptColonists, "acceptColonists", defaultValue: false);
			Scribe_Values.Look(ref onlyAcceptColonists, "onlyAcceptColonists", defaultValue: false);
			Scribe_Values.Look(ref leaveImmediatelyWhenSatisfied, "leaveImmediatelyWhenSatisfied", defaultValue: false);
			Scribe_Values.Look(ref autoload, "autoload", defaultValue: false);
			Scribe_Values.Look(ref dropEverythingIfUnsatisfied, "dropEverythingIfUnsatisfied", defaultValue: false);
			Scribe_Values.Look(ref dropNonRequiredIfUnsatisfied, "dropNonRequiredIfUnsatisfied", defaultValue: false);
			Scribe_Values.Look(ref leaveASAP, "leaveASAP", defaultValue: false);
			Scribe_Values.Look(ref dropEverythingOnArrival, "dropEverythingOnArrival", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				requiredPawns.RemoveAll((Pawn x) => x == null);
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
			return false;
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
