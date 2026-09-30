using KasapOtomasyon.Application.DTOs;

namespace KasapOtomasyon.Application.Interfaces;

public interface IShelfLifeReprocessingService
{
    /// <summary>
    /// Reyon son kullanım tarihi yaklaşan veya dolmuş et partilerini getirir.
    /// </summary>
    Task<List<ExpiringBatchDto>> GetExpiringBatchesAsync(int thresholdDays = 3);

    /// <summary>
    /// Bir stok partisinin Reyon Son Kullanım Tarihini kullanıcı girişiyle günceller.
    /// </summary>
    Task<bool> UpdateShelfDisplayExpiryDateAsync(int stockItemId, DateTime newShelfDate, string reason, string operatorName);

    /// <summary>
    /// Sistemde kayıtlı tüm şarküteri, köfte ve soslu et reçetelerini getirir.
    /// </summary>
    Task<List<ProcessingRecipeDto>> GetAllRecipesAsync();

    /// <summary>
    /// Belirli bir reçeteyi detayları ve bileşenleriyle getirir.
    /// </summary>
    Task<ProcessingRecipeDto?> GetRecipeByIdAsync(int recipeId);

    /// <summary>
    /// Kullanıcının arayüzden girdiği yeni veya güncellenen ürün reçetesini kaydeder.
    /// </summary>
    Task<ProcessingRecipeDto> SaveCustomRecipeAsync(CustomRecipeInputDto recipeDto);

    /// <summary>
    /// Kullanıcı reçetesini siler.
    /// </summary>
    Task<bool> DeleteRecipeAsync(int recipeId);

    /// <summary>
    /// Seçilen et partisi ve reçeteye göre gereken baharat/sos/yağ miktarlarını,
    /// randımanı, yeni birim maliyeti ve uzatılmış SKT'yi canlı olarak simüle eder.
    /// </summary>
    Task<ReprocessingSimulationResultDto> SimulateReprocessingAsync(int stockItemId, int recipeId, decimal rawMeatKg);

    /// <summary>
    /// Dönüştürme işlemini onaylar: Hammaddeyi reyon stokundan düşer,
    /// yeni mamulü (sucuk, köfte, soslu et) yeni parti ve uzatılmış SKT ile stoğa alır,
    /// hareket ve denetim kayıtlarını üretir.
    /// </summary>
    Task<ReprocessingExecutionResultDto> ExecuteReprocessingAsync(ExecuteReprocessingRequestDto request);

    /// <summary>
    /// Kasap işleme makine parkını ve hijyen/sanitasyon durumlarını getirir.
    /// </summary>
    Task<List<MeatProcessingEquipmentDto>> GetEquipmentsAsync();

    /// <summary>
    /// Makine sanitasyon/dezenfeksiyon durumunu günceller.
    /// </summary>
    Task<bool> UpdateEquipmentSanitizationAsync(int equipmentId, string sanitizedBy, string agent, bool isReady);

    /// <summary>
    /// Baharat, katkı, sos, ambalaj ve bağırsak sarf malzeme stoklarını getirir.
    /// </summary>
    Task<List<AuxiliaryMaterialStockDto>> GetAuxiliaryMaterialStocksAsync();

    /// <summary>
    /// Baharat/sos/sarf stok miktarını günceller.
    /// </summary>
    Task<bool> AdjustAuxiliaryMaterialStockAsync(int materialId, decimal quantityDelta, string reason);

    /// <summary>
    /// Yapılan gıda güvenliği, pH, su aktivitesi ve sıcaklık kontrol kayıtlarını listeler.
    /// </summary>
    Task<List<ReprocessingQualityCheckDto>> GetQualityChecksAsync();

    /// <summary>
    /// Yeni bir kalite ve pH/Aw muayenesi kaydeder. pH > 6.20 ise parti otomatik bloke edilir.
    /// </summary>
    Task<ReprocessingQualityCheckDto> RecordQualityCheckAsync(RecordQualityCheckRequestDto request);

    /// <summary>
    /// Reyondan kurtarılan etlerin toplam kg, TL değeri ve sağlanan net kârlılık ROI özetini getirir.
    /// </summary>
    Task<FinancialRoiSummaryDto> GetFinancialRoiSummaryAsync();

    /// <summary>
    /// Bakanlık ve AB/Polish gıda standartlarına uygun bileşen, alerjen ve besin değerli yasal etiket üretir.
    /// </summary>
    Task<LegalThermalLabelDto> GenerateThermalLabelAsync(int stockItemId, int? recipeId = null);

    /// <summary>
    /// Reyon ömrü kritik partiye akıllı kademeli indirim (%15, %20, %30) uygular.
    /// </summary>
    Task<SmartMarkdownResultDto> ApplySmartMarkdownAsync(SmartMarkdownRequestDto request);

    /// <summary>
    /// Sucuk kurutma ve fermantasyon odasındaki partileri getirir.
    /// </summary>
    Task<List<SucukCuringBatchDto>> GetCuringBatchesAsync();

    /// <summary>
    /// Kurutma odasındaki sucuğun günlük tartım, pH ve nem/sıcaklık ölçümünü kaydeder.
    /// </summary>
    Task<bool> RecordCuringMeasurementAsync(RecordCuringMeasurementRequestDto request);

    /// <summary>
    /// Koku, doku ve renk skorlu organoleptik duyusal muayeneleri listeler.
    /// </summary>
    Task<List<OrganolepticSensoryCheckDto>> GetSensoryChecksAsync();

    /// <summary>
    /// Yeni organoleptik muayene kaydeder (Ortalama skor < 3.0 ise reddedilir).
    /// </summary>
    Task<OrganolepticSensoryCheckDto> RecordSensoryCheckAsync(RecordSensoryCheckRequestDto request);

    /// <summary>
    /// Resmi imha ve lisanslı rendering bertaraf tutanaklarını listeler.
    /// </summary>
    Task<List<ReprocessingDisposalRecordDto>> GetDisposalRecordsAsync();

    /// <summary>
    /// Bozuk veya pH kritik etler için çift onaylı resmi bertaraf tutanağı oluşturur.
    /// </summary>
    Task<ReprocessingDisposalRecordDto> CreateDisposalRecordAsync(CreateDisposalRecordRequestDto request);

    /// <summary>
    /// Kasap ve reyon personeli sıfır fire prim ve ödül sıralamasını getirir.
    /// </summary>
    Task<List<ButcherWasteIncentiveDto>> GetButcherIncentivesAsync();

    /// <summary>
    /// Barkod veya parti numarasıyla hızlı reyon SKT taraması yapar ve aksiyon önerir.
    /// </summary>
    Task<FastScanBatchResultDto> ScanBatchBarcodeAsync(string barcodeOrLot);
}
