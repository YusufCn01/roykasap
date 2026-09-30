using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using KasapOtomasyon.Application.Interfaces;

namespace KasapOtomasyon.WPF.Markup;

public class LocalizationSource : INotifyPropertyChanged
{
    private static LocalizationSource? _instance;
    public static LocalizationSource Instance => _instance ??= new LocalizationSource();

    private ILocalizationService? _locService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Initialize(ILocalizationService locService)
    {
        if (_locService != null)
        {
            _locService.LanguageChanged -= OnLanguageChanged;
        }

        _locService = locService;
        _locService.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, string langCode)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    public string this[string key] => _locService != null ? _locService.Get(key) : key;
    public string Get(string key, params object[] args) => _locService != null ? _locService.Get(key, args) : key;
    public string CurrentLanguageCode => _locService?.CurrentLanguageCode ?? "tr-TR";
}

[MarkupExtensionReturnType(typeof(string))]
public class LocExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;

    public LocExtension() { }

    public LocExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
            return string.Empty;

        var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        if (provideValueTarget?.TargetObject is DependencyObject &&
            provideValueTarget.TargetProperty is DependencyProperty)
        {
            var binding = new Binding($"[{Key}]")
            {
                Source = LocalizationSource.Instance,
                Mode = BindingMode.OneWay
            };
            return binding.ProvideValue(serviceProvider);
        }

        return LocalizationSource.Instance[Key];
    }
}
