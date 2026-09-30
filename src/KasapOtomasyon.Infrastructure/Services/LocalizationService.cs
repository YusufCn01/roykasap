using System.Globalization;
using KasapOtomasyon.Application.Interfaces;

namespace KasapOtomasyon.Infrastructure.Services;

public class LocalizationService : ILocalizationService
{
    private string _currentLanguageCode = "tr-TR";
    public event EventHandler<string>? LanguageChanged;

    private static readonly List<LanguageInfo> _supportedLanguages = new()
    {
        new LanguageInfo { Code = "tr-TR", DisplayName = "Türkçe", FlagEmoji = "🇹🇷", CultureName = "tr-TR" },
        new LanguageInfo { Code = "en-GB", DisplayName = "English", FlagEmoji = "🇬🇧", CultureName = "en-GB" },
        new LanguageInfo { Code = "pl-PL", DisplayName = "Polski", FlagEmoji = "🇵🇱", CultureName = "pl-PL" }
    };

    private static readonly Dictionary<string, Dictionary<string, string>> _translations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tr-TR"] = new(StringComparer.OrdinalIgnoreCase)
        {
            // Common
            ["common.save"] = "Kaydet",
            ["common.cancel"] = "İptal",
            ["common.delete"] = "Sil",
            ["common.edit"] = "Düzenle",
            ["common.print"] = "Yazdır",
            ["common.search"] = "Ara...",
            ["common.filter"] = "Filtrele",
            ["common.status"] = "Durum",
            ["common.success"] = "İşlem Başarılı",
            ["common.error"] = "Hata",
            ["common.warning"] = "Uyarı",
            ["common.confirm"] = "Onayla",
            ["common.back"] = "Geri",
            ["common.refresh"] = "Yenile",
            ["common.export"] = "Dışa Aktar",
            ["common.close"] = "Kapat",
            ["common.total"] = "Toplam",
            ["common.date"] = "Tarih",
            ["common.actions"] = "İşlemler",

            // Tenant & Plant
            ["tenant.title"] = "Çoklu Şirket & Tesis Yönetimi",
            ["tenant.holding"] = "Holding / Grup",
            ["tenant.company"] = "Şirket",
            ["tenant.plant"] = "Tesis / Mezbaha",
            ["tenant.unit"] = "İşletme Birimi",

            // Animal & Intake
            ["animal.earTag"] = "Bakanlık Küpe No",
            ["animal.passportNo"] = "Pasaport No",
            ["animal.breed"] = "Hayvan Irkı",
            ["animal.gender"] = "Cinsiyet",
            ["animal.liveWeight"] = "Canlı Ağırlık (kg)",
            ["animal.producer"] = "Üretici / Besici",
            ["animal.farmOrigin"] = "Menşe Çiftlik",
            ["animal.arrivalDate"] = "Kabul Tarihi",
            ["animal.intakeTitle"] = "Hayvan Kabul İstasyonu",

            // Veterinary
            ["vet.title"] = "Veteriner Antemortem Muayene",
            ["vet.doctor"] = "Görevli Veteriner Hekim",
            ["vet.reportNo"] = "Rapor No",
            ["vet.temperature"] = "Vücut Sıcaklığı (°C)",
            ["vet.diagnosis"] = "Klinik Teşhis",
            ["vet.approved"] = "✓ Kesime Uygun (Onaylı)",
            ["vet.blocked"] = "⛔ Kesim Bloke Edildi",
            ["vet.quarantine"] = "Karantinaya Alındı",

            // Slaughter & Carcass
            ["slaughter.title"] = "Kesimhane & Karkas Yönetimi",
            ["slaughter.slaughterNo"] = "Kesim No",
            ["slaughter.carcassNo"] = "Karkas No",
            ["slaughter.butcher"] = "Usta Kasap",
            ["slaughter.hotWeight"] = "Sıcak Karkas (kg)",
            ["slaughter.coldWeight"] = "Soğuk Karkas (kg)",
            ["slaughter.yield"] = "Karkas Randımanı (%)",
            ["slaughter.seurop"] = "SEUROP Sınıfı",
            ["slaughter.fatScore"] = "Yağlılık Skoru (1-5)",
            ["slaughter.marbling"] = "Mermerleşme (BMS)",
            ["slaughter.ph24"] = "24h pH Değeri",
            ["slaughter.scaleRead"] = "⚖️ Kantardan Oku",

            // Deboning & Mass Balance
            ["cutting.title"] = "Karkas Parçalama & Et Çıktıları (BOM)",
            ["cutting.region"] = "Anatomik Bölge",
            ["cutting.cutName"] = "Et Parça Adı",
            ["cutting.netWeight"] = "Net Ağırlık (kg)",
            ["cutting.unitCost"] = "Birim Maliyet (TL/kg)",
            ["cutting.totalValue"] = "Toplam Değer (TL)",
            ["cutting.massBalance"] = "Kütle Dengesi Doğrulaması",
            ["cutting.unexplainedLoss"] = "Açıklanamayan Kütle Farkı",

            // WMS & Traceability
            ["wms.title"] = "Depo & WMS Yönetimi",
            ["wms.warehouse"] = "Depo",
            ["wms.lot"] = "Parti / Lot No",
            ["wms.barcode"] = "Barkod (GS1-128)",
            ["traceability.title"] = "Kırılmaz İzlenebilirlik Zinciri",
            ["traceability.passportPdf"] = "📄 Resmi Hayvan Pasaportu (PDF)",

            // Anti-Fraud & Audit
            ["fraud.title"] = "Denetim & Anti-Fraud Merkezi",
            ["fraud.riskScore"] = "Risk Skoru",
            ["fraud.auditTrail"] = "Değişmez Denetim İzi (SHA-256)",
            ["fraud.supervisorApproval"] = "Süpervizör Onayı Gerekli",

            // Reyon SKT & Reprocessing (TR)
            ["reprocessing.title"] = "Reyon SKT Kurtarma & Katma Değerli Dönüşüm Sistemi",
            ["reprocessing.subtitle"] = "Son kullanım tarihi yaklaşan etleri sucuk, köfte ve soslu ürünlere dönüştürerek fireyi önleyin",
            ["reprocessing.batches.title"] = "Reyon SKT Kritik Partiler (Kurtarma Bekleyen Etler)",
            ["reprocessing.batches.desc"] = "Reyonda son kullanma tarihi 2 gün veya daha az kalmış taze et partileri",
            ["reprocessing.recipes.title"] = "Dönüşüm Reçeteleri (BOM)",
            ["reprocessing.recipes.desc"] = "Kullanıcı tanımlı sucuk, köfte, soslu et reçeteleri ve randıman parametreleri",
            ["reprocessing.simulation.title"] = "Dönüşüm Simülasyonu & Malzeme İhtiyacı",
            ["reprocessing.simulation.desc"] = "Girilen hammaddeye göre gereken baharat, yağ, marinat ve yeni SKT hesaplaması",
            ["reprocessing.urgency.urgent"] = "🚨 Acil Kurtarma (Son Saatler)",
            ["reprocessing.urgency.warning"] = "⚠️ Kritik (1-2 Gün Kaldı)",
            ["reprocessing.urgency.approaching"] = "ℹ️ Yaklaşıyor (3 Gün Kaldı)",
            ["reprocessing.action.urgent"] = "Derhal fermente sucuk veya acılı köfteye dönüştürülmeli (+30 ila 60 gün SKT)",
            ["reprocessing.action.warning"] = "Soslu marinasyona veya kasap köftesine dönüştürülmesi önerilir (+7 ila 10 gün SKT)",
            ["reprocessing.action.approaching"] = "Haftalık şarküteri üretim planına dahil edilebilir",
            ["reprocessing.field.product"] = "Et Ürünü",
            ["reprocessing.field.lotNumber"] = "Parti Lot No",
            ["reprocessing.field.warehouse"] = "Depo / Konum",
            ["reprocessing.field.shelfLocation"] = "Reyon Rafı",
            ["reprocessing.field.currentQty"] = "Mevcut Miktar (kg)",
            ["reprocessing.field.unitCost"] = "Birim Maliyet",
            ["reprocessing.field.shelfExpiry"] = "Reyon SKT",
            ["reprocessing.field.daysRemaining"] = "Kalan Süre",
            ["reprocessing.field.status"] = "Durum / Öneri",
            ["reprocessing.field.action"] = "İşlem",
            ["reprocessing.btn.changeShelfDate"] = "📅 Reyon SKT Değiştir",
            ["reprocessing.btn.selectForConversion"] = "🔄 Dönüşüme Al",
            ["reprocessing.btn.newRecipe"] = "➕ Yeni Reçete Tanımla",
            ["reprocessing.btn.editRecipe"] = "✏️ Reçeteyi Düzenle",
            ["reprocessing.btn.execute"] = "✓ DÖNÜŞÜMÜ GERÇEKLEŞTİR VE STOĞA AL",
            ["reprocessing.modal.shelfDate.title"] = "Reyon Son Kullanım Tarihini Güncelle",
            ["reprocessing.modal.shelfDate.desc"] = "Seçilen partinin satış reyonundaki son teşhir tarihini kullanıcı olarak belirleyin",
            ["reprocessing.modal.shelfDate.newDate"] = "Yeni Reyon Son Kullanma Tarihi",
            ["reprocessing.modal.shelfDate.reason"] = "Güncelleme Gerekçesi / Açıklama",
            ["reprocessing.modal.shelfDate.reasonHint"] = "Örn: Soğuk zincir kontrolü yapıldı, reyon sergileme süresi 1 gün uzatıldı",
            ["reprocessing.modal.recipe.title"] = "Kullanıcı Ürün Reçetesi Tanımlama & Düzenleme",
            ["reprocessing.modal.recipe.desc"] = "Sucuk, köfte veya soslu marine et reçetesinin oranlarını ve yeni raf ömrünü belirleyin",
            ["reprocessing.recipe.code"] = "Reçete Kodu",
            ["reprocessing.recipe.name"] = "Reçete Adı",
            ["reprocessing.recipe.category"] = "Ürün Kategorisi",
            ["reprocessing.recipe.targetProduct"] = "Üretilecek Mamul Adı",
            ["reprocessing.recipe.yield"] = "Beklenen Randıman (%)",
            ["reprocessing.recipe.yieldHelp"] = "Açıklama: Sucukta kuruma nedeniyle %90-92, soslu ette sos emilimiyle %105-110 olur",
            ["reprocessing.recipe.shelfExtension"] = "İlave Raf Ömrü (Gün)",
            ["reprocessing.recipe.shelfExtensionHelp"] = "Açıklama: Dönüşüm sonrası yeni partinin kazanacağı ek kullanım süresi (örn. Sucuk +60 gün, Köfte +6 gün, Soslu et +10 gün)",
            ["reprocessing.recipe.instructions"] = "Hazırlama & Dinlendirme Talimatı",
            ["reprocessing.recipe.instructionsHelp"] = "Usta kasap ve hijyen kuralları için hazırlama yöntemi",
            ["reprocessing.recipe.descHelp"] = "Reçetenin lezzet profili ve kullanım amacı açıklaması",
            ["reprocessing.recipe.ingredients"] = "Reçete Bileşenleri (Hammadde, Yağ, Baharat, Sos, Tuz)",
            ["reprocessing.recipe.addIngredient"] = "➕ Yeni Bileşen Ekle",
            ["reprocessing.recipe.rawMeatUsed"] = "Dönüştürülecek Et Miktarı (kg)",
            ["reprocessing.recipe.rawMeatUsedHelp"] = "Reyondaki partiden ne kadar etin bu reçeteye aktarılacağı",
            ["reprocessing.sim.totalInput"] = "Girdi Et Değeri",
            ["reprocessing.sim.auxCost"] = "Yardımcı Malzeme Maliyeti",
            ["reprocessing.sim.outputKg"] = "Elde Edilecek Net Mamul (kg)",
            ["reprocessing.sim.newUnitCost"] = "Yeni Birim Maliyet (TL/kg)",
            ["reprocessing.sim.newExpiry"] = "Hesaplanan Yeni SKT",
            ["reprocessing.haccp.whyCuring"] = "🥩 Gıda Güvenliği Bilgisi: Neden Marinasyon & Sucuk Yapımı Raf Ömrünü Uzatır?",
            ["reprocessing.haccp.whyCuringText"] = "1) Asidik marinasyon (sirke, limon, yoğurt) yüzey pH'ını düşürerek bakteri üremesini durdurur. 2) Kaya tuzu serbest su aktivitesini (Aw) azaltır. 3) Sarımsak, kimyon, kekik ve biberiye gibi doğal baharatlar güçlü antimikrobiyal ve antioksidan etkiye sahiptir. 4) Bu işlem fireyi ve gıda israfını önleyerek işletmeye yüksek katma değer kazandırır.",
            ["reprocessing.success.completed"] = "✓ Başarılı Dönüşüm: {0} kg {1} eti, {2} kg {3} mamulüne dönüştürüldü. Yeni SKT: {4} (Parti No: {5}). Stok hareketleri işlendi.",
            
            // Categories & Item Types
            ["recipe.category.mince"] = "Kıyma & Kuşbaşı",
            ["recipe.category.freshSausage"] = "Taze Kasap Sucuğu",
            ["recipe.category.dryFermentedSausage"] = "Fermente / Kangal Sucuk",
            ["recipe.category.meatballs"] = "Kasap Köfte & Burger",
            ["recipe.category.marinated"] = "Soslu & Marine Edilmiş Etler",
            ["recipe.category.smoked"] = "Füme & Pastırma",
            ["recipe.category.cooked"] = "Kavurma & Pişmiş Ürünler",
            ["recipe.itemtype.meat"] = "Ana Et Hammaddesi",
            ["recipe.itemtype.fat"] = "Yağ (Kavram/Kuyruk)",
            ["recipe.itemtype.spice"] = "Baharat Karışımı",
            ["recipe.itemtype.sauce"] = "Sos & Sıvı Marinat",
            ["recipe.itemtype.salt"] = "Kaya Tuzu & Kürleme",
            ["recipe.itemtype.casing"] = "Doğal Bağırsak / Klips",
            ["recipe.itemtype.packaging"] = "Vakum Ambalajı",

            // Reprocessing Comprehensive Tabs & Sub-modules
            ["reprocessing.tab.overview"] = "🔪 Kurtarma Tezgâhı & Canlı Simülasyon",
            ["reprocessing.tab.qc"] = "🛡️ Gıda Güvenliği, pH & Kalite (CCP)",
            ["reprocessing.tab.aux"] = "🧂 Baharat, Sos & Ambalaj Deposu",
            ["reprocessing.tab.equipment"] = "⚙️ Makine & Ekipman Parkı",
            ["reprocessing.tab.roi"] = "📊 Finansal Fire Önleme & Kârlılık (ROI)",

            // Equipment Keys
            ["eq.code"] = "Makine Kodu",
            ["eq.name"] = "Ekipman Adı",
            ["eq.type"] = "Cihaz Türü",
            ["eq.capacity"] = "İşleme Kapasitesi",
            ["eq.status"] = "Çalışma Durumu",
            ["eq.sanitized"] = "Son Dezenfeksiyon",
            ["eq.sanitizedBy"] = "Dezenfekte Eden Usta",
            ["eq.agent"] = "Kullanılan Dezenfektan",
            ["eq.markSanitized"] = "✨ Dezenfeksiyonu Doğrula & Hazırla",

            // Auxiliary Stock Keys
            ["aux.code"] = "Malzeme Kodu",
            ["aux.name"] = "Malzeme Adı",
            ["aux.type"] = "Kategori",
            ["aux.stock"] = "Mevcut Miktar",
            ["aux.unitCost"] = "Birim Maliyet",
            ["aux.totalValue"] = "Toplam Değer",
            ["aux.minStock"] = "Kritik Eşik",
            ["aux.storage"] = "Saklama Koşulu",
            ["aux.adjust"] = "Miktar Güncelle (+/-)",

            // Quality & Food Safety Keys
            ["qc.number"] = "Muayene No",
            ["qc.batch"] = "Parti No / Lot",
            ["qc.product"] = "Et Türü / Ürün",
            ["qc.ph"] = "Ölçülen pH",
            ["qc.aw"] = "Su Aktivitesi (Aw)",
            ["qc.temp"] = "Çekirdek Sıcaklığı (°C)",
            ["qc.verdict"] = "Kalite Kararı",
            ["qc.inspector"] = "HACCP Kontrolörü",
            ["qc.newCheck"] = "🧪 Yeni Kalite & pH Kontrolü Ekle",
            ["qc.phRuleTitle"] = "🔬 Kritik Gıda Güvenliği Limiti (CCP):",
            ["qc.phRuleText"] = "Taze et pH değeri normalde 5.40 - 5.80 aralığındadır. pH > 6.20 ise kokuşma ve mikrobiyal bozulma başlamıştır; bu et kesinlikle soslansa dahi kurtarılamaz, sistemce derhal bloke edilir ve imhaya ayrılır.",

            // ROI & Financial Keys
            ["roi.title"] = "Reyon Fire Önleme & Katma Değer Analitiği",
            ["roi.totalKg"] = "Kurtarılan Toplam Et",
            ["roi.totalMeatCost"] = "Kurtarılan Et Maliyeti",
            ["roi.outputValue"] = "Üretilen Mamul Satış Değeri",
            ["roi.netProfit"] = "Kazanılan Net Ekstra Kâr",
            ["roi.efficiency"] = "Fire Önleme Başarısı",
            ["roi.batchesSaved"] = "İmhadan Kurtarılan Parti",

            // Advanced Operations Tabs & Sub-modules
            ["reprocessing.tab.curing"] = "🌡️ Sucuk Kurutma Odası (IoT)",
            ["reprocessing.tab.sensory"] = "👃 Organoleptik & Duyusal Test",
            ["reprocessing.tab.disposal"] = "🗑️ Resmi İmha & Bertaraf Tutanağı",
            ["reprocessing.tab.incentives"] = "🏆 Personel Sıfır Fire Primi",
            ["reprocessing.tab.label"] = "🏷️ Termal Etiket & Alerjen Baskısı",
            ["markdown.apply"] = "Akıllı İndirim Uygula",
            ["fastscan.title"] = "📱 Hızlı Reyon Barkod Tarama",

            // Shell & Navigation (TR)
            ["shell.title"] = "ROY KASAP ENTEGRE ERP",
            ["shell.brand"] = "ROY KASAP ENTERPRISE CONTROL HUB",
            ["shell.activePlant"] = "🏢 Tesis: TR-34 İstanbul Entegre Mezbaha & Et İşleme Tesisi",
            ["shell.plantShort"] = "🏢 İstanbul Mezbaha & İşleme Tesisi (PLN-IST-01)",
            ["shell.scaleConnected"] = "Terazi: Bağlı (Simülasyon Modu)",
            ["shell.scaleReady"] = "Terazi: {0} (Hazır)",
            ["shell.shortcuts"] = "⌨ Kısayollar [F1]",
            ["shell.backToMainMenu"] = "Ana Menüye Dön",
            ["shell.notifications"] = "Bildirimler",
            ["shell.notificationCenter"] = "🔔 Bildirim Merkezi",
            ["shell.closeAll"] = "Tümünü Kapat",
            ["shell.logout"] = "🚪 Çıkış",
            ["shell.mainMenu"] = "Ana Menü",
            ["shell.shortcutsModalTitle"] = "⌨ Sistem Klavye Kısayolları",

            // Shortcuts Guide (TR)
            ["shortcut.guide"] = "Kısayollar Rehberi",
            ["shortcut.pos"] = "Hızlı Satış (POS)",
            ["shortcut.slaughter"] = "Mezbaha & Üretim",
            ["shortcut.stock"] = "Stok & Depo Takibi",
            ["shortcut.products"] = "Ürün & Barkod Tanım",
            ["shortcut.finance"] = "Finans & Kasa Yönetimi",
            ["shortcut.customers"] = "Cari Müşteri Ekstresi",
            ["shortcut.scale"] = "Canlı Terazi Oku",
            ["shortcut.reports"] = "Raporlar & Excel",
            ["shortcut.einvoice"] = "GİB E-Fatura Kesme",
            ["shortcut.fullscreen"] = "Tam Ekran / Pencereli",
            ["shortcut.back"] = "Ana Menüye Dön",

            // Main Menu KPI & Section Titles (TR)
            ["menu.kpi.todayTurnover"] = "BUGÜNKÜ CİRO",
            ["menu.kpi.slaughterWeight"] = "KESİM & KARKAS",
            ["menu.kpi.carcassYield"] = "KARKAS RANDIMANI",
            ["menu.kpi.haccpStatus"] = "HACCP CCP DURUMU",
            ["menu.kpi.haccpSafe"] = "✅ Güvenli & Uyumlu",
            ["menu.section.operations"] = "ET İŞLEME, MEZBAHA & SATIŞ OPERASYONLARI",
            ["menu.section.commercial"] = "TİCARİ, FİNANS VE CARİ HESAPLAR",
            ["menu.section.management"] = "YÖNETİCİ KONTROL MERKEZİ, GÜVENLİK VE SİSTEM AYARLARI",

            // 14 Module Cards (TR)
            ["menu.pos.title"] = "HIZLI DOKUNMATİK SATIŞ (POS)",
            ["menu.pos.badge"] = "CANLI TERAZİ",
            ["menu.pos.desc"] = "CAS terazi canlı tartım, GS1 barkodlar, nakit/kart parçalı ödeme ve anında fiş basımı",
            ["menu.pos.enter"] = "[F2] GİRİŞ ➔",

            ["menu.slaughter.title"] = "Mezbaha & Kesim MES",
            ["menu.slaughter.desc"] = "Canlı kantar, Ante-Mortem muayene, SEUROP derecelendirme ve resmi PDF pasaport",

            ["menu.production.title"] = "Parçalama & Şarküteri (BOM)",
            ["menu.production.desc"] = "Kütle dengesi (Δ ≤ %0.5), sucuk/köfte reçeteleri, kuruma firesi ve parti üretimi",

            ["menu.reprocessing.title"] = "Reyon SKT & Reçeteli Dönüşüm",
            ["menu.reprocessing.desc"] = "SKT yaklaşan etleri sucuk, köfte ve soslu etlere dönüştürme ve reçete yönetimi",

            ["menu.stock.title"] = "WMS Depo & SSCC-18",
            ["menu.stock.desc"] = "Soğuk hava depoları, GS1 SSCC-18 paletleme, raf adresi ve tek tıkla karantina",

            ["menu.products.title"] = "Ürün & Barkod Kataloğu",
            ["menu.products.desc"] = "Et ve şarküteri ürünleri, terazi PLU kodları ve GS1-128 barkod tasarımcısı",

            ["menu.invoices.title"] = "Alım Faturaları & Müstahsil",
            ["menu.invoices.desc"] = "Toptan et/canlı hayvan alımları, tevkifat kesintileri ve müstahsil makbuzu",

            ["menu.finance.title"] = "Finans & Kasa Yönetimi",
            ["menu.finance.desc"] = "Gün açılışı/kapanışı, nakit/kredi kartı sayımı ve kasa devir farkı mutabakatı",

            ["menu.customers.title"] = "Cari Müşteri Hesapları",
            ["menu.customers.desc"] = "Müşteri borç/alacak bakiyeleri, hesap ekstresi ve tek tıkla SMS mutabakatı",

            ["menu.einvoice.title"] = "E-Fatura & E-Arşiv (GİB)",
            ["menu.einvoice.desc"] = "Gelir İdaresi Başkanlığı e-Belge oluşturma, gönderme ve resmi arşivleme",

            ["menu.reports.title"] = "Yönetici Raporları & BI",
            ["menu.reports.desc"] = "Ciro, ABC net karkas maliyeti, randıman analizleri ve Excel (.xlsx) ihracı",

            ["menu.users.title"] = "Personel & 18 Rol (RBAC)",
            ["menu.users.desc"] = "Kullanıcı hesapları, PIN kodları, SHA-256 denetim izi ve yetki matrisi",

            ["menu.license.title"] = "Kurumsal Lisans & RSA",
            ["menu.license.desc"] = "Donanım parmak izi kilidi, şifreli RSA lisans ve modül aktivasyonu",

            ["menu.settings.title"] = "Sistem Ayarları & Donanım",
            ["menu.settings.desc"] = "CAS Terazi COM portu, SMS servisi, veritabanı yedekleme ve tercihler",

            // POS Core Controls (TR)
            ["pos.searchPlaceholder"] = "Barkod Okutun veya Ürün Arayın (Enter)...",
            ["pos.liveScale"] = "⚖ TERAZİ [F8]: ",
            ["pos.all"] = "TÜMÜ",
            ["pos.clear"] = "Temizle",
            ["pos.product"] = "Ürün",
            ["pos.quantity"] = "Miktar",
            ["pos.unitPrice"] = "Birim Fiyat",
            ["pos.total"] = "Toplam",
            ["pos.subtotal"] = "Ara Toplam",
            ["pos.discount"] = "İskonto",
            ["pos.grandTotal"] = "ÖDENECEK TUTAR",
            ["pos.cashSale"] = "💵 NAKİT SATIŞ [F2]",
            ["pos.cardSale"] = "💳 KREDİ KARTI",
            ["pos.splitPay"] = "Parçalı Ödeme",
            ["pos.parkSale"] = "Satışı Beklet",
            ["pos.parkedSales"] = "Bekleyenler",

            // Production & Slaughter Controls (TR)
            ["prod.title"] = "MEZBAHA & PARÇALAMA AĞACI (BOM) MODÜLÜ",
            ["prod.subtitle"] = "Karkas Girişi, Otomatik Randıman Hesabı ve Ağırlıklı Maliyet Dağıtımı",
            ["prod.lotSelect"] = "İşlenecek Karkas Partisi:",
            ["prod.recipeSelect"] = "Parçalama Reçetesi:",
            ["prod.reprocessingBtn"] = "🥩 REYON SKT & REÇETELİ DÖNÜŞÜM",
            ["prod.deliRecipeBtn"] = "🥓 ŞARKÜTERİ & SUCUK REÇETESİ (BOM)",

            // Products & Barcode Catalog (TR)
            ["products.listTitle"] = "Ürün ve Barkod Listesi",
            ["products.productCount"] = "({0} Ürün)",
            ["products.new"] = "+ Yeni Ürün Ekle",
            ["products.code"] = "Kod",
            ["products.name"] = "Ürün Adı",
            ["products.category"] = "Kategori",
            ["products.type"] = "Tür",
            ["products.salePrice"] = "Satış Fiyatı",
            ["products.costPrice"] = "Maliyet Fiyatı",
            ["products.stock"] = "Mevcut Stok",
            ["products.shelfLifeDays"] = "SKT (Gün)",
            ["products.editorTitle"] = "Ürün Tanım ve Barkod Editörü",
            ["products.unit"] = "Ölçü Birimi",
            ["products.barcodePluSettings"] = "Barkod / PLU Ayarları",
            ["products.generateBarcode"] = "⚡ Yeni Barkod Üret",
            ["products.pluCode"] = "PLU Kodu",
            ["products.barcode"] = "Barkod (EAN-13 / Terazi)",
            ["products.quickButton"] = "Hızlı Satış Butonlarında Göster",
            ["products.deleteBtn"] = "Sil",
            ["products.saveBtn"] = "✔ ÜRÜNÜ KAYDET",

            // Finance & Cash Management (TR)
            ["finance.title"] = "Cari Müşteri Borç / Alacak Takibi",
            ["finance.customer"] = "Müşteri / Firma",
            ["finance.phone"] = "Telefon",
            ["finance.balance"] = "Bakiye",
            ["finance.debit"] = "Borç",
            ["finance.credit"] = "Alacak",
            ["finance.action"] = "İşlem",
            ["finance.sendSms"] = "📩 SMS Hatırlat",
            ["finance.statementTitle"] = "Cari Hesap Ekstresi: {0}",
            ["finance.currentBalance"] = "Güncel Bakiye: {0}",
            ["finance.cashManagementTitle"] = "Kasa Yönetimi & Gün Sonu Raporu",
            ["finance.openingBalance"] = "Açılış Devir Nakit:",
            ["finance.cashSales"] = "Nakit Satışlar:",
            ["finance.cardSales"] = "Kredi Kartı Satışlar:",
            ["finance.calculatedBalance"] = "Hesaplanan Kasa Mevcudu:",
            ["finance.actualCount"] = "Kasa Sayım Tutarı (Fiziki Nakit):",
            ["finance.closeSessionBtn"] = "🔒 GÜN SONU KASAYI KAPAT & RAPORLA",

            // Customer Accounts & Aging (TR)
            ["customers.title"] = "KURUMSAL CARİ VE MÜŞTERİ HESAP YÖNETİMİ",
            ["customers.subtitle"] = "Toptan et alıcıları, restoran carileri, çiftlik besicileri ve yaşlandırma analizleri",
            ["customers.totalReceivable"] = "TOPLAM PİYASA ALACAĞI",
            ["customers.overdueRisk"] = "VADESİ GEÇMİŞ RİSK",
            ["customers.currentAging"] = "VADESİNDE (0-30 GÜN)",
            ["customers.critical90Plus"] = "90+ GÜN KRİTİK GECİKME",
            ["customers.all"] = "Tüm Cariler",
            ["customers.debtors"] = "💰 Borçlular",
            ["customers.overdue"] = "⚠️ Vadesi Geçmiş",

            // E-Invoice & GİB (TR)
            ["einvoice.title"] = "GİB E-Fatura & E-Arşiv Listesi",
            ["einvoice.sendGib"] = "🚀 GİB'e İlet",
            ["einvoice.detailsTitle"] = "Fatura Detayları & Kalemler",
            ["einvoice.itemsTitle"] = "Fatura Kalemleri (Et Ürünleri):",
            ["einvoice.vatAmount"] = "KDV Tutarı (%1):",
            ["einvoice.sendSelectedBtn"] = "🚀 SEÇİLİ FATURAYI GİB'E İLET",

            // Reports & Executive BI (TR)
            ["reports.title"] = "YÖNETİCİ KONTROL VE KURUMSAL BI ANALİTİK MERKEZİ",
            ["reports.subtitle"] = "Canlı ciro, ABC karkas net maliyeti, SEUROP randımanları ve kütle dengesi analizi",
            ["reports.yieldExcel"] = "📊 Randıman Excel",
            ["reports.salesExcel"] = "🛒 Satış Excel",
            ["reports.masterExcel"] = "📥 KURUMSAL MASTER EXCEL (.xlsx)",
            ["reports.kpi.turnover"] = "BUGÜNKÜ CİRO",
            ["reports.kpi.tonnage"] = "KESİM & KARKAS TONAJI",
            ["reports.kpi.yield"] = "KARKAS RANDIMANI",
            ["reports.kpi.massBalance"] = "KÜTLE DENGESİ UYUMU",
            ["reports.kpi.haccp"] = "HACCP KRİTİK NOKTALAR (CCP)",
            ["reports.kpi.coldStorage"] = "SOĞUK HAVA DEPOLARI",

            // User Management & RBAC (TR)
            ["users.title"] = "Personel ve Kullanıcı Hesapları",
            ["users.username"] = "Kullanıcı Adı",
            ["users.fullName"] = "Adı Soyadı",
            ["users.role"] = "Rol / Yetki",
            ["users.active"] = "Aktif",
            ["users.lastLogin"] = "Son Giriş",
            ["users.newUserTitle"] = "+ Yeni Personel / Kullanıcı Ekle",
            ["users.password"] = "Giriş Şifresi",
            ["users.pinCode"] = "Hızlı Kasiyer PIN Kodu (4 Haneli)",
            ["users.roleGroup"] = "Rol / Yetki Grubu",
            ["users.saveBtn"] = "✔ PERSONELİ SİSTEME KAYDET",

            // License & Security (TR)
            ["license.title"] = "LİSANS VE GÜVENLİK YÖNETİMİ",
            ["license.subtitle"] = "Donanım Kilitli RSA Lisanslama, Paket Yetkileri ve Aktivasyon",
            ["license.fingerprintTitle"] = "Donanım Kimlik Bilgileri (Hardware Fingerprint)",
            ["license.fingerprintDesc"] = "Bu bilgisayara özel üretilen tekil donanım parmak izidir. Yeni lisans talebinde bulunurken bu kimliği bayinize iletiniz.",
            ["license.copy"] = "Kopyala",
            ["license.currentStatusTitle"] = "Mevcut Lisans Durumu",
            ["license.company"] = "Lisans Sahibi:",
            ["license.status"] = "Lisans Durumu:",
            ["license.validUntil"] = "Son Geçerlilik Tarihi:",
            ["license.remainingDays"] = "Kalan Gün Sayısı:",
            ["license.loadLicenseBtn"] = "📂 .LIC LİSANS DOSYASI YÜKLE & AKTİF ET",
            ["license.featuresTitle"] = "Aktif Modül ve Özellik Listesi",

            // Settings & Hardware (TR)
            ["settings.title"] = "SİSTEM VE DONANIM AYARLARI",
            ["settings.subtitle"] = "Terazi Seri Port Sürücüsü, SMS, E-Fatura Entegrasyonu ve Yedekleme",
            ["settings.scaleSettingsTitle"] = "⚖ Terazi Sürücü & Port Ayarları",
            ["settings.scaleBrand"] = "Terazi Modeli / Protokol",
            ["settings.scalePort"] = "Seri Port (COM Port)",
            ["settings.testScaleBtn"] = "Terazi Bağlantısını Test Et",
            ["settings.integrationsTitle"] = "📩 SMS & E-Fatura Entegrasyonu",
            ["settings.smsProvider"] = "SMS Sağlayıcı",
            ["settings.einvoiceProvider"] = "E-Fatura Özel Entegratör",
            ["settings.testIntegratorBtn"] = "Entegratör Bağlantısını Test Et",
            ["settings.backupTitle"] = "💾 Veritabanı Yedekleme ve Kurtarma",
            ["settings.backupDir"] = "Yedekleme Klasörü",
            ["settings.backupNowBtn"] = "💾 ŞİMDİ YEDEK AL",
            ["settings.backupHistory"] = "Geçmiş Yedek Dosyaları",

            // Procurement Invoices & Producer (TR)
            ["invoices.title"] = "KURUMSAL ALIM FATURALARI & MÜSTAHSİL MAKBUZU YÖNETİMİ",
            ["invoices.subtitle"] = "Karkas et, canlı hayvan, tevkifatlı toptan alım, müstahsil stopajı ve otomatik stok maliyetleme",
            ["invoices.monthlyVolume"] = "AYLIK ALIM HACMİ",
            ["invoices.monthlyTonnage"] = "AYLIK ET TONAJI",
            ["invoices.tevkifat"] = "ET TEVKİFATI (9/10)",
            ["invoices.pendingPayables"] = "BEKLEYEN TEDARİKÇİ BORCU",
            ["invoices.tabList"] = "Alım Faturaları Listesi",
            ["invoices.tabCreate"] = "Yeni Fatura / Müstahsil Girişi",

            // Login Portal (TR)
            ["login.title"] = "ROY KASAP ENTEGRE ERP",
            ["login.subtitle"] = "Global Mezbaha, Et İşleme & Kasap Satış Yönetim Platformu",
            ["login.description"] = "Çiftlikten sofraya tüm et zincirini yöneten, adli denetim güvenceli ve yapay zekâ destekli kurumsal ERP & MES yönetim ekosistemi.",
            ["login.portalTitle"] = "İstasyon Giriş Portalı",
            ["login.portalSubtitle"] = "Lütfen yetkili kullanıcı bilgilerinizi veya 4 haneli PIN kodunuzu giriniz.",
            ["login.quickRoles"] = "HIZLI ROL SEÇİMİ (DEMO / İSTASYON TESTİ)",
            ["login.roleAdmin"] = "👑 Yönetici",
            ["login.roleButcher"] = "🔪 Baş Kasap",
            ["login.roleVet"] = "🩺 Veteriner",
            ["login.roleCashier"] = "🛒 Kasiyer",
            ["login.tabStandard"] = "Kullanıcı Adı / Şifre",
            ["login.tabPin"] = "Hızlı Kasiyer PIN",
            ["login.username"] = "KULLANICI ADI",
            ["login.password"] = "ŞİFRE",
            ["login.showPassword"] = "👁 Göster",
            ["login.rememberMe"] = "Beni Hatırla",
            ["login.loginBtn"] = "SİSTEME GİRİŞ YAP ➔",
            ["login.pinPrompt"] = "4 Haneli İstasyon PIN Kodunuzu Giriniz:",
            ["login.systemStatus"] = "Sistem Durumu: ",
            ["login.onlineStatus"] = "Çevrimiçi & Şifreli Veritabanı Aktif"
        },

