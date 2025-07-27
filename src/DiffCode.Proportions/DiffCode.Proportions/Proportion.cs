using System.Collections.Immutable;


namespace DiffCode.Proportions;

/// <summary>
/// Модель сложной пропорции.
/// </summary>
public record Proportion
{

  /// <summary>
  /// Сумма членов целевой пропорции в естественных единицах.
  /// Путем последовательного приращения одного из членов текущей пропорции на фиксированную величину,
  /// сумма текущих членов пропорции приводится к этой сумме за минимальное количество шагов.
  /// </summary>
  private readonly decimal _targetSum;

  /// <summary>
  /// Словарь с членами сложной пропорции.
  /// Ключ - номер члена пропорции, значение - член сложной пропорции.
  /// </summary>
  private readonly Dictionary<ushort, ProportionItem> _items;

  /// <summary>
  /// Стандартное приращение значения для каждого члена пропорции.
  /// Может быть отрицательным, не должно быть равно 0.
  /// </summary>
  private readonly decimal _increment;

  /// <summary>
  /// Признак последовательного увеличения (true) или уменьшения (false) 
  /// текущих значений сложной пропорции при их приведении к целевым значениям.
  /// </summary>
  private readonly bool _isIncrement;


  private readonly ushort _digits;




  /// <summary>
  /// Создает модель сложной пропорции, все члены которой изначально равны 0, 
  /// а целевое состояние и шаг изменения для которой заданы соответствующими аргументами.
  /// </summary>
  /// <param name="targetSum">Сумма членов целевой пропорции, в естественных единицах.</param>
  /// <param name="increment">Шаг изменения.</param>
  /// <param name="targetPortions">Целевая пропорция в процентном виде.</param>
  /// <exception cref="ArgumentOutOfRangeException"></exception>
  /// <exception cref="ArgumentException"></exception>
  public Proportion(decimal targetSum, decimal increment, params IEnumerable<decimal> targetPortions)
  {
    // Целевая сумма должна быть больше нуля
    if (targetSum <= 0)
      throw new ArgumentOutOfRangeException(nameof(targetSum));

    // Стандартное приращение не должно быть равно 0.
    if (increment == 0)
      throw new ArgumentException(nameof(increment));

    // Количество членов пропорции должно быть больше 1.
    if (targetPortions == null || targetPortions.Count() <= 1)
      throw new ArgumentOutOfRangeException(nameof(targetPortions));

    _targetSum = targetSum;
    _increment = increment;
    _isIncrement = increment > 0;
    _digits = (ushort)(increment - Math.Round(increment, 0, MidpointRounding.ToZero)).ToString().TrimEnd('0').Length;

    var targetPortionsList = targetPortions.ToList();

    _items = new Dictionary<ushort, ProportionItem>();

    Enumerable.Range(1, targetPortionsList.Count).ToList().ForEach(p =>
    {
      _items.Add((ushort)p, new ProportionItem 
      { 
        CurrItem = 0, 
        CurrPortion = 0, 
        TargetPortion = targetPortionsList[p - 1], 
        TargetItem = Math.Round(_targetSum * (targetPortionsList[p - 1] / 100), _digits)
      });
    });
  }

  /// <summary>
  /// Создает модель сложной пропорции с заданными текущими, целевыми значениями и шагом изменения.
  /// </summary>
  /// <param name="targetSum">Сумма членов целевой пропорции, в естественных единицах.</param>
  /// <param name="increment">Шаг изменения.</param>
  /// <param name="itemPairs">
  /// Список пар "значение члена текущей пропорции в естественных единицах" :
  /// "значение члена целевой пропорции в процентном виде"
  /// </param>
  /// <exception cref="ArgumentOutOfRangeException"></exception>
  /// <exception cref="ArgumentException"></exception>
  public Proportion(decimal targetSum, decimal increment, params IEnumerable<(decimal, decimal)> itemPairs)
  {
    // Целевая сумма должна быть больше нуля
    if (targetSum <= 0)
      throw new ArgumentOutOfRangeException(nameof(targetSum));

    // Стандартное приращение не должно быть равно 0.
    if (increment == 0)
      throw new ArgumentException(nameof(increment));

    // Количество членов пропорции должно быть больше 1.
    if (itemPairs == null || itemPairs.Count() <= 1)
      throw new ArgumentOutOfRangeException(nameof(itemPairs));

    _targetSum = targetSum;
    _increment = increment;
    _isIncrement = increment > 0;
    _digits = (ushort)(increment - Math.Round(increment, 0, MidpointRounding.ToZero)).ToString().TrimEnd('0').Length;

    var itemPairsList = itemPairs.ToList();

    _items = new Dictionary<ushort, ProportionItem>();
    var _currSum = itemPairs.Sum(s => s.Item1);

    Enumerable.Range(1, itemPairsList.Count).ToList().ForEach(p =>
    {
      _items.Add((ushort)p, new ProportionItem
      {
        CurrItem = itemPairsList[p - 1].Item1,
        CurrPortion = Math.Round(_currSum != 0 ? 100 * itemPairsList[p - 1].Item1 / _currSum : 0, _digits),
        TargetPortion = Math.Round(itemPairsList[p - 1].Item2, 8),
        TargetItem = Math.Round(_targetSum * (itemPairsList[p - 1].Item2 / 100), _digits)
      });
    });
  }





