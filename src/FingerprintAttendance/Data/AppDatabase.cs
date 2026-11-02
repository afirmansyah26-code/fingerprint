using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using FingerprintAttendance.Domain;
using FingerprintAttendance.Device;

namespace FingerprintAttendance.Data
{
    public sealed class AppDatabase
    {
        private readonly string _databasePath;

        public AppDatabase()
        {
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDirectory = Path.Combine(local, "FingerprintAttendance");
            Directory.CreateDirectory(appDirectory);
            _databasePath = Path.Combine(appDirectory, "attendance.sqlite3");
        }

        public string DatabasePath
        {
            get { return _databasePath; }
        }

        public void Initialize()
        {
            using (NativeSqlite db = Open())
            {
                db.Execute("CREATE TABLE IF NOT EXISTS employees (" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "employee_code TEXT NOT NULL COLLATE NOCASE UNIQUE," +
                    "full_name TEXT NOT NULL," +
                    "department TEXT NULL," +
                    "position TEXT NULL," +
                    "is_active INTEGER NOT NULL DEFAULT 1 CHECK(is_active IN (0,1))," +
                    "created_at TEXT NOT NULL," +
                    "updated_at TEXT NOT NULL)");

                db.Execute("CREATE TABLE IF NOT EXISTS machines (" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "machine_name TEXT NOT NULL," +
                    "host TEXT NOT NULL COLLATE NOCASE," +
                    "port INTEGER NOT NULL CHECK(port BETWEEN 1 AND 65535)," +
                    "serial_number TEXT NULL," +
                    "is_enabled INTEGER NOT NULL DEFAULT 1 CHECK(is_enabled IN (0,1))," +
                    "created_at TEXT NOT NULL," +
                    "updated_at TEXT NOT NULL," +
                    "UNIQUE(host, port))");

                db.Execute("CREATE TABLE IF NOT EXISTS device_users (" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "machine_id INTEGER NOT NULL REFERENCES machines(id) ON DELETE CASCADE," +
                    "employee_id INTEGER NOT NULL REFERENCES employees(id) ON DELETE CASCADE," +
                    "device_user_id TEXT NOT NULL," +
                    "device_name TEXT NULL," +
                    "fingerprint_count INTEGER NULL," +
                    "UNIQUE(machine_id, device_user_id))");

                db.Execute("CREATE TABLE IF NOT EXISTS attendance_logs (" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "machine_id INTEGER NULL REFERENCES machines(id) ON DELETE SET NULL," +
                    "machine_name_snapshot TEXT NOT NULL," +
                    "machine_serial_snapshot TEXT NULL," +
                    "employee_id INTEGER NULL REFERENCES employees(id) ON DELETE CASCADE," +
                    "device_user_id TEXT NOT NULL," +
                    "recorded_at TEXT NOT NULL," +
                    "verify_type TEXT NULL," +
                    "downloaded_at TEXT NOT NULL," +
                    "in_out_mode INTEGER NOT NULL DEFAULT 0," +
                    "work_code INTEGER NOT NULL DEFAULT 0," +
                    "UNIQUE(machine_id, device_user_id, recorded_at, verify_type))");

                EnsureAttendanceColumn(db, "in_out_mode", "INTEGER NOT NULL DEFAULT 0");
                EnsureAttendanceColumn(db, "work_code", "INTEGER NOT NULL DEFAULT 0");

                db.Execute("CREATE TABLE IF NOT EXISTS attendance_adjustments (" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "employee_id INTEGER NOT NULL REFERENCES employees(id) ON DELETE CASCADE," +
                    "work_date TEXT NOT NULL," +
                    "adjustment_type TEXT NOT NULL," +
                    "original_value TEXT NULL," +
                    "adjusted_value TEXT NULL," +
                    "reason TEXT NOT NULL," +
                    "created_at TEXT NOT NULL)");

                db.Execute("CREATE TABLE IF NOT EXISTS shifts (" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "shift_name TEXT NOT NULL UNIQUE COLLATE NOCASE," +
                    "start_time TEXT NOT NULL," +
                    "end_time TEXT NOT NULL," +
                    "late_tolerance_minutes INTEGER NOT NULL DEFAULT 0," +
                    "crosses_midnight INTEGER NOT NULL DEFAULT 0 CHECK(crosses_midnight IN (0,1)))");

                db.Execute("CREATE INDEX IF NOT EXISTS idx_employees_name ON employees(full_name COLLATE NOCASE)");
                db.Execute("CREATE INDEX IF NOT EXISTS idx_attendance_work_time ON attendance_logs(recorded_at)");
                db.Execute("CREATE INDEX IF NOT EXISTS idx_attendance_employee_time ON attendance_logs(employee_id, recorded_at)");
            }
        }

