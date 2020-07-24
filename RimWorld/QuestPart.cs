using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class QuestPart : IExposable, ILoadReferenceable
	{
		public enum SignalListenMode
		{
			OngoingOnly,
			NotYetAcceptedOnly,
			OngoingOrNotYetAccepted,
			HistoricalOnly,
			Always
		}

		public Quest quest;

		public SignalListenMode signalListenMode;

		public string debugLabel;

		public virtual string DescriptionPart
		{
			get;
		}

		public int Index => quest.PartsListForReading.IndexOf(this);

		public virtual IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				yield break;
			}
		}

		public virtual string QuestSelectTargetsLabel => null;

		public virtual IEnumerable<GlobalTargetInfo> QuestSelectTargets
		{
			get
			{
				yield break;
			}
		}

		public virtual IEnumerable<Faction> InvolvedFactions
		{
			get
			{
				yield break;
			}
		}

		public virtual IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
		{
			get
			{
				yield break;
			}
		}

		public virtual bool IncreasesPopulation => false;

		public virtual bool RequiresAccepter => false;

		public virtual bool PreventsAutoAccept => RequiresAccepter;

		public virtual bool QuestPartReserves(Pawn p)
		{
			return false;
		}

		public virtual void Cleanup()
		{
		}

		public virtual void AssignDebugData()
		{
		}

		public virtual void PreQuestAccept()
		{
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref signalListenMode, "signalListenMode", SignalListenMode.OngoingOnly);
			Scribe_Values.Look(ref debugLabel, "debugLabel");
		}

		public virtual void Notify_QuestSignalReceived(Signal signal)
		{
		}

		public virtual void Notify_ThingsProduced(Pawn worker, List<Thing> things)
		{
		}

		public virtual void Notify_PlantHarvested(Pawn worker, Thing harvested)
		{
		}

		public virtual void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
		{
		}

		public virtual void Notify_PreCleanup()
		{
		}

		public virtual void PostQuestAdded()
		{
		}

		public virtual void ReplacePawnReferences(Pawn replace, Pawn with)
		{
		}

		public virtual void DoDebugWindowContents(Rect innerRect, ref float curY)
		{
		}

		public override string ToString()
		{
			string str = GetType().Name + " (index=" + Index;
			if (!debugLabel.NullOrEmpty())
			{
				str = str + ", debugLabel=" + debugLabel;
			}
			return str + ")";
		}

		public string GetUniqueLoadID()
		{
			return "QuestPart_" + quest.id + "_" + Index;
		}
	}
}
