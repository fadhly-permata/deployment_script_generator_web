# IDC.Template (.NET 8 Web API Modular Template)

## Overview
IDC.Template adalah template proyek Web API berbasis .NET 8 yang dirancang modular, scalable, dan mudah dikustomisasi. Template ini mengadopsi dependency injection, konfigurasi berbasis file JSON, serta pemisahan logika melalui partial class dan folder feature.

## Fitur Utama
- **Modular Dependency Injection**:  
   Semua konfigurasi dependency injection terpusat di file `Program.DI.cs` melalui metode `SetupDI()`.  
   > [!NOTE]  
   > Setiap service, middleware, dan handler didaftarkan secara scoped atau singleton sesuai kebutuhan modul.

- **Konfigurasi Dinamis**:  
   Menggunakan dua file konfigurasi utama:  
   - `appconfigs.jsonc` untuk runtime settings, mendukung komentar dan perubahan otomatis.  
   - `appsettings.json` untuk environment settings standar ASP.NET.  
   > [!TIP]  
   > Akses konfigurasi menggunakan dot notation, contoh:  
   > `config.Get(path: "Security.Cors.Enabled")`

- **Partial Program Classes**:  
   Setup aplikasi dipisah ke beberapa file partial seperti `Program.cs`, `Program.DI.cs`, `Program.Middlewares.cs`, dan `Program.Services.cs`.  
   > [!IMPORTANT]  
   > Pemisahan ini memudahkan pengelolaan logika startup dan penambahan fitur baru.

- **Middleware Dinamis**:  
   Berikut adalah daftar middleware yang digunakan pada aplikasi ini:  
   1. **Request Logging**  
      Mencatat setiap permintaan HTTP yang masuk, termasuk path, metode, dan status respons.  
      > [!NOTE]  
      > Logging dapat dikonfigurasi untuk menulis ke file, atau sistem operasi.

   2. **Rate Limiting**  
      Membatasi jumlah permintaan dari satu client dalam periode waktu tertentu untuk mencegah abuse.  
      > [!TIP]  
      > Konfigurasi threshold dan window time dapat diatur melalui `appconfigs.jsonc`.

   3. **Response Compression**  
      Mengompresi respons HTTP (gzip, brotli) untuk menghemat bandwidth dan mempercepat pengiriman data.  
      > [!IMPORTANT]  
      > Compression otomatis aktif untuk konten yang mendukung dan dapat dinonaktifkan via konfigurasi.

   4. **Security Headers**  
      Menambahkan header keamanan seperti `X-Frame-Options`, `X-XSS-Protection`, dan `Content-Security-Policy`.  
      > [!WARNING]  
      > Header dapat disesuaikan untuk memenuhi standar keamanan aplikasi Anda.

   5. **API Key Authentication**  
      Melindungi endpoint dengan validasi API Key pada setiap permintaan, kecuali path yang dikecualikan.  
      > [!NOTE]  
      > Path seperti Swagger UI, CSS, JS, themes, dan images otomatis dikecualikan dari autentikasi.

   6. **Exception Handling**  
      Menangani error secara global dan mengembalikan respons terstruktur dengan kode dan pesan error.  
      > [!TIP]  
      > Error log dapat diintegrasikan dengan sistem monitoring eksternal.

   7. **Swagger UI**  
      Menyediakan dokumentasi interaktif API, mendukung theme switching dan grouping endpoint.  
      > [!IMPORTANT]  
      > Swagger UI hanya tersedia pada environment tertentu dan dapat dikustomisasi.

   8. **Static File Serving**  
      Melayani file statis seperti konfigurasi, tema, gambar, dan log dari folder `wwwroot/`.  
      > [!NOTE]  
      > Path dan akses file statis dapat diatur sesuai kebutuhan aplikasi.
   
   Semua middleware dapat diaktifkan atau dinonaktifkan melalui konfigurasi di `appconfigs.jsonc`.  
   
