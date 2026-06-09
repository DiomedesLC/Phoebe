using System.Collections.Concurrent;
using System.Diagnostics;

namespace Phoebe;

public class Seedfinder {
	public const int MIN_SEED = 1;
	public const int MAX_SEED = 100_000_000;

	public int StartSeed { get; set; } = MIN_SEED;
	public int EndSeed { get; set; } = MAX_SEED;

	public int SearchedSeeds() => EndSeed - StartSeed;

	Partitioner<Tuple<int, int>> CreateChunks() {
		return Partitioner.Create(StartSeed, EndSeed);
	}

	public static void WriteFoundSeeds(string fileName, IEnumerable<int> seeds) {
		File.WriteAllText(fileName, string.Join(", ", seeds));
	}

	public IReadOnlyCollection<int> Where(Func<int, bool> predicate) {
		Stopwatch timer = Stopwatch.StartNew();
		ConcurrentBag<int> foundSeeds = new ConcurrentBag<int>();
		Partitioner<Tuple<int, int>> chunks = CreateChunks();

		Parallel.ForEach(chunks, chunk => {
			(int start, int end) = chunk;
			Console.WriteLine($"Starting chunk! start = {start}. end = {end}");
			for(int i = start; i < end; i++) {
				if(predicate(i)) {
					foundSeeds.Add(i);
				}
			}
		});

		Console.WriteLine($"Took {timer.ElapsedMilliseconds}ms");
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

		try {
			Parallel.ForEach(chunks, options, chunk => {
				(int start, int end) = chunk;
				Console.WriteLine($"Starting chunk! start = {start}. end = {end}");
				for(int i = start; i < end; i++) {
					if(predicate(i)) {
						Console.WriteLine($"Found seed! {i}");
						found = i;
						cts.Cancel();
					}
				}
			});
		} catch(OperationCanceledException) { }

		cts.Dispose();
		Console.WriteLine($"Took {timer.ElapsedMilliseconds}ms");
		return found;
	}
}