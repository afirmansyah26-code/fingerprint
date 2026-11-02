# Arah desain aplikasi

## Pengguna dan pekerjaan

Aplikasi dipakai staf administrasi/SDM pada komputer Windows di lingkungan lokal. Pekerjaan utamanya adalah mengelola data karyawan, mengatur beberapa mesin fingerprint, menyinkronkan karyawan, mengunduh record absensi, lalu memeriksa dan mencetak laporan. Pengguna bekerja di layar desktop dan banyak membaca tabel.

## Identitas sementara

Nama kerja UI: Absensi. Nama produk final belum ditetapkan. Logo lama di Att/_logo.jpg menampilkan teks “solution”; jangan menampilkannya sebagai identitas produk baru tanpa keputusan pemilik. Ikon aplikasi sementara berupa garis sidik jari sederhana, bukan gambar bitmap vendor.

## Arah visual

- Tema terang untuk sesi administrasi yang panjang.
- Kanvas abu hangat yang sangat muda, panel putih, teks gelap, garis pemisah tipis.
- Merah dipakai terbatas sebagai aksen identitas/aksi penting; navigasi aktif menggunakan latar netral.
- Satu sidebar berlabel agar tujuan halaman selalu jelas. Konten menggunakan lebar penuh yang tersedia.
- Tabel mendahulukan identitas karyawan dan informasi yang menentukan tindakan. Tidak memakai zebra striping atau header berwarna.
- Tombol aksi destruktif berada terpisah dan menjelaskan akibatnya.
- Tidak ada angka contoh yang seolah-olah nyata, chart hiasan, gradient, glow, atau animasi tanpa fungsi.
- Semua tombol dan menu memiliki perilaku nyata atau keadaan nonaktif dengan alasan yang terlihat.
- Keadaan kosong menjelaskan mengapa belum ada data dan langkah berikutnya.
- Keyboard dapat dipakai untuk navigasi, tabel, dialog, dan konfirmasi.

## Bahasa

Label, konfirmasi, error, dan empty state menggunakan Bahasa Indonesia. Istilah teknis (misalnya User ID mesin dan Comm Key) diberi label yang jelas.

## Prinsip lintas halaman

- Konsisten pada satu skala spacing 4px dan satu set ukuran teks yang mudah dibaca.
- Data numerik rata kanan dan memakai digit tabular bila font mendukung.
- Status memakai kata dan warna yang kontras; warna saja tidak membawa arti.
- Daftar panjang mendukung pencarian/filter dan mempertahankan header tabel saat discroll.
- Penghapusan karyawan aplikasi menyebut bahwa riwayat lokal terkait dapat ikut terhapus dan data mesin tidak diubah.
- Operasi tulis mesin menampilkan mesin, User ID, dan perubahan yang akan dilakukan sebelum dikirim.
