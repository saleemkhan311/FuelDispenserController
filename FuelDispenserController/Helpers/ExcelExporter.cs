using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OfficeOpenXml;

namespace FuelDispenserController.Helpers;
public static class ExcelExporter
{
    public static void ExportAllTablesToExcel(string dbPath, string outputExcelPath)
    {
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
        connection.Open();

        // Get all table names
        using var getTables = connection.CreateCommand();
        getTables.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";

        using var reader = getTables.ExecuteReader();
        while (reader.Read())
        {
            string tableName = reader.GetString(0);

            var worksheet = package.Workbook.Worksheets.Add(tableName);

            // Read data manually into DataTable
            var tableData = new DataTable();

            using var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = $"SELECT * FROM [{tableName}]";
            using var dataReader = selectCmd.ExecuteReader();

            // Load schema (columns)
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                tableData.Columns.Add(dataReader.GetName(i), dataReader.GetFieldType(i));
            }

            // Load data
            while (dataReader.Read())
            {
                var row = tableData.NewRow();
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    row[i] = dataReader.IsDBNull(i) ? null : dataReader.GetValue(i);
                }
                tableData.Rows.Add(row);
            }

            // Write to worksheet
            for (int col = 0; col < tableData.Columns.Count; col++)
            {
                worksheet.Cells[1, col + 1].Value = tableData.Columns[col].ColumnName;
            }

            for (int row = 0; row < tableData.Rows.Count; row++)
            {
                for (int col = 0; col < tableData.Columns.Count; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = tableData.Rows[row][col];
                }
            }
        }

        // Save the Excel file
        File.WriteAllBytes(outputExcelPath, package.GetAsByteArray());
    }
}
