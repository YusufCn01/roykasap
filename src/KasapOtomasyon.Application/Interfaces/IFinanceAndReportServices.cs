using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Application.Interfaces;

public interface IFinanceService
{
    Task<List<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto> SaveCustomerAsync(Customer customer);
    Task<bool> AddCustomerTransactionAsync(CustomerTransaction transaction);
    Task<List<CustomerTransaction>> GetCustomerStatementAsync(int customerId);
    Task<bool> SendOverduePaymentSmsReminderAsync(int customerId);
    Task<CariAgingReportDto> GetCariAgingReportAsync();
    Task<bool> RecordPaymentAsync(int customerId, decimal amount, PaymentType paymentType, string description, string? documentNo);
    Task<bool> RecordDebitAsync(int customerId, decimal amount, string description, string? documentNo);
    Task<byte[]> ExportCustomerStatementPdfAsync(int customerId);
}

public interface ICashRegisterService
{
    Task<CashSessionSummaryDto?> GetActiveSessionAsync(int cashRegisterId = 1);
    Task<CashSessionSummaryDto> OpenSessionAsync(int cashRegisterId, int userId, string cashierName, decimal openingBalance);
    Task<CashSessionSummaryDto> CloseSessionAsync(int sessionId, decimal actualCashCounted, string? notes);
    Task<bool> AddCashTransactionAsync(int sessionId, string type, decimal amount, string description, int? userId);
    Task<List<CashSessionSummaryDto>> GetSessionHistoryAsync(int limit = 30);
}

public interface IReportService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    Task<List<DailyTurnoverReportDto>> GetDailyTurnoverReportAsync(DateTime startDate, DateTime endDate);
    Task<List<TopSellingProductDto>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int topCount = 10);
    Task<List<ProfitabilityReportDto>> GetProfitabilityReportAsync(DateTime startDate, DateTime endDate);
    Task<List<CarcassYieldByBreedReportDto>> GetCarcassYieldByBreedReportAsync();
    Task<List<DeboningMassBalanceReportDto>> GetDeboningMassBalanceReportAsync();
    Task<List<HaccpQualityComplianceReportDto>> GetHaccpComplianceReportAsync();
    Task<byte[]> ExportSalesToExcelAsync(DateTime startDate, DateTime endDate);
    Task<byte[]> ExportProductionYieldToExcelAsync(int? lotId);
    Task<byte[]> ExportFullExecutiveReportExcelAsync(DateTime startDate, DateTime endDate);
}

public interface IBackupService
{
    Task<bool> CreateBackupAsync(string targetFilePath);
    Task<bool> RestoreBackupAsync(string backupFilePath);
    Task<List<string>> GetBackupHistoryAsync();
}

public interface IAuditService
{
    Task LogAsync(string action, string entityName, string? entityId, string? details, int? userId = null, string userName = "Sistem", string? reason = null, string? supervisorApprover = null, string? oldValues = null, string? newValues = null, int tenantId = 1, int plantId = 1);
    Task<List<AuditLog>> GetAuditLogsAsync(DateTime? from = null, int limit = 100);
}
