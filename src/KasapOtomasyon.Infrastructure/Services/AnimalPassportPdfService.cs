using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;
using KasapOtomasyon.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KasapOtomasyon.Infrastructure.Services;

public class AnimalPassportPdfService : IAnimalPassportPdfService
{
    private readonly KasapDbContext _context;

    public AnimalPassportPdfService(KasapDbContext context)
    {
        _context = context;
    }

    public async Task<AnimalPassportDocumentDto> GetAnimalPassportDataAsync(string earTagOrCarcassNo)
    {
        var cleanQuery = earTagOrCarcassNo.Trim();

        // Find Animal
        var animal = await _context.AnimalIntakes
            .Include(a => a.VeterinaryChecks)
            .Include(a => a.SlaughterRecord)
                .ThenInclude(s => s!.DeboningCuts)
            .FirstOrDefaultAsync(a => a.EarTagNumber.ToLower() == cleanQuery.ToLower() ||
                                      (a.SlaughterRecord != null && a.SlaughterRecord.CarcassNumber.ToLower() == cleanQuery.ToLower()));

        if (animal == null)
        {
            // Try by Carcass Record
            var slaughterRec = await _context.SlaughterRecords
                .Include(s => s.AnimalIntake)
                    .ThenInclude(a => a.VeterinaryChecks)
                .Include(s => s.DeboningCuts)
                .FirstOrDefaultAsync(s => s.CarcassNumber.ToLower() == cleanQuery.ToLower());

            if (slaughterRec != null)
            {
                animal = slaughterRec.AnimalIntake;
                if (animal != null)
                {
                    animal.SlaughterRecord = slaughterRec;
                }
            }
        }

        if (animal == null)
        {
            throw new InvalidOperationException($"'{earTagOrCarcassNo}' küpe veya karkas numarasına ait geçmiş kaydı bulunamadı.");
        }

        var vetCheck = animal.VeterinaryChecks.OrderByDescending(v => v.CheckDate).FirstOrDefault();
        var slaughter = animal.SlaughterRecord;
        var cuts = slaughter?.DeboningCuts.Select(c => new CarcassDeboningCutDto
        {
            Id = c.Id,
            SlaughterRecordId = c.SlaughterRecordId,
            CarcassNumber = c.CarcassNumber,
            EarTagNumber = c.EarTagNumber,
            AnatomicalRegion = c.AnatomicalRegion,
            CutName = c.CutName,
            WeightKg = c.WeightKg,
            YieldPercentage = c.YieldPercentage,
            QualityGrade = c.QualityGrade,
            UnitCostEstimated = c.UnitCostEstimated,
            TotalCutValue = c.TotalCutValue,
            LotNumber = c.LotNumber,
            Barcode = c.Barcode,
            TargetStorageLocation = c.TargetStorageLocation,
            CutDate = c.CutDate,
            MasterButcher = c.MasterButcher
        }).ToList() ?? new List<CarcassDeboningCutDto>();

        var docNum = $"PSP-{animal.EarTagNumber.Replace(" ", "")}-{DateTime.UtcNow:yyyyMMdd}";
        var hashSource = $"{animal.EarTagNumber}|{animal.LiveWeightKg}|{slaughter?.ColdCarcassWeightKg}|{vetCheck?.ReportNumber}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hashSource)))[..16];

        return new AnimalPassportDocumentDto
        {
            DocumentNumber = docNum,
            GeneratedDate = DateTime.UtcNow,
            OrganizationName = "ROYPOS KASAP & MEZBAHA İŞLETMESİ",
            OfficialFacilityCode = "TR-34-MEZ-0089",
            QrVerificationUrl = $"https://roypos.kasap/dogrula?kupe={Uri.EscapeDataString(animal.EarTagNumber)}&doc={docNum}&hash={hash}",
            DocumentSecurityHash = hash,

            // 1. Hayvan Kabul
            EarTagNumber = animal.EarTagNumber,
            PassportNumber = animal.PassportNumber,
            WaybillNumber = animal.WaybillNumber,
            RfidTag = animal.RfidOrQrCode,
            BatchNumber = animal.BatchNumber,
            AnimalType = animal.AnimalType,
            Breed = animal.Breed,
            Gender = animal.Gender.ToString(),
            AgeMonths = animal.AgeMonths,
            LiveWeightKg = animal.LiveWeightKg,
            PurchasePrice = animal.PurchasePrice,
            ProducerName = animal.ProducerName,
            FarmOrigin = animal.FarmOrigin,
            ArrivalDate = animal.ArrivalDate,

            // 2. Veteriner
            HasVetCheck = vetCheck != null,
            VetDoctorName = vetCheck?.VeterinarianName ?? "Muayene Yapılmadı",
            VetDiplomaNumber = vetCheck?.DiplomaNumber ?? "-",
            VetReportNumber = vetCheck?.ReportNumber ?? "-",
            VetCheckDate = vetCheck?.CheckDate ?? DateTime.MinValue,
            VetBodyTemperature = vetCheck?.BodyTemperature ?? 0m,
            VetDiagnosis = vetCheck?.Diagnosis ?? "-",
            VetStatus = vetCheck?.Status.ToString() ?? "Bekliyor",
            IsApprovedForSlaughter = vetCheck?.IsApprovedForSlaughter ?? false,

            // 3. Kesimhane & Karkas
            HasSlaughterRecord = slaughter != null,
            SlaughterNumber = slaughter?.SlaughterNumber ?? "-",
            CarcassNumber = slaughter?.CarcassNumber ?? "-",
            SlaughterDate = slaughter?.SlaughterDate ?? DateTime.MinValue,
            ButcherName = slaughter?.ButcherPersonName ?? "-",
            SlaughterLine = slaughter?.SlaughterLine ?? "-",
            HotCarcassWeightKg = slaughter?.HotCarcassWeightKg ?? 0m,
            ColdCarcassWeightKg = slaughter?.ColdCarcassWeightKg ?? 0m,
            CarcassYieldPercentage = slaughter?.CarcassYieldPercentage ?? 0m,
            HeadWeightKg = slaughter?.HeadWeightKg ?? 0m,
            HideWeightKg = slaughter?.HideWeightKg ?? 0m,
            OffalWeightKg = slaughter?.OffalWeightKg ?? 0m,
            BoneWeightKg = slaughter?.BoneWeightKg ?? 0m,
            FatWeightKg = slaughter?.FatWeightKg ?? 0m,
            SlaughterWasteKg = slaughter?.SlaughterWasteKg ?? 0m,
            ConformationClass = slaughter?.ConformationClass ?? "R",
            FatCoverScore = slaughter?.FatCoverScore ?? 3,
            MarblingScore = slaughter?.MarblingScore ?? 5,
            PostMortemPh24 = slaughter?.PostMortemPh24 ?? 5.65m,
            ColdStorageLocation = slaughter?.ColdStorageLocation ?? "-",
            HookRailNumber = slaughter?.HookRailNumber ?? "-",
            TotalSlaughterCost = slaughter?.TotalSlaughterCost ?? 0m,
            CarcassCostPerKg = slaughter?.CarcassCostPerKg ?? 0m,

            // 4. Parçalama
            DeboningCuts = cuts
        };
    }

