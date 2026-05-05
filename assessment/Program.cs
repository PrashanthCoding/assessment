class Program
{
    static void Main()
    {
        string data = @"Date,SKU,Unit Price,Quantity,Total Price
        2019-01-01,Death by Chocolate,180,5,900
        2019-01-01,Cake Fudge,150,1,150
        2019-01-01,Cake Fudge,150,1,150
        2019-01-01,Cake Fudge,150,3,450
        2019-01-01,Death by Chocolate,180,1,180
        2019-01-01,Vanilla Double Scoop,80,3,240
        2019-01-01,Butterscotch Single Scoop,60,5,300
        2019-01-01,Vanilla Single Scoop,50,5,250
        2019-01-01,Cake Fudge,150,5,750
        2019-01-01,Hot Chocolate Fudge,120,3,360
        2019-02-01,Vanilla Single Scoop,50,2,100
        2019-02-01,Death by Chocolate,180,2,360
        2019-02-01,Cafe Caramel,160,2,320
        2019-03-01,Vanilla Single Scoop,50,5,250
        2019-03-01,Cake Fudge,150,5,750
        2019-03-01,Pista Single Scoop,60,1,60";

        var lines = data.Split('\n');

        double totalSales = 0;

        var monthlySales = new Dictionary<string, double>();
        var monthlyItemQty = new Dictionary<string, Dictionary<string, int>>();
        var monthlyItemRevenue = new Dictionary<string, Dictionary<string, double>>();
        var monthlyItemOrders = new Dictionary<string, Dictionary<string, List<int>>>();
        var errors = new List<string>();

        for (int i = 1; i < lines.Length; i++)
        {
            var row = lines[i].Trim().Split(',');

            if (row.Length != 5)
            {
                errors.Add($"Row {i} malformed");
                continue;
            }

            string dateStr = row[0];
            string sku = row[1];

            if (!DateTime.TryParse(dateStr, out DateTime date))
            {
                errors.Add($"Row {i} invalid date");
                continue;
            }

            if (!double.TryParse(row[2], out double unitPrice) ||
                !int.TryParse(row[3], out int quantity) ||
                !double.TryParse(row[4], out double totalPrice))
            {
                errors.Add($"Row {i} parsing error");
                continue;
            }

            // Validations
            if (unitPrice * quantity != totalPrice)
                errors.Add($"Row {i} price mismatch");

            if (quantity < 1)
                errors.Add($"Row {i} quantity < 1");

            if (unitPrice < 0)
                errors.Add($"Row {i} unit price < 0");

            if (totalPrice < 0)
                errors.Add($"Row {i} total price < 0");

            string month = date.ToString("yyyy-MM");

            // Total sales
            totalSales += totalPrice;

            // Monthly sales
            if (!monthlySales.ContainsKey(month))
                monthlySales[month] = 0;

            monthlySales[month] += totalPrice;

            // Quantity tracking
            if (!monthlyItemQty.ContainsKey(month))
                monthlyItemQty[month] = new Dictionary<string, int>();

            if (!monthlyItemQty[month].ContainsKey(sku))
                monthlyItemQty[month][sku] = 0;

            monthlyItemQty[month][sku] += quantity;

            // Revenue tracking
            if (!monthlyItemRevenue.ContainsKey(month))
                monthlyItemRevenue[month] = new Dictionary<string, double>();

            if (!monthlyItemRevenue[month].ContainsKey(sku))
                monthlyItemRevenue[month][sku] = 0;

            monthlyItemRevenue[month][sku] += totalPrice;

            // Orders tracking
            if (!monthlyItemOrders.ContainsKey(month))
                monthlyItemOrders[month] = new Dictionary<string, List<int>>();

            if (!monthlyItemOrders[month].ContainsKey(sku))
                monthlyItemOrders[month][sku] = new List<int>();

            monthlyItemOrders[month][sku].Add(quantity);
        }

        Console.WriteLine("===== TOTAL SALES =====");
        Console.WriteLine(totalSales);

        Console.WriteLine("\n===== MONTH-WISE SALES =====");
        foreach (var m in monthlySales)
            Console.WriteLine($"{m.Key}: {m.Value}");

        Console.WriteLine("\n===== MOST POPULAR ITEM + STATS =====");
        foreach (var month in monthlyItemQty)
        {
            string maxItem = "";
            int maxQty = 0;

            foreach (var item in month.Value)
            {
                if (item.Value > maxQty)
                {
                    maxQty = item.Value;
                    maxItem = item.Key;
                }
            }

            var orders = monthlyItemOrders[month.Key][maxItem];

            int min = int.MaxValue, max = int.MinValue, sum = 0;

            foreach (var q in orders)
            {
                if (q < min) min = q;
                if (q > max) max = q;
                sum += q;
            }

            double avg = (double)sum / orders.Count;

            Console.WriteLine($"{month.Key}: {maxItem}");
            Console.WriteLine($"   Min: {min}, Max: {max}, Avg: {avg:F2}");
        }

        Console.WriteLine("\n===== HIGHEST REVENUE ITEM =====");
        foreach (var month in monthlyItemRevenue)
        {
            string maxItem = "";
            double maxRevenue = 0;

            foreach (var item in month.Value)
            {
                if (item.Value > maxRevenue)
                {
                    maxRevenue = item.Value;
                    maxItem = item.Key;
                }
            }

            Console.WriteLine($"{month.Key}: {maxItem} ({maxRevenue})");
        }

        Console.WriteLine("\n===== MONTH-TO-MONTH GROWTH (%) =====");

        var months = new List<string>(monthlyItemRevenue.Keys);
        months.Sort();

        for (int i = 1; i < months.Count; i++)
        {
            string prev = months[i - 1];
            string curr = months[i];

            Console.WriteLine($"\n{prev} -> {curr}");

            foreach (var item in monthlyItemRevenue[curr])
            {
                double currVal = item.Value;
                double prevVal = monthlyItemRevenue.ContainsKey(prev) &&
                                 monthlyItemRevenue[prev].ContainsKey(item.Key)
                                 ? monthlyItemRevenue[prev][item.Key]
                                 : 0;

                if (prevVal == 0)
                {
                    Console.WriteLine($"{item.Key}: New item");
                }
                else
                {
                    double growth = ((currVal - prevVal) / prevVal) * 100;
                    Console.WriteLine($"{item.Key}: {growth:F2}%");
                }
            }
        }

        Console.WriteLine("\n===== DATA ERRORS =====");
        foreach (var e in errors)
            Console.WriteLine(e);
    }
}