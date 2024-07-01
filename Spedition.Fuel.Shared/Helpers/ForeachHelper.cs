using Spedition.Fuel.BFF.Constants;

namespace Spedition.Fuel.Shared.Helpers
{
    public static class ForeachHelper<TInner, TOuter>
    {
        public static List<TOuter> MapItemsList(List<TInner> items, Func<TInner, TOuter> mapper, string invoker)
        {
            ConcurrentBag<TOuter> res = new ();

            var result = Parallel.ForEach(
                new ConcurrentBag<TInner>(items ?? new ()),
                new ParallelOptions { MaxDegreeOfParallelism = ConstantsList.MaxDegreeOfParallelism },
                item =>
                {
                    res.Add(mapper(item));
                });

            if (!result.IsCompleted)
            {
                Log.Error($"Ошибка в методе {nameof(MapItemsList)} (вызов из : {invoker ?? string.Empty})" +
                          $"Цикл был прерван! Выполнение завершено на итерации {result.LowestBreakIteration ?? 0}");
            }

            return res?.ToList() ?? new ();
        }
    }
}
