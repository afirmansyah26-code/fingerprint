# Panduan implementasi

## Status saat ini

Fondasi aplikasi dan probe koneksi SDK sudah dibuat serta berhasil dikompilasi sebagai executable x86. Probe belum dijalankan pada mesin GU-06; aplikasi belum siap untuk operasi absensi produksi.

## Yang sudah tersedia

- Shell WinForms dengan navigasi Ringkasan, Karyawan, Mesin fingerprint, Record absensi, Aturan kerja, Laporan, dan Impor data.
- Database SQLite lokal melalui winsqlite3.dll. Inisialisasi database berhasil saat aplikasi dibuka.
- CRUD karyawan lokal: tambah, edit, nonaktifkan/aktifkan, dan hapus.
- Hapus karyawan memperingatkan bahwa riwayat absensi/koreksi lokal ikut terhapus. Operasi ini tidak memanggil SDK dan tidak mengubah pengguna, sidik jari, atau record di mesin.
- Pendaftaran beberapa konfigurasi mesin pada satu komputer. Menambah, mengedit, dan menghapus di halaman ini hanya mengubah konfigurasi lokal.
- Unduh raw record absensi melalui SDK, penyimpanan lokal transaksional dengan deduplikasi, dan daftar hingga 1.000 record terbaru. User ID mesin belum dipetakan ke master karyawan; alur belum diuji pada perangkat.
- Uji koneksi SDK untuk mesin terpilih, termasuk pembacaan nomor seri/firmware; nomor seri tersimpan pada konfigurasi lokal setelah pembacaan berhasil.
- Template CSV karyawan UTF-8 dengan pemisah titik koma, ekspor, baca, validasi kode ganda, pratinjau, dan impor transaksional. Template dapat dibuka di Excel.
- Schema awal untuk karyawan, mesin, pemetaan user per mesin, raw log, koreksi lokal, dan shift.
- Keadaan kosong dan status fitur yang belum tersedia dijelaskan pada layar; tidak ada data contoh karyawan atau mesin.

## Yang belum tersedia

- Pembacaan/preview pengguna mesin serta pemetaan dan sinkronisasi data karyawan antara aplikasi dan mesin.
- Penulisan data karyawan ke mesin dan transfer templat sidik jari.
- Penghapusan atau pengeditan pengguna mesin masih pending. Jangan gunakan ClearGLog.
- Impor Att MDB secara read-only; pemetaan schema dan provider masih perlu diselesaikan.
- Pengaturan shift/jam kerja, toleransi, hari libur, izin, lembur, dan koreksi melalui UI.
- Laporan serta ekspor/cetak laporan.
- Installer lengkap yang memasang dependency SDK secara aman.


## Teknologi

