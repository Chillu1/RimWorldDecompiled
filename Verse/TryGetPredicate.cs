namespace Verse;

public delegate bool TryGetPredicate<T>(out T result);
public delegate bool TryGetPredicate<in T1, O>(T1 arg, out O result);
public delegate bool TryGetPredicate<in T1, in T2, O>(T1 arg1, T2 arg2, out O result);
public delegate bool TryGetPredicate<in T1, in T2, in T3, O>(T1 arg1, T2 arg2, T3 arg3, out O result);
public delegate bool TryGetPredicate<in T1, in T2, in T3, in T4, O>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, out O result);
public delegate bool TryGetPredicate<in T1, in T2, in T3, in T4, in T5, O>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, out O result);
public delegate bool TryGetPredicate<in T1, in T2, in T3, in T4, in T5, in T6, O>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, out O result);