        private static void EnsureAttendanceColumn(NativeSqlite db, string columnName, string declaration)
        {
            List<Dictionary<string, object>> columns = db.Query("PRAGMA table_info(attendance_logs)");
            foreach (Dictionary<string, object> column in columns)
            {
                if (String.Equals(
                    Convert.ToString(column["name"], CultureInfo.InvariantCulture),
                    columnName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            db.Execute("ALTER TABLE attendance_logs ADD COLUMN " + columnName + " " + declaration);
        }
        public List<Employee> GetEmployees(string search)
        {
            string term = String.IsNullOrWhiteSpace(search) ? "" : "%" + search.Trim() + "%";
            List<Dictionary<string, object>> rows;
            using (NativeSqlite db = Open())
            {
                rows = db.Query(
                    "SELECT id, employee_code, full_name, department, position, is_active " +
                    "FROM employees " +
                    "WHERE (?1 = '' OR employee_code LIKE ?1 OR full_name LIKE ?1 OR department LIKE ?1) " +
                    "ORDER BY full_name COLLATE NOCASE, employee_code",
                    term);
            }

            List<Employee> employees = new List<Employee>();
            foreach (Dictionary<string, object> row in rows)
            {
                employees.Add(new Employee
                {
                    Id = Convert.ToInt64(row["id"], CultureInfo.InvariantCulture),
                    Code = Convert.ToString(row["employee_code"], CultureInfo.InvariantCulture),
                    FullName = Convert.ToString(row["full_name"], CultureInfo.InvariantCulture),
                    Department = Convert.ToString(row["department"], CultureInfo.InvariantCulture),
                    Position = Convert.ToString(row["position"], CultureInfo.InvariantCulture),
                    IsActive = Convert.ToInt64(row["is_active"], CultureInfo.InvariantCulture) == 1
                });
            }

            return employees;
        }

        public HashSet<string> GetEmployeeCodes()
        {
            List<Dictionary<string, object>> rows;
            using (NativeSqlite db = Open())
            {
                rows = db.Query("SELECT employee_code FROM employees");
            }

            HashSet<string> codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Dictionary<string, object> row in rows)
            {
                codes.Add(Convert.ToString(row["employee_code"], CultureInfo.InvariantCulture));
            }

            return codes;
        }

        public long AddEmployee(Employee employee)
        {
            using (NativeSqlite db = Open())
            {
                string now = UtcNow();
                db.Execute(
                    "INSERT INTO employees(employee_code, full_name, department, position, is_active, created_at, updated_at) " +
                    "VALUES(?1, ?2, ?3, ?4, ?5, ?6, ?6)",
                    Clean(employee.Code),
                    Clean(employee.FullName),
                    NullIfBlank(employee.Department),
                    NullIfBlank(employee.Position),
                    employee.IsActive ? 1 : 0,
                    now);
                return db.ScalarInt64("SELECT last_insert_rowid()");
            }
        }

        public void UpdateEmployee(Employee employee)
        {
            using (NativeSqlite db = Open())
            {
                db.Execute(
                    "UPDATE employees SET employee_code=?1, full_name=?2, department=?3, position=?4, is_active=?5, updated_at=?6 WHERE id=?7",
                    Clean(employee.Code),
                    Clean(employee.FullName),
                    NullIfBlank(employee.Department),
                    NullIfBlank(employee.Position),
                    employee.IsActive ? 1 : 0,
                    UtcNow(),
                    employee.Id);
            }
        }

        public void SetEmployeeActive(long id, bool isActive)
        {
            using (NativeSqlite db = Open())
            {
                db.Execute(
                    "UPDATE employees SET is_active=?1, updated_at=?2 WHERE id=?3",
                    isActive ? 1 : 0,
                    UtcNow(),
                    id);
            }
        }

        public void DeleteEmployee(long id)
        {
            using (NativeSqlite db = Open())
            {
                db.Execute("DELETE FROM employees WHERE id=?1", id);
            }
        }

        public void ImportEmployees(IList<Employee> employees)
        {
            using (NativeSqlite db = Open())
            {
                db.Execute("BEGIN IMMEDIATE");
                try
                {
                    string now = UtcNow();
                    foreach (Employee employee in employees)
                    {
                        db.Execute(
                            "INSERT INTO employees(employee_code, full_name, department, position, is_active, created_at, updated_at) " +
                            "VALUES(?1, ?2, ?3, ?4, ?5, ?6, ?6)",
                            Clean(employee.Code),
                            Clean(employee.FullName),
                            NullIfBlank(employee.Department),
                            NullIfBlank(employee.Position),
                            employee.IsActive ? 1 : 0,
                            now);
                    }
                    db.Execute("COMMIT");
                }
                catch
                {
                    try { db.Execute("ROLLBACK"); } catch { }
                    throw;
                }
            }
        }

        public List<Machine> GetMachines()
        {
            List<Dictionary<string, object>> rows;
            using (NativeSqlite db = Open())
            {
                rows = db.Query(
                    "SELECT id, machine_name, host, port, serial_number, is_enabled " +
                    "FROM machines ORDER BY machine_name COLLATE NOCASE");
            }

            List<Machine> machines = new List<Machine>();
            foreach (Dictionary<string, object> row in rows)
            {
                machines.Add(new Machine
                {
                    Id = Convert.ToInt64(row["id"], CultureInfo.InvariantCulture),
                    Name = Convert.ToString(row["machine_name"], CultureInfo.InvariantCulture),
                    Host = Convert.ToString(row["host"], CultureInfo.InvariantCulture),
                    Port = Convert.ToInt32(row["port"], CultureInfo.InvariantCulture),
                    SerialNumber = Convert.ToString(row["serial_number"], CultureInfo.InvariantCulture),
                    IsEnabled = Convert.ToInt64(row["is_enabled"], CultureInfo.InvariantCulture) == 1
                });
            }

            return machines;
        }

        public long AddMachine(Machine machine)
        {
            using (NativeSqlite db = Open())
            {
                string now = UtcNow();
                db.Execute(
                    "INSERT INTO machines(machine_name, host, port, serial_number, is_enabled, created_at, updated_at) " +
                    "VALUES(?1, ?2, ?3, ?4, ?5, ?6, ?6)",
                    Clean(machine.Name),
                    Clean(machine.Host),
                    machine.Port,
                    NullIfBlank(machine.SerialNumber),
                    machine.IsEnabled ? 1 : 0,
                    now);
                return db.ScalarInt64("SELECT last_insert_rowid()");
            }
        }

        public void UpdateMachine(Machine machine)
        {
            using (NativeSqlite db = Open())
            {
                db.Execute(
                    "UPDATE machines SET machine_name=?1, host=?2, port=?3, serial_number=?4, is_enabled=?5, updated_at=?6 WHERE id=?7",
                    Clean(machine.Name),
                    Clean(machine.Host),
                    machine.Port,
                    NullIfBlank(machine.SerialNumber),
                    machine.IsEnabled ? 1 : 0,
                    UtcNow(),
                    machine.Id);
            }
        }

        public void UpdateMachineSerialNumber(long id, string serialNumber)
        {
            using (NativeSqlite db = Open())
            {
                db.Execute(
                    "UPDATE machines SET serial_number=?1, updated_at=?2 WHERE id=?3",
                    NullIfBlank(serialNumber),
                    UtcNow(),
                    id);
            }
        }
        public void DeleteMachine(long id)
        {
            using (NativeSqlite db = Open())
            {
                db.Execute("DELETE FROM machines WHERE id=?1", id);
            }
        }

        internal int SaveAttendanceRecords(Machine machine, IList<DeviceAttendanceRecord> records)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            if (records == null)
            {
                throw new ArgumentNullException("records");
            }

            using (NativeSqlite db = Open())
            {
                db.Execute("BEGIN IMMEDIATE");
                try
                {
                    int inserted = 0;
                    string downloadedAt = UtcNow();
                    foreach (DeviceAttendanceRecord record in records)
                    {
                        inserted += db.Execute(
                            "INSERT OR IGNORE INTO attendance_logs(" +
                            "machine_id, machine_name_snapshot, machine_serial_snapshot, employee_id, " +
                            "device_user_id, recorded_at, verify_type, downloaded_at, in_out_mode, work_code) " +
                            "VALUES(?1, ?2, ?3, NULL, ?4, ?5, ?6, ?7, ?8, ?9)",
                            machine.Id,
                            Clean(machine.Name),
                            NullIfBlank(machine.SerialNumber),
                            Clean(record.UserId),
                            record.RecordedAt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                            record.VerifyMode.ToString(CultureInfo.InvariantCulture),
                            downloadedAt,
                            record.InOutMode,
                            record.WorkCode);
                    }
                    db.Execute("COMMIT");
                    return inserted;
                }
                catch
                {
                    try { db.Execute("ROLLBACK"); } catch { }
                    throw;
                }
            }
        }

        public List<AttendanceRecord> GetAttendanceRecords(int limit)
        {
            int safeLimit = Math.Max(1, Math.Min(limit, 10000));
            List<Dictionary<string, object>> rows;
            using (NativeSqlite db = Open())
            {
                rows = db.Query(
                    "SELECT machine_name_snapshot, device_user_id, recorded_at, verify_type, in_out_mode, work_code " +
                    "FROM attendance_logs ORDER BY recorded_at DESC LIMIT ?1",
                    safeLimit);
            }

            List<AttendanceRecord> records = new List<AttendanceRecord>();
            foreach (Dictionary<string, object> row in rows)
            {
                int verifyMode;
                Int32.TryParse(Convert.ToString(row["verify_type"], CultureInfo.InvariantCulture),
                    NumberStyles.Integer, CultureInfo.InvariantCulture, out verifyMode);
                records.Add(new AttendanceRecord
                {
                    MachineName = Convert.ToString(row["machine_name_snapshot"], CultureInfo.InvariantCulture),
                    DeviceUserId = Convert.ToString(row["device_user_id"], CultureInfo.InvariantCulture),
                    RecordedAt = DateTime.Parse(
                        Convert.ToString(row["recorded_at"], CultureInfo.InvariantCulture),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None),
                    VerifyMode = verifyMode,
                    InOutMode = Convert.ToInt32(row["in_out_mode"], CultureInfo.InvariantCulture),
                    WorkCode = Convert.ToInt32(row["work_code"], CultureInfo.InvariantCulture)
                });
            }
            return records;
        }
        public long CountEmployees(bool activeOnly)
        {
            using (NativeSqlite db = Open())
            {
                return db.ScalarInt64(activeOnly
                    ? "SELECT COUNT(*) FROM employees WHERE is_active=1"
                    : "SELECT COUNT(*) FROM employees");
            }
        }

        public long CountMachines()
        {
            using (NativeSqlite db = Open())
            {
                return db.ScalarInt64("SELECT COUNT(*) FROM machines");
            }
        }

        public long CountAttendanceRecords()
        {
            using (NativeSqlite db = Open())
            {
                return db.ScalarInt64("SELECT COUNT(*) FROM attendance_logs");
            }
        }

        private NativeSqlite Open()
        {
            return new NativeSqlite(_databasePath);
        }

        private static string UtcNow()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        }