- **Swagger UI**:  
   Mendukung theme switching secara runtime, memungkinkan pengguna memilih tampilan sesuai preferensi. Mendukung grouping endpoint menggunakan atribut `[ApiExplorerSettings(GroupName = "...")]` untuk memudahkan navigasi API. Menyediakan dokumentasi interaktif dengan fitur pencarian, filter, dan try-out langsung pada endpoint. Mendukung custom header dan autentikasi API Key secara otomatis pada permintaan yang relevan. Swagger UI otomatis mengecualikan path seperti CSS, JS, themes, dan images dari API Key Auth.  

   > [!TIP]  
   > Swagger UI dapat dikustomisasi melalui konfigurasi, termasuk pengaturan tema, logo, dan aksesibilitas endpoint.

- **Dynamic Endpoint Generator**:  
   Mendukung penambahan endpoint API secara dinamis melalui file `endpoint_generator.jsonc` tanpa perlu modifikasi kode.  
   Endpoint baru dapat diatur path, method, response, dan autentikasinya langsung dari konfigurasi.  
   > [!NOTE]  
   > Fitur ini memudahkan integrasi API baru dan prototyping cepat.

- **Primary Constructor Controllers**:  
   Semua controller menggunakan primary constructor untuk dependency injection yang lebih ringkas dan aman.  
   Controller dipisah per fitur menggunakan partial class, memudahkan pengelolaan dan pengembangan.  
   > [!IMPORTANT]  
   > Pattern ini meningkatkan testabilitas dan maintainability kode.

- **Auto Persist Config**:  
   Perubahan konfigurasi runtime otomatis tersimpan ke `appconfigs.jsonc` tanpa restart aplikasi.  
   Mendukung rollback dan audit perubahan konfigurasi.  
   > [!TIP]  
   > Konfigurasi dapat diubah melalui API atau UI admin jika tersedia.

- **Extensible Utility Library**:  
   Menggunakan `IDC.Utilities.dll` sebagai library eksternal untuk helper, extension, dan model umum.  
   Library ini dapat diperluas sesuai kebutuhan proyek dan diintegrasikan secara modular.  
   > [!NOTE]  
   > Referensi lokal memastikan kompatibilitas dan kontrol versi internal.

- **Comprehensive Error Handling**:  
   Global exception handler mengembalikan respons terstruktur dengan kode dan pesan error yang jelas.  
   Mendukung integrasi dengan sistem monitoring eksternal dan logging detail.  
   > [!WARNING]  
   > Error log dapat dikonfigurasi untuk dikirim ke file, terminal, dan sistem operasi.

- **Scoped Middleware Registration**:  
   Semua middleware didaftarkan secara scoped untuk efisiensi dan keamanan dependency injection.  
   > [!IMPORTANT]  
   > Pattern ini mencegah memory leak dan memastikan lifecycle yang tepat.

- **Advanced Configuration Access**:  
   Mendukung akses konfigurasi nested dengan dot notation dan default value.  
   > [!TIP]  
   > Contoh: `config.Get(path: "Security.Cors.Enabled")`

- **Async Method Variants**:  
   Setiap method penting memiliki versi async dengan callback dan cancellation token.  
   > [!NOTE]  
   > Tidak mengubah method sync yang sudah ada, menjaga backward compatibility.

- **XML Documentation & DocFX Alerts**:  
   Semua method terdokumentasi dengan XML DocFX style, lengkap dengan alert, contoh kode, dan penjelasan formal.  
   > [!TIP]  
   > Dokumentasi dapat di-generate otomatis untuk kebutuhan internal dan eksternal.

- **Internal Licensing**:  
   Proyek menggunakan lisensi internal IDC, dengan opsi penggunaan eksternal melalui persetujuan tim pengembang.  
   > [!IMPORTANT]  
   > Hubungi tim pengembang untuk informasi lisensi dan kontribusi.

