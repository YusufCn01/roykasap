using System.Text.RegularExpressions;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;

namespace KasapOtomasyon.Infrastructure.Services;

public class BarcodeService : IBarcodeService
{
    public EmbeddedWeightBarcodeDto ParseBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return new EmbeddedWeightBarcodeDto { IsValid = false };

        barcode = barcode.Trim();

        // Check if it is a scale weight-embedded barcode (starts with 20, 27, 28, 29)
        // Standard Turkish Butcher Scale Format: 20 + 5-digit PLU + 5-digit Weight (gr) + 1-digit Checksum
        // e.g. "2000001012503" -> PLU: "00001", Weight: 1250 gr (1.250 kg)
        if (barcode.Length == 13 && (barcode.StartsWith("20") || barcode.StartsWith("27") || barcode.StartsWith("28")))
        {
            var prefix = barcode[..2];
            var pluCode = barcode.Substring(2, 5);
            var weightString = barcode.Substring(7, 5);

            if (decimal.TryParse(weightString, out var weightGr))
            {
                var weightKg = weightGr / 1000m;
                return new EmbeddedWeightBarcodeDto
                {
                    IsValid = true,
                    RawBarcode = barcode,
                    PluCode = pluCode,
                    WeightKg = weightKg,
                    IsWeightEmbedded = true
                };
            }
        }

        // Standard Product Barcode
        return new EmbeddedWeightBarcodeDto
        {
            IsValid = true,
            RawBarcode = barcode,
            PluCode = barcode,
            WeightKg = 1.0m,
            IsWeightEmbedded = false
        };
    }

    public string GenerateEan13(string productCodeOrPlu)
    {
        var digitsOnly = Regex.Replace(productCodeOrPlu, @"[^\d]", "");
        var base12 = ("869000000000" + digitsOnly);
        base12 = base12.Substring(base12.Length - 12, 12);

        var checkDigit = CalculateEan13CheckDigit(base12);
        return base12 + checkDigit;
    }

    public string GenerateWeightedBarcode(string pluCode, decimal weightKg)
    {
        var digitsOnly = Regex.Replace(pluCode, @"[^\d]", "");
        var paddedPlu = digitsOnly.PadLeft(5, '0');
        if (paddedPlu.Length > 5) paddedPlu = paddedPlu[^5..];

        var gr = (int)Math.Round(weightKg * 1000m);
        var paddedWeight = gr.ToString().PadLeft(5, '0');
        if (paddedWeight.Length > 5) paddedWeight = paddedWeight[^5..];

        var base12 = $"20{paddedPlu}{paddedWeight}";
        var checkDigit = CalculateEan13CheckDigit(base12);
        return base12 + checkDigit;
    }

    public string GeneratePriceEmbeddedBarcode(string pluCode, decimal priceTotal)
    {
        var digitsOnly = Regex.Replace(pluCode, @"[^\d]", "");
        var paddedPlu = digitsOnly.PadLeft(5, '0');
        if (paddedPlu.Length > 5) paddedPlu = paddedPlu[^5..];

        var kurus = (int)Math.Round(priceTotal * 100m);
        var paddedPrice = kurus.ToString().PadLeft(5, '0');
        if (paddedPrice.Length > 5) paddedPrice = paddedPrice[^5..];

        var base12 = $"28{paddedPlu}{paddedPrice}";
        var checkDigit = CalculateEan13CheckDigit(base12);
        return base12 + checkDigit;
    }

    private static int CalculateEan13CheckDigit(string base12)
    {
        if (base12.Length != 12) return 0;

        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = base12[i] - '0';
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        int mod = sum % 10;
        return (mod == 0) ? 0 : 10 - mod;
    }
}
