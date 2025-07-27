using DiffCode.Proportions;


IEnumerable<(decimal, decimal)> itemPairs = [
  (7M, (18M/54M*100M)),
  (3M, (12M/54M*100M)),
  (13M, (24M/54M*100M))
  ];

var prp = new Proportion(54, 1M, itemPairs);


Func<string> stepDetails = () => 
  $"{string.Join(" : ", prp.Items.Values.Select(s => s.CurrItem))} -- {string.Join(" : ", prp.Items.Values.Select(s => Math.Round(s.CurrPortion, 2)))} -- {prp.NextItemKeyToIncrement}-й член следующий";


Console.WriteLine(stepDetails());

while (prp.CanIncrement)
{
  prp.IncrementItem();
  Console.WriteLine(stepDetails());
}





Console.WriteLine();