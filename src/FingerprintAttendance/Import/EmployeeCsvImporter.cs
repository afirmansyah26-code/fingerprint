using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using FingerprintAttendance.Domain;

namespace FingerprintAttendance.Import
{
    public sealed class EmployeeImportRow
    {
        public int RowNumber { get; set; }
        public Employee Employee { get; set; }
        public string Error { get; set; }
    }

    public sealed class EmployeeImportResult
    {
        public List<EmployeeImportRow> Rows { get; private set; }
        public List<string> FileErrors { get; private set; }

        public EmployeeImportResult()
        {
            Rows = new List<EmployeeImportRow>();
            FileErrors = new List<string>();
        }

        public int ValidCount
        {
            get
            {
                int count = 0;
                foreach (EmployeeImportRow row in Rows)
                {
                    if (String.IsNullOrEmpty(row.Error))
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public int ErrorCount
        {
            get
            {
                int count = 0;
                foreach (EmployeeImportRow row in Rows)
                {
                    if (!String.IsNullOrEmpty(row.Error))
                    {
                        count++;
                    }
                }
                return count + FileErrors.Count;
            }
        }
    }

    public static class EmployeeCsvImporter
    {
        public static EmployeeImportResult Read(string path, ISet<string> existingEmployeeCodes)
        {
            EmployeeImportResult result = new EmployeeImportResult();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using (TextFieldParser parser = new TextFieldParser(path, Encoding.UTF8))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(";");
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.TrimWhiteSpace = false;

                    if (parser.EndOfData)
                    {
                        result.FileErrors.Add("File CSV kosong.");
                        return result;
                    }

                    string[] headers = parser.ReadFields();
                    Dictionary<string, int> columns = MapHeaders(headers);
                    if (!columns.ContainsKey("employee_code") || !columns.ContainsKey("full_name"))
                    {
                        result.FileErrors.Add("Header wajib: employee_code dan full_name. Unduh template dari halaman Impor.");
                        return result;
                    }

                    int rowNumber = 1;
                    while (!parser.EndOfData)
                    {
                        rowNumber++;
                        string[] fields;
                        try
                        {
                            fields = parser.ReadFields();
                        }
                        catch (MalformedLineException ex)
                        {
                            result.Rows.Add(new EmployeeImportRow
                            {
                                RowNumber = rowNumber,
                                Error = "Format CSV tidak valid: " + ex.Message
                            });
                            continue;
                        }

                        if (fields == null || IsBlank(fields))
                        {
                            continue;
                        }

                        Employee employee = new Employee
                        {
                            Code = Field(fields, columns, "employee_code").Trim(),
                            FullName = Field(fields, columns, "full_name").Trim(),
                            Department = Field(fields, columns, "department").Trim(),
                            Position = Field(fields, columns, "position").Trim(),
                            IsActive = true
                        };

                        List<string> errors = new List<string>();
                        if (String.IsNullOrWhiteSpace(employee.Code))
                        {
                            errors.Add("employee_code wajib diisi");
                        }
                        if (String.IsNullOrWhiteSpace(employee.FullName))
                        {
                            errors.Add("full_name wajib diisi");
                        }
                        if (!String.IsNullOrWhiteSpace(employee.Code))
                        {
                            if (!seen.Add(employee.Code))
                            {
                                errors.Add("employee_code duplikat di file");
                            }
                            if (existingEmployeeCodes != null && existingEmployeeCodes.Contains(employee.Code))
                            {
                                errors.Add("employee_code sudah ada di aplikasi");
                            }
                        }

                        string active = Field(fields, columns, "is_active").Trim();
                        if (!String.IsNullOrEmpty(active))
                        {
                            if (IsTrue(active))
                            {
                                employee.IsActive = true;
                            }
                            else if (IsFalse(active))
                            {
                                employee.IsActive = false;
                            }
                            else
                            {
                                errors.Add("is_active harus bernilai 1/0, true/false, aktif/nonaktif");
                            }
                        }

                        result.Rows.Add(new EmployeeImportRow
                        {
                            RowNumber = rowNumber,
                            Employee = employee,
                            Error = errors.Count == 0 ? String.Empty : String.Join("; ", errors.ToArray())
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                result.FileErrors.Add("File tidak dapat dibaca: " + ex.Message);
            }

            if (result.Rows.Count == 0 && result.FileErrors.Count == 0)
            {
                result.FileErrors.Add("Tidak ada baris karyawan untuk diimpor.");
            }

            return result;
        }

        public static void WriteTemplate(string path)
        {
            string template = "employee_code;full_name;department;position;is_active\r\n";
            File.WriteAllText(path, template, new UTF8Encoding(true));
        }

        private static Dictionary<string, int> MapHeaders(string[] headers)
        {
            Dictionary<string, int> columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (headers == null)
            {
                return columns;
            }

            int i;
            for (i = 0; i < headers.Length; i++)
            {
                string header = (headers[i] ?? String.Empty).Trim().TrimStart('\uFEFF').ToLowerInvariant();
                if (!columns.ContainsKey(header))
                {
                    columns.Add(header, i);
                }
            }
            return columns;
        }

        private static string Field(string[] fields, Dictionary<string, int> columns, string name)
        {
            int index;
            if (fields == null || !columns.TryGetValue(name, out index) || index >= fields.Length)
            {
                return String.Empty;
            }
            return fields[index] ?? String.Empty;
        }

        private static bool IsBlank(string[] fields)
        {
            foreach (string field in fields)
            {
                if (!String.IsNullOrWhiteSpace(field))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsTrue(string value)
        {
            string normalized = value.Trim().ToLowerInvariant();
            return normalized == "1" || normalized == "true" || normalized == "aktif" || normalized == "ya";
        }

        private static bool IsFalse(string value)
        {
            string normalized = value.Trim().ToLowerInvariant();
            return normalized == "0" || normalized == "false" || normalized == "nonaktif" || normalized == "tidak";
        }
    }
}
