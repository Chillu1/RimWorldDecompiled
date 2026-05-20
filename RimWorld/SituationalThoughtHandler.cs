using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public sealed class SituationalThoughtHandler
{
	private class CachedSocialThoughts
	{
		public readonly List<Thought_SituationalSocial> thoughts = new List<Thought_SituationalSocial>();

		public readonly List<Thought_SituationalSocial> activeThoughts = new List<Thought_SituationalSocial>();

		public int lastRecalculationTick = -99999;

		public int lastQueryTick = -99999;

		private const int ExpireAfterTicks = 300;

		public bool Expired => Find.TickManager.TicksGame - lastQueryTick >= 300;

		public bool ShouldRecalculateState => Find.TickManager.TicksGame - lastRecalculationTick >= 100;
	}

	public Pawn pawn;

	private readonly List<Thought_Situational> cachedThoughts = new List<Thought_Situational>();

	private readonly Dictionary<Pawn, CachedSocialThoughts> cachedSocialThoughts = new Dictionary<Pawn, CachedSocialThoughts>();

	private bool thoughtsDirty = true;

	private const int RecalculateStateEveryTicks = 100;

	private readonly HashSet<ThoughtDef> tmpCachedSocialThoughts = new HashSet<ThoughtDef>();

	public SituationalThoughtHandler(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void SituationalThoughtInterval()
	{
		UpdateAllMoodThoughts();
		RemoveExpiredThoughtsFromCache();
	}

	public void AppendMoodThoughts(List<Thought> outThoughts)
	{
		if (thoughtsDirty)
		{
			UpdateAllMoodThoughts();
		}
		foreach (Thought_Situational cachedThought in cachedThoughts)
		{
			if (cachedThought.Active)
			{
				outThoughts.Add(cachedThought);
			}
		}
	}

	public void AppendMoodThoughts(ThoughtDef def, List<Thought> thoughts)
	{
		bool flag = false;
		foreach (Thought_Situational cachedThought in cachedThoughts)
		{
			if (cachedThought.def == def && cachedThought.Active)
			{
				cachedThought.RecalculateState();
				if (cachedThought.Active)
				{
					flag = true;
					thoughts.Add(cachedThought);
				}
			}
		}
		if (!flag)
		{
			Thought_Situational thought_Situational = TryCreateThought(def);
			if (thought_Situational != null)
			{
				cachedThoughts.Add(thought_Situational);
				thoughts.Add(thought_Situational);
			}
		}
	}

	public void AppendSocialThoughts(Pawn otherPawn, List<ISocialThought> outThoughts)
	{
		CheckRecalculateSocialThoughts(otherPawn);
		CachedSocialThoughts obj = cachedSocialThoughts[otherPawn];
		obj.lastQueryTick = Find.TickManager.TicksGame;
		List<Thought_SituationalSocial> activeThoughts = obj.activeThoughts;
		for (int i = 0; i < activeThoughts.Count; i++)
		{
			outThoughts.Add(activeThoughts[i]);
		}
	}

	private void UpdateAllMoodThoughts()
	{
		thoughtsDirty = false;
		List<ThoughtDef> situationalNonSocialThoughtDefs = ThoughtUtility.situationalNonSocialThoughtDefs;
		foreach (Thought_Situational cachedThought in cachedThoughts)
		{
			cachedThought.RecalculateState();
		}
		foreach (ThoughtDef item in situationalNonSocialThoughtDefs)
		{
			bool flag = false;
			foreach (Thought_Situational cachedThought2 in cachedThoughts)
			{
				if (cachedThought2.def == item)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				Thought_Situational thought_Situational = TryCreateThought(item);
				if (thought_Situational != null)
				{
					cachedThoughts.Add(thought_Situational);
				}
			}
		}
		if (!ModsConfig.IdeologyActive || pawn.Ideo == null)
		{
			return;
		}
		foreach (Precept item2 in pawn.Ideo.PreceptsListForReading)
		{
			foreach (Thought_Situational item3 in item2.SituationThoughtsToAdd(pawn, cachedThoughts))
			{
				cachedThoughts.Add(item3);
			}
		}
	}

	private void CheckRecalculateSocialThoughts(Pawn otherPawn)
	{
		try
		{
			if (!cachedSocialThoughts.TryGetValue(otherPawn, out var value))
			{
				value = new CachedSocialThoughts();
				cachedSocialThoughts.Add(otherPawn, value);
			}
			if (!value.ShouldRecalculateState)
			{
				return;
			}
			value.lastRecalculationTick = Find.TickManager.TicksGame;
			tmpCachedSocialThoughts.Clear();
			for (int i = 0; i < value.thoughts.Count; i++)
			{
				Thought_SituationalSocial thought_SituationalSocial = value.thoughts[i];
				thought_SituationalSocial.RecalculateState();
				tmpCachedSocialThoughts.Add(thought_SituationalSocial.def);
			}
			List<ThoughtDef> situationalSocialThoughtDefs = ThoughtUtility.situationalSocialThoughtDefs;
			int j = 0;
			for (int count = situationalSocialThoughtDefs.Count; j < count; j++)
			{
				if (!tmpCachedSocialThoughts.Contains(situationalSocialThoughtDefs[j]))
				{
					Thought_SituationalSocial thought_SituationalSocial2 = TryCreateSocialThought(situationalSocialThoughtDefs[j], otherPawn);
					if (thought_SituationalSocial2 != null)
					{
						value.thoughts.Add(thought_SituationalSocial2);
					}
				}
			}
			value.activeThoughts.Clear();
			for (int k = 0; k < value.thoughts.Count; k++)
			{
				Thought_SituationalSocial thought_SituationalSocial3 = value.thoughts[k];
				if (thought_SituationalSocial3.Active)
				{
					value.activeThoughts.Add(thought_SituationalSocial3);
				}
			}
		}
		catch (Exception arg)
		{
			Log.Error($"Exception when recalculating social thoughts for pawn {pawn}: {arg}");
		}
		finally
		{
		}
	}

	private Thought_Situational TryCreateThought(ThoughtDef def)
	{
		Thought_Situational thought_Situational = null;
		try
		{
			if (!ThoughtUtility.CanGetThought(pawn, def))
			{
				return null;
			}
			if (!def.Worker.CurrentState(pawn).ActiveFor(def))
			{
				return null;
			}
			thought_Situational = (Thought_Situational)ThoughtMaker.MakeThought(def);
			thought_Situational.pawn = pawn;
			if (def.Worker is ThoughtWorker_Precept)
			{
				thought_Situational.sourcePrecept = pawn.Ideo.GetFirstPreceptAllowingSituationalThought(def);
			}
			thought_Situational.RecalculateState();
		}
		catch (Exception ex)
		{
			Log.Error("Exception while recalculating " + def?.ToString() + " thought state for pawn " + pawn?.ToString() + ": " + ex);
		}
		return thought_Situational;
	}

	private Thought_SituationalSocial TryCreateSocialThought(ThoughtDef def, Pawn otherPawn)
	{
		Thought_SituationalSocial thought_SituationalSocial = null;
		try
		{
			if (!ThoughtUtility.CanGetThought(pawn, def))
			{
				return null;
			}
			if (!def.Worker.CurrentSocialState(pawn, otherPawn).ActiveFor(def))
			{
				return null;
			}
			if (!def.socialTargetDevelopmentalStageFilter.HasAny(otherPawn.DevelopmentalStage))
			{
				return null;
			}
			if (def.ignoreSubhumans && otherPawn.IsSubhuman)
			{
				return null;
			}
			thought_SituationalSocial = (Thought_SituationalSocial)ThoughtMaker.MakeThought(def);
			thought_SituationalSocial.pawn = pawn;
			thought_SituationalSocial.otherPawn = otherPawn;
			if (def.Worker is ThoughtWorker_Precept_Social)
			{
				thought_SituationalSocial.sourcePrecept = pawn.Ideo.GetFirstPreceptAllowingSituationalThought(def);
			}
			thought_SituationalSocial.RecalculateState();
		}
		catch (Exception ex)
		{
			Log.Error("Exception while recalculating " + def?.ToString() + " thought state for pawn " + pawn?.ToString() + ": " + ex);
		}
		return thought_SituationalSocial;
	}

	public void Notify_SituationalThoughtsDirty()
	{
		cachedThoughts.Clear();
		cachedSocialThoughts.Clear();
		thoughtsDirty = true;
	}

	private void RemoveExpiredThoughtsFromCache()
	{
		cachedSocialThoughts.RemoveAll((KeyValuePair<Pawn, CachedSocialThoughts> x) => x.Value.Expired || x.Key.Discarded);
	}
}
