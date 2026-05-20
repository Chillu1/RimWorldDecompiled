using System.Collections;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class IncidentQueue : IExposable
{
	private List<QueuedIncident> queuedIncidents = new List<QueuedIncident>();

	public int Count => queuedIncidents.Count;

	public string DebugQueueReadout
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (QueuedIncident queuedIncident in queuedIncidents)
			{
				stringBuilder.AppendLine(queuedIncident.ToString() + " (in " + (queuedIncident.FireTick - Find.TickManager.TicksGame) + " ticks)");
			}
			return stringBuilder.ToString();
		}
	}

	public IEnumerator GetEnumerator()
	{
		foreach (QueuedIncident queuedIncident in queuedIncidents)
		{
			yield return queuedIncident;
		}
	}

	public void Clear()
	{
		queuedIncidents.Clear();
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref queuedIncidents, "queuedIncidents", LookMode.Deep);
	}

	public bool Add(QueuedIncident qi)
	{
		queuedIncidents.Add(qi);
		queuedIncidents.Sort((QueuedIncident a, QueuedIncident b) => a.FireTick.CompareTo(b.FireTick));
		return true;
	}

	public bool Add(IncidentDef def, int fireTick, IncidentParms parms = null, int retryDurationTicks = 0)
	{
		QueuedIncident qi = new QueuedIncident(new FiringIncident(def, null, parms), fireTick, retryDurationTicks);
		Add(qi);
		return true;
	}

	public void IncidentQueueTick()
	{
		for (int num = queuedIncidents.Count - 1; num >= 0; num--)
		{
			QueuedIncident queuedIncident = queuedIncidents[num];
			if (!queuedIncident.TriedToFire)
			{
				if (queuedIncident.FireTick <= Find.TickManager.TicksGame)
				{
					bool num2 = Find.Storyteller.TryFire(queuedIncident.FiringIncident, queued: true);
					queuedIncident.Notify_TriedToFire();
					if (num2 || queuedIncident.RetryDurationTicks == 0)
					{
						queuedIncidents.Remove(queuedIncident);
					}
				}
			}
			else if (queuedIncident.FireTick + queuedIncident.RetryDurationTicks <= Find.TickManager.TicksGame)
			{
				queuedIncidents.Remove(queuedIncident);
			}
			else if (Find.TickManager.TicksGame % 833 == Rand.RangeSeeded(0, 833, queuedIncident.FireTick))
			{
				bool num3 = Find.Storyteller.TryFire(queuedIncident.FiringIncident, queued: true);
				queuedIncident.Notify_TriedToFire();
				if (num3)
				{
					queuedIncidents.Remove(queuedIncident);
				}
			}
		}
	}

	public void Notify_MapRemoved(Map map)
	{
		queuedIncidents.RemoveAll((QueuedIncident x) => x.FiringIncident.parms.target == map);
	}
}
