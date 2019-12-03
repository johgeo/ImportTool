using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ImportAndValidationTool.Validation;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ImportAndValidationTool.Helpers
{
    public static class ExcelHelper
    {
        public static List<string> GetLinesFromExcel(string pathToFile)
        {
            using (var streamReader = new StreamReader(pathToFile))
            using (var excel = new ExcelPackage(streamReader.BaseStream))
            {
                const int startingIndex = 1;
                var firstWorkSheet = excel.Workbook.Worksheets.First();
                var lastColumnCount = firstWorkSheet.Dimension.Columns + startingIndex;
                var allLines = new List<string>();
                for (var row = 2; row < firstWorkSheet.Dimension.Rows; row++)
                {
                    var sb = new StringBuilder();
                    for (var column = startingIndex; column < lastColumnCount; column++)
                    {
                        var value = firstWorkSheet.GetValue(row, column);
                        if (column != lastColumnCount)
                            sb.Append(value).Append("|");
                    }
                    allLines.Add(sb.ToString());
                }

                Console.WriteLine($"{allLines.Count} lines have been identified.");
                return allLines;
            }
        }

        public static void SaveToExcel(IList<ValidationError> errorList, string title)
        {
            var errorListPartitions = PartitionByTypes(errorList).ToList();
            foreach (var partitionedErrorList in errorListPartitions)
            {
                if (!partitionedErrorList.Any())
                    continue;
                var folderPath = string.Empty;
                if (partitionedErrorList.OfType<RowError>().Any())
                    folderPath = CreateFolderPath("row_validation_error");
                else if (partitionedErrorList.OfType<GlobalError>().Any())
                    folderPath = CreateFolderPath("file_validation_error");

                using (var excel = CreateExcel(partitionedErrorList.Cast<ValidationError>().ToList(), title))
                using (var fs = new FileStream(folderPath, FileMode.Create, FileAccess.Write))
                    excel.Stream.CopyTo(fs);

                Console.WriteLine($"Found {errorList.Count} validation errors");
                Console.WriteLine($"Saved validation errors to file {folderPath}");
            }
        }
        private static ExcelPackage CreateExcel<TValidationError>(IList<TValidationError> errorList, string workSheetTitle) where TValidationError : ValidationError
        {
            ExcelPackage excel = null;

            try
            {
                PropertyInfo[] typeProps = null;

                if (!errorList.Any())
                    return null;

                if (errorList.OfType<RowError>().Any())
                    typeProps = new RowError().GetType().GetProperties();
                else if (errorList.OfType<GlobalError>().Any())
                    typeProps = new GlobalError().GetType().GetProperties();

                if (typeProps is null)
                    throw new InvalidCastException(
                        $"Could not cast error list to either {typeof(RowError).Name} or {typeof(GlobalError).Name}");

                excel = new ExcelPackage();
                var ws = excel.Workbook.Worksheets.Add(workSheetTitle);

                foreach (var prop in typeProps)
                {
                    var cell = ws.Cells[1, (ws.Dimension?.Columns ?? 0) + 1];
                    cell.Value = prop.Name;
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    cell.Style.Font.Bold = true;
                }

                for (var i = 0; i < errorList.Count; i++)
                {
                    var row = i + 2;
                    var props = errorList[i].GetType().GetProperties();
                    for (var j = 0; j < props.Length; j++)
                    {
                        var column = j + 1;
                        var cell = ws.Cells[row, column];
                        cell.Value = props[j].GetValue(errorList[i]);
                        cell.Style.Border.Bottom.Style = ExcelBorderStyle.None;
                        cell.Style.Font.Bold = false;
                    }
                }

                ws.Cells.AutoFitColumns(0);
                for (var i = 1; i <= ws.Dimension?.Columns; i++)
                    ws.Column(i).Width += 2;

                excel.Save();
                excel.Stream.Position = 0;
            }
            catch (Exception)
            {
                excel?.Dispose();
                throw;
            }

            return excel;
        }

        private static string CreateFolderPath(string fileName)
        {
            var pathSeparated = Application.FilePath.Split('\\');
            var pathRemoved = pathSeparated.Take(pathSeparated.Length - 1).ToList();
            pathRemoved.Add($"{fileName}.xlsx");
            var folderPath = string.Join("\\", pathRemoved.ToArray());
            return folderPath;
        }

        private static Type GetObjectTypeOrNull(object o)
        {
            return o == null ? null : o.GetType();
        }

        private static IEnumerable<List<object>> PartitionByTypes(IEnumerable<object> values)
        {
            if (values == null) throw new ArgumentNullException("values");
            if (values.Count() == 0) yield break;

            var currentType = GetObjectTypeOrNull(values);
            var buffer = new List<object>();
            foreach (var value in values)
            {
                var valueType = GetObjectTypeOrNull(value);
                if (valueType != currentType)
                {
                    yield return buffer;
                    buffer = new List<object>();
                }

                currentType = valueType;
                buffer.Add(value);
            }

            if (buffer.Count > 0)
            {
                yield return buffer;
            }
        }
    }
}
