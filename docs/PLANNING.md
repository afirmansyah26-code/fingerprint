# Rencana Aplikasi Absensi Fingerprint

**Status:** jalur baca record dan probe koneksi sudah diimplementasikan; belum diuji pada mesin.  
**Versi dokumen:** 1.2  
**Bahasa dokumen:** Indonesia

## Tujuan

Membuat aplikasi desktop Windows standalone sebagai pengganti Att untuk kebutuhan inti karyawan dan absensi. Aplikasi mengelola satu atau lebih mesin fingerprint dari satu komputer, membaca data absensi dari mesin melalui SDK yang kompatibel, menyimpan data lokal, dan menghasilkan laporan.

Dokumen ini menjadi konteks handoff untuk agent yang melanjutkan proyek. Bedakan temuan terverifikasi, keputusan pengguna, rekomendasi, dan hal yang belum diketahui. Jangan menganggap operasi yang belum diverifikasi di mesin sebagai kemampuan yang sudah tersedia.

## Keputusan yang sudah disepakati

- Platform target: Windows 10 ke atas.
- Aplikasi: desktop standalone; satu komputer dapat mengelola beberapa mesin fingerprint.
- Mesin saat ini terhubung melalui LAN. Att memakai port TCP 4370.
- ADMS adalah aplikasi PHP di ZimaOS pada LAN dan hanya menerima/membaca record absensi. ADMS tidak menjadi bagian integrasi aplikasi baru.
- Tidak perlu akun admin/pengguna aplikasi pada tahap awal.
- Fitur inti: manajemen karyawan, sinkronisasi karyawan dengan mesin, unduh dan pengelolaan record absensi, aturan absensi, dan laporan.
- Pengaturan shift mencakup shift siang dan kemampuan shift malam untuk kebutuhan mendatang. Lembaga saat ini tidak menggunakan shift malam.
- Laporan awal: harian dan bulanan, per karyawan dan departemen, dapat dicetak atau diekspor ke PDF dan Excel.
- Impor data memiliki dua jalur: database Att lama (`att2000.mdb`) atau template impor bila database Att tidak tersedia.
- Hapus karyawan dari aplikasi hanya mengubah data aplikasi dan tidak mengirim perintah ke mesin. Tindakan hapus lokal memberi peringatan tentang data/riwayat aplikasi yang ikut dihapus.
- Hapus pengguna pada mesin masih **pending** sampai pemilik memeriksa perilaku fitur tersebut di Att.
- Jangan menggunakan `ClearGLog`: SDK mengindikasikan fungsi tersebut menghapus log seluruh mesin. Jangan menyediakan penghapusan seluruh log sebagai jalan pintas untuk menghapus satu karyawan.

## Lingkup fitur

### 1. Karyawan aplikasi

- Tambah dan edit data karyawan.
- Nonaktifkan dan aktifkan kembali karyawan; status nonaktif mempertahankan riwayat aplikasi.
- Hapus karyawan dari database aplikasi setelah konfirmasi dampak.
- Penghapusan lokal tidak mengubah pengguna, templat sidik jari, atau record pada mesin.
- Sediakan pemetaan antara ID karyawan di aplikasi dan User ID pada setiap mesin. Nilai tersebut bisa berbeda antar-mesin.

Field kandidat untuk master karyawan: nomor/NIP, nama, departemen, jabatan, tanggal mulai, status, dan ID pengguna per mesin. Field final perlu dicocokkan dengan skema `att2000.mdb` dan kemampuan SDK.

### 2. Mesin dan sinkronisasi karyawan

- Daftar mesin dengan nama, alamat IP, port (default 4370), status koneksi, serta identitas mesin jika tersedia.
- Baca daftar pengguna dari mesin dan bandingkan dengan master aplikasi.
- Kirim data karyawan dari aplikasi ke mesin melalui operasi eksplisit.
- Tampilkan pratinjau perbedaan sebelum sinkronisasi: hanya di aplikasi, hanya di mesin, atau data yang berbeda.
- Jangan menghapus pengguna atau data mesin secara otomatis sebagai akibat dari penghapusan lokal atau sinkronisasi.
- Sinkronisasi templat sidik jari antar-mesin belum termasuk asumsi awal. Verifikasi dukungan SDK dan persetujuan pemilik sebelum menambahkan fitur tersebut.
- Field yang dapat dibaca/ditulis mengikuti kemampuan SDK dan firmware; jangan menjanjikan field sebelum dipastikan.

