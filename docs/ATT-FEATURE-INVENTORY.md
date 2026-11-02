# Inventaris fitur Att

**Status:** inventaris awal dari layar utama; halaman menu dan perilaku tombol belum diperiksa.
**Tanggal bukti:** 2 November 2026.

## Batas bukti

- Nama menu di bawah disalin dari screenshot layar utama Att yang diberikan pengguna.
- Screenshot menunjukkan status mesin **Koneksi Putus**. Tidak ada tombol koneksi atau operasi mesin yang ditekan selama inventaris ini.
- Pengguna menyatakan tidak ada data yang dikirim ke mesin sebelum tombol **Koneksikan** ditekan. Pernyataan ini dicatat sebagai keterangan pengguna, bukan hasil uji protokol.
- REA mengidentifikasi `Att.exe` sebagai PE32 x86 tanpa metadata .NET. Provider dekompilasi native belum tersedia pada sesi pemeriksaan ini, sehingga perilaku tombol tidak disimpulkan dari kode.
- Label yang terpotong ditandai dengan elipsis; kelanjutan teksnya belum dipastikan.
- Alamat LAN, nama mesin lokal, nomor seri, dan data absensi tidak disalin ke dokumentasi ini.

## Menu utama

- Data
- Presensi
- Cari / Cetak
- Pengaturan
- Pengaturan Mesin
- Bantuan

## Tombol toolbar

- Karyawan
- Data Presensi
- Laporan
- Mesin
- Hapus Mesin
- Koneksikan
- Putuskan
- Keluar

## Panel navigasi

### Pengaturan Data

- Impor Data Presensi
- Ekspor Data Presensi
- Backup Database
- Pengaturan Flash Disk

### Mesin

- Download Data Presensi
- Download Data Dan Sidik Jari K... (label terpotong)
- Upload Data Dan Sidik Jari Kary... (label terpotong)
- Manajemen Foto Presensi
- Pengaturan Mesin

### Pengaturan

- Pengaturan Departemen
- Administrator
- Karyawan
- Pengaturan Database

### Jadwal Karyawan

- Pengaturan Jam Kerja
- Pengaturan Shift
- Jadwal Karyawan
- Aturan Presensi

## Kolom yang terlihat pada halaman mesin

Nama Mesin, Status, No Mesin, Tipe Komunikasi, Baud Rate, Alamat IP, Port, Nama Produk, beberapa kolom jumlah yang judulnya terpotong, Jumlah Data, dan Nomor Seri.

Pada area bawah terlihat tabel dengan kolom No. Urut, No. ID, Nama, Tgl & Jam, Mesin, Verifikasi, dan `checktype`; tabel sebelahnya menampilkan No. ID, Status, dan Waktu. Arti serta pemicu pembaruan tabel ini belum diverifikasi.

## Belum diverifikasi

- Isi setiap halaman, formulir, filter, dan laporan.
- Fungsi tepat tombol impor/ekspor, backup, flash disk, download, upload, dan hapus mesin.
- Apakah unduh data dan sidik jari merupakan satu operasi atau dua jenis data terpisah.
- Apakah **Hapus Mesin** hanya menghapus konfigurasi lokal atau turut mengubah mesin.
- Menu **Administrator** terlihat pada aplikasi lama; kebutuhan tersebut tidak otomatis masuk ke lingkup aplikasi pengganti.

## Cara melanjutkan pemeriksaan

Gunakan screenshot setiap halaman atau sesi UI yang dapat ditargetkan. Buka menu navigasi biasa terlebih dahulu. Jangan menekan **Koneksikan**, **Download**, **Upload**, **Putuskan**, atau **Hapus Mesin** sebelum efek tombol terkait dipastikan dan disetujui untuk diuji.
