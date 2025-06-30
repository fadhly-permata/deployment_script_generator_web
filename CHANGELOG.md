# Changelog

Semua perubahan penting pada proyek ini akan didokumentasikan dalam file ini.

## [1.0.1] - 2024-03-17

### Keamanan
- Implementasi autentikasi API key:
  * Sistem validasi API key
  * Pengelolaan akses berbasis token
- Penerapan pembatasan rate:
  * Konfigurasi batas request per IP
  * Sistem pencatatan dan monitoring
- Peningkatan keamanan sistem:
  * Validasi input yang lebih ketat
  * Enkripsi data sensitif
  * Penanganan error yang aman

### File yang Diubah
- Program.cs
- Startup.cs
- Security/ApiKeyAuthenticationHandler.cs
- Middleware/RateLimitingMiddleware.cs
- Configuration/SecuritySettings.cs