### 3. Absensi

- Unduh record absensi dari masing-masing mesin ke penyimpanan lokal.
- Catat mesin sumber untuk setiap record. ID mesin perlu menjadi bagian identitas record agar unduhan beberapa mesin dapat dibedakan.
- Sediakan pencarian/filter tanggal, mesin, karyawan, dan departemen.
- Deteksi duplikat secara konsisten ketika unduhan dijalankan ulang.
- Koreksi absensi yang dibuat di aplikasi harus dibedakan dari record asli mesin.
- Penyuntingan atau penghapusan record langsung pada mesin per karyawan belum dijanjikan; belum ditemukan bukti dukungan SDK untuk operasi selektif tersebut.
- `ClearGLog` tidak digunakan dan fitur hapus semua log mesin tidak termasuk lingkup.

### 4. Aturan kerja dan absensi

Dapat dikonfigurasi dari aplikasi:

- Jam masuk dan pulang.
- Shift, termasuk shift yang melewati tengah malam.
- Hari kerja dan hari libur.
- Batas toleransi keterlambatan.
- Perhitungan keterlambatan, pulang awal, dan lembur.
- Izin/cuti dan koreksi/pengecualian absensi.

Konfigurasi awal dapat memakai jadwal siang yang digunakan lembaga. Aturan akhir perhitungan perlu disepakati sebelum implementasi agar hasil laporan konsisten.

### 5. Laporan

- Harian dan bulanan.
- Rekap per karyawan dan per departemen.
- Detail record absensi mentah dan hasil perhitungan kehadiran.
- Cetak dan ekspor PDF/Excel.
- Tata letak dan kolom laporan disesuaikan dengan laporan Att yang benar-benar dibutuhkan.

### 6. Impor

**Dari Att lama**

- Pengguna memilih salinan `att2000.mdb`.
- Baca sumber secara read-only; jangan memodifikasi database Att.
- Pratinjau jumlah dan kategori data yang akan diimpor, pemetaan field, serta potensi duplikat.
- Impor data yang relevan seperti departemen, karyawan, aturan yang tersedia, dan riwayat absensi bila struktur database mendukung.
- Validasi hasil sebelum data dimasukkan ke database baru.
- Jangan mengimpor atau menyalin templat biometrik tanpa kebutuhan dan persetujuan eksplisit.

**Tanpa database Att**

- Sediakan template Excel untuk impor master karyawan.
- Sertakan petunjuk kolom wajib, format tanggal, departemen, dan contoh baris.
- Template untuk impor record absensi dari sumber lain merupakan keputusan terpisah; belum termasuk kebutuhan awal yang dipastikan.
- Validasi baris dan tampilkan kesalahan sebelum impor final.

## Rekomendasi teknologi dan arsitektur

Rekomendasi awal: **C# WinForms x86, .NET Framework 4.8, dan SQLite**.

Alasan: SDK yang ditemukan menggunakan COM 32-bit (`zkemkeeper`), sehingga aplikasi 32-bit berbasis .NET memberi jalur integrasi yang lebih langsung. Aplikasi WinForms dan SQLite sesuai untuk penggunaan lokal ringan tanpa server. Pilihan ini rekomendasi, bukan keputusan implementasi yang sudah diuji.

Pemisahan komponen yang disarankan:

1. UI desktop untuk navigasi dan operasi pengguna.
2. Layanan aplikasi untuk impor, sinkronisasi, absensi, aturan kerja, dan laporan.
3. Adapter perangkat terpisah untuk komunikasi melalui SDK COM.
4. Database SQLite lokal untuk master karyawan, konfigurasi, pemetaan mesin, raw log, koreksi, dan hasil perhitungan.
5. Modul backup/restore database lokal.

