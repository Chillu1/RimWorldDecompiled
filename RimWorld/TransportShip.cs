using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class TransportShip : IExposable, ILoadReferenceable
	{
		public int loadID = -1;

		public TransportShipDef def;

		public ShipJob curJob;

		public Thing shipThing;

		private List<ShipJob> shipJobs = new List<ShipJob>();

		public List<string> questTags = new List<string>();

		public bool started;

		private bool disposed;

		private bool shipThingSpawnedOrHasOwner;

		[Unsaved(false)]
		private CompShuttle cachedCompShuttle;

		[Unsaved(false)]
		private CompTransporter cachedCompTransporter;

		public bool ShipExistsAndIsSpawned
		{
			get
			{
				if (shipThing != null)
				{
					return shipThing.Spawned;
				}
				return false;
			}
		}

		public bool Disposed => disposed;

		public bool Waiting
		{
			get
			{
				if (ShipExistsAndIsSpawned)
				{
					if (curJob != null)
					{
						return typeof(ShipJob_Wait).IsAssignableFrom(curJob.GetType());
					}
					return true;
				}
				return false;
			}
		}

		public bool ShowGizmos
		{
			get
			{
				if (curJob != null)
				{
					return curJob.ShowGizmos;
				}
				return false;
			}
		}

		private bool CanDispose
		{
			get
			{
				if (started && curJob == null)
				{
					Thing thing = shipThing;
					if (thing == null || !thing.Spawned)
					{
						thing = shipThing;
						if (thing == null || !(thing.ParentHolder is ActiveTransporterInfo))
						{
							Thing thing2 = shipThing;
							if ((thing2 == null || !thing2.IsInCaravan()) && !shipJobs.Any((ShipJob x) => x.def.blocksDisposalIfQueuedUnspawned) && !Find.World.worldObjects.TravellingTransporters.Any((TravellingTransporters x) => x.arrivalAction is TransportersArrivalAction_TransportShip transportersArrivalAction_TransportShip && transportersArrivalAction_TransportShip.transportShip == this) && !Find.QuestManager.IsReservedByAnyQuest(this))
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		public CompTransporter TransporterComp
		{
			get
			{
				if (cachedCompTransporter == null)
				{
					cachedCompTransporter = shipThing.TryGetComp<CompTransporter>();
				}
				return cachedCompTransporter;
			}
		}

		public CompShuttle ShuttleComp
		{
			get
			{
				if (cachedCompShuttle == null)
				{
					cachedCompShuttle = shipThing.TryGetComp<CompShuttle>();
				}
				return cachedCompShuttle;
			}
		}

		public bool LeavingSoonAutomatically
		{
			get
			{
				if (curJob != null && curJob.def == ShipJobDefOf.FlyAway)
				{
					return true;
				}
				foreach (ShipJob shipJob in shipJobs)
				{
					if (shipJob.Interruptible)
					{
						return false;
					}
					if (shipJob.def == ShipJobDefOf.FlyAway)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasPredeterminedDestination
		{
			get
			{
				if (curJob != null && curJob.HasDestination)
				{
					return true;
				}
				foreach (ShipJob shipJob in shipJobs)
				{
					if (shipJob.HasDestination)
					{
						return true;
					}
				}
				return false;
			}
		}

		public TransportShip()
		{
		}

		public TransportShip(TransportShipDef def)
		{
			this.def = def;
			loadID = Find.UniqueIDsManager.GetNextTransportShipID();
			Find.TransportShipManager.RegisterShipObject(this);
		}

		public void Start()
		{
			if (!started)
			{
				started = true;
				TryGetNextJob();
			}
		}

		public void Tick()
		{
			if (started)
			{
				if (curJob == null || curJob.jobState == ShipJobState.Ended)
				{
					TryGetNextJob();
				}
				if (curJob != null)
				{
					curJob.TickInterval(1);
				}
				else if (CanDispose)
				{
					Dispose();
				}
			}
		}

		public void ForceJob(ShipJob shipJob)
		{
			if (curJob != null && curJob.Interruptible)
			{
				EndCurrentJob();
			}
			shipJobs.Clear();
			AddJob(shipJob);
			TryGetNextJob();
		}

		public void ForceJob(ShipJobDef def)
		{
			ForceJob(ShipJobMaker.MakeShipJob(def));
		}

		public void SetNextJob(ShipJob shipJob)
		{
			if (Disposed)
			{
				Log.Error("Trying to add a job to a disposed transport ship. id=" + GetUniqueLoadID());
				return;
			}
			shipJob.transportShip = this;
			shipJobs.Insert(0, shipJob);
		}

		public void ForceJob_DelayCurrent(ShipJob shipJob)
		{
			if (curJob != null)
			{
				shipJobs.Insert(0, curJob);
				curJob = null;
			}
			if (!started)
			{
				Start();
			}
			SetNextJob(shipJob);
			TryGetNextJob();
		}

		public void AddJob(ShipJob shipJob)
		{
			if (Disposed)
			{
				Log.Error("Trying to add a job to a disposed transport ship. id=" + GetUniqueLoadID());
				return;
			}
			shipJob.transportShip = this;
			shipJobs.Add(shipJob);
		}

		public void AddJob(ShipJobDef def)
		{
			AddJob(ShipJobMaker.MakeShipJob(def));
		}

		public void AddJobs(params ShipJobDef[] defs)
		{
			if (defs != null)
			{
				for (int i = 0; i < defs.Length; i++)
				{
					AddJob(defs[i]);
				}
			}
		}

		public void EndCurrentJob()
		{
			if (curJob != null)
			{
				curJob.End();
			}
			curJob = null;
		}

		public void TryGetNextJob()
		{
			EndCurrentJob();
			if (!shipJobs.NullOrEmpty())
			{
				ShipJob shipJob = shipJobs.First();
				if (shipJob.TryStart())
				{
					curJob = shipJob;
					shipJobs.Remove(shipJob);
				}
			}
		}

		public void RemoveJob(ShipJob job)
		{
			if (curJob == job)
			{
				EndCurrentJob();
			}
			else if (shipJobs.Contains(job))
			{
				shipJobs.Remove(job);
			}
		}

		public void ArriveAt(IntVec3 cell, MapParent mapParent)
		{
			if (curJob != null && curJob.Interruptible)
			{
				EndCurrentJob();
			}
			ShipJob_Arrive shipJob_Arrive = (ShipJob_Arrive)ShipJobMaker.MakeShipJob(ShipJobDefOf.Arrive);
			shipJob_Arrive.cell = cell;
			shipJob_Arrive.mapParent = mapParent;
			SetNextJob(shipJob_Arrive);
			Start();
			if (curJob == null || curJob.Interruptible)
			{
				TryGetNextJob();
			}
		}

		public void Dispose()
		{
			if (def.playerShuttle)
			{
				shipThing.TryGetComp<CompShuttle>().shipParent = null;
			}
			else
			{
				Thing thing = shipThing;
				if (thing != null && !thing.Destroyed)
				{
					shipThing.Destroy(DestroyMode.QuestLogic);
				}
			}
			QuestUtility.SendQuestTargetSignals(questTags, "Disposed", this.Named("SUBJECT"));
			Find.TransportShipManager.DeregisterShipObject(this);
		}

		public void LogJobs()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Jobs for " + GetUniqueLoadID() + ":");
			if (curJob == null)
			{
				stringBuilder.AppendLine("  - CurJob: null");
			}
			else
			{
				stringBuilder.AppendLine("  - CurJob: " + curJob.GetJobInfo());
			}
			foreach (ShipJob shipJob in shipJobs)
			{
				stringBuilder.AppendLine("  - " + shipJob.GetJobInfo());
			}
			Log.Message(stringBuilder.ToString());
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref loadID, "loadID", 0);
			Scribe_Values.Look(ref disposed, "disposed", defaultValue: false);
			Scribe_Values.Look(ref started, "started", defaultValue: false);
			Scribe_Defs.Look(ref def, "def");
			Scribe_Deep.Look(ref curJob, "curJob");
			Scribe_Collections.Look(ref shipJobs, "shipJobs", LookMode.Deep);
			Scribe_Collections.Look(ref questTags, "questTags", LookMode.Value);
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (shipThing != null && shipThing.Destroyed)
				{
					shipThing = null;
				}
				shipThingSpawnedOrHasOwner = shipThing != null && (shipThing.SpawnedOrAnyParentSpawned || shipThing.ParentHolder != null);
			}
			Scribe_Values.Look(ref shipThingSpawnedOrHasOwner, "shipThingSpawned", defaultValue: false);
			if (shipThingSpawnedOrHasOwner)
			{
				Scribe_References.Look(ref shipThing, "shipThing");
			}
			else
			{
				Scribe_Deep.Look(ref shipThing, "shipThing");
			}
		}

		public string GetUniqueLoadID()
		{
			return "TransportShip_" + loadID;
		}
	}
}