  /// <summary>
  /// Изменяет значение члена текущей сложной пропорции, номер которого вычислен 
  /// в свойстве <see cref="NextItemKeyToIncrement"/>.
  /// </summary>
  public void IncrementItem()
  {
    if (CanIncrement)
    {
      ushort itemKey = NextItemKeyToIncrement;
      Items[itemKey].CurrItem += _increment;
      foreach(var item in Items)
      {
        item.Value.CurrPortion = CurrentSum != 0 ? 100 * item.Value.CurrItem / CurrentSum : 0;
      }
    }
  }



  private decimal CalcStandardDeviationInPercents(params IEnumerable<decimal> items)
  {
    var itemsList = items.ToList();
    var sum = itemsList.Sum();

    var dict = Enumerable.Range(1, itemsList.Count).Select(s => new KeyValuePair<ushort, decimal>((ushort)s, itemsList[s - 1])).ToDictionary(k => k.Key, v => v.Value);
    var portions = dict.Select(s => new KeyValuePair<ushort, decimal>(s.Key, sum != 0 ? 100 * s.Value / sum : 0)).ToDictionary(k => k.Key, v => v.Value);
    var diff = Items.Select(s => new KeyValuePair<ushort, decimal>(s.Key, s.Value.TargetPortion - portions[s.Key])).ToDictionary(k => k.Key, v => v.Value);

    var diffAverage = diff.Values.Average();
    var diffDispersion = diff.Values.Select(s => (decimal)Math.Pow((double)(s - diffAverage), 2)).Average();

    return (decimal)Math.Sqrt((double)diffDispersion);
  }





  /// <summary>
  /// Словарь с членами сложной пропорции.
  /// Ключ - номер члена пропорции, значение - член сложной пропорции.
  /// </summary>
  public ImmutableDictionary<ushort, ProportionItem> Items => _items.ToImmutableDictionary();

  /// <summary>
  /// Сумма текущей сложной пропорции, в естественных единицах.
  /// </summary>
  public decimal CurrentSum => Items.Values.Sum(s => s.CurrItem);

  /// <summary>
  /// Сумма членов целевой пропорции, в естественных единицах.
  /// </summary>
  public decimal TargetSum => _targetSum;

  /// <summary>
  /// Среднее арифметическое последовательности отличий <see cref="ProportionItem.PortionDiff"/>.
  /// </summary>
  public decimal DiffAverage => Items.Values.Average(a => a.PortionDiff);

  /// <summary>
  /// Дисперсия последовательности отличий <see cref="ProportionItem.PortionDiff"/>.
  /// </summary>
  public decimal DiffDispersion => Items.Values.Select(s => (decimal)Math.Pow((double)(s.PortionDiff - DiffAverage), 2)).Average();

  /// <summary>
  /// Стандартное отклонение последовательности отличий <see cref="ProportionItem.PortionDiff"/>.
  /// </summary>
  public decimal DiffStandardDeviation => (decimal)Math.Sqrt((double)DiffDispersion);

  /// <summary>
  /// Номер члена текущей сложной пропорции, подлежащий изменению 
  /// при следующем запуске алгоритма приведения, либо 0, если алгоритм 
  /// не может быть запущен для текущей сложной пропорции.
  /// </summary>
  public ushort NextItemKeyToIncrement
  {
    get
    {
      if (CanIncrement)
      {
        var tempDict = new Dictionary<ushort, (string, decimal)>();

        foreach (var it in Items)
        {
          var tempList = Items.Values.Select(s => s.CurrItem).ToList();
          tempList[it.Key - 1] = Items[it.Key].CurrItem + _increment;
          var stDev = CalcStandardDeviationInPercents(tempList.ToArray());
          tempDict[it.Key] = (string.Join(":", tempList), stDev);
        }

        var itemNumber = tempDict.MinBy(m => m.Value.Item2).Key;

        return itemNumber;
      }
      else
      {
        return 0;
      }
    }
  }

  /// <summary>
  /// Вычисляемое условие запуска алгоритма приведения.
  /// </summary>
  public bool CanIncrement => (_isIncrement && (CurrentSum + _increment <= TargetSum)) || (!_isIncrement && (CurrentSum + _increment >= TargetSum));

}
