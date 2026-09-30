using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Domain.Entities;

public class AnimalBreed : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Simental, Angus, Holstein, Merinos, Kıvırcık, Akkaraman, Saanen...
    public AnimalType AnimalType { get; set; } = AnimalType.Buyukbas;
    public string? Description { get; set; }
}

public class AnimalIntake : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string EarTagNumber { get; set; } = string.Empty; // TR340019283 (Benzersiz Küpe No)
    public string PassportNumber { get; set; } = string.Empty; // Hayvan Pasaport No
    public AnimalType AnimalType { get; set; } = AnimalType.Buyukbas;
    public string Breed { get; set; } = "Simental"; // Irk: Simental, Angus, Holstein, Merinos vb.
    public AnimalGender Gender { get; set; } = AnimalGender.Tosun;
    public int AgeMonths { get; set; } = 18;
    
    public decimal LiveWeightKg { get; set; } // Canlı Ağırlık (Örn: 612 kg)
    public decimal PurchasePrice { get; set; } // Alış Fiyatı (Örn: 115.000 TL)
    
    public int? ProducerCustomerId { get; set; } // Hayvan Sahibi / Üretici Cari
    public virtual Customer? ProducerCustomer { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string WaybillNumber { get; set; } = string.Empty; // Sevk İrsaliyesi No
    public string FarmOrigin { get; set; } = string.Empty; // Besi Çiftliği / İl / İlçe
    
    public DateTime ArrivalDate { get; set; } = DateTime.UtcNow;
    public DateTime? PlannedSlaughterDate { get; set; }
    public int SlaughterOrderNumber { get; set; } = 1; // Günlük Kesim Sırası
    public string BatchNumber { get; set; } = string.Empty; // PRT-2026-001
    public string RfidOrQrCode { get; set; } = string.Empty;
    
    // Status Flow
    public VeterinaryCheckStatus VeterinaryStatus { get; set; } = VeterinaryCheckStatus.Uygun;
    public bool IsApprovedForSlaughter { get; set; } = true; // Bloke Bayrağı
    public SlaughterStatus SlaughterStatus { get; set; } = SlaughterStatus.Bekliyor;
    public string? BlockReason { get; set; }
    
    public int? LivestockTransportTripId { get; set; }
    public virtual LivestockTransportTrip? LivestockTransportTrip { get; set; }

    public virtual ICollection<VeterinaryCheck> VeterinaryChecks { get; set; } = new List<VeterinaryCheck>();
    public virtual SlaughterRecord? SlaughterRecord { get; set; }
}

public class VeterinaryCheck : BaseEntity
{
    public int AnimalIntakeId { get; set; }
    public virtual AnimalIntake AnimalIntake { get; set; } = null!;

    public string VeterinarianName { get; set; } = string.Empty; // Dr. Vet. Ahmet
    public string DiplomaNumber { get; set; } = string.Empty;
    public DateTime CheckDate { get; set; } = DateTime.UtcNow;
    
    public bool IsAntemortem { get; set; } = true; // true = Kesim Öncesi, false = Kesim Sonrası Karkas
    public VeterinaryCheckStatus Status { get; set; } = VeterinaryCheckStatus.Uygun;
    public bool IsApprovedForSlaughter { get; set; } = true;
    
    public decimal BodyTemperature { get; set; } = 38.5m; // Vücut Sıcaklığı °C
    public string Diagnosis { get; set; } = "Klinik muayenede patolojik bulguya rastlanmadı. Kesime uygundur.";
    public string? QuarantineNotes { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
}

public class SlaughterRecord : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string SlaughterNumber { get; set; } = string.Empty; // KES-2026-000152
    public string CarcassNumber { get; set; } = string.Empty;   // KRK-2026-000152
    
    public int AnimalIntakeId { get; set; }
    public virtual AnimalIntake AnimalIntake { get; set; } = null!;

    public DateTime SlaughterDate { get; set; } = DateTime.UtcNow;
    public string ButcherPersonName { get; set; } = string.Empty;
    public string VeterinarianName { get; set; } = string.Empty;
    public string SlaughterLine { get; set; } = "Hat-A (Büyükbaş)";