- C# WinForms x86.
- Project memakai target API CLR 4.0 karena compiler dan MSBuild lokal tersedia tanpa .NET targeting pack. Windows menggunakan runtime .NET Framework 4.x yang kompatibel secara in-place. Kode fase awal sengaja membatasi pemakaian API ke permukaan CLR 4.0.
- SQLite lokal menggunakan API Windows winsqlite3.dll supaya fase ini tidak bergantung pada paket NuGet eksternal. Microsoft mencantumkan API SQLite di winsqlite3.dll mulai Windows 10 build 10586. Jika dukungan Windows 10 build sebelum itu diperlukan, siapkan SQLite privat pada installer; jangan menyalin atau mengganti DLL sistem. [Microsoft: Windows 10 extension APIs](https://learn.microsoft.com/en-us/uwp/win32-and-com/win32-extension-apis)
- Adapter SDK harus terisolasi dari UI dan mengikuti arsitektur 32-bit COM.

## Lingkungan build yang ditemukan

- .NET SDK dotnet tidak terpasang.
- Compiler C# ditemukan di C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe; compiler mendukung C# 5.
- MSBuild ditemukan di C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe.
- Targeting pack .NET Framework tidak ditemukan. MSBuild berhasil membangun dengan fallback GAC dan mengeluarkan warning MSB3644. Pasang targeting pack yang sesuai pada mesin build sebelum menganggap build rilis final.
- winsqlite3.dll tersedia pada OS mesin pengembangan untuk x86 dan x64.
- COM zkemkeeper 32-bit v6.2.5.7 terdaftar pada mesin pengembangan. Keberadaan lokal ini belum berarti SDK sudah dikemas di installer.

## Struktur kode

- src/FingerprintAttendance/Program.cs — startup dan pesan kegagalan.
- src/FingerprintAttendance/Data/AppDatabase.cs — schema, repository lokal, dan interop SQLite.
- src/FingerprintAttendance/Device/DeviceConnectionProbe.cs — probe koneksi dan baca identitas mesin melalui COM.
- src/FingerprintAttendance/Device/DeviceDataReader.cs — baca record absensi dari buffer SDK.
- src/FingerprintAttendance/Domain/AttendanceRecord.cs — model untuk record lokal.
- src/FingerprintAttendance/UI/MachineProbeActions.cs — aksi uji koneksi pada daftar mesin.
- src/FingerprintAttendance/Domain/ — model karyawan dan mesin.
- src/FingerprintAttendance/UI/ — theme, shell, halaman, dan dialog.
- src/FingerprintAttendance/Import/ — parser dan layar impor CSV.
- docs/ — rencana, desain, implementasi, dan catatan perubahan.

## Lokasi data

Database runtime berada di %LOCALAPPDATA%\FingerprintAttendance\attendance.sqlite3. Database tidak diletakkan di folder program dan tidak memakai Att/att2000.mdb sebagai database berjalan. Hapus employee secara lokal menghapus raw log dan koreksi lokal yang terhubung melalui foreign key cascade.

## Build

Dari PowerShell pada root proyek:

& 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe' 'src\FingerprintAttendance\FingerprintAttendance.csproj' /p:Configuration=Debug /p:Platform=x86

Output default: src\FingerprintAttendance\bin\x86\Debug\FingerprintAttendance.exe

Build terbaru berhasil ke src\FingerprintAttendance\bin\x86\ReviewAttendanceRead\FingerprintAttendance.exe. Warning MSB3644 masih berlaku karena targeting pack .NET Framework belum tersedia.

## Aturan pengembangan

- Semua dokumentasi proyek berada di docs/. Perubahan struktur, keputusan, fitur, dan status harus diikuti pembaruan dokumentasi pada tahap yang sama.
- Jangan menjalankan operasi tulis/hapus mesin ketika CRUD lokal digunakan.
- Jangan gunakan ClearGLog.
- Jangan modifikasi database Att saat membaca atau mengimpor.
- Status koneksi hanya boleh ditampilkan setelah ada operasi koneksi nyata yang berhasil.
- Sinkronisasi harus menampilkan perbedaan dan target mesin sebelum mengirim perubahan. Jangan menghapus pengguna mesin secara otomatis.
- Jangan menganggap sinkronisasi data karyawan berarti menyalin templat sidik jari.

## Tahap koneksi SDK

- Halaman Mesin fingerprint menyediakan tombol **Uji koneksi** untuk mesin aktif yang dipilih. Operasi berjalan di latar belakang agar layar tetap responsif.
- Probe memakai COM zkemkeeper.ZKEM.1, memanggil Connect_Net(host, port), lalu mencoba membaca nomor seri dan versi firmware melalui GetSerialNumber serta GetFirmwareVersion. Koneksi dilepas melalui Disconnect.
- Probe tidak membaca pengguna, templat, atau record absensi, dan tidak mengirim perubahan ke mesin. Belum ada koneksi yang dijalankan terhadap ICON GU-06 dalam sesi implementasi ini. Nomor seri yang berhasil dibaca disimpan lokal; status koneksi hanya berlaku selama halaman mesin terbuka.
- Probe memerlukan registrasi SDK COM 32-bit di komputer. Installer dan hak redistribusi SDK belum ditetapkan.

### Bukti TypeLib lokal

TypeLib dari C:\Windows\SysWOW64\zkemkeeper.dll diperiksa tanpa membuat instance perangkat atau menjalankan metode SDK. Signature yang relevan:

- Connect_Net(BSTR IPAdd, I4 Port) -> VARIANT_BOOL. Adapter memakai nomor mesin SDK 1 untuk membaca identitas; angka ini mengikuti konvensi penggunaan SDK dan belum divalidasi pada GU-06.
- GetSerialNumber(I4 dwMachineNumber, out BSTR dwSerialNumber) -> VARIANT_BOOL.
- GetFirmwareVersion(I4 dwMachineNumber, out BSTR strVersion) -> VARIANT_BOOL.
- SSR_GetAllUserInfo(I4 dwMachineNumber, out BSTR dwEnrollNumber, out BSTR Name, out BSTR Password, out I4 Privilege, out VARIANT_BOOL Enabled) -> VARIANT_BOOL.
- SSR_GetGeneralLogData menyediakan ID pengguna, metode verifikasi, mode in/out, waktu, dan kode kerja melalui parameter output.
- ClearGLog(I4 dwMachineNumber) terdaftar, tetapi tidak pernah dipanggil aplikasi.

TypeLib menjelaskan deklarasi API, bukan jaminan kompatibilitas dengan firmware GU-06. Alur baca record sudah dibuat tetapi belum diuji pada perangkat. Pembacaan pengguna dan semua operasi tulis masih perlu diimplementasikan serta diuji secara terkontrol.

## Unduh record absensi

- Tombol Unduh record membaca log mesin melalui ReadAllGLogData dan SSR_GetGeneralLogData pada worker latar belakang.
- Record disimpan lokal sebagai waktu mesin, User ID perangkat, metode verifikasi, mode absensi, kode kerja, dan snapshot mesin. User ID yang belum dipetakan ke master karyawan tetap disimpan tanpa menebak kecocokan.
- Unduhan ulang mengabaikan duplikat dengan kunci mesin, User ID, waktu, dan metode verifikasi. Log asli mesin tidak dihapus atau diubah.
- Halaman Record absensi menampilkan hingga 1.000 record terbaru; filter tanggal/mesin dan pencocokan identitas karyawan belum tersedia.
- Schema menambahkan kolom mode absensi serta kode kerja. Inisialisasi menambahkan kolom tersebut ke database SQLite lokal lama bila belum ada.
- Alur baca ini sudah dikompilasi, tetapi belum dijalankan pada ICON GU-06. Semantik detail mode verifikasi dan mode absensi perlu dicocokkan dengan firmware perangkat.
## Repositori GitHub

- Remote origin: https://github.com/afirmansyah26-code/fingerprint.git
- Branch kerja utama: main.
- Git melacak source di src/ dan dokumentasi di docs/.
- .gitignore mengecualikan Att/ (database, log, dan binari aplikasi lama), hasil build, serta berkas database lokal.