        ["en-GB"] = new(StringComparer.OrdinalIgnoreCase)
        {
            // Common
            ["common.save"] = "Save",
            ["common.cancel"] = "Cancel",
            ["common.delete"] = "Delete",
            ["common.edit"] = "Edit",
            ["common.print"] = "Print",
            ["common.search"] = "Search...",
            ["common.filter"] = "Filter",
            ["common.status"] = "Status",
            ["common.success"] = "Operation Succeeded",
            ["common.error"] = "Error",
            ["common.warning"] = "Warning",
            ["common.confirm"] = "Confirm",
            ["common.back"] = "Back",
            ["common.refresh"] = "Refresh",
            ["common.export"] = "Export",
            ["common.close"] = "Close",
            ["common.total"] = "Total",
            ["common.date"] = "Date",
            ["common.actions"] = "Actions",

            // Tenant & Plant
            ["tenant.title"] = "Multi-Company & Plant Management",
            ["tenant.holding"] = "Holding / Group",
            ["tenant.company"] = "Company",
            ["tenant.plant"] = "Plant / Slaughterhouse",
            ["tenant.unit"] = "Facility Unit",

            // Animal & Intake
            ["animal.earTag"] = "Ministry Ear Tag No",
            ["animal.passportNo"] = "Passport No",
            ["animal.breed"] = "Breed",
            ["animal.gender"] = "Gender",
            ["animal.liveWeight"] = "Live Weight (kg)",
            ["animal.producer"] = "Farmer / Supplier",
            ["animal.farmOrigin"] = "Farm Origin",
            ["animal.arrivalDate"] = "Arrival Date",
            ["animal.intakeTitle"] = "Livestock Receiving Station",

            // Veterinary
            ["vet.title"] = "Ante-Mortem Veterinary Inspection",
            ["vet.doctor"] = "Official Veterinarian",
            ["vet.reportNo"] = "Report No",
            ["vet.temperature"] = "Body Temperature (°C)",
            ["vet.diagnosis"] = "Clinical Findings",
            ["vet.approved"] = "✓ Approved for Slaughter",
            ["vet.blocked"] = "⛔ Slaughter Blocked",
            ["vet.quarantine"] = "Quarantined",

            // Slaughter & Carcass
            ["slaughter.title"] = "Slaughterhouse & Carcass Management",
            ["slaughter.slaughterNo"] = "Slaughter No",
            ["slaughter.carcassNo"] = "Carcass No",
            ["slaughter.butcher"] = "Master Butcher",
            ["slaughter.hotWeight"] = "Hot Carcass (kg)",
            ["slaughter.coldWeight"] = "Cold Carcass (kg)",
            ["slaughter.yield"] = "Carcass Yield (%)",
            ["slaughter.seurop"] = "SEUROP Class",
            ["slaughter.fatScore"] = "Fat Score (1-5)",
            ["slaughter.marbling"] = "Marbling (BMS)",
            ["slaughter.ph24"] = "24h pH Value",
            ["slaughter.scaleRead"] = "⚖️ Read from Scale",

            // Deboning & Mass Balance
            ["cutting.title"] = "Carcass Deboning & Primal Cuts (BOM)",
            ["cutting.region"] = "Anatomical Region",
            ["cutting.cutName"] = "Primal Cut Name",
            ["cutting.netWeight"] = "Net Weight (kg)",
            ["cutting.unitCost"] = "Unit Cost",
            ["cutting.totalValue"] = "Total Value",
            ["cutting.massBalance"] = "Mass Balance Verification",
            ["cutting.unexplainedLoss"] = "Unexplained Mass Variance",

            // WMS & Traceability
            ["wms.title"] = "Warehouse & WMS Management",
            ["wms.warehouse"] = "Warehouse",
            ["wms.lot"] = "Batch / Lot No",
            ["wms.barcode"] = "Barcode (GS1-128)",
            ["traceability.title"] = "Unbroken Traceability Chain",
            ["traceability.passportPdf"] = "📄 Official Animal Passport (PDF)",

            // Anti-Fraud & Audit
            ["fraud.title"] = "Audit & Anti-Fraud Center",
            ["fraud.riskScore"] = "Risk Score",
            ["fraud.auditTrail"] = "Immutable Audit Trail (SHA-256)",
            ["fraud.supervisorApproval"] = "Supervisor Approval Required",

            // Reyon SKT & Reprocessing (EN)
            ["reprocessing.title"] = "Display Shelf-Life Salvage & Value-Added Reprocessing Engine",
            ["reprocessing.subtitle"] = "Prevent food waste by transforming near-expiry counter meat into sausages, patties, and marinated cuts",
            ["reprocessing.batches.title"] = "Critical Shelf-Life Batches (Awaiting Salvage)",
            ["reprocessing.batches.desc"] = "Fresh meat batches with shelf display expiry of 2 days or less",
            ["reprocessing.recipes.title"] = "Reprocessing Recipes (BOM)",
            ["reprocessing.recipes.desc"] = "User-customizable sausage, meatball, and marinated cut formulas with yield parameters",
            ["reprocessing.simulation.title"] = "Transformation Simulation & Ingredient Requirements",
            ["reprocessing.simulation.desc"] = "Real-time calculation of required spices, fats, sauces, and new extended expiry date",
            ["reprocessing.urgency.urgent"] = "🚨 Urgent Salvage (Final Hours)",
            ["reprocessing.urgency.warning"] = "⚠️ Critical (1-2 Days Left)",
            ["reprocessing.urgency.approaching"] = "ℹ️ Approaching (3 Days Left)",
            ["reprocessing.action.urgent"] = "Must be converted immediately into fermented sausage or spicy patties (+30 to 60 days shelf life)",
            ["reprocessing.action.warning"] = "Recommended for sauce marination or butcher patties (+7 to 10 days shelf life)",
            ["reprocessing.action.approaching"] = "Eligible for scheduled weekly deli production batch",
            ["reprocessing.field.product"] = "Meat Product",
            ["reprocessing.field.lotNumber"] = "Batch Lot No",
            ["reprocessing.field.warehouse"] = "Warehouse / Location",
            ["reprocessing.field.shelfLocation"] = "Display Shelf",
            ["reprocessing.field.currentQty"] = "Available Weight (kg)",
            ["reprocessing.field.unitCost"] = "Unit Cost",
            ["reprocessing.field.shelfExpiry"] = "Shelf Expiry Date",
            ["reprocessing.field.daysRemaining"] = "Days Left",
            ["reprocessing.field.status"] = "Status / Suggestion",
            ["reprocessing.field.action"] = "Action",
            ["reprocessing.btn.changeShelfDate"] = "📅 Update Shelf Expiry",
            ["reprocessing.btn.selectForConversion"] = "🔄 Transform Meat",
            ["reprocessing.btn.newRecipe"] = "➕ Create New Recipe",
            ["reprocessing.btn.editRecipe"] = "✏️ Edit Recipe",
            ["reprocessing.btn.execute"] = "✓ EXECUTE REPROCESSING & STOCK IN",
            ["reprocessing.modal.shelfDate.title"] = "Update Display Shelf Expiry Date",
            ["reprocessing.modal.shelfDate.desc"] = "Directly specify or adjust the retail display expiration date for this batch",
            ["reprocessing.modal.shelfDate.newDate"] = "New Shelf Expiry Date",
            ["reprocessing.modal.shelfDate.reason"] = "Adjustment Reason / Notes",
            ["reprocessing.modal.shelfDate.reasonHint"] = "E.g.: Cold chain verified, shelf display window adjusted by 1 day",
            ["reprocessing.modal.recipe.title"] = "Custom Meat Processing Recipe Configuration",
            ["reprocessing.modal.recipe.desc"] = "Define recipe ingredients, fat/spice/sauce ratios, yield %, and shelf-life extension",
            ["reprocessing.recipe.code"] = "Recipe Code",
            ["reprocessing.recipe.name"] = "Recipe Name",
            ["reprocessing.recipe.category"] = "Product Category",
            ["reprocessing.recipe.targetProduct"] = "Target Product Name",
            ["reprocessing.recipe.yield"] = "Expected Yield (%)",
            ["reprocessing.recipe.yieldHelp"] = "Explanation: Fermented sausage yields 90-92% due to drying, while sauced meats yield 105-110% due to marinade uptake",
            ["reprocessing.recipe.shelfExtension"] = "Shelf Life Extension (Days)",
            ["reprocessing.recipe.shelfExtensionHelp"] = "Explanation: Additional days granted to transformed finished product (e.g. Sausage +60 days, Patties +6 days, Marinated +10 days)",
            ["reprocessing.recipe.instructions"] = "Preparation & Curing Instructions",
            ["reprocessing.recipe.instructionsHelp"] = "Butchery hygiene guidelines and resting times",
            ["reprocessing.recipe.descHelp"] = "Flavor profile, culinary style, and intended application",
            ["reprocessing.recipe.ingredients"] = "Recipe Ingredients (Meat, Fat, Spices, Sauces, Salt)",
            ["reprocessing.recipe.addIngredient"] = "➕ Add Ingredient",
            ["reprocessing.recipe.rawMeatUsed"] = "Raw Meat Quantity to Use (kg)",
            ["reprocessing.recipe.rawMeatUsedHelp"] = "Amount of batch meat to repurpose into this recipe",
            ["reprocessing.sim.totalInput"] = "Raw Meat Value",
            ["reprocessing.sim.auxCost"] = "Auxiliary Ingredients Cost",
            ["reprocessing.sim.outputKg"] = "Expected Finished Product (kg)",
            ["reprocessing.sim.newUnitCost"] = "New Unit Cost (/kg)",
            ["reprocessing.sim.newExpiry"] = "Calculated New Expiry Date",
            ["reprocessing.haccp.whyCuring"] = "🥩 Food Safety & HACCP: Why Marination & Curing Extends Meat Shelf Life?",
            ["reprocessing.haccp.whyCuringText"] = "1) Acidic marination (vinegar, citrus, yoghurt) drops surface pH, halting bacterial growth. 2) Rock salt reduces water activity (Aw). 3) Natural spices (garlic, cumin, rosemary, oregano) contain powerful antimicrobial and antioxidant phytochemicals. 4) Reprocessing converts perishable inventory into high-value artisan goods, eliminating food waste.",
            ["reprocessing.success.completed"] = "✓ Transformation Completed: {0} kg of {1} successfully converted into {2} kg of {3}. New Expiry Date: {4} (Batch No: {5}). Stock updated.",
            
            // Categories & Item Types
            ["recipe.category.mince"] = "Mince & Ground Meat",
            ["recipe.category.freshSausage"] = "Fresh Butcher Sausage",
            ["recipe.category.dryFermentedSausage"] = "Dry Fermented Sausage (Sucuk)",
            ["recipe.category.meatballs"] = "Butcher Patties & Meatballs",
            ["recipe.category.marinated"] = "Marinated & Sauced Primal Cuts",
            ["recipe.category.smoked"] = "Pastrami & Smoked Cuts",
            ["recipe.category.cooked"] = "Cooked & Deli Meats (Kavurma)",
            ["recipe.itemtype.meat"] = "Meat Raw Material",
            ["recipe.itemtype.fat"] = "Fat (Tallow/Internal)",
            ["recipe.itemtype.spice"] = "Spice Mix",
            ["recipe.itemtype.sauce"] = "Sauce & Liquid Marinade",
            ["recipe.itemtype.salt"] = "Rock Salt & Curing",
            ["recipe.itemtype.casing"] = "Natural Casing / Clips",
            ["recipe.itemtype.packaging"] = "Vacuum Bag / Packaging",

            // Reprocessing Comprehensive Tabs & Sub-modules
            ["reprocessing.tab.overview"] = "🔪 Salvage Workbench & Simulation",
            ["reprocessing.tab.qc"] = "🛡️ Food Safety, pH & Quality (CCP)",
            ["reprocessing.tab.aux"] = "🧂 Spices, Sauces & Packaging Depot",
            ["reprocessing.tab.equipment"] = "⚙️ Machinery & Equipment Park",
            ["reprocessing.tab.roi"] = "📊 Financial Salvage & Waste ROI",

            // Equipment Keys
            ["eq.code"] = "Equipment Code",
            ["eq.name"] = "Equipment Name",
            ["eq.type"] = "Device Type",
            ["eq.capacity"] = "Processing Capacity",
            ["eq.status"] = "Operating Status",
            ["eq.sanitized"] = "Last Sanitization",
            ["eq.sanitizedBy"] = "Sanitized By",
            ["eq.agent"] = "Sanitizing Agent",
            ["eq.markSanitized"] = "✨ Verify Sanitization & Mark Ready",

            // Auxiliary Stock Keys
            ["aux.code"] = "Material Code",
            ["aux.name"] = "Material Name",
            ["aux.type"] = "Category",
            ["aux.stock"] = "Current Stock",
            ["aux.unitCost"] = "Unit Cost",
            ["aux.totalValue"] = "Total Value",
            ["aux.minStock"] = "Critical Threshold",
            ["aux.storage"] = "Storage Condition",
            ["aux.adjust"] = "Adjust Quantity (+/-)",

            // Quality & Food Safety Keys
            ["qc.number"] = "Inspection No",
            ["qc.batch"] = "Lot / Batch No",
            ["qc.product"] = "Meat Type / Product",
            ["qc.ph"] = "Measured pH",
            ["qc.aw"] = "Water Activity (Aw)",
            ["qc.temp"] = "Core Temp (°C)",
            ["qc.verdict"] = "Quality Verdict",
            ["qc.inspector"] = "HACCP Inspector",
            ["qc.newCheck"] = "🧪 Record Quality & pH Check",
            ["qc.phRuleTitle"] = "🔬 Critical Food Safety Limit (CCP):",
            ["qc.phRuleText"] = "Normal fresh meat pH ranges from 5.40 to 5.80. If pH > 6.20, protein breakdown and bacterial spoilage has begun; this meat CANNOT be repurposed even with heavy marinades, and is immediately quarantined for destruction.",

            // ROI & Financial Keys
            ["roi.title"] = "Waste Prevention & Value-Added Analytics",
            ["roi.totalKg"] = "Total Salvaged Meat",
            ["roi.totalMeatCost"] = "Salvaged Meat Cost Value",
            ["roi.outputValue"] = "Processed Products Market Value",
            ["roi.netProfit"] = "Net Additional Profit Added",
            ["roi.efficiency"] = "Waste Prevention Efficiency",
            ["roi.batchesSaved"] = "Critical Batches Saved",

            // Advanced Operations Tabs & Sub-modules
            ["reprocessing.tab.curing"] = "🌡️ Curing & Drying Chamber (IoT)",
            ["reprocessing.tab.sensory"] = "👃 Sensory & Organoleptic Test",
            ["reprocessing.tab.disposal"] = "🗑️ Official Disposal & Rendering",
            ["reprocessing.tab.incentives"] = "🏆 Zero-Waste Butcher Incentive",
            ["reprocessing.tab.label"] = "🏷️ Thermal Label & Allergens",
            ["markdown.apply"] = "Apply Smart Markdown",
            ["fastscan.title"] = "📱 Fast Shelf Barcode Scanner",

            // Shell & Navigation (EN)
            ["shell.title"] = "ROY MEAT INTEGRATED ERP",
            ["shell.brand"] = "ROY MEAT ENTERPRISE CONTROL HUB",
            ["shell.activePlant"] = "🏢 Facility: TR-34 Istanbul Integrated Slaughterhouse & Meat Processing Plant",
            ["shell.plantShort"] = "🏢 Istanbul Meat Processing Plant (PLN-IST-01)",
            ["shell.scaleConnected"] = "Scale: Connected (Simulation Mode)",
            ["shell.scaleReady"] = "Scale: {0} (Ready)",
            ["shell.shortcuts"] = "⌨ Shortcuts [F1]",
            ["shell.backToMainMenu"] = "Back to Main Menu",
            ["shell.notifications"] = "Notifications",
            ["shell.notificationCenter"] = "🔔 Notification Center",
            ["shell.closeAll"] = "Close All",
            ["shell.logout"] = "🚪 Logout",
            ["shell.mainMenu"] = "Main Menu",
            ["shell.shortcutsModalTitle"] = "⌨ System Keyboard Shortcuts",

            // Shortcuts Guide (EN)
            ["shortcut.guide"] = "Shortcuts Guide",
            ["shortcut.pos"] = "Quick Touch Sales (POS)",
            ["shortcut.slaughter"] = "Slaughter & Production",
            ["shortcut.stock"] = "Stock & Cold Warehouse",
            ["shortcut.products"] = "Product & Barcode Catalog",
            ["shortcut.finance"] = "Finance & Cash Register",
            ["shortcut.customers"] = "Customer Accounts & Ledger",
            ["shortcut.scale"] = "Read Live Scale",
            ["shortcut.reports"] = "Reports & Excel Analytics",
            ["shortcut.einvoice"] = "Electronic Invoicing (GİB)",
            ["shortcut.fullscreen"] = "Fullscreen / Windowed",
            ["shortcut.back"] = "Back to Main Menu",

            // Main Menu KPI & Section Titles (EN)
            ["menu.kpi.todayTurnover"] = "TODAY'S REVENUE",
            ["menu.kpi.slaughterWeight"] = "SLAUGHTER & CARCASS",
            ["menu.kpi.carcassYield"] = "CARCASS YIELD",
            ["menu.kpi.haccpStatus"] = "HACCP CCP STATUS",
            ["menu.kpi.haccpSafe"] = "✅ Safe & Compliant",
            ["menu.section.operations"] = "MEAT PROCESSING, SLAUGHTER & RETAIL OPERATIONS",
            ["menu.section.commercial"] = "COMMERCIAL, FINANCE & CUSTOMER ACCOUNTS",
            ["menu.section.management"] = "EXECUTIVE CONTROL CENTER, SECURITY & SETTINGS",

            // 14 Module Cards (EN)
            ["menu.pos.title"] = "QUICK TOUCH POS SALES",
            ["menu.pos.badge"] = "LIVE SCALE",
            ["menu.pos.desc"] = "CAS live scale weighing, GS1 barcodes, split cash/card payments and instant receipt printing",
            ["menu.pos.enter"] = "[F2] ENTER ➔",

            ["menu.slaughter.title"] = "Slaughterhouse & Abattoir MES",
            ["menu.slaughter.desc"] = "Livestock scale, Ante-Mortem inspection, SEUROP carcass grading and official PDF passport",

            ["menu.production.title"] = "Deboning & Charcuterie (BOM)",
            ["menu.production.desc"] = "Mass balance (Δ ≤ 0.5%), sausage/meatball recipes, shrinkage loss and batch manufacturing",

            ["menu.reprocessing.title"] = "Display Shelf-Life Salvage (BOM)",
            ["menu.reprocessing.desc"] = "Prevent waste by transforming near-expiry counter meat into sausages, patties & marinated cuts",

            ["menu.stock.title"] = "WMS Warehouse & SSCC-18",
            ["menu.stock.desc"] = "Cold storage, GS1 SSCC-18 palletizing, bin location routing and one-click quarantine",

            ["menu.products.title"] = "Product & Barcode Catalog",
            ["menu.products.desc"] = "Fresh & deli meat definitions, scale PLU codes, and GS1-128 thermal label designer",

            ["menu.invoices.title"] = "Purchase Invoices & Farmer",
            ["menu.invoices.desc"] = "Wholesale meat/livestock purchasing, tax withholding deductions and farmer receipts",

            ["menu.finance.title"] = "Finance & Cash Register",
            ["menu.finance.desc"] = "Shift open/close, cash/card drawer reconciliation and cashier variance settlement",

            ["menu.customers.title"] = "Customer Accounts & Ledger",
            ["menu.customers.desc"] = "Customer debit/credit balances, account statements, and one-click SMS notifications",

            ["menu.einvoice.title"] = "E-Invoice & E-Archive",
            ["menu.einvoice.desc"] = "Tax authority e-Document generation, dispatching, status inquiry and legal archiving",

            ["menu.reports.title"] = "Executive Reports & BI",
            ["menu.reports.desc"] = "Real-time revenue charts, ABC carcass net costing, yield analytics, and Excel export",

            ["menu.users.title"] = "Staff & 18 Roles (RBAC)",
            ["menu.users.desc"] = "User credentials, cashier PIN codes, SHA-256 audit trails, and permission matrix",

            ["menu.license.title"] = "Enterprise License & RSA",
            ["menu.license.desc"] = "Hardware fingerprint locking, encrypted RSA key licensing, and module activation",

            ["menu.settings.title"] = "System Settings & Hardware",
            ["menu.settings.desc"] = "CAS Scale COM port, SMS gateway configuration, database backup, and preferences",

            // POS Core Controls (EN)
            ["pos.searchPlaceholder"] = "Scan Barcode or Search Product (Enter)...",
            ["pos.liveScale"] = "⚖ SCALE [F8]: ",
            ["pos.all"] = "ALL",
            ["pos.clear"] = "Clear",
            ["pos.product"] = "Product",
            ["pos.quantity"] = "Quantity",
            ["pos.unitPrice"] = "Unit Price",
            ["pos.total"] = "Total",
            ["pos.subtotal"] = "Subtotal",
            ["pos.discount"] = "Discount",
            ["pos.grandTotal"] = "TOTAL DUE",
            ["pos.cashSale"] = "💵 CASH SALE [F2]",
            ["pos.cardSale"] = "💳 CREDIT CARD",
            ["pos.splitPay"] = "Split Payment",
            ["pos.parkSale"] = "Park Sale",
            ["pos.parkedSales"] = "Parked Sales",

            // Production & Slaughter Controls (EN)
            ["prod.title"] = "SLAUGHTER & DEBONING TREE (BOM) MODULE",
            ["prod.subtitle"] = "Carcass Intake, Automated Yield Calculation & Weighted Cost Distribution",
            ["prod.lotSelect"] = "Carcass Batch to Process:",
            ["prod.recipeSelect"] = "Deboning Recipe:",
            ["prod.reprocessingBtn"] = "🥩 SHELF-LIFE SALVAGE & REPROCESSING",
            ["prod.deliRecipeBtn"] = "🥓 CHARCUTERIE & SAUSAGE RECIPE (BOM)",

            // Products & Barcode Catalog (EN)
            ["products.listTitle"] = "Product & Barcode Catalog",
            ["products.productCount"] = "({0} Products)",
            ["products.new"] = "+ Add New Product",
            ["products.code"] = "Code",
            ["products.name"] = "Product Name",
            ["products.category"] = "Category",
            ["products.type"] = "Type",
            ["products.salePrice"] = "Sale Price",
            ["products.costPrice"] = "Cost Price",
            ["products.stock"] = "Current Stock",
            ["products.shelfLifeDays"] = "Shelf Life (Days)",
            ["products.editorTitle"] = "Product & Barcode Editor",
            ["products.unit"] = "Unit of Measure",
            ["products.barcodePluSettings"] = "Barcode / PLU Settings",
            ["products.generateBarcode"] = "⚡ Generate Barcode",
            ["products.pluCode"] = "PLU Code",
            ["products.barcode"] = "Barcode (EAN-13 / Scale)",
            ["products.quickButton"] = "Show on Quick POS Buttons",
            ["products.deleteBtn"] = "Delete",
            ["products.saveBtn"] = "✔ SAVE PRODUCT",

            // Finance & Cash Management (EN)
            ["finance.title"] = "Customer Debt / Credit Tracking",
            ["finance.customer"] = "Customer / Company",
            ["finance.phone"] = "Phone",
            ["finance.balance"] = "Balance",
            ["finance.debit"] = "Debit",
            ["finance.credit"] = "Credit",
            ["finance.action"] = "Action",
            ["finance.sendSms"] = "📩 Send SMS Reminder",
            ["finance.statementTitle"] = "Customer Statement: {0}",
            ["finance.currentBalance"] = "Current Balance: {0}",
            ["finance.cashManagementTitle"] = "Cash Register & End-of-Day Report",
            ["finance.openingBalance"] = "Opening Cash Balance:",
            ["finance.cashSales"] = "Cash Sales:",
            ["finance.cardSales"] = "Credit Card Sales:",
            ["finance.calculatedBalance"] = "Expected Cash Balance:",
            ["finance.actualCount"] = "Physical Cash Count:",
            ["finance.closeSessionBtn"] = "🔒 CLOSE CASH REGISTER & REPORT",

            // Customer Accounts & Aging (EN)
            ["customers.title"] = "CORPORATE CUSTOMER & ACCOUNT MANAGEMENT",
            ["customers.subtitle"] = "Wholesale meat buyers, restaurant accounts, livestock farmers & aging analysis",
            ["customers.totalReceivable"] = "TOTAL RECEIVABLES",
            ["customers.overdueRisk"] = "OVERDUE EXPOSURE",
            ["customers.currentAging"] = "CURRENT (0-30 DAYS)",
            ["customers.critical90Plus"] = "90+ DAYS CRITICAL OVERDUE",
            ["customers.all"] = "All Accounts",
            ["customers.debtors"] = "💰 Debtors",
            ["customers.overdue"] = "⚠️ Overdue",

            // E-Invoice & GİB (EN)
            ["einvoice.title"] = "Official E-Invoice & E-Archive Registry",
            ["einvoice.sendGib"] = "🚀 Transmit to Tax Authority",
            ["einvoice.detailsTitle"] = "Invoice Details & Line Items",
            ["einvoice.itemsTitle"] = "Invoice Items (Meat Products):",
            ["einvoice.vatAmount"] = "VAT Amount:",
            ["einvoice.sendSelectedBtn"] = "🚀 SUBMIT INVOICE TO TAX PORTAL",

            // Reports & Executive BI (EN)
            ["reports.title"] = "EXECUTIVE CONTROL & CORPORATE BI ANALYTICS",
            ["reports.subtitle"] = "Real-time turnover, ABC carcass costing, SEUROP yields & mass balance",
            ["reports.yieldExcel"] = "📊 Yield Excel",
            ["reports.salesExcel"] = "🛒 Sales Excel",
            ["reports.masterExcel"] = "📥 CORPORATE MASTER EXCEL (.xlsx)",
            ["reports.kpi.turnover"] = "TODAY'S TURNOVER",
            ["reports.kpi.tonnage"] = "SLAUGHTER & CARCASS WEIGHT",
            ["reports.kpi.yield"] = "CARCASS YIELD",
            ["reports.kpi.massBalance"] = "MASS BALANCE COMPLIANCE",
            ["reports.kpi.haccp"] = "HACCP CRITICAL CONTROL POINTS",
            ["reports.kpi.coldStorage"] = "COLD STORAGE TELEMETRY",

            // User Management & RBAC (EN)
            ["users.title"] = "Staff & User Accounts",
            ["users.username"] = "Username",
            ["users.fullName"] = "Full Name",
            ["users.role"] = "Role / Permission",
            ["users.active"] = "Active",
            ["users.lastLogin"] = "Last Login",
            ["users.newUserTitle"] = "+ Add New Staff / User",
            ["users.password"] = "Login Password",
            ["users.pinCode"] = "Quick Cashier PIN Code (4 Digits)",
            ["users.roleGroup"] = "Role / Authorization Group",
            ["users.saveBtn"] = "✔ SAVE USER TO SYSTEM",

            // License & Security (EN)
            ["license.title"] = "LICENSE & SECURITY MANAGEMENT",
            ["license.subtitle"] = "Hardware-Locked RSA Licensing, Tier Permissions & Activation",
            ["license.fingerprintTitle"] = "Hardware Identifier (Hardware Fingerprint)",
            ["license.fingerprintDesc"] = "Unique hardware fingerprint generated for this machine. Provide this ID when requesting a license.",
            ["license.copy"] = "Copy",
            ["license.currentStatusTitle"] = "Current License Status",
            ["license.company"] = "Licensee:",
            ["license.status"] = "License Status:",
            ["license.validUntil"] = "Valid Until:",
            ["license.remainingDays"] = "Remaining Days:",
            ["license.loadLicenseBtn"] = "📂 LOAD & ACTIVATE .LIC FILE",
            ["license.featuresTitle"] = "Active Modules & Feature Matrix",

            // Settings & Hardware (EN)
            ["settings.title"] = "SYSTEM & HARDWARE SETTINGS",
            ["settings.subtitle"] = "Scale Serial Port Driver, SMS Gateway, E-Invoice & Backups",
            ["settings.scaleSettingsTitle"] = "⚖ Scale Driver & COM Port Settings",
            ["settings.scaleBrand"] = "Scale Model / Protocol",
            ["settings.scalePort"] = "Serial Port (COM)",
            ["settings.testScaleBtn"] = "Test Scale Connection",
            ["settings.integrationsTitle"] = "📩 SMS & E-Invoice Integration",
            ["settings.smsProvider"] = "SMS Gateway Provider",
            ["settings.einvoiceProvider"] = "E-Invoice Certified Provider",
            ["settings.testIntegratorBtn"] = "Test Provider Connection",
            ["settings.backupTitle"] = "💾 Database Backup & Disaster Recovery",
            ["settings.backupDir"] = "Backup Directory",
            ["settings.backupNowBtn"] = "💾 BACKUP DATABASE NOW",
            ["settings.backupHistory"] = "Backup Archive History",

            // Procurement Invoices & Producer (EN)
            ["invoices.title"] = "PROCUREMENT INVOICES & FARMER RECEIPTS",
            ["invoices.subtitle"] = "Carcass meat, live animals, withholding tax, producer receipts & automated costing",
            ["invoices.monthlyVolume"] = "MONTHLY PURCHASES",
            ["invoices.monthlyTonnage"] = "MONTHLY TONNAGE",
            ["invoices.tevkifat"] = "WITHHOLDING TAX",
            ["invoices.pendingPayables"] = "PENDING SUPPLIER DEBT",
            ["invoices.tabList"] = "Purchase Invoices List",
            ["invoices.tabCreate"] = "New Invoice / Farmer Intake",

            // Login Portal (EN)
            ["login.title"] = "ROY KASAP ENTERPRISE ERP",
            ["login.subtitle"] = "Global Slaughterhouse, Meat Processing & Butcher POS Platform",
            ["login.description"] = "Complete farm-to-fork meat chain management with forensic audit integrity and AI-assisted enterprise ERP & MES.",
            ["login.portalTitle"] = "Station Login Portal",
            ["login.portalSubtitle"] = "Please enter your authorized credentials or 4-digit station PIN.",
            ["login.quickRoles"] = "QUICK ROLE SELECTION (DEMO / STATION TEST)",
            ["login.roleAdmin"] = "👑 Administrator",
            ["login.roleButcher"] = "🔪 Head Butcher",
            ["login.roleVet"] = "🩺 Veterinarian",
            ["login.roleCashier"] = "🛒 Cashier",
            ["login.tabStandard"] = "Username / Password",
            ["login.tabPin"] = "Quick Cashier PIN",
            ["login.username"] = "USERNAME",
            ["login.password"] = "PASSWORD",
            ["login.showPassword"] = "👁 Show",
            ["login.rememberMe"] = "Remember Me",
            ["login.loginBtn"] = "SIGN IN TO SYSTEM ➔",
            ["login.pinPrompt"] = "Enter Your 4-Digit Station PIN:",
            ["login.systemStatus"] = "System Status: ",
            ["login.onlineStatus"] = "Online & Encrypted Database Active"
        },

