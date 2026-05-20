using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public abstract class ScenPart : IExposable
	{
		[TranslationHandle]
		public ScenPartDef def;

		public bool visible = true;

		public bool summarized;

		public static float RowHeight => Text.LineHeight;

		public virtual string Label => def.LabelCap;

		public virtual bool OverrideDangerMusic => false;

		public virtual bool Valid()
		{
			return def != null;
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
		}

		public ScenPart CopyForEditing()
		{
			ScenPart scenPart = CopyForEditingInner();
			scenPart.def = def;
			return scenPart;
		}

		protected virtual ScenPart CopyForEditingInner()
		{
			return (ScenPart)MemberwiseClone();
		}

		public virtual void DoEditInterface(Listing_ScenEdit listing)
		{
			listing.GetScenPartRect(this, RowHeight);
		}

		public virtual string Summary(Scenario scen)
		{
			return def.description;
		}

		public virtual IEnumerable<string> GetSummaryListEntries(string tag)
		{
			return Enumerable.Empty<string>();
		}

		public virtual void Randomize()
		{
		}

		public virtual bool TryMerge(ScenPart other)
		{
			return false;
		}

		public virtual bool CanCoexistWith(ScenPart other)
		{
			return true;
		}

		public virtual IEnumerable<Page> GetConfigPages()
		{
			return Enumerable.Empty<Page>();
		}

		public virtual bool AllowPlayerStartingPawn(Pawn pawn, bool tryingToRedress, PawnGenerationRequest req)
		{
			return true;
		}

		public virtual void Notify_NewPawnGenerating(Pawn pawn, PawnGenerationContext context)
		{
		}

		public virtual void Notify_PawnGenerated(Pawn pawn, PawnGenerationContext context, bool redressed)
		{
		}

		public virtual void Notify_PawnDied(Corpse corpse)
		{
		}

		public virtual void PreConfigure()
		{
		}

		public virtual void PostWorldGenerate()
		{
		}

		public virtual void PostIdeoChosen()
		{
		}

		public virtual void PreMapGenerate()
		{
		}

		public virtual IEnumerable<Thing> PlayerStartingThings()
		{
			return Enumerable.Empty<Thing>();
		}

		public virtual void GenerateIntoMap(Map map)
		{
		}

		public virtual void PostMapGenerate(Map map)
		{
		}

		public virtual void PostGravshipLanded(Map map)
		{
		}

		public virtual void MapRemoved(Map map)
		{
		}

		public virtual void PostGameStart()
		{
		}

		public virtual void Tick()
		{
		}

		public virtual IEnumerable<Alert> GetAlerts()
		{
			return Enumerable.Empty<Alert>();
		}

		public virtual IEnumerable<string> ConfigErrors()
		{
			if (def == null)
			{
				yield return GetType()?.ToString() + " has null def.";
			}
		}

		public virtual bool HasNullDefs()
		{
			return def == null;
		}

		public override int GetHashCode()
		{
			if (def == null)
			{
				return 0;
			}
			return def.GetHashCode();
		}
	}
}
