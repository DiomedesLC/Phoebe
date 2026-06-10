using System.Collections.Concurrent;
using System.Diagnostics;

namespace Phoebe;

public class Seedfinder {
	public const int MIN_SEED = 1;
	public const int MAX_SEED = 100_000_000;

	public int StartSeed { get; set; } = MIN_SEED;
	public int EndSeed { get; set; } = MAX_SEED;
	public int? CustomChunkSize { get; set; }
	public IProgress<int>? ProgressCallback;

	public int SearchedSeeds() => EndSeed - StartSeed;

	Partitioner<Tuple<int, int>> CreateChunks() {
		if(CustomChunkSize == null) {
			return Partitioner.Create(StartSeed, EndSeed);
		} else {
			return Partitioner.Create(StartSeed, EndSeed, CustomChunkSize.Value);
		}
	}

	public static void WriteFoundSeeds(string fileName, IEnumerable<int> seeds) {
		File.WriteAllText(fileName, string.Join(", ", seeds));
	}

	public IReadOnlyCollection<int> Where(Func<int, bool> predicate) {
		Stopwatch timer = Stopwatch.StartNew();
		ConcurrentBag<int> foundSeeds = new ConcurrentBag<int>();
		Partitioner<Tuple<int, int>> chunks = CreateChunks();
		int scannedSeeds = 0;

		Parallel.ForEach(chunks, chunk => {
			(int start, int end) = chunk;
			for(int i = start; i < end; i++) {
				if(predicate(i)) {
					foundSeeds.Add(i);
				}
			}
			Interlocked.Add(ref scannedSeeds, end - start);
			ProgressCallback?.Report(scannedSeeds);
		});

		return foundSeeds;
	}

	public int? First(Func<int, bool> predicate) {
		Stopwatch timer = Stopwatch.StartNew();
		int? found = null;
		CancellationTokenSource cts = new CancellationTokenSource();
		ParallelOptions options = new ParallelOptions() {
			CancellationToken = cts.Token
		};

		Partitioner<Tuple<int, int>> chunks = CreateChunks();
		int scannedSeeds = 0;

		try {
			Parallel.ForEach(chunks, options, chunk => {
				(int start, int end) = chunk;
				for(int i = start; i < end; i++) {
					if(predicate(i)) {
						found = i;
						cts.Cancel();
					}
				}
				Interlocked.Add(ref scannedSeeds, end - start);
				ProgressCallback?.Report(scannedSeeds);
			});
		} catch(OperationCanceledException) { }

		cts.Dispose();
		return found;
	}
}