        private static string Clean(string value)
        {
            return (value ?? String.Empty).Trim();
        }

        private static object NullIfBlank(string value)
        {
            return String.IsNullOrWhiteSpace(value) ? null : (object)value.Trim();
        }
    }

    internal sealed class NativeSqlite : IDisposable
    {
        private const string Library = "winsqlite3.dll";
        private const int Ok = 0;
        private const int Row = 100;
        private const int Done = 101;
        private const int Integer = 1;
        private const int Float = 2;
        private const int Text = 3;
        private const int Blob = 4;
        private const int Null = 5;
        private static readonly IntPtr Transient = new IntPtr(-1);
        private IntPtr _handle;

        public NativeSqlite(string path)
        {
            IntPtr handle;
            int result = Open16(path, out handle);
            _handle = handle;

            if (result != Ok)
            {
                string message = ErrorMessage();
                if (_handle != IntPtr.Zero)
                {
                    CloseV2(_handle);
                    _handle = IntPtr.Zero;
                }
                throw new InvalidOperationException("SQLite gagal membuka database: " + message);
            }

            BusyTimeout(_handle, 5000);
            Execute("PRAGMA foreign_keys = ON");
        }

        public int Execute(string sql, params object[] parameters)
        {
            IntPtr statement = Prepare(sql, parameters);
            try
            {
                int result;
                do
                {
                    result = Step(statement);
                }
                while (result == Row);

                if (result != Done)
                {
                    throw new InvalidOperationException("SQLite gagal menjalankan perintah: " + ErrorMessage());
                }

                return Changes(_handle);
            }
            finally
            {
                Finalize(statement);
            }
        }

