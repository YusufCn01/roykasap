using System.IO.Ports;
using System.Text.RegularExpressions;
using KasapOtomasyon.Application.Interfaces.Adapters;

namespace KasapOtomasyon.Infrastructure.Adapters;

public class CasScaleAdapter : ITeraziAdapter
{
    private SerialPort? _serialPort;
    public string BrandName => "CAS";
    public bool IsConnected => _serialPort != null && _serialPort.IsOpen;

    public Task<bool> ConnectAsync(string portName, int baudRate = 9600)
    {
        try
        {
            _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 1500,
                WriteTimeout = 1500
            };
            _serialPort.Open();
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task DisconnectAsync()
    {
        try
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }
        }
        catch { }
        return Task.CompletedTask;
    }

    public Task<ScaleReadingResult> ReadWeightAsync()
    {
        if (!IsConnected || _serialPort == null)
        {
            return Task.FromResult(new ScaleReadingResult { Success = false, ErrorMessage = "Terazi bağlı değil." });
        }

        try
        {
            var line = _serialPort.ReadLine();
            // CAS Format: ST,GS,+  1.250kg
            var match = Regex.Match(line, @"[+-]?\s*([0-9]+\.[0-9]+)");
            if (match.Success && decimal.TryParse(match.Groups[1].Value, out var weight))
            {
                var isStable = line.Contains("ST") || !line.Contains("US");
                return Task.FromResult(new ScaleReadingResult
                {
                    Success = true,
                    WeightKg = weight,
                    IsStable = isStable,
                    Unit = "kg"
                });
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ScaleReadingResult { Success = false, ErrorMessage = ex.Message });
        }

        return Task.FromResult(new ScaleReadingResult { Success = false, ErrorMessage = "Veri okunamadı." });
    }

    public Task<bool> TareAsync()
    {
        if (IsConnected && _serialPort != null)
        {
            try
            {
                _serialPort.WriteLine("T");
                return Task.FromResult(true);
            }
            catch { }
        }
        return Task.FromResult(false);
    }

    public Task<bool> ZeroAsync()
    {
        if (IsConnected && _serialPort != null)
        {
            try
            {
                _serialPort.WriteLine("Z");
                return Task.FromResult(true);
            }
            catch { }
        }
        return Task.FromResult(false);
    }
}

public class MockScaleAdapter : ITeraziAdapter
{
    private static readonly Random Rnd = new();
    private decimal _currentWeight = 1.450m;
    private bool _isConnected;

    public string BrandName => "SIMULATOR";
    public bool IsConnected => _isConnected;

    public Task<bool> ConnectAsync(string portName, int baudRate = 9600)
    {
        _isConnected = true;
        return Task.FromResult(true);
    }

    public Task DisconnectAsync()
    {
        _isConnected = false;
        return Task.CompletedTask;
    }

    public Task<ScaleReadingResult> ReadWeightAsync()
    {
        if (!_isConnected)
        {
            _isConnected = true; // Auto-connect simulation
        }

        // Realistic meat portion simulation between 0.350kg and 3.800kg
        var weights = new[] { 0.650m, 1.250m, 0.820m, 1.840m, 2.150m, 0.480m, 1.500m, 3.200m };
        _currentWeight = weights[Rnd.Next(weights.Length)] + Math.Round((decimal)Rnd.NextDouble() * 0.050m, 3);

        return Task.FromResult(new ScaleReadingResult
        {
            Success = true,
            WeightKg = _currentWeight,
            IsStable = true,
            Unit = "kg"
        });
    }

    public Task<bool> TareAsync()
    {
        _currentWeight = 0m;
        return Task.FromResult(true);
    }

    public Task<bool> ZeroAsync()
    {
        _currentWeight = 0m;
        return Task.FromResult(true);
    }
}
