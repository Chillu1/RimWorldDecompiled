using System;
using System.Collections.Generic;

namespace Verse;

public class LoadFolder : IEquatable<LoadFolder>
{
	public string folderName;

	public List<string> requiredAnyOfPackageIds;

	public List<string> requiredAllOfPackageIds;

	public List<string> disallowedAnyOfPackageIds;

	private readonly int hashCodeCached;

	public bool ShouldLoad
	{
		get
		{
			if ((requiredAnyOfPackageIds.NullOrEmpty() || ModLister.AnyModActiveNoSuffix(requiredAnyOfPackageIds)) && (requiredAllOfPackageIds.NullOrEmpty() || ModLister.AllModsActiveNoSuffix(requiredAllOfPackageIds)))
			{
				if (!disallowedAnyOfPackageIds.NullOrEmpty())
				{
					return !ModLister.AnyModActiveNoSuffix(disallowedAnyOfPackageIds);
				}
				return true;
			}
			return false;
		}
	}

	public LoadFolder(string folderName, List<string> requiredAnyOfPackageIds, List<string> requiredAllOfPackageIds, List<string> disallowedAnyOfPackageIds)
	{
		this.folderName = folderName;
		this.requiredAnyOfPackageIds = requiredAnyOfPackageIds;
		this.requiredAllOfPackageIds = requiredAllOfPackageIds;
		this.disallowedAnyOfPackageIds = disallowedAnyOfPackageIds;
		hashCodeCached = folderName?.GetHashCode() ?? 0;
		hashCodeCached = Gen.HashCombine(hashCodeCached, requiredAnyOfPackageIds?.GetHashCode() ?? 0);
		hashCodeCached = Gen.HashCombine(hashCodeCached, requiredAllOfPackageIds?.GetHashCode() ?? 0);
		hashCodeCached = Gen.HashCombine(hashCodeCached, disallowedAnyOfPackageIds?.GetHashCode() ?? 0);
	}

	public bool Equals(LoadFolder other)
	{
		if (other != null)
		{
			return hashCodeCached == other.GetHashCode();
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is LoadFolder other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return hashCodeCached;
	}
}
