namespace DiffCode.Proportions;

/// <summary>
/// Модель данных для описания члена сложной пропорции.
/// </summary>
public record ProportionItem
{

  /// <summary>
  /// Текущее значение члена сложной пропорции в естественных единицах.
  /// </summary>
  public decimal CurrItem { get; set; }

  /// <summary>
  /// Текущее значение члена сложной пропорции в процентах от текущей ее суммы.
  /// </summary>
  public decimal CurrPortion { get; set; }

  /// <summary>
  /// Целевое значение члена сложной пропорции в естественных единицах.
  /// </summary>
  public decimal TargetItem { get; set; }

  /// <summary>
  /// Целевое значение члена сложной пропорции в процентах от целевой ее суммы.
  /// </summary>
  public decimal TargetPortion { get; set; }

  /// <summary>
  /// Отличие текущего процентного значения члена сложной пропорции от целевого.
  /// </summary>
  public decimal PortionDiff => TargetPortion - CurrPortion;

}