        public long ScalarInt64(string sql, params object[] parameters)
        {
            List<Dictionary<string, object>> rows = Query(sql, parameters);
            if (rows.Count == 0 || rows[0].Count == 0)
            {
                return 0;
            }
            foreach (object value in rows[0].Values)
            {
                return value == null ? 0 : Convert.ToInt64(value, CultureInfo.InvariantCulture);
            }
            return 0;
        }

        public List<Dictionary<string, object>> Query(string sql, params object[] parameters)
        {
            IntPtr statement = Prepare(sql, parameters);
            try
            {
                int columnCount = ColumnCount(statement);
                string[] names = new string[columnCount];
                int i;
                for (i = 0; i < columnCount; i++)
                {
                    names[i] = PtrToString(ColumnName16(statement, i));
                }

                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                while (true)
                {
                    int result = Step(statement);
                    if (result == Done)
                    {
                        break;
                    }
                    if (result != Row)
                    {
                        throw new InvalidOperationException("SQLite gagal membaca data: " + ErrorMessage());
                    }

                    Dictionary<string, object> row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    for (i = 0; i < columnCount; i++)
                    {
                        int type = ColumnType(statement, i);
                        object value;
                        if (type == Integer)
                        {
                            value = ColumnInt64(statement, i);
                        }
                        else if (type == Float)
                        {
                            value = ColumnDouble(statement, i);
                        }
                        else if (type == Text)
                        {
                            value = PtrToString(ColumnText16(statement, i));
                        }
                        else if (type == Blob)
                        {
                            value = null;
                        }
                        else
                        {
                            value = null;
                        }
                        row[names[i]] = value;
                    }
                    rows.Add(row);
                }

                return rows;
            }
            finally
            {
                Finalize(statement);
            }
        }