    // Detailed Weight Measurements (Tartım Entegrasyonu)
    public decimal LiveWeightKg { get; set; }        // 612 kg
    public decimal HotCarcassWeightKg { get; set; }   // 328 kg (Sıcak Karkas)
    public decimal ColdCarcassWeightKg { get; set; }  // 321.4 kg (Soğuk Karkas - Soğuma Firesi düşülmüş)
    
    public decimal HeadWeightKg { get; set; }         // 28 kg (Baş)
    public decimal HideWeightKg { get; set; }         // 42 kg (Deri)
    public decimal OffalWeightKg { get; set; }        // 36 kg (Sakatat: Ciğer, İşkembe vb.)
    public decimal FatWeightKg { get; set; }          // 14 kg (Kavram / İç Yağı)
    public decimal BoneWeightKg { get; set; }         // 48 kg
    public decimal SlaughterWasteKg { get; set; }     // Kesim Firesi / Kan / Mide içeriği

    // Auto-Calculated Yield (Randıman Formülü)
    public decimal CarcassYieldPercentage => LiveWeightKg > 0 ? Math.Round((HotCarcassWeightKg / LiveWeightKg) * 100m, 2) : 0; // %53.59
    
    // Comprehensive Costing Engine (Activity-Based Costing)
    public decimal AnimalPurchaseCost { get; set; }   // 115.000 TL
    public decimal SlaughterLaborCost { get; set; }   // 2.500 TL (Kesim İşçilik)
    public decimal TransportationCost { get; set; }  // 3.000 TL (Nakliye)
    public decimal CoolingElectricityCost { get; set; } // 1.500 TL (Soğutma/Enerji)
    public decimal GeneralOverheadCost { get; set; }   // 3.000 TL (Genel Gider)
    
    public decimal TotalSlaughterCost => AnimalPurchaseCost + SlaughterLaborCost + TransportationCost + CoolingElectricityCost + GeneralOverheadCost; // 125.000 TL
    public decimal CarcassCostPerKg => ColdCarcassWeightKg > 0 ? Math.Round(TotalSlaughterCost / ColdCarcassWeightKg, 2) : 0; // 388.92 TL/kg

    public int? ColdStorageRoomId { get; set; }
    public string ColdStorageLocation { get; set; } = "Soğuk Hava Odası #1 - Bölme A-04";
    public string HookRailNumber { get; set; } = "Ray-1 / Askı-14"; // Monoray / Kanca Askı No
    
    // Industrial Meat Classification & SEUROP Standards (AB ve Dünya Standardı)
    public string ConformationClass { get; set; } = "R"; // S (Superior), E (Excellent), U (Very Good), R (Good), O (Fair), P (Poor)
    public int FatCoverScore { get; set; } = 3;           // 1 (Çok Az Yağlı) - 5 (Aşırı Yağlı)
    public int MarblingScore { get; set; } = 5;           // 1 - 9 BMS Mermerleşme Skoru
    public decimal PostMortemPh24 { get; set; } = 5.65m;  // 24. Saat Karkas pH Değeri (Normal: 5.4 - 5.8)
    
    public SlaughterStatus Status { get; set; } = SlaughterStatus.KarkasTartildi;
    
    public bool IsDeboned { get; set; } // Parçalamaya girdi mi?
    public string BarcodeOrQr { get; set; } = string.Empty;
    public virtual ICollection<CarcassDeboningCut> DeboningCuts { get; set; } = new List<CarcassDeboningCut>();
}

public class CarcassDeboningCut : BaseEntity
{
    public int SlaughterRecordId { get; set; }
    public virtual SlaughterRecord? SlaughterRecord { get; set; }

    public int? CuttingOrderId { get; set; }
    public virtual CuttingOrder? CuttingOrder { get; set; }
    
    public string CarcassNumber { get; set; } = string.Empty; // KRK-2026-000152
    public string EarTagNumber { get; set; } = string.Empty;   // TR340019283
    
