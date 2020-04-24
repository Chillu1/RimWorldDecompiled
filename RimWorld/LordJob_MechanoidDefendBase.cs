using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class LordJob_MechanoidDefendBase : LordJob
	{
		public List<Thing> things = new List<Thing>();

		protected List<Thing> thingsToNotifyOnDefeat = new List<Thing>();

		protected IntVec3 defSpot;

		protected Faction faction;

		protected float defendRadius;

		protected bool canAssaultColony;

		protected bool isMechCluster;

		protected bool mechClusterDefeated;

		public override void LordJobTick()
		{
			base.LordJobTick();
			if (isMechCluster && !mechClusterDefeated && !MechClusterUtility.AnyThreatBuilding(things))
			{
				OnDefeat();
			}
		}

		public override void Notify_LordDestroyed()
		{
			if (isMechCluster && !mechClusterDefeated)
			{
				OnDefeat();
			}
		}

		private void OnDefeat()
		{
			foreach (Thing thing in things)
			{
				thing.SetFaction(null);
				thing.TryGetComp<CompSendSignalOnPawnProximity>()?.Expire();
				CompSendSignalOnCountdown compSendSignalOnCountdown = thing.TryGetComp<CompSendSignalOnCountdown>();
				if (compSendSignalOnCountdown != null)
				{
					compSendSignalOnCountdown.ticksLeft = 0;
				}
				ThingWithComps thingWithComps;
				if ((thingWithComps = (thing as ThingWithComps)) != null)
				{
					thingWithComps.BroadcastCompSignal("MechClusterDefeated");
				}
			}
			lord.Notify_MechClusterDefeated();
			for (int i = 0; i < thingsToNotifyOnDefeat.Count; i++)
			{
				thingsToNotifyOnDefeat[i].Notify_LordDestroyed();
			}
			mechClusterDefeated = true;
			QuestUtility.SendQuestTargetSignals(lord.questTags, "AllEnemiesDefeated");
			Messages.Message("MessageMechClusterDefeated".Translate(), new LookTargets(defSpot, base.Map), MessageTypeDefOf.PositiveEvent);
		}

		public void AddThingToNotifyOnDefeat(Thing t)
		{
			thingsToNotifyOnDefeat.AddDistinct(t);
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref defSpot, "defSpot");
			Scribe_References.Look(ref faction, "faction");
			Scribe_Values.Look(ref defendRadius, "defendRadius", 0f);
			Scribe_Values.Look(ref canAssaultColony, "canAssaultColony", defaultValue: false);
			Scribe_Collections.Look(ref things, "things", LookMode.Reference);
			Scribe_Collections.Look(ref thingsToNotifyOnDefeat, "thingsToNotifyOnDefeat", LookMode.Reference);
			Scribe_Values.Look(ref isMechCluster, "isMechCluster", defaultValue: false);
			Scribe_Values.Look(ref mechClusterDefeated, "mechClusterDefeated", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				things.RemoveAll((Thing x) => x.DestroyedOrNull());
				thingsToNotifyOnDefeat.RemoveAll((Thing x) => x.DestroyedOrNull());
			}
		}
	}
}
