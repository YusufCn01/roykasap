# RoyPos Kasap — Kasap ve Mezbaha Otomasyon Sistemi

Visual Studio (.NET 8, C#), WPF ve **MVVM Community Toolkit** mimarisiyle geliştirilmiş, **Temiz Beyaz & Kasap Kırmızısı** temalı, **Tam Ekran (Fullscreen)** pencereli, büyük kartlı ana menülü, her modüle özel tam sayfalı ve donanım kilitli RSA lisanslamaya sahip kurumsal masaüstü otomasyon çözümü.

---

## 🥩 Temel Özellikler ve Mimari

### 1. 🖥 Tam Ekran Pencere ve Özel Başlık Çubuğu
- `WindowState="Maximized"`, `WindowStyle="None"`, `1366x768`'den `4K`'ya kadar responsive tasarım.
- **[F11]** tuşu ile pencereli / tam ekran arasında anında geçiş.
- Özel üst başlık çubuğu: **RoyPos Kasap** logosu, Terminal/Şube bilgisi, Canlı Terazi Hazır rozeti, Dijital saat, **[F1]** Kısayollar butonu ve pencere kontrolleri.

### 2. 🔐 Giriş (Login) Ekranı — Yüksek Okunabilirlik & Beyaz Kart
- Temiz açık zemin üzerinde ortalanmış modern beyaz giriş kartı.
- **Kullanıcı Adı / Şifre**: Min 48px yükseklikte alanlar, şifre maskeleme + 👁 göster/gizle ikonu, "Beni Hatırla" seçeneği.
- **Hızlı Kasiyer PIN**: Büyük dokunmatik numpad (min 50px tuşlar).
- Hata durumunda kırmızı, ikonlu net okunabilir banner.

### 3. 🥩 RoyPos Kasap Ana Menü (Hub) — 3 Ana Kategori
- Girişten sonra açılan tam ekran modül merkezi:
  - **Bölüm 1: Satış ve Müşteri Hizmetleri**
    - 🥩 **Hızlı Dokunmatik Satış (POS) `[F2]`** *(Canlı terazi okuma, ağırlık gömülü barkodlar, nakit/kart parçalı tahsilat)*
    - 💰 **Finans & Kasa Yönetimi `[F6]`** *(Gün açılışı/kapanışı, kasa sayımı ve fark raporu)*
    - 👥 **Cari Hesaplar `[F7]`** *(Müşteri borç/alacak takibi, ekstre ve tek tıkla SMS)*
  - **Bölüm 2: Mezbaha, Üretim ve Depo Yönetimi**
    - 🔪 **Üretim & Mezbaha (BOM) `[F3]`** *(Karkas parti girişi, parçalama reçetesi, randıman ve ağırlıklı maliyet dağıtımı)*
    - 📦 **Stok ve Depo Takibi `[F4]`** *(Soğuk hava depoları, geriye dönük parti izlenebilirliği ve kritik SKT takibi)*
    - 🏷 **Ürün & Barkod Tanımları `[F5]`** *(Et tanımları, tartılı PLU kodları, EAN-13 terazi barkodu ve etiket tasarımcısı)*
  - **Bölüm 3: Yönetim, Raporlar ve Entegrasyon**
    - 📊 **Raporlar & Analiz `[F9]`** *(Ciro, kârlılık, randıman ve Excel .xlsx çıktısı)*
    - 📑 **E-Fatura / E-Arşiv `[F10]`** *(GİB e-Belge oluşturma, gönderme ve arşiv)*
    - 👤 **Personel & Yetki** *(Kullanıcı hesapları ve PIN kodları)*
    - 🔑 **Lisans Yönetimi** *(Donanım kimliği ve RSA lisans yükleme)*
    - ⚙ **Sistem Ayarları** *(Terazi COM portu, SMS ve veritabanı yedekleme)*

---

## 🔑 4. Lisans Sistemi ve Aktivasyon Akışı

- **Donanım Kilidi (Machine Fingerprint)**: CPU ID + Disk Seri No + MAC Hash.
- **2048-bit RSA Dijital İmza**: `KasapOtomasyon.LicenseManager` aracıyla bağımsız `.lic` dosyası üretme ve yükleme.
- Otomatik **15 Günlük Deneme Sürümü**.

---

## 🚀 5. Çalıştırma ve Kurulum

```powershell
# Çözümü Derleme
dotnet build

# Birim Testleri Çalıştırma (7/7 Başarılı - %100)
dotnet test

# RoyPos Kasap Ana Uygulamasını Başlatma
dotnet run --project "src/KasapOtomasyon.WPF/KasapOtomasyon.WPF.csproj"

# Lisans Üretim Aracını Başlatma (Satıcı Paneli)
dotnet run --project "src/KasapOtomasyon.LicenseManager/KasapOtomasyon.LicenseManager.csproj"
```

**Varsayılan Giriş Bilgileri:**
- **Yönetici**: `admin` / `admin123` (PIN: `1234`)
- **Kasiyer**: `kasiyer1` / `kasa123` (PIN: `0000`)
- **Kasap / Üretim Sorumlusu**: `kasap1` / `kasap123` (PIN: `1111`)
