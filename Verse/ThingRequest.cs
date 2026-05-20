namespace Verse;

public struct ThingRequest
{
	public ThingDef singleDef;

	public ThingRequestGroup group;

	public bool IsUndefined
	{
		get
		{
			if (singleDef == null)
			{
				return group == ThingRequestGroup.Undefined;
			}
			return false;
		}
	}

	public bool CanBeFoundInRegion
	{
		get
		{
			if (IsUndefined)
			{
				return false;
			}
			if (singleDef == null && group != ThingRequestGroup.Nothing)
			{
				return group.StoreInRegion();
			}
			return true;
		}
	}

	public static ThingRequest ForUndefined()
	{
		return new ThingRequest
		{
			singleDef = null,
			group = ThingRequestGroup.Undefined
		};
	}

	public static ThingRequest ForDef(ThingDef singleDef)
	{
		return new ThingRequest
		{
			singleDef = singleDef,
			group = ThingRequestGroup.Undefined
		};
	}

	public static ThingRequest ForGroup(ThingRequestGroup group)
	{
		return new ThingRequest
		{
			singleDef = null,
			group = group
		};
	}

	public bool Accepts(Thing t)
	{
		if (singleDef != null)
		{
			return t.def == singleDef;
		}
		if (group != ThingRequestGroup.Everything)
		{
			return group.Includes(t.def);
		}
		return true;
	}

	public override string ToString()
	{
		string text = ((singleDef == null) ? ("group " + group) : ("singleDef " + singleDef.defName));
		return "ThingRequest(" + text + ")";
	}
}