Installer perlu menyediakan komponen runtime dan dependensi SDK yang tepat. Pengaturan COM dan dependensi native harus diverifikasi pada Windows 10+ bersih. Hindari menjalankan skrip registrasi SDK lama secara buta: skrip yang diperiksa menyalin sejumlah DLL ke `SysWOW64`, dan skrip penghapusannya dapat menghapus DLL sistem. Installer harus mencatat berkas yang dipasang dan menghapus hanya berkas yang memang dimilikinya.

## Temuan lingkungan dan batas bukti

### Perangkat

Dari foto informasi perangkat yang diberikan pengguna:

- Model/label: ICON GU-06.
- Manufacturer: HT Corporation.
- Platform: ZLM60_TFT.
- Algoritma sidik jari: Finger VX10.0.
- Penggunaan saat ini melalui LAN; port 4370 untuk Att menurut pengguna.

Perangkat OEM dan firmware dapat membatasi perilaku SDK. Model serta informasi layar belum membuktikan dukungan setiap operasi.

### Att dan SDK

- Workspace proyek berisi folder `Att` dan database `att2000.mdb`.
- Instalasi ini terdeteksi memakai COM `zkemkeeper` 32-bit versi 6.2.5.7. Salinan SDK yang cocok ditemukan di luar folder `Att` pada mesin pengembangan.
- Folder `Att` sendiri tidak berisi SDK COM tersebut.
- Daftar API yang diperiksa mencakup koneksi jaringan, pembacaan log umum, pembacaan pengguna, penulisan informasi pengguna, dan operasi penghapusan pengguna/templat.
- `ClearGLog` hanya menerima nomor mesin pada signature yang diperiksa; ini mendukung kesimpulan bahwa operasinya bersifat keseluruhan mesin. Tidak ada API selektif untuk edit/hapus record individual yang berhasil dipastikan dari pemeriksaan tersebut.
- Pemeriksaan daftar API bukan pengujian terhadap mesin. Probe koneksi aplikasi kini memanggil Connect_Net, GetSerialNumber, GetFirmwareVersion, dan Disconnect atas tindakan pengguna; belum dijalankan terhadap GU-06. Hasil operasi write/delete harus diverifikasi dengan aman sebelum fitur dirilis.
- TypeLib lokal menyatakan Connect_Net(BSTR, I4) -> VARIANT_BOOL; nomor seri dan firmware memakai parameter output BSTR. SSR_GetAllUserInfo tercatat, tetapi pembacaan pengguna belum diaktifkan. SSR_GetGeneralLogData digunakan untuk unduh record, tetapi belum diuji pada GU-06. Adapter memakai nomor mesin SDK 1 berdasarkan konvensi penggunaan SDK.

Lokasi SDK yang ditemukan pada mesin pengembangan:

`E:\Software\ABSENSI SIDIK JARI\Software Attendance - Indonesia_2\Software Attendance - Indonesia\files\sdk\zkemkeeper.dll`

Lokasi dan versi SDK harus diperiksa ulang pada mesin build/instalasi. Jangan mengandalkan path lokal tersebut sebagai path runtime aplikasi.

### Data Att

- `att2000.mdb` tersedia di folder `Att`.
- Pemeriksaan skema dilakukan secara read-only; record karyawan, log, dan biometrik tidak dibaca untuk dokumen ini.
- Isi dan kualitas data harus diperiksa melalui alur impor yang aman sebelum menetapkan pemetaan final.

### Pemisahan tingkat kepastian

- **Keputusan pengguna:** semua butir di bagian “Keputusan yang sudah disepakati”.
- **Temuan lokal:** nama berkas, SDK/COM yang terdeteksi, daftar metode, dan struktur laporan Att yang diperiksa.
- **Rekomendasi:** teknologi, penyimpanan lokal, bentuk sinkronisasi dengan pratinjau, dan desain modul.
- **Belum diketahui:** hasil nyata perintah write/delete pada ICON GU-06; efek hapus pengguna pada log mesin; field pengguna yang dapat ditulis; format pasti data lama; serta skema laporan final.

