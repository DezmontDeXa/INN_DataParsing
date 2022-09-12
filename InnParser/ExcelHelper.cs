using System.Collections.Generic;
using OfficeOpenXml;
using System.Linq;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Drawing;

namespace InnParser
{
    public class ExcelHelper
    {
        public List<string> GetINNsFromFile(string fileName)
        {
            ExcelPackage pack = new ExcelPackage(fileName);
            var book = pack.Workbook;
            var sheet = book.Worksheets[0];
            var content = sheet.Cells[$"A2:A{sheet.Dimension.Rows}"].ToText();
            var result = content.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();
            return result.ToList();
        }

        public void SaveResults(ConcurrentBag<InnSource> results, string resultFile)
        {
            if (File.Exists(resultFile))
                File.Delete(resultFile);

            ExcelPackage pack = new ExcelPackage(resultFile);
            var sheet = pack.Workbook.Worksheets.Add("Results");
            DrawHeaders(sheet);

            int id = 1;
            foreach (var result in results)
            {
                id++;

                var fields = new List<string>() { result.Inn };
                if (result.Exception != null || result?.Data == null)
                {
                    fields.Add(result?.Exception?.ToString() ?? "NO_EXCEPTION_OR_INFO");
                    DrawLine(sheet, fields, id);
                    continue;
                }
                fields.AddRange(new List<string>()
                    {
                        result.Data.Messages.Count.ToString() ?? "",
                        String.Join("\r\n", result.Data.Messages),
                        result.Data.ActPublishDate ?? "",
                        result.Data.FIO?? "",
                        result.Data.Birthday?? "",
                        result.Data.Place?? "",
                        result.Data.SNILS?? "",
                        result.Data.Arbitr?? "",
                        result.Data.CorrepondentAddress?? "",
                        result.Data.SPOAY?? "",
                        result.Data.Court?? "",
                        result.Data.PrevFIO?? "",
                        result.Data.CaseNo?? "",
                        result.Data.ResolutionDate?? "",
                        result.Data.AttachedFiles?? "",
                        result.Data.Text?? ""
                    });
                DrawLine(sheet, fields, id);
            }

            pack.Save();
        }
              
        private static void SetCellColor(ExcelWorksheet sheet, string cellAddress, Color color)
        {
            sheet.Cells[cellAddress].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            sheet.Cells[cellAddress].Style.Fill.BackgroundColor.SetColor(color);
        }

        private void DrawHeaders(ExcelWorksheet sheet)
        {
            List<string> headers = new List<string>()
            {
                "ИНН",
                "Кол-во записей",
                "Типы сообщений",
                "Дата публикации",
                "ФИО должника",
                "Дата рождения",
                "Место жительства",
                "СНИЛС",
                "Арбитражный управляющий",
                "Адрес для корреспонденции",
                "СРО АУ",
                "Суд",
                "Ранее имевшиеся ФИО",
                "№ дела",
                "Дата решения",
                "Прикрепленные файлы",
                "Текст"
            };

            DrawLine(sheet, headers, 1);
        }

        private void DrawLine(ExcelWorksheet sheet, List<string> fields, int row)
        {
            var alfabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            for (var i = 0; i < fields.Count; i++)
                sheet.SetValue($"{alfabet[i]}{row}", fields[i]);
        }
    }
}