        private IntPtr Prepare(string sql, object[] parameters)
        {
            IntPtr statement;
            int result = Prepare16V2(_handle, sql, -1, out statement, IntPtr.Zero);
            if (result != Ok)
            {
                throw new InvalidOperationException("SQLite gagal menyiapkan perintah: " + ErrorMessage());
            }

            int i;
            for (i = 0; i < parameters.Length; i++)
            {
                Bind(statement, i + 1, parameters[i]);
            }
            return statement;
        }

        private void Bind(IntPtr statement, int index, object value)
        {
            int result;
            if (value == null || value == DBNull.Value)
            {
                result = BindNull(statement, index);
            }
            else if (value is bool)
            {
                result = BindInt64(statement, index, (bool)value ? 1 : 0);
            }
            else if (value is byte || value is short || value is int || value is long)
            {
                result = BindInt64(statement, index, Convert.ToInt64(value, CultureInfo.InvariantCulture));
            }
            else if (value is float || value is double || value is decimal)
            {
                result = BindDouble(statement, index, Convert.ToDouble(value, CultureInfo.InvariantCulture));
            }
            else
            {
                string text = value is DateTime
                    ? ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture)
                    : Convert.ToString(value, CultureInfo.InvariantCulture);
                result = BindText16(statement, index, text ?? String.Empty, -1, Transient);
            }