    public string GeneratePassportHtml(AnimalPassportDocumentDto data)
    {
        var cutsRowsHtml = new StringBuilder();
        if (data.DeboningCuts.Any())
        {
            int index = 1;
            foreach (var cut in data.DeboningCuts)
            {
                cutsRowsHtml.AppendLine($@"
                <tr>
                    <td style='text-align:center;'>{index++}</td>
                    <td><b>{cut.AnatomicalRegion}</b></td>
                    <td><b>{cut.CutName}</b></td>
                    <td style='text-align:right; font-weight:bold; color:#16a34a;'>{cut.WeightKg:N2} kg</td>
                    <td style='text-align:right;'>%{cut.YieldPercentage:N1}</td>
                    <td><span class='badge'>{cut.QualityGrade}</span></td>
                    <td style='text-align:right;'>{cut.UnitCostEstimated:C2}</td>
                    <td style='text-align:right; font-weight:bold; color:#b45309;'>{cut.TotalCutValue:C2}</td>
                    <td style='font-family:monospace; font-size:11px;'>{cut.LotNumber}</td>
                    <td style='font-size:11px;'>{cut.TargetStorageLocation}</td>
                </tr>");
            }
        }
        else
        {
            cutsRowsHtml.AppendLine("<tr><td colspan='10' style='text-align:center; color:#64748b; padding:15px;'>Bu karkas için henüz anatomik parçalama kaydı girilmemiştir.</td></tr>");
        }

        var vetStatusBadge = data.IsApprovedForSlaughter 
            ? "<span class='badge badge-success'>✓ KESİME UYGUN (ONAYLI)</span>" 
            : "<span class='badge badge-danger'>⛔ KESİM ENGELLENDİ / KARANTİNA</span>";

        return $@"<!DOCTYPE html>
<html lang='tr'>
<head>
    <meta charset='UTF-8'>
    <title>Hayvan Soykütüğü ve Mezbaha İzlenebilirlik Pasaportu - {data.EarTagNumber}</title>
    <style>
        @page {{
            size: A4 portrait;
            margin: 12mm 15mm;
        }}
        body {{
            font-family: 'Segoe UI', Arial, Helvetica, sans-serif;
            color: #0f172a;
            background-color: #ffffff;
            margin: 0;
            padding: 0;
            font-size: 12px;
            line-height: 1.4;
        }}
        .header {{
            border-bottom: 3px solid #dc2626;
            padding-bottom: 10px;
            margin-bottom: 14px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }}
        .header-logo {{
            font-size: 22px;
            font-weight: 900;
            color: #dc2626;
            letter-spacing: -0.5px;
        }}
        .header-subtitle {{
            font-size: 11px;
            color: #475569;
            font-weight: 600;
            text-transform: uppercase;
        }}
        .doc-meta {{
            text-align: right;
            font-size: 11px;
            color: #334155;
        }}
        .section-title {{
            background: #f8fafc;
            border-left: 4px solid #dc2626;
            padding: 6px 10px;
            font-size: 12.5px;
            font-weight: bold;
            color: #0f172a;
            margin: 12px 0 8px 0;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }}
        .grid-2 {{
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 10px;
            margin-bottom: 10px;
        }}
        .grid-3 {{
            display: grid;
            grid-template-columns: 1fr 1fr 1fr;
            gap: 10px;
            margin-bottom: 10px;
        }}
        .grid-4 {{
            display: grid;
            grid-template-columns: 1fr 1fr 1fr 1fr;
            gap: 10px;
            margin-bottom: 10px;
        }}
        .card {{
            border: 1px solid #e2e8f0;
            border-radius: 6px;
            padding: 8px 10px;
            background: #ffffff;
        }}
        .card-label {{
            font-size: 10px;
            color: #64748b;
            font-weight: bold;
            text-transform: uppercase;
            margin-bottom: 2px;
        }}
        .card-value {{
            font-size: 13px;
            font-weight: bold;
            color: #0f172a;
        }}
        .badge {{
            display: inline-block;
            padding: 3px 8px;
            border-radius: 4px;
            font-size: 10.5px;
            font-weight: bold;
            background: #e2e8f0;
            color: #334155;
        }}
        .badge-success {{
            background: #dcfce7;
            color: #15803d;
            border: 1px solid #86efac;
        }}
        .badge-danger {{
            background: #fee2e2;
            color: #b91c1c;
            border: 1px solid #fca5a5;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 6px;
            font-size: 11px;
        }}
        th {{
            background: #f1f5f9;
            color: #334155;
            text-align: left;
            padding: 6px 8px;
            border: 1px solid #cbd5e1;
            font-weight: bold;
            font-size: 10.5px;
            text-transform: uppercase;
        }}
        td {{
            padding: 5px 8px;
            border: 1px solid #e2e8f0;
        }}
        tr:nth-child(even) {{
            background: #f8fafc;
        }}
        .signatures {{
            margin-top: 20px;
            display: grid;
            grid-template-columns: 1fr 1fr 1fr;
            gap: 15px;
            text-align: center;
            font-size: 11px;
            page-break-inside: avoid;
        }}
        .sig-box {{
            border-top: 1px dashed #94a3b8;
            padding-top: 6px;
        }}
        .footer {{
            margin-top: 16px;
            padding-top: 8px;
            border-top: 1px solid #e2e8f0;
            font-size: 9.5px;
            color: #64748b;
            display: flex;
            justify-content: space-between;
        }}
        @media print {{
            .no-print {{ display: none; }}
            body {{ -webkit-print-color-adjust: exact; print-color-adjust: exact; }}
        }}
    </style>
</head>
<body>

    <!-- Print Action Bar (Hidden on physical print) -->
    <div class='no-print' style='background:#fef2f2; border:1px solid #f87171; padding:12px; border-radius:8px; margin-bottom:15px; text-align:center;'>
        <b style='color:#b91c1c; font-size:14px;'>📄 HAYVAN İZLENEBİLİRLİK VE MEZBAHA PASAPORTU HAZIRLANDI</b>
        <div style='margin-top:8px;'>
            <button onclick='window.print()' style='background:#dc2626; color:white; border:none; padding:8px 22px; font-weight:bold; font-size:13px; border-radius:6px; cursor:pointer;'>🖨 YAZDIR / PDF OLARAK KAYDET</button>
            <span style='margin-left:12px; color:#475569; font-size:12px;'>Tarayıcı penceresinde <b>Hedef: 'PDF olarak Kaydet'</b> seçerek doğrudan PDF indirebilirsiniz.</span>
        </div>
    </div>

    <!-- Official Header -->
    <div class='header'>
        <div>
            <div class='header-logo'>🥩 {data.OrganizationName}</div>
            <div class='header-subtitle'>T.C. Tarım ve Orman Bakanlığı Onaylı Mezbaha &amp; İzlenebilirlik Pasaportu</div>
            <div style='font-size:10px; color:#64748b;'>Tesis Onay No: <b>{data.OfficialFacilityCode}</b> | Doğrulama Kodu: <b>{data.DocumentSecurityHash}</b></div>
        </div>
        <div class='doc-meta'>
            <div><b>Belge No:</b> {data.DocumentNumber}</div>
            <div><b>Tarih:</b> {data.GeneratedDate:dd.MM.yyyy HH:mm}</div>
            <div><b>Küpe No:</b> <span style='font-size:14px; color:#dc2626; font-weight:bold;'>{data.EarTagNumber}</span></div>
        </div>
    </div>

    <!-- 1. HAYVAN KİMLİK VE KABUL BİLGİLERİ -->
    <div class='section-title'>1. 📋 Hayvan Kimlik, Pedigri ve Kabul Bilgileri</div>
    <div class='grid-4'>
        <div class='card'>
            <div class='card-label'>Bakanlık Küpe No</div>
            <div class='card-value' style='color:#dc2626;'>{data.EarTagNumber}</div>
        </div>
        <div class='card'>
            <div class='card-label'>Hayvan Türü &amp; Irkı</div>
            <div class='card-value'>{data.Breed}</div>
        </div>
        <div class='card'>
            <div class='card-label'>Cinsiyet &amp; Yaş</div>
            <div class='card-value'>{data.Gender} / {data.AgeMonths} Ay</div>
        </div>
        <div class='card'>
            <div class='card-label'>Canlı Kantar Ağırlığı</div>
            <div class='card-value' style='color:#16a34a;'>{data.LiveWeightKg:N1} kg</div>
        </div>
    </div>
    <div class='grid-3'>
        <div class='card'>
            <div class='card-label'>Üretici / Besici</div>
            <div class='card-value'>{data.ProducerName}</div>
        </div>
        <div class='card'>
            <div class='card-label'>Menşe Çiftlik Konumu</div>
            <div class='card-value'>{data.FarmOrigin}</div>
        </div>
        <div class='card'>
            <div class='card-label'>Sevk İrsaliyesi / Giriş Tarihi</div>
            <div class='card-value'>{data.WaybillNumber} | {data.ArrivalDate:dd.MM.yyyy}</div>
        </div>
    </div>

    <!-- 2. VETERİNER ANTEMORTEM MUAYENE VE SAĞLIK RAPORU -->
    <div class='section-title'>2. 🩺 Veteriner Hekim Antemortem Muayene ve Kesim İzni</div>
    <div class='grid-4'>
        <div class='card'>
            <div class='card-label'>Görevli Veteriner Hekim</div>
            <div class='card-value'>{data.VetDoctorName}</div>
        </div>
        <div class='card'>
            <div class='card-label'>Rapor No &amp; Tarih</div>
            <div class='card-value'>{data.VetReportNumber}</div>
        </div>
        <div class='card'>
            <div class='card-label'>Vücut Sıcaklığı</div>
            <div class='card-value'>{data.VetBodyTemperature:N1} °C</div>
        </div>
        <div class='card'>
            <div class='card-label'>Kesim Onay Durumu</div>
            <div class='card-value'>{vetStatusBadge}</div>
        </div>
    </div>
    <div class='card' style='margin-bottom:10px;'>
        <div class='card-label'>Klinik Tanı ve Muayene Bulguları</div>
        <div style='font-size:12px; font-weight:500; color:#334155;'>{data.VetDiagnosis}</div>
    </div>

    <!-- 3. KESİMHANE, KARKAS VE SEUROP DERECELENDİRME -->
    <div class='section-title'>3. 🔪 Kesimhane, Karkas Tartım &amp; SEUROP Sınıflandırma</div>
    <div class='grid-4'>
        <div class='card'>
            <div class='card-label'>Karkas No / Kesim No</div>
            <div class='card-value' style='color:#b45309;'>{data.CarcassNumber}</div>
        </div>
        <div class='card'>
            <div class='card-label'>Sıcak Karkas Ağırlığı</div>
            <div class='card-value'>{data.HotCarcassWeightKg:N1} kg</div>
        </div>
        <div class='card'>
            <div class='card-label'>Soğuk Karkas Ağırlığı</div>
            <div class='card-value' style='color:#16a34a;'>{data.ColdCarcassWeightKg:N1} kg</div>
        </div>
        <div class='card'>
            <div class='card-label'>Karkas Randımanı</div>
            <div class='card-value' style='color:#16a34a;'>%{data.CarcassYieldPercentage:N2}</div>
        </div>
    </div>

    <div class='grid-4'>
        <div class='card'>
            <div class='card-label'>SEUROP Konformasyon</div>
            <div class='card-value'>Sınıf {data.ConformationClass}</div>
        </div>
        <div class='card'>
            <div class='card-label'>Yağlılık Derecesi</div>
            <div class='card-value'>{data.FatCoverScore} / 5</div>
        </div>
        <div class='card'>
            <div class='card-label'>Mermerleşme Skoru</div>
            <div class='card-value'>BMS {data.MarblingScore}</div>
        </div>
        <div class='card'>
            <div class='card-label'>24. Saat Karkas pH</div>
            <div class='card-value'>{data.PostMortemPh24:N2} pH</div>
        </div>
    </div>

    <div class='grid-3'>
        <div class='card'>
            <div class='card-label'>Soğuk Depo &amp; Kanca Askı</div>
            <div class='card-value'>{data.ColdStorageLocation} ({data.HookRailNumber})</div>
        </div>
        <div class='card'>
            <div class='card-label'>Yan Ürünler (Deri/Baş/Sakatat)</div>
            <div class='card-value'>Deri: {data.HideWeightKg:N0} kg | Sakatat: {data.OffalWeightKg:N0} kg</div>
        </div>
        <div class='card'>
            <div class='card-label'>Hesaplanan Karkas Kg Maliyeti</div>
            <div class='card-value' style='color:#dc2626;'>{data.CarcassCostPerKg:C2} / kg</div>
        </div>
    </div>

    <!-- 4. KARKAS PARÇALAMA VE ANATOMİK ET DÖKÜMÜ -->
    <div class='section-title'>4. 🥩 Karkas Parçalama ve Anatomik Et Çıktıları Dökümü (BOM)</div>
    <table>
        <thead>
            <tr>
                <th style='width:30px; text-align:center;'>#</th>
                <th>Anatomik Bölge</th>
                <th>Et Parça Adı</th>
                <th style='text-align:right;'>Net Kg</th>
                <th style='text-align:right;'>Randıman</th>
                <th>Kalite</th>
                <th style='text-align:right;'>Birim Maliyet</th>
                <th style='text-align:right;'>Toplam Değer</th>
                <th>Lot Numarası</th>
                <th>Depo Konumu</th>
            </tr>
        </thead>
        <tbody>
            {cutsRowsHtml}
        </tbody>
        <tfoot>
            <tr style='background:#f1f5f9; font-weight:bold;'>
                <td colspan='3' style='text-align:right;'>TOPLAM ET ÇIKTISI:</td>
                <td style='text-align:right; color:#16a34a; font-size:12px;'>{data.TotalDebonedKg:N2} kg</td>
                <td style='text-align:right;'>%{data.DeboningEfficiencyYield:N1}</td>
                <td colspan='2' style='text-align:right;'>TOPLAM ET DEĞERİ:</td>
                <td style='text-align:right; color:#b45309; font-size:12px;'>{data.TotalDebonedValue:C2}</td>
                <td colspan='2'></td>
            </tr>
        </tfoot>
    </table>

    <!-- 5. ONAY VE İMZA ALANI -->
    <div class='signatures'>
        <div class='sig-box'>
            <b>Sorumlu Veteriner Hekim</b><br>
            <span>{data.VetDoctorName}</span><br>
            <span style='color:#64748b; font-size:10px;'>Dip. No: {data.VetDiplomaNumber}</span><br><br>
            <span>İmza / Kaşe: _________________</span>
        </div>
        <div class='sig-box'>
            <b>Baş Kasap / Parçalama Şefi</b><br>
            <span>{data.ButcherName}</span><br>
            <span style='color:#64748b; font-size:10px;'>Mezbaha Hat Sorumlusu</span><br><br>
            <span>İmza: _________________</span>
        </div>
        <div class='sig-box'>
            <b>İşletme Müdürü / Kalite Güvence</b><br>
            <span>RoyPos Kalite Güvence Onayı</span><br>
            <span style='color:#64748b; font-size:10px;'>İzlenebilirlik Kodu: {data.DocumentSecurityHash}</span><br><br>
            <span>İmza / Mühür: _________________</span>
        </div>
    </div>

    <!-- 6. FOOTER -->
    <div class='footer'>
        <div>Bu belge RoyPos Kasap &amp; Mezbaha Otomasyon Sistemi tarafından dijital olarak üretilmiştir.</div>
        <div>Doğrulama URL: <b>{data.QrVerificationUrl}</b></div>
    </div>

</body>
</html>";
    }

    public async Task<string> ExportAndOpenPassportPdfAsync(string earTagOrCarcassNo, string? targetFolder = null)
    {
        var data = await GetAnimalPassportDataAsync(earTagOrCarcassNo);
        var html = GeneratePassportHtml(data);

        // Resolve folder
        if (string.IsNullOrEmpty(targetFolder))
        {
            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            targetFolder = Path.Combine(myDocs, "RoyPosReports");
        }

        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        var fileName = $"Hayvan_Pasaportu_{data.EarTagNumber.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.html";
        var filePath = Path.Combine(targetFolder, fileName);

        await File.WriteAllTextAsync(filePath, html, Encoding.UTF8);

        // Open directly in browser / print engine
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Otomatik rapor açma hatası: {Msg}", ex.Message);
        }

        return filePath;
    }
}
