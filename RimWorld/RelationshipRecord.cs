using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RelationshipRecord : IExposable, ILoadReferenceable
{
	private int id;

	private Gender gender;

	private string name;

	private HashSet<Pawn> references = new HashSet<Pawn>();

	public int ID => id;

	public Gender Gender => gender;

	public string Name => name;

	public IReadOnlyCollection<Pawn> References
	{
		get
		{
			if (references.Contains(null))
			{
				references.Remove(null);
			}
			return references;
		}
	}

	public RelationshipRecord()
	{
	}

	public RelationshipRecord(int id, Gender gender, string name)
	{
		this.id = id;
		this.gender = gender;
		this.name = name;
	}

	public void AddReference(Pawn pawn)
	{
		if (pawn == null)
		{
			Log.Error("Attempted to create relationship record reference from a pawn which is null!");
		}
		else
		{
			references.Add(pawn);
		}
	}

	public void RemoveReference(Pawn pawn)
	{
		if (references.Contains(pawn))
		{
			references.Remove(pawn);
		}
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref id, "id", 0);
		Scribe_Values.Look(ref gender, "gender", Gender.None);
		Scribe_Values.Look(ref name, "name");
		Scribe_Collections.Look(ref references, saveDestroyedThings: true, "references", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && references.Contains(null))
		{
			references.Remove(null);
		}
	}

	public string GetUniqueLoadID()
	{
		return $"RelationshipRecord_{ID}";
	}
}
