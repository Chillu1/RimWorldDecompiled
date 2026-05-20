using System;

namespace Verse.AI;

public struct ThinkResult : IEquatable<ThinkResult>
{
	private Job jobInt;

	private ThinkNode sourceNodeInt;

	private JobTag? tag;

	private bool fromQueue;

	public Job Job => jobInt;

	public ThinkNode SourceNode => sourceNodeInt;

	public JobTag? Tag => tag;

	public bool FromQueue => fromQueue;

	public bool IsValid => Job != null;

	public static ThinkResult NoJob => new ThinkResult(null, null);

	public ThinkResult(Job job, ThinkNode sourceNode, JobTag? tag = null, bool fromQueue = false)
	{
		jobInt = job;
		sourceNodeInt = sourceNode;
		this.tag = tag;
		this.fromQueue = fromQueue;
	}

	public override string ToString()
	{
		string text = ((Job != null) ? Job.ToString() : "null");
		string text2 = ((SourceNode != null) ? SourceNode.ToString() : "null");
		return "(job=" + text + " sourceNode=" + text2 + ")";
	}

	public override int GetHashCode()
	{
		return Gen.HashCombineStruct(Gen.HashCombine(Gen.HashCombine(Gen.HashCombine(0, jobInt), sourceNodeInt), tag), fromQueue);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ThinkResult))
		{
			return false;
		}
		return Equals((ThinkResult)obj);
	}

	public bool Equals(ThinkResult other)
	{
		if (jobInt == other.jobInt && sourceNodeInt == other.sourceNodeInt && tag == other.tag)
		{
			return fromQueue == other.fromQueue;
		}
		return false;
	}

	public static bool operator ==(ThinkResult lhs, ThinkResult rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(ThinkResult lhs, ThinkResult rhs)
	{
		return !(lhs == rhs);
	}
}
