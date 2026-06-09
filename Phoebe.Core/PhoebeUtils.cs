using System.Runtime.CompilerServices;

namespace Phoebe;

public static class PhoebeUtils {
#if NET10_0
	public const MethodImplOptions Optimized = MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization;
#else
	public const MethodImplOptions Optimized = MethodImplOptions.AggressiveInlining;
#endif
}