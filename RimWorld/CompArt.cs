using Verse;

namespace RimWorld;

public class CompArt : ThingComp
{
	private TaggedString authorNameInt = null;

	protected TaggedString titleInt = null;

	private TaleReference taleRef;

	public TaggedString AuthorName
	{
		get
		{
			if (authorNameInt.NullOrEmpty())
			{
				return "UnknownLower".Translate().CapitalizeFirst();
			}
			return authorNameInt.Resolve();
		}
	}

	public string Title
	{
		get
		{
			if (parent.StyleSourcePrecept != null)
			{
				return parent.StyleSourcePrecept.LabelCap;
			}
			if (titleInt.NullOrEmpty())
			{
				Log.Error("CompArt got title but it wasn't configured.");
				titleInt = "Error";
			}
			return titleInt;
		}
		set
		{
			titleInt = value;
		}
	}

	public TaleReference TaleRef => taleRef;

	public bool CanShowArt
	{
		get
		{
			if (Props.mustBeFullGrave && !(parent is Building_Grave { HasCorpse: not false }))
			{
				return false;
			}
			if (!parent.TryGetQuality(out var qc))
			{
				return true;
			}
			return (int)qc >= (int)Props.minQualityForArtistic;
		}
	}

	public virtual bool Active => taleRef != null;

	public CompProperties_Art Props => (CompProperties_Art)props;

	public override string TransformLabel(string label)
	{
		if (Active && parent.StyleSourcePrecept != null)
		{
			return Title;
		}
		return base.TransformLabel(label);
	}

	public void InitializeArt(ArtGenerationContext source)
	{
		InitializeArtInternal(null, source);
	}

	public void InitializeArt(Thing relatedThing)
	{
		InitializeArtInternal(relatedThing, ArtGenerationContext.Colony);
	}

	protected virtual void InitializeArtInternal(Thing relatedThing, ArtGenerationContext source)
	{
		if (!titleInt.NullOrEmpty())
		{
			return;
		}
		if (taleRef != null)
		{
			taleRef.ReferenceDestroyed();
			taleRef = null;
		}
		if (CanShowArt)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				if (relatedThing != null)
				{
					taleRef = Find.TaleManager.GetRandomTaleReferenceForArtConcerning(relatedThing);
				}
				else
				{
					taleRef = Find.TaleManager.GetRandomTaleReferenceForArt(source);
				}
			}
			else
			{
				taleRef = TaleReference.Taleless;
			}
			titleInt = GenerateTitle(source);
		}
		else
		{
			titleInt = null;
			taleRef = null;
		}
	}

	public virtual void JustCreatedBy(Pawn pawn)
	{
		if (CanShowArt)
		{
			authorNameInt = pawn.NameFullColored;
		}
	}

	public void Clear()
	{
		authorNameInt = null;
		titleInt = null;
		if (taleRef != null)
		{
			taleRef.ReferenceDestroyed();
			taleRef = null;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref authorNameInt, "authorName", null);
		Scribe_Values.Look(ref titleInt, "title", null);
		Scribe_Deep.Look(ref taleRef, "taleRef");
	}

	public override string CompInspectStringExtra()
	{
		if (!Active)
		{
			return null;
		}
		return (string)("Author".Translate() + ": " + AuthorName) + ("\n" + "Title".Translate() + ": " + Title);
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		base.PostDestroy(mode, previousMap);
		if (taleRef != null)
		{
			taleRef.ReferenceDestroyed();
			taleRef = null;
		}
	}

	public override string GetDescriptionPart()
	{
		if (!Active)
		{
			return null;
		}
		return string.Concat(string.Concat("" + Title, "\n\n") + GenerateImageDescription(), "\n\n") + ("Author".Translate() + ": " + AuthorName);
	}

	public override bool AllowStackWith(Thing other)
	{
		if (Active)
		{
			return false;
		}
		return true;
	}

	public virtual TaggedString GenerateImageDescription()
	{
		if (taleRef == null)
		{
			Log.Error("Did CompArt.GenerateImageDescription without initializing art: " + parent);
			InitializeArt(ArtGenerationContext.Outsider);
		}
		return taleRef.GenerateText(TextGenerationPurpose.ArtDescription, Props.descriptionMaker);
	}

	protected virtual string GenerateTitle(ArtGenerationContext context)
	{
		if (taleRef == null)
		{
			Log.Error("Did CompArt.GenerateTitle without initializing art: " + parent);
			InitializeArt(ArtGenerationContext.Outsider);
		}
		return GenText.CapitalizeAsTitle(taleRef.GenerateText(TextGenerationPurpose.ArtName, Props.nameMaker));
	}
}
