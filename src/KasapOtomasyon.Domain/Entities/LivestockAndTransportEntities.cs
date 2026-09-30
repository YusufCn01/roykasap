namespace KasapOtomasyon.Domain.Entities;

public class Farm : BaseEntity, ICompanyScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;

    public string RegistrationNumber { get; set; } = string.Empty; // TR-34-FARM-0012
    public string FarmName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "TR";
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string AssignedVeterinarian { get; set; } = string.Empty;
    public string HealthCertificateNumber { get; set; } = string.Empty;
    public decimal QualityScore { get; set; } = 95.0m; // 0 - 100 Supplier Quality Score
    public decimal HistoricalCarcassYieldAverage { get; set; } = 54.5m; // %
    public int TotalAnimalsDelivered { get; set; } = 0;

    public virtual ICollection<LivestockTransportTrip> TransportTrips { get; set; } = new List<LivestockTransportTrip>();
}

public class TransportVehicle : BaseEntity, ICompanyScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;

    public string PlateNumber { get; set; } = string.Empty; // 34 ABC 123
    public string TrailerPlateNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = "Çift Katlı Hayvan Nakil Kamyonu";
    public int AnimalCapacityHead { get; set; } = 40;
    public bool HasGpsTracking { get; set; } = true;
    public bool HasTemperatureSensors { get; set; } = true;
    public string OfficialTransportPermitNo { get; set; } = "NAK-2026-0081";
    public DateTime InspectionValidUntil { get; set; } = DateTime.UtcNow.AddYears(1);
}

public class TransportDriver : BaseEntity, ICompanyScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;

    public string FullName { get; set; } = string.Empty;
    public string NationalIdNumber { get; set; } = string.Empty;
    public string DriverLicenseNumber { get; set; } = string.Empty;
    public string LivestockHandlerCertificateNo { get; set; } = "SRC5-CANLI-9921"; // Canlı Hayvan Taşıyıcı Yeterlilik Belgesi
    public string PhoneNumber { get; set; } = string.Empty;
}

public enum TransportTripStatus
{
    Scheduled = 1,  // Planlandı
    InTransit = 2,  // Yolda / Sevk Ediliyor
    Arrived = 3,    // Mezbahaya Ulaştı
    Completed = 4,  // Kabul Tamamlandı
    Incident = 5    // Kaza / Anormal Durum
}

public class LivestockTransportTrip : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string TripCode { get; set; } = string.Empty; // TRP-2026-0001
    public int FarmId { get; set; }
    public virtual Farm Farm { get; set; } = null!;

    public int VehicleId { get; set; }
    public virtual TransportVehicle Vehicle { get; set; } = null!;

    public int DriverId { get; set; }
    public virtual TransportDriver Driver { get; set; } = null!;

    public string WaybillNumber { get; set; } = string.Empty;
    public string VeterinaryHealthReportNumber { get; set; } = string.Empty;

    public DateTime DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public decimal AverageCompartmentTempCelsius { get; set; } = 18.2m;

    public int TotalAnimalsLoaded { get; set; }
    public int TotalAnimalsReceivedAlive { get; set; }
    public int MortalityCount { get; set; } = 0; // Taşıma Sırasındaki Telefat / Ölüm
    public int InjuredCount { get; set; } = 0;   // Yaralanan Hayvan

    public TransportTripStatus Status { get; set; } = TransportTripStatus.Arrived;
    public string? IncidentNotes { get; set; }

    public virtual ICollection<AnimalIntake> Animals { get; set; } = new List<AnimalIntake>();
}
