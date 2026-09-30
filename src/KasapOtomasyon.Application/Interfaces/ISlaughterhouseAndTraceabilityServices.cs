using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Domain.Entities;

namespace KasapOtomasyon.Application.Interfaces;

public interface ISlaughterhouseService
{
    Task<List<string>> GetAllBreedsAsync(KasapOtomasyon.Domain.Enums.AnimalType? animalType = null);
    Task<AnimalBreed> AddBreedAsync(string breedName, KasapOtomasyon.Domain.Enums.AnimalType animalType);
    Task<List<AnimalIntakeDto>> GetAllIntakesAsync();
    Task<AnimalIntakeDto> RegisterAnimalIntakeAsync(AnimalIntake intake);
    Task<VeterinaryCheckDto> PerformVeterinaryCheckAsync(VeterinaryCheck check);
    Task<SlaughterRecordDto> ExecuteSlaughterAndWeighingAsync(SlaughterRecord slaughter);
    Task<List<SlaughterRecordDto>> GetAllSlaughterRecordsAsync();
    Task<List<ColdStorageRoom>> GetColdStorageRoomsAsync();
    Task<WasteLogDto> LogWasteAsync(WasteLog waste);
    Task<List<WasteLogDto>> GetAllWasteLogsAsync();
    Task<List<CarcassDeboningCutDto>> GetDeboningCutsByCarcassAsync(string carcassNumber);
    Task<List<CarcassDeboningCutDto>> SaveDeboningCutsAsync(string carcassNumber, List<CarcassDeboningCut> cuts);
    Task<List<CarcassDeboningCut>> GenerateStandardDeboningTemplateAsync(string carcassNumber, decimal carcassWeight, KasapOtomasyon.Domain.Enums.AnimalType animalType);
    Task<List<SlaughterServiceInvoiceDto>> GetSlaughterInvoicesAsync();
    Task<SlaughterServiceInvoiceDto> CreateSlaughterInvoiceAsync(SlaughterServiceInvoice invoice);
}

public interface ITraceabilityService
{
    Task<BidirectionalTraceabilityTreeDto?> GetTraceabilityTreeByEarTagAsync(string earTag);
    Task<BidirectionalTraceabilityTreeDto?> GetTraceabilityTreeByBarcodeOrLotAsync(string barcodeOrLot);
}

public interface IForensicAnomalyService
{
    Task<List<ForensicAnomalyDto>> GetActiveAnomaliesAsync();
    Task<List<ForensicAnomalyDto>> ScanAndDetectAnomaliesAsync();
    Task<bool> ResolveAnomalyAsync(int anomalyId, string notes);
}
