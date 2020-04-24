namespace Verse
{
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
			ThingRequest result = default(ThingRequest);
			result.singleDef = null;
			result.group = ThingRequestGroup.Undefined;
			return result;
		}

		public static ThingRequest ForDef(ThingDef singleDef)
		{
			ThingRequest result = default(ThingRequest);
			result.singleDef = singleDef;
			result.group = ThingRequestGroup.Undefined;
			return result;
		}

		public static ThingRequest ForGroup(ThingRequestGroup group)
		{
			ThingRequest result = default(ThingRequest);
			result.singleDef = null;
			result.group = group;
			return result;
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
			string str = (singleDef == null) ? ("group " + group.ToString()) : ("singleDef " + singleDef.defName);
			return "ThingRequest(" + str + ")";
		}
	}
}