            if (result != Ok)
            {
                throw new InvalidOperationException("SQLite gagal mengikat nilai: " + ErrorMessage());
            }
        }

        private string ErrorMessage()
        {
            if (_handle == IntPtr.Zero)
            {
                return "database tidak tersedia.";
            }
            return PtrToString(ErrorMessage16(_handle));
        }

        private static string PtrToString(IntPtr value)
        {
            return value == IntPtr.Zero ? String.Empty : Marshal.PtrToStringUni(value);
        }

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                CloseV2(_handle);
                _handle = IntPtr.Zero;
            }
        }

        [DllImport(Library, EntryPoint = "sqlite3_open16", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int Open16(string filename, out IntPtr db);

        [DllImport(Library, EntryPoint = "sqlite3_close_v2", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CloseV2(IntPtr db);

        [DllImport(Library, EntryPoint = "sqlite3_busy_timeout", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BusyTimeout(IntPtr db, int milliseconds);

        [DllImport(Library, EntryPoint = "sqlite3_prepare16_v2", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int Prepare16V2(IntPtr db, string sql, int bytes, out IntPtr statement, IntPtr tail);

        [DllImport(Library, EntryPoint = "sqlite3_step", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Step(IntPtr statement);

        [DllImport(Library, EntryPoint = "sqlite3_finalize", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Finalize(IntPtr statement);

        [DllImport(Library, EntryPoint = "sqlite3_bind_text16", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int BindText16(IntPtr statement, int index, [MarshalAs(UnmanagedType.LPWStr)] string value, int bytes, IntPtr destructor);

        [DllImport(Library, EntryPoint = "sqlite3_bind_int64", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BindInt64(IntPtr statement, int index, long value);

        [DllImport(Library, EntryPoint = "sqlite3_bind_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BindDouble(IntPtr statement, int index, double value);

        [DllImport(Library, EntryPoint = "sqlite3_bind_null", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BindNull(IntPtr statement, int index);

        [DllImport(Library, EntryPoint = "sqlite3_column_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ColumnCount(IntPtr statement);

        [DllImport(Library, EntryPoint = "sqlite3_column_name16", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ColumnName16(IntPtr statement, int column);

        [DllImport(Library, EntryPoint = "sqlite3_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ColumnType(IntPtr statement, int column);

        [DllImport(Library, EntryPoint = "sqlite3_column_int64", CallingConvention = CallingConvention.Cdecl)]
        private static extern long ColumnInt64(IntPtr statement, int column);

        [DllImport(Library, EntryPoint = "sqlite3_column_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double ColumnDouble(IntPtr statement, int column);

        [DllImport(Library, EntryPoint = "sqlite3_column_text16", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ColumnText16(IntPtr statement, int column);

        [DllImport(Library, EntryPoint = "sqlite3_errmsg16", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ErrorMessage16(IntPtr db);

        [DllImport(Library, EntryPoint = "sqlite3_changes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Changes(IntPtr db);


    }
}