- **Auto Persist Config**:  
   Setiap perubahan konfigurasi melalui aplikasi akan otomatis tersimpan ke file `appconfigs.jsonc` tanpa perlu restart.  
   > [!NOTE]  
   > Fitur ini memastikan konsistensi konfigurasi runtime.

- **Extensible Utility**:  
   Menggunakan library eksternal `IDC.Utilities.dll` yang direferensikan secara lokal dari  
   [`Repository IDC.Utilities`](https://scm.idecision.ai/idecision_source_net8/idc.utility)  
   > [!TIP]  
   > Library ini menyediakan berbagai helper, extension, dan model yang dapat digunakan di seluruh proyek.

## Directory Structures
   ```
   ├── Controllers/               # Partial controllers per feature
   ├── Utilities/                 # Helpers, Extensions, Models, etc.
   ├── wwwroot/                   # Static files, configs, themes, logs
   ├── Program.*.cs               # Partial program setup files
   ├── appconfigs.jsonc           # Runtime config (with comments)
   ├── appsettings.json           # Standard ASP.NET settings
   └── endpoint_generator.jsonc   # Dynamic endpoint definitions
   ```

## Coding Standards
- Selalu gunakan nama argumen pada pemanggilan method:  
  `config.Get(path: "app.name", defaultValue: "default")`
- Implementasi null safety & nullable.
- Inisialisasi koleksi secara ringkas.
- Fungsi return class type untuk chaining.
- Controller wajib pakai primary constructor:  
  `public class DemoController(SystemLogging systemLogging, Language language)`
- Satu baris perintah tanpa kurung kurawal `{}`.
- Tidak perlu deklarasi variabel jika hanya digunakan sekali.

## Dokumentasi
- Semua method (termasuk private/internal) wajib XML DocFX style.
- Bahasa Inggris formal, max 100 karakter per baris.
- Sertakan `<summary>`, `<remarks>`, `<example>`, `<code>`, `<returns>`, `<exception>`.
- Gunakan DocFX alert:  
  `> [!NOTE]`, `> [!TIP]`, `> [!IMPORTANT]`, dll.
- Contoh kode wajib pada `<remarks>`.

## Installasi
### Installation Steps

1. **Clone the Repository**  
   Jalankan perintah berikut untuk meng-clone repo:  
   ```bash
   git clone https://scm.idecision.ai/idecision_source_net8/idc.template
   ```

2. **Run the Installer**  
   Jalankan salah satu file installer sesuai OS:  
   - Windows:  
     ```powershell
     .\installer.ps1
     ```
   - Linux/macOS:  
     ```bash
     ./installer.sh
     ```

3. **Follow Installation Instructions**  
   Ikuti instruksi pada terminal hingga proses instalasi selesai.  

> [!IMPORTANT]  
> Pastikan semua dependensi dan konfigurasi terpasang sebelum menjalankan aplikasi.

## Contoh Penggunaan Konfigurasi
```csharp
// Nested config access dengan dot notation
var isEnabled = _appConfigs.Get<bool>(path: "Security.Cors.Enabled");
var maxItems = _appConfigs.Get(path: "app.settings.maxItems", defaultValue: 100);
```

## Menambah Method Async
- Tambahkan versi async dengan callback & cancellation token.
- Tidak mengubah method sync yang sudah ada.

## Komunikasi
- Penjelasan teknis dalam Bahasa Indonesia jika diperlukan.
- Kode harus jelas, minim penjelasan tambahan.

## Referensi Eksternal
- `IDC.Utilities.dll`: [`Repository IDC.Utilities`](https://scm.idecision.ai/idecision_source_net8/idc.utility)

## Lisensi
Proyek ini menggunakan lisensi internal IDC. Untuk penggunaan eksternal, silakan hubungi tim pengembang.

---

> [!TIP]
> Untuk detail arsitektur, lihat file partial di root dan folder `Controllers/`, serta dokumentasi XML pada setiap method.
