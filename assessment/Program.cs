class Program
{
    static void Main()
    {
        string data = GetData();

        var result = ProcessData(data);

        PrintResults(result);
    }

    static string GetData()
    {
        return @"Date,SKU,Unit Price,Quantity,Total Price
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
    }

    static ResultModel ProcessData(string data)
    {
        var lines = data.Split('\n');

        var result = new ResultModel();

        for (int i = 1; i < lines.Length; i++)
        {
            var row = lines[i].Trim().Split(',');

            if (row.Length != 5)
            {
                result.Errors.Add($"Row {i} malformed");
                continue;
            }

            if (!DateTime.TryParse(row[0], out DateTime date))
            {
                result.Errors.Add($"Row {i} invalid date");
                continue;
            }

            string sku = row[1];

            if (!double.TryParse(row[2], out double unitPrice) ||
                !int.TryParse(row[3], out int quantity) ||
                !double.TryParse(row[4], out double totalPrice))
            {
                result.Errors.Add($"Row {i} parsing error");
                continue;
            }

            ValidateRow(i, unitPrice, quantity, totalPrice, result.Errors);

            string month = date.ToString("yyyy-MM");

            UpdateSales(result, month, sku, quantity, totalPrice);
        }

        return result;
    }

    static void ValidateRow(int rowNum, double unitPrice, int qty, double total, List<string> errors)
    {
        if (unitPrice * qty != total)
            errors.Add($"Row {rowNum} price mismatch");

        if (qty < 1)
            errors.Add($"Row {rowNum} quantity < 1");

        if (unitPrice < 0)
            errors.Add($"Row {rowNum} unit price < 0");

        if (total < 0)
            errors.Add($"Row {rowNum} total price < 0");
    }

    static void UpdateSales(ResultModel result, string month, string sku, int qty, double total)
    {
        result.TotalSales += total;

        if (!result.MonthlySales.ContainsKey(month))
            result.MonthlySales[month] = 0;

        result.MonthlySales[month] += total;

        if (!result.MonthlyItemQty.ContainsKey(month))
            result.MonthlyItemQty[month] = new Dictionary<string, int>();

        if (!result.MonthlyItemQty[month].ContainsKey(sku))
            result.MonthlyItemQty[month][sku] = 0;

        result.MonthlyItemQty[month][sku] += qty;

        if (!result.MonthlyItemRevenue.ContainsKey(month))
            result.MonthlyItemRevenue[month] = new Dictionary<string, double>();

        if (!result.MonthlyItemRevenue[month].ContainsKey(sku))
            result.MonthlyItemRevenue[month][sku] = 0;

        result.MonthlyItemRevenue[month][sku] += total;

        if (!result.MonthlyItemOrders.ContainsKey(month))
            result.MonthlyItemOrders[month] = new Dictionary<string, List<int>>();

        if (!result.MonthlyItemOrders[month].ContainsKey(sku))
            result.MonthlyItemOrders[month][sku] = new List<int>();

        result.MonthlyItemOrders[month][sku].Add(qty);
    }

    static void PrintResults(ResultModel result)
    {
        Console.WriteLine("===== TOTAL SALES =====");
        Console.WriteLine(result.TotalSales);

        Console.WriteLine("\n===== MONTH-WISE SALES =====");
        foreach (var m in result.MonthlySales)
            Console.WriteLine($"{m.Key}: {m.Value}");

        Console.WriteLine("\n===== MOST POPULAR ITEM =====");
        foreach (var month in result.MonthlyItemQty)
        {
            var maxItem = month.Value.OrderByDescending(x => x.Value).First();

            var orders = result.MonthlyItemOrders[month.Key][maxItem.Key];

            Console.WriteLine($"{month.Key}: {maxItem.Key}");
            Console.WriteLine($"Min: {orders.Min()}, Max: {orders.Max()}, Avg: {orders.Average():F2}");
        }

        Console.WriteLine("\n===== HIGHEST REVENUE ITEM =====");
        foreach (var month in result.MonthlyItemRevenue)
        {
            var maxItem = month.Value.OrderByDescending(x => x.Value).First();
            Console.WriteLine($"{month.Key}: {maxItem.Key} ({maxItem.Value})");
        }

        Console.WriteLine("\n===== ERRORS =====");
        foreach (var e in result.Errors)
            Console.WriteLine(e);
    }
}

class ResultModel
{
    public double TotalSales = 0;
    public Dictionary<string, double> MonthlySales = new();
    public Dictionary<string, Dictionary<string, int>> MonthlyItemQty = new();
    public Dictionary<string, Dictionary<string, double>> MonthlyItemRevenue = new();
    public Dictionary<string, Dictionary<string, List<int>>> MonthlyItemOrders = new();
    public List<string> Errors = new();
}