    public string AnatomicalRegion { get; set; } = "Sırt / Bel"; // Sırt/Bel, But (Arka Çeyrek), Kol/Kürek (Ön Çeyrek), Gerdan, Döş/Göğüs, Kemik/Sakatat/Yan Ürün
    public string CutName { get; set; } = "Antrikot";            // Antrikot, Bonfile, Kontrfile, Tranç, Nuar, Kontrnuar, Sokum, Döş Kıyma, Kuşbaşı, İncik, Kemik vb.
    public decimal WeightKg { get; set; }                        // 11.5 kg
    public decimal YieldPercentage { get; set; }                 // % 3.5
    public decimal TheoreticalStandardRatio { get; set; }        // Standart Teori Oranı (Örn: %3.5)
    public decimal VariancePercentage { get; set; }              // Standart vs Gerçek Sapma
    public string QualityGrade { get; set; } = "1. Sınıf Lüks";   // 1. Sınıf Lüks, 2. Sınıf Kasaplık, Sanayi/Kıymalık
    public decimal UnitCostEstimated { get; set; }               // 850 TL/kg
    public decimal TotalCutValue { get; set; }                   // WeightKg * UnitCostEstimated
    
    public string LotNumber { get; set; } = string.Empty;        // PRC-2026-000152-01
    public string Barcode { get; set; } = string.Empty;          // 280015201150
    public string Gs1Barcode128 { get; set; } = string.Empty;    // (01)086800001(10)LOT(3102)01150(7003)260823
    public string TargetStorageLocation { get; set; } = "Soğuk Hava Odası #1 - Parçalama";
    public DateTime CutDate { get; set; } = DateTime.UtcNow;
    public string MasterButcher { get; set; } = string.Empty;
}

public class SlaughterServiceInvoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty; // FSN-2026-0001
    public int ProducerCustomerId { get; set; }
    public virtual Customer ProducerCustomer { get; set; } = null!;
    public int AnimalIntakeId { get; set; }
    public virtual AnimalIntake AnimalIntake { get; set; } = null!;

    public decimal SlaughterFeePerHead { get; set; } = 1500m;
    public decimal SlaughterFeePerKg { get; set; }
    public decimal DeboningServiceFee { get; set; }
    public decimal PackagingServiceFee { get; set; }
    public decimal ColdStorageServiceFee { get; set; }
    
    public decimal TotalServiceFee => SlaughterFeePerHead + SlaughterFeePerKg + DeboningServiceFee + PackagingServiceFee + ColdStorageServiceFee;
    public decimal PaidAmount { get; set; }
    public decimal RemainingBalance => TotalServiceFee - PaidAmount;
    public bool IsPaid { get; set; }
}

public class WasteLog : BaseEntity
{
    public string WasteNumber { get; set; } = string.Empty; // ZYT-2026-0001
    public WasteCauseType CauseType { get; set; } = WasteCauseType.ParcalamaFiresi;
    
    public int? ProductId { get; set; }
    public virtual Product? Product { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string LotOrCarcassNumber { get; set; } = string.Empty;
    
    public decimal WeightKg { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost => Math.Round(WeightKg * UnitCost, 2);
    
    public string ResponsiblePerson { get; set; } = string.Empty;
    public string ApprovedByPerson { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}

public class ColdStorageRoom : BaseEntity
{
    public string RoomCode { get; set; } = "SH-01";
    public string RoomName { get; set; } = "Karkas Dinlendirme Soğuk Hava Odası 1";
    public ColdStorageType StorageType { get; set; } = ColdStorageType.SogukHava;
    public decimal CurrentTemperature { get; set; } = 2.4m; // °C
    public decimal TargetMinTemp { get; set; } = 0.0m;
    public decimal TargetMaxTemp { get; set; } = 4.0m;
    public decimal HumidityPercentage { get; set; } = 85.0m;
    public bool IsAlarmTriggered => CurrentTemperature > TargetMaxTemp || CurrentTemperature < TargetMinTemp;
    public string AlarmMessage => IsAlarmTriggered ? $"⚠ Sıcaklık Sınır Dışı! ({CurrentTemperature:N1}°C - Limit: {TargetMinTemp}-{TargetMaxTemp}°C)" : "Normal";
}

public class ForensicAnomalyRecord : BaseEntity
{
    public string AnomalyCode { get; set; } = string.Empty;
    public AnomalyType Type { get; set; } = AnomalyType.KarkasStokKayip;
    public string Severity { get; set; } = "Danger"; // Danger, Warning, Info
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty; // KRK-2026-000152
    public decimal ExpectedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal DifferenceQuantity => Math.Abs(ExpectedQuantity - ActualQuantity);
    public decimal FinancialLossEstimated { get; set; }
    public DateTime DetectedDate { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; }
    public string? ResolutionNotes { get; set; }
}
