# Catatan perubahan

## 2026-11-02 — Fondasi aplikasi

### Implemented

- Membuat project C# WinForms x86 dan shell desktop dengan navigasi berbahasa Indonesia.
- Menambahkan database SQLite lokal dengan schema awal dan penyimpanan di LocalAppData.
- Menambahkan CRUD karyawan lokal, status aktif/nonaktif, dan konfirmasi hapus yang menyebut data lokal yang ikut terhapus.
- Menambahkan pendaftaran beberapa konfigurasi mesin di satu komputer. Perubahan konfigurasi tidak mengirim perintah ke mesin.
- Menambahkan template CSV UTF-8, validasi kode karyawan duplikat, pratinjau, dan impor transaksional.
- Menambahkan halaman status untuk absensi, aturan kerja, dan laporan yang belum tersedia.

### Catatan

- Build x86 berhasil dengan MSBuild lokal. Targeting pack tidak tersedia sehingga MSBuild mengeluarkan warning MSB3644 dan menggunakan GAC.
- Startup aplikasi berhasil membuka SQLite dan menampilkan angka nol dari database baru.
- Tinjauan visual menemukan panel ringkasan memakai ukuran default; source diperbaiki dan hasil perbaikannya berhasil dikompilasi di folder Review. Tinjauan visual ulang masih pending.
- Belum ada integrasi SDK, operasi mesin, impor MDB, aturan kerja, atau laporan.
- Penghapusan pengguna mesin tetap pending. ClearGLog tidak digunakan.

### Langkah berikutnya

- Lengkapi pemetaan data dan impor Att MDB secara read-only.
- Implementasikan adapter SDK x86 untuk membaca pengguna dan record, lalu koneksi sinkronisasi yang menampilkan pratinjau.
- Setelah itu tambahkan pengaturan aturan kerja, laporan, dan installer.

## 2026-11-02 — Probe koneksi SDK

### Implemented

- Menambahkan adapter x86 late-bound COM untuk zkemkeeper.ZKEM.1.
- Menambahkan tombol **Uji koneksi** pada halaman Mesin fingerprint untuk memeriksa koneksi dan membaca nomor seri/firmware. Operasi dijalankan di latar belakang; koneksi dilepas setelah pembacaan. Nomor seri yang dibaca tersimpan pada konfigurasi lokal; status koneksi sementara.
- Memeriksa metadata TypeLib lokal secara baca-saja dan mencatat signature penting di panduan implementasi.
- Memperbarui rencana dan panduan agar membedakan API yang terdaftar dari perilaku yang sudah diuji pada perangkat.

### Batasan

- Tidak ada koneksi yang dijalankan ke mesin dan tidak ada data mesin yang diubah pada tahap ini.
- Pembacaan daftar karyawan dan record belum diaktifkan; sinkronisasi write, impor MDB, aturan kerja, laporan, dan installer tetap belum tersedia.
- Komponen SDK terpasang di mesin pengembangan, tetapi hak redistribusi dan pemasangan bersih Windows belum dikonfirmasi.
- Build compile-only ditujukan ke folder review terpisah agar executable yang sedang terbuka tidak tertimpa. Build berhasil dengan warning MSB3644 karena targeting pack belum tersedia.

## 2026-11-02 — Unduh record absensi

### Implemented

- Menambahkan pembaca log SDK menggunakan ReadAllGLogData dan SSR_GetGeneralLogData.
- Menambahkan transaksi simpan lokal dengan pengabaian duplikat, snapshot sumber mesin, dan kolom mode absensi/kode kerja.
- Menambahkan migrasi aman untuk menambahkan kolom baru pada database SQLite lokal yang sudah ada.
- Menambahkan tombol Unduh record dan halaman daftar 1.000 record terbaru.
- Catatan mode verifikasi dan mode absensi tetap dalam bentuk angka SDK; interpretasi firmware belum diverifikasi.

### Batasan

- Tidak ada unduhan yang dijalankan pada mesin dalam sesi ini; hasil kompatibilitas dan jumlah record belum diketahui.
- Alur tidak memanggil ClearGLog dan tidak mengubah log pada mesin.
- Pemetaan user mesin ke master karyawan, filter, laporan, serta pembacaan daftar pengguna masih belum tersedia.
## 2026-11-02 — Setup repositori GitHub

- Menginisialisasi repository lokal pada branch main dan menghubungkan remote origin ke repositori pengguna.
- Menyiapkan .gitignore agar Att/, database/log lokal, binari SDK lama, dan output build tidak ikut terlacak.
- Source dan dokumentasi proyek disiapkan untuk commit awal.