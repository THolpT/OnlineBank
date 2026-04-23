using System.Globalization;
using System.Xml.Linq;
using WebApplication1.Domains.Models;

namespace WebApplication1.Utils;

public class Converter
{
    private async Task<Dictionary<string, CurrencyRate>> GetRatesAsync()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.All
        };

        using var http = new HttpClient(handler);

        var bytes = await http.GetByteArrayAsync("https://www.cbr.ru/scripts/XML_daily.asp");
        var xml = System.Text.Encoding.GetEncoding("windows-1251").GetString(bytes);

        var doc = XDocument.Parse(xml);

        var rates = doc.Descendants("Valute")
            .Select(v => new CurrencyRate
            {
                CharCode = v.Element("CharCode")!.Value,
                Nominal = int.Parse(v.Element("Nominal")!.Value),
                Value = decimal.Parse(v.Element("Value")!.Value.Replace(',', '.'),
                    CultureInfo.InvariantCulture)
            })
            .ToDictionary(x => x.CharCode, x => x);
        
        rates["RUB"] = new CurrencyRate
        {
            CharCode = "RUB",
            Nominal = 1,
            Value = 1
        };

        return rates;
    }
    
    public async Task<decimal> ConvertCurrency(
        decimal amount,
        string fromCurrency,
        string toCurrency)
    {
        var rates = await GetRatesAsync();

        if (!rates.ContainsKey(fromCurrency) || !rates.ContainsKey(toCurrency))
            throw new Exception("Неизвестная валюта");

        var from = rates[fromCurrency];
        var to = rates[toCurrency];
        
        decimal inRub = amount * (from.Value / from.Nominal);
        
        decimal result = inRub / (to.Value / to.Nominal);

        return Math.Round(result, 2);
    }
}