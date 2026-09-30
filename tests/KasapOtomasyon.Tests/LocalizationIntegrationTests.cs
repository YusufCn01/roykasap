using FluentAssertions;
using KasapOtomasyon.Infrastructure.Services;
using Xunit;

namespace KasapOtomasyon.Tests;

public class LocalizationIntegrationTests
{
    private readonly LocalizationService _loc = new();

    [Fact]
    public void Should_Support_Trilingual_Languages_TR_EN_PL()
    {
        var supported = _loc.SupportedLanguages;
        supported.Should().HaveCount(3);
        supported.Select(l => l.Code).Should().Contain(new[] { "tr-TR", "en-GB", "pl-PL" });
    }

    [Theory]
    [InlineData("tr-TR", "Türkçe", "🇹🇷")]
    [InlineData("en-GB", "English", "🇬🇧")]
    [InlineData("pl-PL", "Polski", "🇵🇱")]
    public void Should_Switch_Language_And_Fire_LanguageChanged_Event(string code, string expectedName, string expectedFlag)
    {
        string? notifiedCode = null;
        _loc.LanguageChanged += (s, lang) => notifiedCode = lang;

        _loc.SetLanguage(code);

        _loc.CurrentLanguageCode.Should().Be(code);
        _loc.CurrentLanguage.DisplayName.Should().Be(expectedName);
        _loc.CurrentLanguage.FlagEmoji.Should().Be(expectedFlag);
        notifiedCode.Should().Be(code);
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-GB")]
    [InlineData("pl-PL")]
    public void Should_Contain_All_Shell_And_Shortcut_Keys(string langCode)
    {
        _loc.SetLanguage(langCode);

        var keys = new[]
        {
            "shell.title",
            "shell.brand",
            "shell.activePlant",
            "shell.shortcuts",
            "shell.backToMainMenu",
            "shell.notifications",
            "shell.notificationCenter",
            "shell.closeAll",
            "shell.logout",
            "shell.mainMenu",
            "shell.shortcutsModalTitle",
            "shortcut.guide",
            "shortcut.pos",
            "shortcut.slaughter",
            "shortcut.stock",
            "shortcut.products",
            "shortcut.finance",
            "shortcut.customers",
            "shortcut.scale",
            "shortcut.reports",
            "shortcut.einvoice",
            "shortcut.fullscreen",
            "shortcut.back"
        };

        foreach (var key in keys)
        {
            var translation = _loc.Get(key);
            translation.Should().NotBeNullOrWhiteSpace($"Key '{key}' should have a valid translation in {langCode}");
            translation.Should().NotBe(key, $"Key '{key}' should not return raw key in {langCode}");
        }
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-GB")]
    [InlineData("pl-PL")]
    public void Should_Contain_All_14_Module_Cards(string langCode)
    {
        _loc.SetLanguage(langCode);

        var moduleCardKeys = new[]
        {
            "menu.pos.title", "menu.pos.desc",
            "menu.slaughter.title", "menu.slaughter.desc",
            "menu.production.title", "menu.production.desc",
            "menu.reprocessing.title", "menu.reprocessing.desc",
            "menu.stock.title", "menu.stock.desc",
            "menu.products.title", "menu.products.desc",
            "menu.invoices.title", "menu.invoices.desc",
            "menu.finance.title", "menu.finance.desc",
            "menu.customers.title", "menu.customers.desc",
            "menu.einvoice.title", "menu.einvoice.desc",
            "menu.reports.title", "menu.reports.desc",
            "menu.users.title", "menu.users.desc",
            "menu.license.title", "menu.license.desc",
            "menu.settings.title", "menu.settings.desc"
        };

        foreach (var key in moduleCardKeys)
        {
            var translation = _loc.Get(key);
            translation.Should().NotBeNullOrWhiteSpace($"Module key '{key}' should have translation in {langCode}");
            translation.Should().NotBe(key, $"Module key '{key}' should not return raw key in {langCode}");
        }
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-GB")]
    [InlineData("pl-PL")]
    public void Should_Contain_All_KPI_And_Section_Headers(string langCode)
    {
        _loc.SetLanguage(langCode);

        var sectionKeys = new[]
        {
            "menu.kpi.todayTurnover",
            "menu.kpi.slaughterWeight",
            "menu.kpi.carcassYield",
            "menu.kpi.haccpStatus",
            "menu.kpi.haccpSafe",
            "menu.section.operations",
            "menu.section.commercial",
            "menu.section.management"
        };

        foreach (var key in sectionKeys)
        {
            var translation = _loc.Get(key);
            translation.Should().NotBeNullOrWhiteSpace($"Section key '{key}' should have translation in {langCode}");
            translation.Should().NotBe(key, $"Section key '{key}' should not return raw key in {langCode}");
        }
    }

    [Fact]
    public void Should_Provide_Distinct_Translations_Across_Languages()
    {
        _loc.SetLanguage("tr-TR");
        var trPos = _loc.Get("menu.pos.title");
        var trSlaughter = _loc.Get("menu.slaughter.title");
        var trReprocessing = _loc.Get("menu.reprocessing.title");

        _loc.SetLanguage("en-GB");
        var enPos = _loc.Get("menu.pos.title");
        var enSlaughter = _loc.Get("menu.slaughter.title");
        var enReprocessing = _loc.Get("menu.reprocessing.title");

        _loc.SetLanguage("pl-PL");
        var plPos = _loc.Get("menu.pos.title");
        var plSlaughter = _loc.Get("menu.slaughter.title");
        var plReprocessing = _loc.Get("menu.reprocessing.title");

        // Assert distinct values
        trPos.Should().NotBe(enPos);
        trPos.Should().NotBe(plPos);
        enPos.Should().NotBe(plPos);

        trSlaughter.Should().NotBe(enSlaughter);
        trSlaughter.Should().NotBe(plSlaughter);
        enSlaughter.Should().NotBe(plSlaughter);

        trReprocessing.Should().NotBe(enReprocessing);
        trReprocessing.Should().NotBe(plReprocessing);
        enReprocessing.Should().NotBe(plReprocessing);
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-GB")]
    [InlineData("pl-PL")]
    public void Should_Contain_All_Login_Portal_Keys(string langCode)
    {
        _loc.SetLanguage(langCode);

        var loginKeys = new[]
        {
            "login.title",
            "login.subtitle",
            "login.description",
            "login.portalTitle",
            "login.portalSubtitle",
            "login.quickRoles",
            "login.roleAdmin",
            "login.roleButcher",
            "login.roleVet",
            "login.roleCashier",
            "login.tabStandard",
            "login.tabPin",
            "login.username",
            "login.password",
            "login.showPassword",
            "login.rememberMe",
            "login.loginBtn",
            "login.pinPrompt",
            "login.systemStatus",
            "login.onlineStatus"
        };

        foreach (var key in loginKeys)
        {
            var translation = _loc.Get(key);
            translation.Should().NotBeNullOrWhiteSpace($"Login key '{key}' should have translation in {langCode}");
            translation.Should().NotBe(key, $"Login key '{key}' should not return raw key in {langCode}");
        }
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-GB")]
    [InlineData("pl-PL")]
    public void Should_Contain_All_Submodule_Keys_For_Catalog_Finance_And_Customers(string langCode)
    {
        _loc.SetLanguage(langCode);

        var keys = new[]
        {
            // Products & Catalog
            "products.listTitle", "products.new", "products.code", "products.name",
            "products.category", "products.type", "products.salePrice", "products.costPrice",
            "products.stock", "products.shelfLifeDays", "products.editorTitle", "products.unit",
            "products.barcodePluSettings", "products.generateBarcode", "products.pluCode",
            "products.barcode", "products.quickButton", "products.deleteBtn", "products.saveBtn",

            // Finance
            "finance.title", "finance.customer", "finance.phone", "finance.balance",
            "finance.debit", "finance.credit", "finance.action", "finance.sendSms",
            "finance.statementTitle", "finance.currentBalance", "finance.cashManagementTitle",
            "finance.openingBalance", "finance.cashSales", "finance.cardSales",
            "finance.calculatedBalance", "finance.actualCount", "finance.closeSessionBtn",

            // Customers
            "customers.title", "customers.subtitle", "customers.totalReceivable",
            "customers.overdueRisk", "customers.currentAging", "customers.critical90Plus",
            "customers.all", "customers.debtors", "customers.overdue"
        };

        foreach (var key in keys)
        {
            var translation = _loc.Get(key);
            translation.Should().NotBeNullOrWhiteSpace($"Submodule key '{key}' should have translation in {langCode}");
            translation.Should().NotBe(key, $"Submodule key '{key}' should not return raw key in {langCode}");
        }
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-GB")]
    [InlineData("pl-PL")]
    public void Should_Contain_All_Submodule_Keys_For_Reports_Users_License_Settings_And_Invoices(string langCode)
    {
        _loc.SetLanguage(langCode);

        var keys = new[]
        {
            // Reports & BI
            "reports.title", "reports.subtitle", "reports.yieldExcel", "reports.salesExcel", "reports.masterExcel",
            "reports.kpi.turnover", "reports.kpi.tonnage", "reports.kpi.yield", "reports.kpi.massBalance", "reports.kpi.haccp", "reports.kpi.coldStorage",

            // E-Invoice
            "einvoice.title", "einvoice.sendGib", "einvoice.detailsTitle", "einvoice.itemsTitle", "einvoice.vatAmount", "einvoice.sendSelectedBtn",

            // Users & RBAC
            "users.title", "users.username", "users.fullName", "users.role", "users.active", "users.lastLogin",
            "users.newUserTitle", "users.password", "users.pinCode", "users.roleGroup", "users.saveBtn",

            // License & Security
            "license.title", "license.subtitle", "license.fingerprintTitle", "license.fingerprintDesc",
            "license.copy", "license.currentStatusTitle", "license.company", "license.status",
            "license.validUntil", "license.remainingDays", "license.loadLicenseBtn", "license.featuresTitle",

            // Settings & Hardware
            "settings.title", "settings.subtitle", "settings.scaleSettingsTitle", "settings.scaleBrand",
            "settings.scalePort", "settings.testScaleBtn", "settings.integrationsTitle", "settings.smsProvider",
            "settings.einvoiceProvider", "settings.testIntegratorBtn", "settings.backupTitle",
            "settings.backupDir", "settings.backupNowBtn", "settings.backupHistory",

            // Purchase Invoices
            "invoices.title", "invoices.subtitle", "invoices.monthlyVolume", "invoices.monthlyTonnage",
            "invoices.tevkifat", "invoices.pendingPayables", "invoices.tabList", "invoices.tabCreate"
        };

        foreach (var key in keys)
        {
            var translation = _loc.Get(key);
            translation.Should().NotBeNullOrWhiteSpace($"Submodule key '{key}' should have translation in {langCode}");
            translation.Should().NotBe(key, $"Submodule key '{key}' should not return raw key in {langCode}");
        }
    }
}