        ["pl-PL"] = new(StringComparer.OrdinalIgnoreCase)
        {
            // Common
            ["common.save"] = "Zapisz",
            ["common.cancel"] = "Anuluj",
            ["common.delete"] = "Usuń",
            ["common.edit"] = "Edytuj",
            ["common.print"] = "Drukuj",
            ["common.search"] = "Szukaj...",
            ["common.filter"] = "Filtruj",
            ["common.status"] = "Status",
            ["common.success"] = "Operacja zakończona sukcesem",
            ["common.error"] = "Błąd",
            ["common.warning"] = "Ostrzeżenie",
            ["common.confirm"] = "Zatwierdź",
            ["common.back"] = "Wstecz",
            ["common.refresh"] = "Odśwież",
            ["common.export"] = "Eksportuj",
            ["common.close"] = "Zamknij",
            ["common.total"] = "Razem",
            ["common.date"] = "Data",
            ["common.actions"] = "Akcje",

            // Tenant & Plant
            ["tenant.title"] = "Zarządzanie wieloma zakładami",
            ["tenant.holding"] = "Holding / Grupa",
            ["tenant.company"] = "Firma",
            ["tenant.plant"] = "Zakład / Rzeźnia",
            ["tenant.unit"] = "Jednostka produkcyjna",

            // Animal & Intake
            ["animal.earTag"] = "Numer kolczyka (ARiMR)",
            ["animal.passportNo"] = "Numer paszportu",
            ["animal.breed"] = "Rasa zwierzęcia",
            ["animal.gender"] = "Płeć",
            ["animal.liveWeight"] = "Waga żywa (kg)",
            ["animal.producer"] = "Hodowca / Dostawca",
            ["animal.farmOrigin"] = "Pochodzenie / Gospodarstwo",
            ["animal.arrivalDate"] = "Data przyjęcia",
            ["animal.intakeTitle"] = "Stacja przyjęcia żywca",

            // Veterinary
            ["vet.title"] = "Badanie przedubojowe (Ante-Mortem)",
            ["vet.doctor"] = "Urzędowy Lekarz Weterynarii",
            ["vet.reportNo"] = "Nr protokołu",
            ["vet.temperature"] = "Temperatura ciała (°C)",
            ["vet.diagnosis"] = "Wynik badania",
            ["vet.approved"] = "✓ Zgoda na ubój wydana",
            ["vet.blocked"] = "⛔ Ubój zablokowany",
            ["vet.quarantine"] = "Kwarantanna",

            // Slaughter & Carcass
            ["slaughter.title"] = "Ubój i klasyfikacja tusz",
            ["slaughter.slaughterNo"] = "Numer uboju",
            ["slaughter.carcassNo"] = "Numer tuszy",
            ["slaughter.butcher"] = "Mistrz rzeźnicki",
            ["slaughter.hotWeight"] = "Waga ciepła tuszy (kg)",
            ["slaughter.coldWeight"] = "Waga zimna tuszy (kg)",
            ["slaughter.yield"] = "Wydajność poubojowa (%)",
            ["slaughter.seurop"] = "Klasa EUROP",
            ["slaughter.fatScore"] = "Klasa otłuszczenia (1-5)",
            ["slaughter.marbling"] = "Marmurkowatość (BMS)",
            ["slaughter.ph24"] = "Wartość pH 24h",
            ["slaughter.scaleRead"] = "⚖️ Odczytaj z wagi",

            // Deboning & Mass Balance
            ["cutting.title"] = "Rozbiór tuszy i elementy (BOM)",
            ["cutting.region"] = "Partia anatomiczna",
            ["cutting.cutName"] = "Nazwa elementu",
            ["cutting.netWeight"] = "Waga netto (kg)",
            ["cutting.unitCost"] = "Koszt jednostkowy",
            ["cutting.totalValue"] = "Wartość całkowita",
            ["cutting.massBalance"] = "Bilans masowy rozbioru",
            ["cutting.unexplainedLoss"] = "Niewyjaśniony ubytek masy",

            // WMS & Traceability
            ["wms.title"] = "Zarządzanie magazynem WMS",
            ["wms.warehouse"] = "Magazyn",
            ["wms.lot"] = "Numer partii / Lot",
            ["wms.barcode"] = "Kod kreskowy (GS1-128)",
            ["traceability.title"] = "Nienaruszalny łańcuch identyfikowalności",
            ["traceability.passportPdf"] = "📄 Paszport zwierzęcia (PDF)",

            // Anti-Fraud & Audit
            ["fraud.title"] = "Centrum audytu i anty-fraud",
            ["fraud.riskScore"] = "Wskaźnik ryzyka",
            ["fraud.auditTrail"] = "Niezmienny dziennik audytu (SHA-256)",
            ["fraud.supervisorApproval"] = "Wymagana akceptacja przełożonego",

            // Reyon SKT & Reprocessing (PL - Lehçe)
            ["reprocessing.title"] = "System ratowania mięsa z lady i przetwórstwa z recepturą (BOM)",
            ["reprocessing.subtitle"] = "Zapobiegaj stratom mięsa z krótkim terminem przydatności przetwarzając je na kiełbasy, burgery i mięsa w marynacie",
            ["reprocessing.batches.title"] = "Krytyczne partie z lady (Oczekujące na przetwórstwo)",
            ["reprocessing.batches.desc"] = "Partie świeżego mięsa, których termin przydatności na ladzie wynosi 2 dni lub mniej",
            ["reprocessing.recipes.title"] = "Receptury przetwórcze (BOM)",
            ["reprocessing.recipes.desc"] = "Definiowane przez użytkownika receptury na kiełbasę, kotlety i mięsa w sosie z wydajnością",
            ["reprocessing.simulation.title"] = "Symulacja przetwórstwa i zapotrzebowanie na surowce",
            ["reprocessing.simulation.desc"] = "Kalkulacja w czasie rzeczywistym przypraw, tłuszczu, marynaty oraz nowego wydłużonego terminu",
            ["reprocessing.urgency.urgent"] = "🚨 Pilne ratowanie (Ostatnie godziny)",
            ["reprocessing.urgency.warning"] = "⚠️ Krytyczny (Pozostało 1-2 dni)",
            ["reprocessing.urgency.approaching"] = "ℹ️ Zbliża się (Pozostało 3 dni)",
            ["reprocessing.action.urgent"] = "Należy natychmiast przerobić na kiełbasę suszoną lub pikantne kotlety (+30 do 60 dni trwałości)",
            ["reprocessing.action.warning"] = "Zalecane do zamarynowania w sosie lub wyrobienia burgerów (+7 do 10 dni trwałości)",
            ["reprocessing.action.approaching"] = "Kwalifikuje się do tygodniowego planu produkcji wyrobów garmażeryjnych",
            ["reprocessing.field.product"] = "Produkt mięsny",
            ["reprocessing.field.lotNumber"] = "Nr partii (Lot)",
            ["reprocessing.field.warehouse"] = "Magazyn / Lokalizacja",
            ["reprocessing.field.shelfLocation"] = "Lada ekspozycyjna",
            ["reprocessing.field.currentQty"] = "Dostępna ilość (kg)",
            ["reprocessing.field.unitCost"] = "Koszt jednostkowy",
            ["reprocessing.field.shelfExpiry"] = "Termin przydatności na ladzie",
            ["reprocessing.field.daysRemaining"] = "Pozostałe dni",
            ["reprocessing.field.status"] = "Status / Rekomendacja",
            ["reprocessing.field.action"] = "Akcja",
            ["reprocessing.btn.changeShelfDate"] = "📅 Zmień datę przydatności",
            ["reprocessing.btn.selectForConversion"] = "🔄 Przetwórz partię",
            ["reprocessing.btn.newRecipe"] = "➕ Zdefiniuj nową recepturę",
            ["reprocessing.btn.editRecipe"] = "✏️ Edytuj recepturę",
            ["reprocessing.btn.execute"] = "✓ ZATWIERDŹ PRZETWÓRSTWO I PRZYJMIJ NA STAN",
            ["reprocessing.modal.shelfDate.title"] = "Aktualizacja terminu przydatności na ladzie",
            ["reprocessing.modal.shelfDate.desc"] = "Wprowadź lub zmodyfikuj termin przydatności danej partii mięsa na ladzie chłodniczej",
            ["reprocessing.modal.shelfDate.newDate"] = "Nowy termin przydatności na ladzie",
            ["reprocessing.modal.shelfDate.reason"] = "Uzasadnienie modyfikacji / Notatki",
            ["reprocessing.modal.shelfDate.reasonHint"] = "Np.: Zweryfikowano ciąg chłodniczy, przedłużono ekspozycję o 1 dzień",
            ["reprocessing.modal.recipe.title"] = "Konfigurator receptury przetwórczej użytkownika",
            ["reprocessing.modal.recipe.desc"] = "Określ proporcje mięsa, przypraw, marynaty, wydajność % oraz wydłużenie terminu ważności",
            ["reprocessing.recipe.code"] = "Kod receptury",
            ["reprocessing.recipe.name"] = "Nazwa receptury",
            ["reprocessing.recipe.category"] = "Kategoria produktu",
            ["reprocessing.recipe.targetProduct"] = "Nazwa gotowego wyrobu",
            ["reprocessing.recipe.yield"] = "Oczekiwana wydajność (%)",
            ["reprocessing.recipe.yieldHelp"] = "Objaśnienie: Przy kiełbasie suszonej wydajność to 90-92% (ubytek wody), a przy mięsach marynowanych 105-110% (wchłonięcie sosu)",
            ["reprocessing.recipe.shelfExtension"] = "Wydłużenie trwałości (Dni)",
            ["reprocessing.recipe.shelfExtensionHelp"] = "Objaśnienie: Dodatkowe dni trwałości dla wyrobu (np. Kiełbasa +60 dni, Kotlety +6 dni, Marynata +10 dni)",
            ["reprocessing.recipe.instructions"] = "Instrukcja przygotowania i marynowania",
            ["reprocessing.recipe.instructionsHelp"] = "Wskazówki technologiczne i zasady higieny rozbioru",
            ["reprocessing.recipe.descHelp"] = "Profil smakowy wyrobu i przeznaczenie kulinarne",
            ["reprocessing.recipe.ingredients"] = "Składniki receptury (Mięso, Tłuszcz, Przyprawy, Sosy, Sól)",
            ["reprocessing.recipe.addIngredient"] = "➕ Dodaj składnik",
            ["reprocessing.recipe.rawMeatUsed"] = "Ilość użytego surowego mięsa (kg)",
            ["reprocessing.recipe.rawMeatUsedHelp"] = "Ilość mięsa z partii przeznaczona do przerobu",
            ["reprocessing.sim.totalInput"] = "Wartość surowca mięsnego",
            ["reprocessing.sim.auxCost"] = "Koszt dodatków i przypraw",
            ["reprocessing.sim.outputKg"] = "Gotowy wyrób (kg)",
            ["reprocessing.sim.newUnitCost"] = "Nowy koszt jednostkowy (/kg)",
            ["reprocessing.sim.newExpiry"] = "Obliczony nowy termin ważności",
            ["reprocessing.haccp.whyCuring"] = "🥩 Bezpieczeństwo HACCP: Dlaczego marynowanie i peklowanie przedłuża trwałość mięsa?",
            ["reprocessing.haccp.whyCuringText"] = "1) Kwaśna marynata (ocet, cytryna, jogurt) obniża pH powierzchni, blokując rozwój bakterii. 2) Sól kamienna obniża aktywność wody (Aw). 3) Czosnek, kminek, oregano i rozmaryn zawierają silne fitochemikalia przeciwutleniające i antybakteryjne. 4) Przetwórstwo ratuje surowiec przed zepsuciem, tworząc wysokomarżowy produkt rzemieślniczy.",
            ["reprocessing.success.completed"] = "✓ Przetwórstwo zakończone: {0} kg mięsa {1} przetworzono na {2} kg wyrobu {3}. Nowy termin ważności: {4} (Nr partii: {5}). Magazyn zaktualizowany.",
            
            // Categories & Item Types
            ["recipe.category.mince"] = "Mięso mielone i gulaszowe",
            ["recipe.category.freshSausage"] = "Świeża kiełbasa rzeźnicka",
            ["recipe.category.dryFermentedSausage"] = "Kiełbasa suszona / fermentowana (Sucuk)",
            ["recipe.category.meatballs"] = "Kotlety i burgery rzeźnickie",
            ["recipe.category.marinated"] = "Mięsa w marynacie i sosie",
            ["recipe.category.smoked"] = "Wędzonki i pastrami",
            ["recipe.category.cooked"] = "Mięsa pieczone i gotowane (Kavurma)",
            ["recipe.itemtype.meat"] = "Główny surowiec mięsny",
            ["recipe.itemtype.fat"] = "Tłuszcz (Łój / Słonina)",
            ["recipe.itemtype.spice"] = "Mieszanka przypraw",
            ["recipe.itemtype.sauce"] = "Sos i płynna marynata",
            ["recipe.itemtype.salt"] = "Sól kamienna i peklowanie",
            ["recipe.itemtype.casing"] = "Jelito naturalne / Klipsy",
            ["recipe.itemtype.packaging"] = "Worek próżniowy / Opakowanie",

            // Reprocessing Comprehensive Tabs & Sub-modules
            ["reprocessing.tab.overview"] = "🔪 Warsztat ratowania i symulacja",
            ["reprocessing.tab.qc"] = "🛡️ Bezpieczeństwo, pH i jakość (CCP)",
            ["reprocessing.tab.aux"] = "🧂 Przyprawy, sosy i opakowania",
            ["reprocessing.tab.equipment"] = "⚙️ Park maszynowy i urządzenia",
            ["reprocessing.tab.roi"] = "📊 Finansowy zwrot i redukcja strat (ROI)",

            // Equipment Keys
            ["eq.code"] = "Kod maszyny",
            ["eq.name"] = "Nazwa urządzenia",
            ["eq.type"] = "Typ / Kategoria",
            ["eq.capacity"] = "Wydajność robocza",
            ["eq.status"] = "Status gotowości",
            ["eq.sanitized"] = "Ostatnia sanityzacja",
            ["eq.sanitizedBy"] = "Zdezynfekował",
            ["eq.agent"] = "Środek myjąco-dezynfekujący",
            ["eq.markSanitized"] = "✨ Potwierdź sanityzację i oznacz jako gotowe",

            // Auxiliary Stock Keys
            ["aux.code"] = "Kod materiału",
            ["aux.name"] = "Nazwa materiału",
            ["aux.type"] = "Kategoria",
            ["aux.stock"] = "Aktualny stan",
            ["aux.unitCost"] = "Koszt jednostkowy",
            ["aux.totalValue"] = "Wartość zapasu",
            ["aux.minStock"] = "Próg krytyczny",
            ["aux.storage"] = "Warunki magazynowe",
            ["aux.adjust"] = "Koryguj stan (+/-)",

            // Quality & Food Safety Keys
            ["qc.number"] = "Numer protokołu",
            ["qc.batch"] = "Partia / Lot",
            ["qc.product"] = "Gatunek / Produkt",
            ["qc.ph"] = "Zmierzone pH",
            ["qc.aw"] = "Aktywność wody (Aw)",
            ["qc.temp"] = "Temperatura rdzenia (°C)",
            ["qc.verdict"] = "Decyzja jakościowa",
            ["qc.inspector"] = "Inspektor HACCP",
            ["qc.newCheck"] = "🧪 Wprowadź badanie jakości i pH",
            ["qc.phRuleTitle"] = "🔬 Krytyczny limit bezpieczeństwa (CCP):",
            ["qc.phRuleText"] = "Prawidłowe pH świeżego mięsa wynosi 5.40 - 5.80. Jeśli pH > 6.20, rozpoczął się rozkład białka i proces psucia; takiego mięsa NIE WOLNO przetwarzać nawet w mocnej marynacie – zostaje natychmiast zablokowane i skierowane do utylizacji.",

            // ROI & Financial Keys
            ["roi.title"] = "Analiza redukcji strat i tworzenia wartości dodanej",
            ["roi.totalKg"] = "Uratowane mięso łącznie",
            ["roi.totalMeatCost"] = "Wartość uratowanego surowca",
            ["roi.outputValue"] = "Wartość wyrobów gotowych",
            ["roi.netProfit"] = "Dodatkowy wypracowany zysk",
            ["roi.efficiency"] = "Skuteczność redukcji strat",
            ["roi.batchesSaved"] = "Uratowane partie krytyczne",

            // Advanced Operations Tabs & Sub-modules
            ["reprocessing.tab.curing"] = "🌡️ Komora dojrzewania i suszenia kiełbas (IoT)",
            ["reprocessing.tab.sensory"] = "👃 Ocena organoleptyczna i sensoryczna",
            ["reprocessing.tab.disposal"] = "🗑️ Protokół utylizacji i renderingu",
            ["reprocessing.tab.incentives"] = "🏆 Premia rzeźnika za zero strat",
            ["reprocessing.tab.label"] = "🏷️ Etykieta termiczna i alergeny",
            ["markdown.apply"] = "Zastosuj dynamiczną obniżkę ceny",
            ["fastscan.title"] = "📱 Szybki skaner kodów z lady",

            // Shell & Navigation (PL)
            ["shell.title"] = "ROY RZEŹNIA ZINTEGROWANY ERP",
            ["shell.brand"] = "ROY KASAP CENTRUM KONTROLI PRZEDSIĘBIORSTWA",
            ["shell.activePlant"] = "🏢 Zakład: TR-34 Stambuł Zintegrowana Rzeźnia i Zakład Mięsny",
            ["shell.plantShort"] = "🏢 Stambuł Zakład Rozbioru i Przetwórstwa (PLN-IST-01)",
            ["shell.scaleConnected"] = "Waga: Połączona (Tryb Symulacji)",
            ["shell.scaleReady"] = "Waga: {0} (Gotowa)",
            ["shell.shortcuts"] = "⌨ Skróty [F1]",
            ["shell.backToMainMenu"] = "Powrót do menu głównego",
            ["shell.notifications"] = "Powiadomienia",
            ["shell.notificationCenter"] = "🔔 Centrum Powiadomień",
            ["shell.closeAll"] = "Zamknij wszystko",
            ["shell.logout"] = "🚪 Wyloguj",
            ["shell.mainMenu"] = "Menu Główne",
            ["shell.shortcutsModalTitle"] = "⌨ Systemowe Skróty Klawiszowe",

            // Shortcuts Guide (PL)
            ["shortcut.guide"] = "Przewodnik po skrótach",
            ["shortcut.pos"] = "Szybka sprzedaż (POS)",
            ["shortcut.slaughter"] = "Ubój i produkcja",
            ["shortcut.stock"] = "Magazyn chłodniczy i zapasy",
            ["shortcut.products"] = "Katalog produktów i kodów",
            ["shortcut.finance"] = "Finanse i rozliczenie kasy",
            ["shortcut.customers"] = "Konta i rozrachunki klientów",
            ["shortcut.scale"] = "Odczyt z wagi na żywo",
            ["shortcut.reports"] = "Raporty i eksport Excel",
            ["shortcut.einvoice"] = "Wystawianie faktur (KSeF/GİB)",
            ["shortcut.fullscreen"] = "Pełny ekran / W oknie",
            ["shortcut.back"] = "Powrót do menu",

            // Main Menu KPI & Section Titles (PL)
            ["menu.kpi.todayTurnover"] = "DZISIEJSZY OBRÓT",
            ["menu.kpi.slaughterWeight"] = "UBÓJ I TUSZE",
            ["menu.kpi.carcassYield"] = "WYDAJNOŚĆ TUSZ",
            ["menu.kpi.haccpStatus"] = "STATUS HACCP CCP",
            ["menu.kpi.haccpSafe"] = "✅ Bezpieczny i zgodny",
            ["menu.section.operations"] = "PRZETWÓRSTWO MIĘSA, UBÓJ I SPRZEDAŻ (POS)",
            ["menu.section.commercial"] = "HANDEL, FINANSE I ROZRACHUNKI",
            ["menu.section.management"] = "CENTRUM ZARZĄDZANIA, BEZPIECZEŃSTWO I USTAWIENIA",

            // 14 Module Cards (PL)
            ["menu.pos.title"] = "SZYBKA SPRZEDAŻ DOTYKOWA (POS)",
            ["menu.pos.badge"] = "WAGA NA ŻYWO",
            ["menu.pos.desc"] = "Ważenie na żywo z wag CAS, kody GS1, płatności dzielone gotówka/karta i natychmiastowy paragon",
            ["menu.pos.enter"] = "[F2] WEJDŹ ➔",

            ["menu.slaughter.title"] = "Rzeźnia & System Uboju MES",
            ["menu.slaughter.desc"] = "Waga żywca, badanie przedubojowe, klasyfikacja tusz SEUROP i urzędowy paszport PDF",

            ["menu.production.title"] = "Rozbiór & Wędliniarstwo (BOM)",
            ["menu.production.desc"] = "Bilans masowy (Δ ≤ 0.5%), receptury kiełbas i burgerów, ubytki suszenia i produkcja partii",

            ["menu.reprocessing.title"] = "Ratowanie Lady i Przetwórstwo (BOM)",
            ["menu.reprocessing.desc"] = "Przetwarzanie mięsa z krótkim terminem na ladzie w kiełbasy, burgery i mięsa marynowane",

            ["menu.stock.title"] = "Magazyn WMS & Palety SSCC-18",
            ["menu.stock.desc"] = "Chłodnie składowe, paletyzacja GS1 SSCC-18, lokalizacje regałowe i szybka kwarantanna",

            ["menu.products.title"] = "Katalog Produktów i Kodów",
            ["menu.products.desc"] = "Wyroby mięsne i wędliny, kody PLU wag etykietujących i projektant kodów GS1-128",

            ["menu.invoices.title"] = "Faktury Zakupu i Rolnik RR",
            ["menu.invoices.desc"] = "Hurtowy skup żywca i mięsa, potrącenia podatkowe i faktury dla rolników ryczałtowych",

            ["menu.finance.title"] = "Finanse i Zarządzanie Kasą",
            ["menu.finance.desc"] = "Otwarcie/zamknięcie zmiany, liczenie gotówki/kart i rozliczenie różnic kasowych",

            ["menu.customers.title"] = "Rozrachunki z Kontrahentami",
            ["menu.customers.desc"] = "Salda należności/zobowiązań klientów, wyciągi z konta i powiadomienia SMS jednym kliknięciem",

            ["menu.einvoice.title"] = "E-Faktury i E-Archiwum",
            ["menu.einvoice.desc"] = "Wystawianie faktur ustrukturyzowanych, wysyłka do administracji skarbowej i archiwum",

            ["menu.reports.title"] = "Raporty Zarządcze i BI",
            ["menu.reports.desc"] = "Wykres obrotu na żywo, kalkulacja kosztów tusz ABC, wydajność poubojowa i eksport do Excela",

            ["menu.users.title"] = "Personel & 18 Ról (RBAC)",
            ["menu.users.desc"] = "Konta użytkowników, kody PIN kasjerów, niezmienny dziennik audytu SHA-256 i matryca ról",

            ["menu.license.title"] = "Licencja Korporacyjna & RSA",
            ["menu.license.desc"] = "Blokada identyfikatora sprzętowego, szyfrowana licencja RSA i aktywacja pakietów",

            ["menu.settings.title"] = "Ustawienia Systemu i Sprzętu",
            ["menu.settings.desc"] = "Port COM wagi CAS, bramka powiadomień SMS, kopie zapasowe bazy danych i preferencje",

            // POS Core Controls (PL)
            ["pos.searchPlaceholder"] = "Zeskanuj kod lub szukaj produktu (Enter)...",
            ["pos.liveScale"] = "⚖ WAGA [F8]: ",
            ["pos.all"] = "WSZYSTKIE",
            ["pos.clear"] = "Wyczyść",
            ["pos.product"] = "Produkt",
            ["pos.quantity"] = "Ilość",
            ["pos.unitPrice"] = "Cena jedn.",
            ["pos.total"] = "Razem",
            ["pos.subtotal"] = "Suma częściowa",
            ["pos.discount"] = "Rabat",
            ["pos.grandTotal"] = "DO ZAPŁATY",
            ["pos.cashSale"] = "💵 GOTÓWKA [F2]",
            ["pos.cardSale"] = "💳 KARTA PŁATNICZA",
            ["pos.splitPay"] = "Płatność dzielona",
            ["pos.parkSale"] = "Zawieś sprzedaż",
            ["pos.parkedSales"] = "Zawieszone",

            // Production & Slaughter Controls (PL)
            ["prod.title"] = "MODUŁ ROZBIORU TUSZ I DRZEWA BOM",
            ["prod.subtitle"] = "Przyjęcie tuszy, automatyczna wydajność poubojowa i alokacja kosztów",
            ["prod.lotSelect"] = "Partia tuszy do rozbioru:",
            ["prod.recipeSelect"] = "Receptura rozbioru:",
            ["prod.reprocessingBtn"] = "🥩 RATOWANIE LADY I PRZETWÓRSTWO",
            ["prod.deliRecipeBtn"] = "🥓 RECEPTURA WĘDLIN I KIEŁBAS (BOM)",

            // Products & Barcode Catalog (PL)
            ["products.listTitle"] = "Katalog produktów i kodów kreskowych",
            ["products.productCount"] = "({0} Produkty)",
            ["products.new"] = "+ Dodaj nowy produkt",
            ["products.code"] = "Kod",
            ["products.name"] = "Nazwa produktu",
            ["products.category"] = "Kategoria",
            ["products.type"] = "Typ",
            ["products.salePrice"] = "Cena sprzedaży",
            ["products.costPrice"] = "Cena zakupu/kosztu",
            ["products.stock"] = "Stan magazynowy",
            ["products.shelfLifeDays"] = "Termin (Dni)",
            ["products.editorTitle"] = "Edytor produktu i kodu kreskowego",
            ["products.unit"] = "Jednostka miary",
            ["products.barcodePluSettings"] = "Ustawienia kodu kreskowego i PLU",
            ["products.generateBarcode"] = "⚡ Wygeneruj kod",
            ["products.pluCode"] = "Kod PLU",
            ["products.barcode"] = "Kod kreskowy (EAN-13 / Waga)",
            ["products.quickButton"] = "Pokaż na przyciskach szybkiej sprzedaży",
            ["products.deleteBtn"] = "Usuń",
            ["products.saveBtn"] = "✔ ZAPISZ PRODUKT",

            // Finance & Cash Management (PL)
            ["finance.title"] = "Rozrachunki z klientami (Należności / Zobowiązania)",
            ["finance.customer"] = "Klient / Firma",
            ["finance.phone"] = "Telefon",
            ["finance.balance"] = "Saldo",
            ["finance.debit"] = "Należność (Wn)",
            ["finance.credit"] = "Zobowiązanie (Ma)",
            ["finance.action"] = "Operacja",
            ["finance.sendSms"] = "📩 Wyślij SMS z przypomnieniem",
            ["finance.statementTitle"] = "Wyciąg z konta klienta: {0}",
            ["finance.currentBalance"] = "Aktualne saldo: {0}",
            ["finance.cashManagementTitle"] = "Zarządzanie kasą i raport dobowy",
            ["finance.openingBalance"] = "Stan początkowy kasy:",
            ["finance.cashSales"] = "Sprzedaż gotówkowa:",
            ["finance.cardSales"] = "Sprzedaż kartą:",
            ["finance.calculatedBalance"] = "Wyliczony stan kasy:",
            ["finance.actualCount"] = "Fizyczny stan kasy (spis):",
            ["finance.closeSessionBtn"] = "🔒 ZAMKNIJ DZIEŃ I GENERUJ RAPORT",

            // Customer Accounts & Aging (PL)
            ["customers.title"] = "ZARZĄDZANIE KONTAMI KLIENTÓW I ROZRACHUNKAMI",
            ["customers.subtitle"] = "Odbiorcy hurtowi mięsa, gastronomia, hodowcy i analiza wiekowania",
            ["customers.totalReceivable"] = "ŁĄCZNE NALEŻNOŚCI",
            ["customers.overdueRisk"] = "PRZETERMINOWANE NALEŻNOŚCI",
            ["customers.currentAging"] = "W TERMINIE (0-30 DNI)",
            ["customers.critical90Plus"] = "90+ DNI KRYTYCZNE OPÓŹNIENIE",
            ["customers.all"] = "Wszyscy kontrahenci",
            ["customers.debtors"] = "💰 Dłużnicy",
            ["customers.overdue"] = "⚠️ Przeterminowane",

            // E-Invoice & GİB (PL)
            ["einvoice.title"] = "Rejestr e-Faktur i e-Archiwum",
            ["einvoice.sendGib"] = "🚀 Wyślij do KSeF/Urzędu",
            ["einvoice.detailsTitle"] = "Szczegóły faktury i pozycje",
            ["einvoice.itemsTitle"] = "Pozycje faktury (Produkty mięsne):",
            ["einvoice.vatAmount"] = "Kwota VAT:",
            ["einvoice.sendSelectedBtn"] = "🚀 WYŚLIJ FAKTURĘ DO SYSTEMU",

            // Reports & Executive BI (PL)
            ["reports.title"] = "CENTRUM ANALITYKI BIZNESOWEJ I RAPORTOWANIA BI",
            ["reports.subtitle"] = "Obrót na żywo, koszty ABC tusz, wydajność EUROP i bilans masowy",
            ["reports.yieldExcel"] = "📊 Wydajność Excel",
            ["reports.salesExcel"] = "🛒 Sprzedaż Excel",
            ["reports.masterExcel"] = "📥 KORPORACYJNY MASTER EXCEL (.xlsx)",
            ["reports.kpi.turnover"] = "DZISIEJSZY OBRÓT",
            ["reports.kpi.tonnage"] = "TONAŻ UBOJU I TUSZ",
            ["reports.kpi.yield"] = "WYDAJNOŚĆ POUBOJOWA",
            ["reports.kpi.massBalance"] = "ZGODNOŚĆ BILANSU MASOWEGO",
            ["reports.kpi.haccp"] = "PUNKTY KONTROLNE HACCP (CCP)",
            ["reports.kpi.coldStorage"] = "CHŁODNIE MAGAZYNOWE",

            // User Management & RBAC (PL)
            ["users.title"] = "Konta personelu i użytkowników",
            ["users.username"] = "Nazwa użytkownika",
            ["users.fullName"] = "Imię i nazwisko",
            ["users.role"] = "Rola / Uprawnienia",
            ["users.active"] = "Aktywny",
            ["users.lastLogin"] = "Ostatnie logowanie",
            ["users.newUserTitle"] = "+ Dodaj nowego pracownika",
            ["users.password"] = "Hasło dostępu",
            ["users.pinCode"] = "Szybki kod PIN kasjera (4 cyfry)",
            ["users.roleGroup"] = "Grupa uprawnień / Rola",
            ["users.saveBtn"] = "✔ ZAPISZ PRACOWNIKA",

            // License & Security (PL)
            ["license.title"] = "ZARZĄDZANIE LICENCJAMI I BEZPIECZEŃSTWEM",
            ["license.subtitle"] = "Licencjonowanie RSA powiązane ze sprzętem i aktywacja pakietów",
            ["license.fingerprintTitle"] = "Identyfikator sprzętowy (Hardware Fingerprint)",
            ["license.fingerprintDesc"] = "Unikalny identyfikator sprzętowy tego komputera. Przekaż go dystrybutorowi przy zamawianiu licencji.",
            ["license.copy"] = "Kopiuj",
            ["license.currentStatusTitle"] = "Aktualny stan licencji",
            ["license.company"] = "Właściciel licencji:",
            ["license.status"] = "Status licencji:",
            ["license.validUntil"] = "Ważna do:",
            ["license.remainingDays"] = "Pozostało dni:",
            ["license.loadLicenseBtn"] = "📂 ZAŁADUJ PLIK .LIC I AKTYWUJ",
            ["license.featuresTitle"] = "Aktywne moduły i uprawnienia pakietu",

            // Settings & Hardware (PL)
            ["settings.title"] = "USTAWIENIA SYSTEMU I URZĄDZEŃ",
            ["settings.subtitle"] = "Sterowniki wag COM, bramka SMS, integracja e-Faktur i kopie zapasowe",
            ["settings.scaleSettingsTitle"] = "⚖ Ustawienia wag i portów COM",
            ["settings.scaleBrand"] = "Model wagi / Protokół",
            ["settings.scalePort"] = "Port szeregowy (COM)",
            ["settings.testScaleBtn"] = "Testuj połączenie z wagą",
            ["settings.integrationsTitle"] = "📩 Integracja SMS i e-Faktur",
            ["settings.smsProvider"] = "Bramka SMS",
            ["settings.einvoiceProvider"] = "Operator e-Faktur",
            ["settings.testIntegratorBtn"] = "Testuj połączenie integratora",
            ["settings.backupTitle"] = "💾 Kopia zapasowa bazy danych i odzyskiwanie",
            ["settings.backupDir"] = "Folder kopii zapasowej",
            ["settings.backupNowBtn"] = "💾 WYKONAJ KOPIĘ ZAPASOWĄ TERAZ",
            ["settings.backupHistory"] = "Historia plików kopii zapasowych",

            // Procurement Invoices & Producer (PL)
            ["invoices.title"] = "FAKTURY ZAKUPU I DOWODY DOSTAW HODOWLANYCH",
            ["invoices.subtitle"] = "Tusze, żywiec, mechanizm podzielonej płatności, skup żywca i wycena magazynu",
            ["invoices.monthlyVolume"] = "MIESIĘCZNY ZAKUP",
            ["invoices.monthlyTonnage"] = "MIESIĘCZNY TONAŻ",
            ["invoices.tevkifat"] = "PODATEK POTRĄCONY",
            ["invoices.pendingPayables"] = "ZOBOWIĄZANIA WOBEC DOSTAWCÓW",
            ["invoices.tabList"] = "Lista faktur zakupu",
            ["invoices.tabCreate"] = "Nowa faktura / Przyjęcie od rolnika",

            // Login Portal (PL)
            ["login.title"] = "ROY KASAP ZINTEGROWANY ERP",
            ["login.subtitle"] = "Zintegrowany system rzeźni, przetwórstwa mięsa i punktów sprzedaży",
            ["login.description"] = "Kompleksowy łańcuch od pola do stołu z niezmiennym audytem i wsparciem AI w systemie ERP & MES.",
            ["login.portalTitle"] = "Portal logowania do stacji",
            ["login.portalSubtitle"] = "Wprowadź dane uwierzytelniające lub 4-cyfrowy kod PIN stacji.",
            ["login.quickRoles"] = "SZYBKI WYBÓR ROLI (TEST STACJI)",
            ["login.roleAdmin"] = "👑 Administrator",
            ["login.roleButcher"] = "🔪 Główny Rzeźnik",
            ["login.roleVet"] = "🩺 Weterynarz",
            ["login.roleCashier"] = "🛒 Kasjer",
            ["login.tabStandard"] = "Login / Hasło",
            ["login.tabPin"] = "Szybki PIN kasjera",
            ["login.username"] = "NAZWA UŻYTKOWNIKA",
            ["login.password"] = "HASŁO",
            ["login.showPassword"] = "👁 Pokaż",
            ["login.rememberMe"] = "Zapamiętaj mnie",
            ["login.loginBtn"] = "ZALOGUJ DO SYSTEMU ➔",
            ["login.pinPrompt"] = "Wprowadź 4-cyfrowy kod PIN stacji:",
            ["login.systemStatus"] = "Status systemu: ",
            ["login.onlineStatus"] = "Online & Baza zaszyfrowana aktywna"
        }
    };

    public string CurrentLanguageCode => _currentLanguageCode;

    public LanguageInfo CurrentLanguage => 
        _supportedLanguages.FirstOrDefault(l => l.Code.Equals(_currentLanguageCode, StringComparison.OrdinalIgnoreCase)) 
        ?? _supportedLanguages[0];

    public IReadOnlyList<LanguageInfo> SupportedLanguages => _supportedLanguages.AsReadOnly();

    public void SetLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode)) return;

        var matched = _supportedLanguages.FirstOrDefault(l => l.Code.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
        if (matched != null)
        {
            _currentLanguageCode = matched.Code;
            try
            {
                var culture = new CultureInfo(matched.CultureName);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch { }

            LanguageChanged?.Invoke(this, _currentLanguageCode);
        }
    }

    public string Get(string key, params object[] args)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;

        if (_translations.TryGetValue(_currentLanguageCode, out var dict) && dict.TryGetValue(key, out var val))
        {
            return args.Length > 0 ? string.Format(val, args) : val;
        }

        // Fallback to English, then Turkish
        if (_translations["en-GB"].TryGetValue(key, out var enVal))
        {
            return args.Length > 0 ? string.Format(enVal, args) : enVal;
        }

        if (_translations["tr-TR"].TryGetValue(key, out var trVal))
        {
            return args.Length > 0 ? string.Format(trVal, args) : trVal;
        }

        return key;
    }

    public string this[string key] => Get(key);
}
