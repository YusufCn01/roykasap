using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using KasapOtomasyon.Application.DTOs;

namespace KasapOtomasyon.Infrastructure.Licensing;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public class HardwareFingerprintProvider
{
    private const string Salt = "KASAP_MEZBAHA_OTOMASYON_v2026_SECURE_SALT";

    public MachineFingerprintDto GetMachineFingerprint()
    {
        var cpuId = GetCpuId();
        var diskSerial = GetDiskSerial();
        var macAddress = GetMacAddress();
        var machineName = Environment.MachineName;

        var rawString = $"{cpuId}|{diskSerial}|{macAddress}|{Salt}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawString));
        var hexString = Convert.ToHexString(hashBytes);

        // Format into readable chunks: e.g. KASAP-A1B2-C3D4-E5F6-7890
        var formatted = $"KASAP-{hexString[..4]}-{hexString.Substring(4, 4)}-{hexString.Substring(8, 4)}-{hexString.Substring(12, 4)}";

        return new MachineFingerprintDto
        {
            Fingerprint = formatted,
            CpuId = cpuId,
            DiskSerial = diskSerial,
            MacAddress = macAddress,
            MachineName = machineName
        };
    }

    private string GetCpuId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                var id = obj["ProcessorId"]?.ToString();
                if (!string.IsNullOrEmpty(id))
                    return id.Trim();
            }
        }
        catch
        {
            // Fallback for non-WMI or restricted environments
        }

        return Environment.ProcessorCount.ToString() + "-" + Environment.UserName;
    }

    private string GetDiskSerial()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE DeviceID='C:'");
            foreach (var obj in searcher.Get())
            {
                var serial = obj["VolumeSerialNumber"]?.ToString();
                if (!string.IsNullOrEmpty(serial))
                    return serial.Trim();
            }
        }
        catch
        {
            // Fallback
        }

        return Environment.SystemDirectory;
    }

    private string GetMacAddress()
    {
        try
        {
            var nic = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up &&
                                     n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            if (nic != null)
            {
                return nic.GetPhysicalAddress().ToString();
            }
        }
        catch
        {
            // Fallback
        }

        return "001122334455";
    }
}