## Operasi berisiko dan aturan keselamatan

- Tidak menjalankan operasi hapus mesin ketika pengguna menghapus karyawan di aplikasi.
- Tidak menjalankan `ClearGLog`.
- Hapus pengguna mesin tetap pending sampai pemeriksaan Att dan verifikasi perilaku perangkat selesai.
- Sebelum fitur sinkronisasi write atau penghapusan mesin dibuat, pratinjaukan target mesin, User ID, field terdampak, dan perubahan yang akan dikirim.
- Record yang diunduh dari mesin tidak boleh disamakan dengan koreksi lokal.
- Jangan mengubah atau mengganti `Att\att2000.mdb` selama proses impor.
- Jangan menjalankan `Register_SDK.bat` atau skrip uninstall lama tanpa analisis dependensi dan konsekuensi instalasi terlebih dahulu.

## Rencana kerja bertahap

1. **Spesifikasi:** finalisasi field karyawan, aturan perhitungan absensi, laporan yang dipakai, serta format template impor.
2. **Verifikasi kompatibilitas:** dokumentasikan API yang tersedia, kebutuhan x86/COM, dependensi SDK, dan perilaku perangkat untuk koneksi, pembacaan, serta penulisan yang disetujui.
3. **Rancangan data:** master karyawan, pemetaan per mesin, mesin, raw attendance log, shift/kalender, koreksi, dan hasil rekap.
4. **Rancangan alur impor:** Att MDB read-only dan template Excel, dengan pratinjau, validasi, dan laporan kesalahan.
5. **Rancangan sinkronisasi:** bandingkan data per mesin, tunjukkan konflik, lalu kirim perubahan eksplisit tanpa penghapusan otomatis.
6. **Desain layar:** Dashboard, Karyawan, Mesin & Sinkronisasi, Absensi, Aturan Kerja, Laporan, dan Pengaturan/Backup.
7. **Implementasi dan installer:** sedang berjalan. Fondasi aplikasi, probe koneksi, dan jalur unduh record telah dibuat; lanjutkan pembacaan pengguna, impor MDB, sinkronisasi terpratinjau, aturan kerja, laporan, lalu installer. Packaging SDK menunggu verifikasi lisensi, dependensi, dan pemasangan bersih Windows 10+.

## Hal yang masih terbuka

- Pemilik akan memeriksa di Att apakah pengguna dapat dihapus dari mesin dan apakah record absensinya ikut terpengaruh. Sampai ada hasilnya, penghapusan pengguna mesin tidak menjadi fitur aktif.
- Format pasti template (xlsx; kemungkinan CSV tambahan) dan apakah impor absensi dari spreadsheet juga diperlukan.
- Daftar kolom laporan yang benar-benar digunakan.
- Field karyawan Att yang dipertahankan dan relasinya dengan User ID perangkat.
- Ketersediaan Comm Key/password komunikasi pada setiap mesin dan aturan penyimpanannya.
- Perilaku sinkronisasi bila pengguna/field berbeda di aplikasi dan mesin; usulan awal adalah pratinjau dan keputusan eksplisit, tanpa delete otomatis.
- Backup/retensi dan pemindahan database lokal saat aplikasi dipasang ulang.

## Instruksi handoff untuk agent berikutnya

1. Baca dokumen ini terlebih dahulu, lalu periksa kondisi terbaru folder `Att` sebelum membuat asumsi.
2. Jangan menganggap folder atau instalasi Att boleh dimodifikasi; database sumber harus diperlakukan read-only saat impor.
3. Jangan mengimplementasikan penghapusan pengguna mesin sampai status pending dicabut oleh pengguna.
4. Jangan memakai `ClearGLog`.
5. Jangan menganggap sinkronisasi karyawan berarti sinkronisasi templat sidik jari.
6. Jika fakta SDK/mesin berbeda dari dokumen ini, catat sumber dan tingkat kepastian, lalu perbarui rencana sebelum mengubah lingkup.
7. Implementasi sudah dimulai atas arahan pengguna. Jaga seluruh dokumentasi proyek di docs/ dan perbarui status setiap tahap.


