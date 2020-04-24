using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class SituationalThoughtHandler
	{
		private class CachedSocialThoughts
		{
			public List<Thought_SituationalSocial> thoughts = new List<Thought_SituationalSocial>();

			public List<Thought_SituationalSocial> activeThoughts = new List<Thought_SituationalSocial>();

			public int lastRecalculationTick = -99999;

			public int lastQueryTick = -99999;

			private const int ExpireAfterTicks = 300;

			public bool Expired => Find.TickManager.TicksGame - lastQueryTick >= 300;

			public bool ShouldRecalculateState => Find.TickManager.TicksGame - lastRecalculationTick >= 100;
		}

		public Pawn pawn;

		private List<Thought_Situational> cachedThoughts = new List<Thought_Situational>();

		private int lastMoodThoughtsRecalculationTick = -99999;

		private Dictionary<Pawn, CachedSocialThoughts> cachedSocialThoughts = new Dictionary<Pawn, CachedSocialThoughts>();

		private const int RecalculateStateEveryTicks = 100;

		private HashSet<ThoughtDef> tmpCachedThoughts = new HashSet<ThoughtDef>();

		private HashSet<ThoughtDef> tmpCachedSocialThoughts = new HashSet<ThoughtDef>();

		public SituationalThoughtHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void SituationalThoughtInterval()
		{
			RemoveExpiredThoughtsFromCache();
		}

		public void AppendMoodThoughts(List<Thought> outThoughts)
		{
			CheckRecalculateMoodThoughts();
			for (int i = 0; i < cachedThoughts.Count; i++)
			{
				Thought_Situational thought_Situational = cachedThoughts[i];
				if (thought_Situational.Active)
				{
					outThoughts.Add(thought_Situational);
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

		private void CheckRecalculateMoodThoughts()
		{
			int ticksGame = Find.TickManager.TicksGame;
			if (ticksGame - lastMoodThoughtsRecalculationTick >= 100)
			{
				lastMoodThoughtsRecalculationTick = ticksGame;
				try
				{
					tmpCachedThoughts.Clear();
					for (int i = 0; i < cachedThoughts.Count; i++)
					{
						cachedThoughts[i].RecalculateState();
						tmpCachedThoughts.Add(cachedThoughts[i].def);
					}
					List<ThoughtDef> situationalNonSocialThoughtDefs = ThoughtUtility.situationalNonSocialThoughtDefs;
					int j = 0;
					for (int count = situationalNonSocialThoughtDefs.Count; j < count; j++)
					{
						if (!tmpCachedThoughts.Contains(situationalNonSocialThoughtDefs[j]))
						{
							Thought_Situational thought_Situational = TryCreateThought(situationalNonSocialThoughtDefs[j]);
							if (thought_Situational != null)
							{
								cachedThoughts.Add(thought_Situational);
							}
						}
					}
				}
				finally
				{
				}
			}
		}

		private void CheckRecalculateSocialThoughts(Pawn otherPawn)
		{
			try
			{
				if (!cachedSocialThoughts.TryGetValue(otherPawn, out CachedSocialThoughts value))
				{
					value = new CachedSocialThoughts();
					cachedSocialThoughts.Add(otherPawn, value);
				}
				if (value.ShouldRecalculateState)
				{
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
				thought_Situational.RecalculateState();
				return thought_Situational;
			}
			catch (Exception ex)
			{
				Log.Error("Exception while recalculating " + def + " thought state for pawn " + pawn + ": " + ex);
				return thought_Situational;
			}
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
				thought_SituationalSocial = (Thought_SituationalSocial)ThoughtMaker.MakeThought(def);
				thought_SituationalSocial.pawn = pawn;
				thought_SituationalSocial.otherPawn = otherPawn;
				thought_SituationalSocial.RecalculateState();
				return thought_SituationalSocial;
			}
			catch (Exception ex)
			{
				Log.Error("Exception while recalculating " + def + " thought state for pawn " + pawn + ": " + ex);
				return thought_SituationalSocial;
			}
		}

		public void Notify_SituationalThoughtsDirty()
		{
			cachedThoughts.Clear();
			cachedSocialThoughts.Clear();
			lastMoodThoughtsRecalculationTick = -99999;
		}

		private void RemoveExpiredThoughtsFromCache()
		{
			cachedSocialThoughts.RemoveAll((KeyValuePair<Pawn, CachedSocialThoughts> x) => x.Value.Expired || x.Key.Discarded);
		}
	}
}
