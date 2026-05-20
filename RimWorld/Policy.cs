using Verse;

namespace RimWorld;

public abstract class Policy : IExposable, ILoadReferenceable, IRenameable
{
	public int id;

	public string label;

	public string RenamableLabel
	{
		get
		{
			return label;
		}
		set
		{
			label = value;
		}
	}

	public string BaseLabel => "UnnamedPolicy".Translate();

	public string InspectLabel => RenamableLabel;

	protected abstract string LoadKey { get; }

	protected Policy()
	{
	}

	protected Policy(int id, string label)
	{
		this.id = id;
		this.label = label;
	}

	public abstract void CopyFrom(Policy other);

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref id, "id", 0);
		Scribe_Values.Look(ref label, "label");
	}

	public string GetUniqueLoadID()
	{
		return $"{LoadKey}_{label}_{id}";
	}
}
