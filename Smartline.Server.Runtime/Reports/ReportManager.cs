using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Ext.Net;
using Newtonsoft.Json;
using Smartline.Mapping;
using Smartline.Common.Runtime;
using System.IO;
using VerticalAlignmentValues = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues;

namespace Smartline.Server.Runtime.Reports {
    public class ReportManager {
        private static readonly Dictionary<ReportTypes, string> ReportTypesName = new Dictionary<ReportTypes, string>() {                                                                                                               
        {ReportTypes.Summary, "Сводный отчет"},
        {ReportTypes.Detail, "Детальный отчет"},
        {ReportTypes.Parking, "Отчет по стоянкам"}};

        public static string GetReportNameByType(ReportTypes reportType) {
            return ReportTypesName[reportType];
        }

        #region common report

        public static JsonObject GenerateSummaryReport(DateTime from, DateTime to, DateTime fromTimeSpan, DateTime toTimeSpan, IEnumerable<int> trackers) {
            var returnObject = new List<object>();
            Dictionary<int, List<ReportAdapted>> data = GetSummaryReportData(from, to, fromTimeSpan, toTimeSpan, trackers);
            List<Tracker> trackerObjects = CouchbaseManager.GetTrackers(trackers.ToArray());
            foreach (KeyValuePair<int, List<ReportAdapted>> tracker in data) {
                Tracker trackerObject = trackerObjects.FirstOrDefault(o => o.Id == tracker.Key);
                List<ReportAdapted> adaptedValues = tracker.Value;
                int summaryMoving = adaptedValues.Sum(o => o.Moving);
                int summaryParking = adaptedValues.Sum(o => o.Parking);
                int summaryDistance = adaptedValues.Sum(o => o.Distance ?? 0);

                returnObject.Add(new {
                    tracker = tracker.Key.ToString(CultureInfo.InvariantCulture),
                    Days = adaptedValues,
                    Summary = new { moving = summaryMoving.ToWordsWithDays(), parking = summaryParking.ToWordsWithDays(), distance = summaryDistance.ToDistanceInKilometersAndConsumption(trackerObject == null ? 0 : trackerObject.Consumption) }
                });
            }
            var fromDate = from.AddHours(fromTimeSpan.Hour).AddMinutes(fromTimeSpan.Minute);
            var toDate = to.AddHours(toTimeSpan.Hour).AddMinutes(toTimeSpan.Minute);
            var obj = new JsonObject { { "Data", returnObject }, 
            { "Title", "Сводный отчет за " + fromDate.ToString("dd-MM-yyyy HH:mm") + " - " + toDate.ToString("dd-MM-yyyy HH:mm") } ,
            { "Caption", GetReportNameByType(ReportTypes.Summary)  } ,
            {"Id", Guid.NewGuid()}
            };
            return obj;
        }

        public static Dictionary<int, List<ReportAdapted>> GetSummaryReportData(DateTime from, DateTime to, DateTime fromTimeSpan, DateTime toTimeSpan, IEnumerable<int> trackers) {
            var data = new Dictionary<int, List<ReportAdapted>>();
            List<Tracker> trackerObjects = CouchbaseManager.GetTrackers(trackers.ToArray());
            foreach (int tracker in trackers) {
                Tracker trackerObject = trackerObjects.FirstOrDefault(o => o.Id == tracker);
                List<ReportFull> result = LoadHistories(tracker, from, to, LoadingReportType.SkipDetailes);
                if (result != null && result.Count > 0) {
                    if (from.Date == to.Date) {
                        ReportFull firstDay = result.First();
                        IEnumerable<ReportCommon> commons = firstDay.Commons.Where(o => o.Hour >= fromTimeSpan.Hour && o.Hour <= toTimeSpan.Hour);
                        UpdateReportCommon(firstDay, commons);
                    } else {
                        if (fromTimeSpan.Hour > 0) {
                            ReportFull firstDay = result.First();
                            IEnumerable<ReportCommon> commons = firstDay.Commons.Where(o => o.Hour >= fromTimeSpan.Hour);
                            UpdateReportCommon(firstDay, commons);
                        }
                        if (toTimeSpan.Hour < 23) {
                            ReportFull lastDay = result.Last();
                            IEnumerable<ReportCommon> commons = lastDay.Commons.Where(o => o.Hour <= toTimeSpan.Hour);
                            UpdateReportCommon(lastDay, commons);
                        }
                    }
                }

                var adaptedValues = (from c in result
                                     select new ReportAdapted(c, trackerObject == null ? 0 : trackerObject.Consumption)).ToList();
                data[tracker] = adaptedValues;
            }
            return data;
        }

        private static void UpdateReportCommon(ReportFull reportFull, IEnumerable<ReportCommon> commons) {
            reportFull.AvgSpeed = 0;
            reportFull.Distance = 0;
            reportFull.MaxSpeed = 0;
            reportFull.Moving = 0;
            reportFull.Parking = 0;
            int count = 0;
            foreach (ReportCommon reportCommon in commons) {
                reportFull.AvgSpeed += reportCommon.AvgSpeed;
                reportFull.Distance += reportCommon.Distance ?? 0;
                if (reportCommon.MaxSpeed > reportFull.MaxSpeed) { reportFull.MaxSpeed = reportCommon.MaxSpeed; }
                reportFull.Moving += reportCommon.Moving;
                reportFull.Parking += reportCommon.Parking;
                count++;
            }
            if (count > 0) { reportFull.AvgSpeed = Math.Round(reportFull.AvgSpeed / count, 2); }
        }

        private static List<ReportFull> LoadHistories(int trackerId, DateTime from, DateTime to, LoadingReportType loadingType) {
            List<GpsDay> days = GpsDayHelper.GetDaysByPeriod(from, to, trackerId);
            var reports = new List<ReportFull>();
            foreach (GpsDay day in days) {
                var reportGenerate = new ReportGenerate(trackerId, loadingType == LoadingReportType.WithDetailes);
                day.Packages.ForEach(reportGenerate.AddItem);
                reports.Add(reportGenerate.ReportFull);
            }

            return reports;
        }

        #endregion

        #region detail report

        public static JsonObject GenerateDetailedReport(DateTime from, DateTime to, DateTime fromTimeSpan, DateTime toTimeSpan,
            int[] trackers, string caption) {
            var returnObject = new List<object>();
            Dictionary<int, List<DetailReportItemDay>> data = GetDetailedReportData(from, to, fromTimeSpan, toTimeSpan, trackers);
            foreach (var keyValue in data) {
                returnObject.Add(new { tracker = keyValue.Key.ToString(CultureInfo.InvariantCulture), Days = keyValue.Value });
            }
            DateTime fromDate = from.AddHours(fromTimeSpan.Hour).AddMinutes(fromTimeSpan.Minute);
            DateTime toDate = to.AddHours(toTimeSpan.Hour).AddMinutes(toTimeSpan.Minute);
            var obj = new JsonObject { { "Data", returnObject }, 
            { "Title", caption+" за " + fromDate.ToFullDateTime() + " - " + toDate.ToFullDateTime() } ,
            { "Caption", caption   } ,
            {"Id", Guid.NewGuid()}
            };
            return obj;
        }

        public static Dictionary<int, List<DetailReportItemDay>> GetDetailedReportData(DateTime from, DateTime to, DateTime fromTimeSpan, DateTime toTimeSpan,
            IEnumerable<int> trackers) {
            var data = new Dictionary<int, List<DetailReportItemDay>>();
            List<Tracker> trackerObjects = CouchbaseManager.GetTrackers(trackers.ToArray());
            int iterator = 1;
            foreach (Tracker tracker in trackerObjects) {
                List<ReportFull> result = LoadHistories(tracker.Id, from, to, LoadingReportType.WithDetailes);
                if (result != null && result.Count > 0) {
                    if (from.Date == to.Date) {
                        ReportFull firstDay = result.First();
                        firstDay.Commons.RemoveAll(o => o.Hour < fromTimeSpan.Hour || o.Hour >= toTimeSpan.Hour);
                    } else {
                        if (fromTimeSpan.Hour > 0) {
                            var firstDay = result.First();
                            firstDay.Commons.RemoveAll(o => o.Hour < fromTimeSpan.Hour);
                        }
                        if (toTimeSpan.Hour < 23) {
                            var lastDay = result.Last();
                            lastDay.Commons.RemoveAll(o => o.Hour >= toTimeSpan.Hour);
                        }
                    }
                }
                var days = new List<DetailReportItemDay>();
                foreach (ReportFull reportFull in result) {
                    var commons = new List<ReportHistory>();
                    foreach (ReportCommon common in reportFull.Commons) {
                        commons.AddRange(common.Histories);
                    }
                    days.Add(new DetailReportItemDay { Day = reportFull.Date.ToString("dd-MM-yyyy"), Items = ComputeDetailedItems(commons, tracker, ref iterator) });
                }
                data[tracker.Id] = days;
            }
            return data;
        }

        private static List<DetailReportItem> ComputeDetailedItems(IEnumerable<ReportHistory> histories, Tracker tracker, ref int iterator) {
            var hst = new List<ReportHistory>();
            ReportHistory currentItem = null;
            foreach (ReportHistory t in histories) {
                if (currentItem == null) {
                    currentItem = t;
                    hst.Add(currentItem);
                    continue;
                }
                if (currentItem.IsMoving == t.IsMoving) {
                    currentItem.Distance += t.Distance;
                    currentItem.End = t.End ?? t.Start;
                } else {
                    if (currentItem.IsMoving != true && currentItem.End == null) {
                        currentItem.End = t.Start;
                        if (currentItem.Latitude == 0) {
                            currentItem.Latitude = t.Latitude;
                            currentItem.Longitude = t.Longitude;
                        }
                    }
                    currentItem = t;
                    hst.Add(currentItem);
                }
            }
            var items = new List<DetailReportItem>();
            for (var j = 0; j < hst.Count; j++) {
                var obj = new DetailReportItem();
                if (hst[j].IsMoving == true) {
                    obj.Moving = new ReportHistoryItem(iterator++, hst[j], tracker.Consumption);
                    if (hst.Count > j + 1) { obj.Parking = new ReportHistoryItem(iterator++, hst[++j]); } else { obj.Parking = new ReportHistoryItem(iterator++); }
                } else {
                    obj.Moving = new ReportHistoryItem(iterator++);
                    obj.Parking = new ReportHistoryItem(iterator++, hst[j]);
                }
                items.Add(obj);
            }
            return items;
        }

        private static void TryUpdateGeoLocation(ReportHistoryItem historyItem) {
            if (historyItem == null || historyItem.IsMoving == true || !string.IsNullOrWhiteSpace(historyItem.GeoLocation)) { return; }
            historyItem.GeoLocation = RevertGeoCoding.Decode(historyItem.Latitude, historyItem.Longitude);
        }

        #endregion

        #region export summary

        public static MemoryStream ExportSummaryReport(DateTime from, DateTime to, DateTime fromTimeSpan, DateTime toTimeSpan,
            IEnumerable<int> trackers, IEnumerable<string> names) {
            var data = GetSummaryReportData(from, to, fromTimeSpan, toTimeSpan, trackers);
            var stream = GenerateExcel(CreateSheetDataSummaryReport(data, names, from, to, fromTimeSpan, toTimeSpan), CreateColumnData(1, 8, 23d));
            return stream;
        }

        private static IEnumerable<OpenXmlElement> CreateSheetDataSummaryReport(Dictionary<int, List<ReportAdapted>> data, IEnumerable<string> names
            , DateTime from, DateTime to, DateTime fromTimeSpan, DateTime toTimeSpan) {
            var fromDate = from.AddHours(fromTimeSpan.Hour).AddMinutes(fromTimeSpan.Minute);
            var toDate = to.AddHours(toTimeSpan.Hour).AddMinutes(toTimeSpan.Minute);
            string caption = "Сводный отчет за " + fromDate.ToString("dd-MM-yyyy HH:mm") + " - " + toDate.ToString("dd-MM-yyyy HH:mm");
            var elements = new List<OpenXmlElement>();
            elements.Add(CreateCaptionRow(caption));
            elements.Add(CreateHeaderRowSummary(new[] { "Дата", "Время движения, ч", "Время стоянки, ч", "Средняя скорость, км/ч", "Макс. скорость, км/ч", "Пробег, км" }));
            int index = 0;
            foreach (var keyValue in data) {
                elements.Add(CreateCarGroupSummary(names.ElementAt(index++)));
                elements.AddRange(CreateCarDataSummary(keyValue.Value));
                elements.Add(CreateTotalSummary(keyValue.Value));
            }
            return elements;
        }

        #endregion

        public static MemoryStream ExportDetailReport(DateTime from, DateTime to, DateTime fromTimeSpan, DateTime toTimeSpan,
            IEnumerable<int> trackers, IEnumerable<string> names) {
            Dictionary<int, List<DetailReportItemDay>> data = GetDetailedReportData(from, to, fromTimeSpan, toTimeSpan, trackers);
            UpdateGeoLocation(data);
            MemoryStream stream = GenerateExcel(CreateSheetDataDetailReport(data, names, from, to, fromTimeSpan, toTimeSpan), CreateColumnData(1, 8, 16d));
            return stream;
        }

        private static void UpdateGeoLocation(Dictionary<int, List<DetailReportItemDay>> data) {
            foreach (KeyValuePair<int, List<DetailReportItemDay>> item in data) {
                foreach (DetailReportItemDay detailReportItemDay in item.Value) {
                    foreach (DetailReportItem detailReportItem in detailReportItemDay.Items) {
                        if (detailReportItem.Parking == null) { continue; }
                        TryUpdateGeoLocation(detailReportItem.Parking);
                    }
                }
            }
        }

        private static IEnumerable<OpenXmlElement> CreateSheetDataDetailReport(Dictionary<int, List<DetailReportItemDay>> data, IEnumerable<string> names
            , DateTime from, DateTime to, DateTime fromTimeSpan, DateTime toTimeSpan) {
            var fromDate = from.AddHours(fromTimeSpan.Hour).AddMinutes(fromTimeSpan.Minute);
            var toDate = to.AddHours(toTimeSpan.Hour).AddMinutes(toTimeSpan.Minute);
            string caption = "Детальный отчет за " + fromDate.ToString("dd-MM-yyyy HH:mm") + " - " + toDate.ToString("dd-MM-yyyy HH:mm");
            var elements = new List<OpenXmlElement>();
            elements.Add(CreateCaptionRow(caption));
            elements.Add(CreateMovingParkingHeaders());

            elements.Add(CreateHeaderRowSummary(new[] { "Начало", "Конец", "Общее время", "Пробег, км", "Начало", "Конец", "Общее время", "Адрес" }));
            int index = 0;
            foreach (var keyValue in data) {
                elements.Add(CreateCarGroupSummary(names.ElementAt(index++)));
                foreach (var element in keyValue.Value) {
                    elements.Add(CreateDayHeaders(element.Day));
                    elements.AddRange(CreateCarDataDetail(element.Items));
                }
            }
            return elements;
        }

        private static Row CreateMovingParkingHeaders() {
            var row = new Row();
            row.Append(CreateCell("", 2U));
            row.Append(CreateCell("Движение", 2U));
            row.Append(CreateCell("", 2U));
            row.Append(CreateCell("", 2U));
            row.Append(CreateCell("", 2U));
            row.Append(CreateCell("Стоянка", 2U));
            return row;
        }

        private static Row CreateDayHeaders(string caption) {
            var row = new Row();
            row.Append(CreateCell("", 3U));
            row.Append(CreateCell("", 3U));
            row.Append(CreateCell("", 3U));
            row.Append(CreateCell("", 3U));
            row.Append(CreateCell(caption, 3U));
            row.Append(CreateCell("", 3U));
            row.Append(CreateCell("", 3U));
            row.Append(CreateCell("", 3U));
            return row;
        }

        private static Row CreateDayHeadersParking(string caption) {
            var row = new Row();
            row.Append(CreateCell("", 3U));
            row.Append(CreateCell("", 3U));
            row.Append(CreateCell(caption, 3U));
            row.Append(CreateCell("", 3U));
            return row;
        }

        private static IEnumerable<Row> CreateCarDataDetail(IEnumerable<DetailReportItem> data) {
            var rows = new List<Row>();
            foreach (var item in data) {
                var cells = new List<Cell>();
                cells.Add(CreateCell(item.Moving.StartStr, 0U));
                cells.Add(CreateCell(item.Moving.EndStr, 0U));
                cells.Add(CreateCell(item.Moving.DiffTimeStr, 0U));
                cells.Add(CreateCell(item.Moving.DistanceStr, 0U));
                cells.Add(CreateCell(item.Parking.StartStr, 0U));
                cells.Add(CreateCell(item.Parking.EndStr, 0U));
                cells.Add(CreateCell(item.Parking.DiffTimeStr, 0U));
                cells.Add(CreateCell(item.Parking.GeoLocation, 0U));
                var row = new Row();
                row.Append(cells);
                rows.Add(row);
            }
            return rows;
        }

        private static IEnumerable<Row> CreateCarDataParking(IEnumerable<DetailReportItem> data) {
            var rows = new List<Row>();
            foreach (var item in data) {
                var cells = new List<Cell>();
                cells.Add(CreateCell(item.Parking.StartStr, 0U));
                cells.Add(CreateCell(item.Parking.EndStr, 0U));
                cells.Add(CreateCell(item.Parking.DiffTimeStr, 0U));
                cells.Add(CreateCell(item.Parking.GeoLocation, 0U));
                var row = new Row();
                row.Append(cells);
                rows.Add(row);
            }
            return rows;
        }

        public static MemoryStream ExportParkingReport(DateTime from, DateTime to, DateTime fromTimeSpan, DateTime toTimeSpan,
            IEnumerable<int> trackers, IEnumerable<string> names) {
            Dictionary<int, List<DetailReportItemDay>> data = GetDetailedReportData(from, to, fromTimeSpan, toTimeSpan, trackers);
            UpdateGeoLocation(data);
            MemoryStream stream = GenerateExcel(CreateSheetDataParkingReport(data, names, from, to, fromTimeSpan, toTimeSpan), CreateColumnData(1, 8, 16d));
            return stream;
        }

        private static IEnumerable<OpenXmlElement> CreateSheetDataParkingReport(Dictionary<int, List<DetailReportItemDay>> data, IEnumerable<string> names
           , DateTime from, DateTime to, DateTime fromTimeSpan, DateTime toTimeSpan) {
            var fromDate = from.AddHours(fromTimeSpan.Hour).AddMinutes(fromTimeSpan.Minute);
            var toDate = to.AddHours(toTimeSpan.Hour).AddMinutes(toTimeSpan.Minute);
            string caption = "Отчет по стоянкам за " + fromDate.ToString("dd-MM-yyyy HH:mm") + " - " + toDate.ToString("dd-MM-yyyy HH:mm");
            var elements = new List<OpenXmlElement>();
            elements.Add(CreateCaptionRow(caption));
            elements.Add(CreateHeaderRowSummary(new[] { "Начало", "Конец", "Общее время", "Адрес" }));
            int index = 0;
            foreach (var keyValue in data) {
                elements.Add(CreateCarGroupSummary(names.ElementAt(index++)));
                foreach (var element in keyValue.Value) {
                    elements.Add(CreateDayHeadersParking(element.Day));
                    elements.AddRange(CreateCarDataParking(element.Items));
                }
            }
            return elements;
        }

        private static Row CreateTotalSummary(List<ReportAdapted> data) {
            var row = new Row();
            var summaryMoving = data.Sum(o => o.Moving);
            var summaryParking = data.Sum(o => o.Parking);
            var summaryDistance = data.Sum(o => o.Distance ?? 0);
            row.Append(CreateCell("", 3U));
            row.Append(CreateCell(summaryMoving.ToWordsWithDays(), 3U));
            row.Append(CreateCell(summaryParking.ToWordsWithDays(), 3U));
            row.Append(CreateCell("", 3U));
            row.Append(CreateCell("", 3U));
            row.Append(CreateCell(summaryDistance.ToDistanceInKilometers(), 3U));
            return row;
        }

        private static Row CreateCaptionRow(string caption) {
            var row = new Row();
            row.Height = DoubleValue.FromDouble(22);
            row.CustomHeight = true;
            var cells = new List<Cell>();

            var cell = new Cell();
            cell.DataType = CellValues.String;
            cell.CellValue = new CellValue(caption);
            cell.StyleIndex = 2U;
            cells.Add(cell);
            row.Append(cells);
            return row;
        }

        private static Row CreateHeaderRowSummary(string[] headers) {
            var row = new Row();
            row.Height = DoubleValue.FromDouble(22);
            row.CustomHeight = true;
            var cells = new List<Cell>();

            foreach (string header in headers) {
                var cell = new Cell();
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(header);
                cell.StyleIndex = 1U;
                cells.Add(cell);
            }
            row.Append(cells);
            return row;
        }

        private static Row CreateCarGroupSummary(string carName) {
            var row = new Row();
            var cells = new List<Cell>();

            var cell = new Cell();
            cell.DataType = CellValues.String;
            cell.CellValue = new CellValue(carName);
            cell.StyleIndex = 2U;
            cells.Add(cell);

            row.Append(cells);
            return row;
        }

        private static IEnumerable<Row> CreateCarDataSummary(IEnumerable<ReportAdapted> data) {
            var rows = new List<Row>();
            foreach (var item in data) {
                var cells = new List<Cell>();
                cells.Add(CreateCell(item.DateStr, 0U));
                cells.Add(CreateCell(item.MovingStr, 0U));
                cells.Add(CreateCell(item.ParkingStr, 0U));
                cells.Add(CreateCell(item.AvgSpeed, 0U));
                cells.Add(CreateCell(item.MaxSpeed, 0U));
                cells.Add(CreateCell(item.DistanceStr, 0U));
                var row = new Row();
                row.Append(cells);
                rows.Add(row);
            }
            return rows;
        }

        private static Cell CreateCell(object value, UInt32Value styleIndex) {
            var cell = new Cell();
            cell.DataType = CellValues.String;
            cell.CellValue = new CellValue(value + "");
            cell.StyleIndex = styleIndex;
            return cell;
        }

        private static DocumentFormat.OpenXml.Spreadsheet.Column CreateColumnData(UInt32 StartColumnIndex, UInt32 EndColumnIndex, double ColumnWidth) {
            DocumentFormat.OpenXml.Spreadsheet.Column column;
            column = new DocumentFormat.OpenXml.Spreadsheet.Column();
            column.Min = StartColumnIndex;
            column.Max = EndColumnIndex;
            column.Width = ColumnWidth;
            column.CustomWidth = true;
            return column;
        }

        private static void GenerateWorkbookStylesPart1Content(WorkbookStylesPart workbookStylesPart1) {
            Stylesheet stylesheet1 = new Stylesheet();

            var fonts1 = new Fonts() { Count = 3U };

            var font1 = new Font();
            var fontSize1 = new FontSize { Val = 11D };
            var color1 = new Color { Theme = 1U };
            var fontName1 = new FontName() { Val = "Calibri" };
            var fontFamilyNumbering1 = new FontFamilyNumbering() { Val = 2 };
            var fontScheme1 = new FontScheme() { Val = FontSchemeValues.Minor };

            font1.Append(fontSize1);
            font1.Append(color1);
            font1.Append(fontName1);
            font1.Append(fontFamilyNumbering1);
            font1.Append(fontScheme1);

            var font2 = new Font();
            font2.Bold = new Bold();
            var fontSize2 = new FontSize() { Val = 11D };

            var fontName2 = new FontName() { Val = "Calibri" };
            var fontFamilyNumbering2 = new FontFamilyNumbering() { Val = 2 };
            var fontScheme2 = new FontScheme() { Val = FontSchemeValues.Minor };

            font2.Append(fontSize2);
            font2.Append(fontName2);
            font2.Append(fontFamilyNumbering2);
            font2.Append(fontScheme2);

            var fontCarName = new Font();
            fontCarName.Bold = new Bold();
            var fontSize3 = new FontSize { Val = 14D };
            var color3 = new Color { Theme = 1U };
            var fontName3 = new FontName() { Val = "Calibri" };
            var fontFamilyNumbering3 = new FontFamilyNumbering() { Val = 2 };
            var fontScheme3 = new FontScheme() { Val = FontSchemeValues.Minor };

            fontCarName.Append(fontSize3);
            fontCarName.Append(color3);
            fontCarName.Append(fontName3);
            fontCarName.Append(fontFamilyNumbering3);
            fontCarName.Append(fontScheme3);

            fonts1.Append(font1);
            fonts1.Append(font2);
            fonts1.Append(fontCarName);

            var g = new DocumentFormat.OpenXml.Spreadsheet.Alignment();
            g.Vertical = new EnumValue<VerticalAlignmentValues>();
            g.Vertical.Value = VerticalAlignmentValues.Center;

            var fills1 = new Fills { Count = 3U };

            var fill1 = new Fill();
            var patternFill1 = new PatternFill { PatternType = PatternValues.None };

            fill1.Append(patternFill1);

            var fill2 = new Fill();
            var patternFill2 = new PatternFill { PatternType = PatternValues.Solid };
            var foregroundColor2 = new BackgroundColor() { Rgb = "FF73e834" };
            patternFill2.Append(foregroundColor2);

            fill2.Append(patternFill2);

            var fill3 = new Fill();
            var patternFill3 = new PatternFill { PatternType = PatternValues.Solid };
            var foregroundColor1 = new ForegroundColor { Rgb = "FFd8e4f2" };
            patternFill3.Append(foregroundColor1);

            fill3.Append(patternFill3);

            var fill4 = new Fill();
            var patternFill4 = new PatternFill { PatternType = PatternValues.Solid };
            var foregroundColor4 = new ForegroundColor() { Rgb = "FFefefef" };
            patternFill4.Append(foregroundColor4);

            fill4.Append(patternFill4);

            fills1.Append(fill1);
            fills1.Append(fill2);
            fills1.Append(fill3);
            fills1.Append(fill4);

            var borders1 = new Borders() { Count = 1U };

            var border1 = new Border();
            var leftBorder1 = new LeftBorder();
            var rightBorder1 = new RightBorder();
            var topBorder1 = new TopBorder();
            topBorder1.Color = new Color() { Rgb = "FF000000" };
            var bottomBorder1 = new BottomBorder();
            var diagonalBorder1 = new DiagonalBorder();

            border1.Append(leftBorder1);
            border1.Append(rightBorder1);
            border1.Append(topBorder1);
            border1.Append(bottomBorder1);
            border1.Append(diagonalBorder1);

            borders1.Append(border1);

            var cellStyleFormats1 = new CellStyleFormats { Count = 4U };
            var cellFormat1 = new CellFormat { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 0U };
            var cellFormat2 = new CellFormat { NumberFormatId = 0U, FontId = 1U, FillId = 2U, BorderId = 0U, ApplyNumberFormat = false, ApplyBorder = false, ApplyAlignment = true };
            var cellFormatCarName = new CellFormat { NumberFormatId = 0U, FontId = 2U, FillId = 0U, BorderId = 0U };
            var cellFormatTotal = new CellFormat { NumberFormatId = 0U, FontId = 1U, FillId = 3U, BorderId = 0U, ApplyNumberFormat = false, ApplyBorder = false };

            cellStyleFormats1.Append(cellFormat1);
            cellStyleFormats1.Append(cellFormat2);
            cellStyleFormats1.Append(cellFormatCarName);
            cellStyleFormats1.Append(cellFormatTotal);

            var cellFormats1 = new CellFormats { Count = 4U };
            var cellFormat3 = new CellFormat { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 0U, FormatId = 0U };
            var cellFormat4 = new CellFormat { NumberFormatId = 0U, FontId = 1U, FillId = 2U, BorderId = 0U, FormatId = 1U, ApplyAlignment = true, Alignment = g };
            var cellFormatCarName1 = new CellFormat { NumberFormatId = 0U, FontId = 2U, FillId = 0U, BorderId = 0U, FormatId = 2U };
            var cellFormatTotal1 = new CellFormat { NumberFormatId = 0U, FontId = 1U, FillId = 3U, BorderId = 0U, FormatId = 1U };

            cellFormats1.Append(cellFormat3);
            cellFormats1.Append(cellFormat4);
            cellFormats1.Append(cellFormatCarName1);
            cellFormats1.Append(cellFormatTotal1);

            var cellStyles1 = new CellStyles { Count = 4U };
            var cellStyle1 = new CellStyle { Name = "Good", FormatId = 1U, BuiltinId = 26U };
            var cellStyle2 = new CellStyle { Name = "Normal", FormatId = 0U, BuiltinId = 0U, };
            var cellStyleCarName = new CellStyle { Name = "CarName", FormatId = 0U, BuiltinId = 0U };
            var cellStyleTotal = new CellStyle { Name = "TotalName", FormatId = 0U, BuiltinId = 1U, };

            cellStyles1.Append(cellStyle1);
            cellStyles1.Append(cellStyle2);
            cellStyles1.Append(cellStyleCarName);
            cellStyles1.Append(cellStyleTotal);
            var differentialFormats1 = new DifferentialFormats { Count = 0U };
            var tableStyles1 = new TableStyles { Count = 0U, DefaultTableStyle = "TableStyleMedium9", DefaultPivotStyle = "PivotStyleLight16" };

            stylesheet1.Append(fonts1);
            stylesheet1.Append(fills1);
            stylesheet1.Append(borders1);
            stylesheet1.Append(cellStyleFormats1);
            stylesheet1.Append(cellFormats1);
            stylesheet1.Append(cellStyles1);
            stylesheet1.Append(differentialFormats1);
            stylesheet1.Append(tableStyles1);
            workbookStylesPart1.Stylesheet = stylesheet1;
        }

        private static MemoryStream GenerateExcel(IEnumerable<OpenXmlElement> data, DocumentFormat.OpenXml.Spreadsheet.Column column) {
            var stream = new MemoryStream();
            using (SpreadsheetDocument myWorkbook =
                SpreadsheetDocument.Create(stream,
                SpreadsheetDocumentType.Workbook)) {

                WorkbookPart workbookPart = myWorkbook.AddWorkbookPart();
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                string relId = workbookPart.GetIdOfPart(worksheetPart);

                var fileVersion = new FileVersion { ApplicationName = "Microsoft Office Excel" };

                var wbsp = workbookPart.AddNewPart<WorkbookStylesPart>();
                GenerateWorkbookStylesPart1Content(wbsp);
                wbsp.Stylesheet.Save();

                var sheets = new Sheets();
                var sheet = new Sheet { Name = "Отчет", SheetId = 1, Id = relId };
                sheets.Append(sheet);

                var sheetData = new SheetData(data);
                var workbook = new Workbook();
                workbook.Append(fileVersion);
                workbook.Append(sheets);
                var worksheet = new Worksheet();
                var columns = new Columns();
                columns.Append(column);
                worksheet.Append(columns);
                worksheet.Append(sheetData);
                worksheetPart.Worksheet = worksheet;
                worksheetPart.Worksheet.Save();
                myWorkbook.WorkbookPart.Workbook = workbook;
                myWorkbook.WorkbookPart.Workbook.Save();
                myWorkbook.Close();
            }
            return stream;
        }

        private enum LoadingReportType {
            WithDetailes = 0,
            SkipDetailes = 1
        }
    }

    public class DetailReportItem {
        [JsonProperty("parking")]
        public ReportHistoryItem Parking { get; set; }
        [JsonProperty("moving")]
        public ReportHistoryItem Moving { get; set; }
    }

    public class ReportHistoryItem {
        private ReportHistory _reportHistory;
        private readonly decimal _consumption;
        public ReportHistoryItem(int iterator, ReportHistory reportHistory)
            : this(iterator, reportHistory, 0) {
        }

        public ReportHistoryItem(int iterator, ReportHistory reportHistory, decimal consumption)
            : this(iterator) {
            _consumption = consumption;
            Set(reportHistory);
        }

        public ReportHistoryItem(int iterator) {
            Id = iterator;
            StartStr = string.Empty;
            EndStr = string.Empty;
            DistanceStr = string.Empty;
            GeoLocation = string.Empty;
            DiffTimeStr = string.Empty;
        }

        [JsonProperty("s")]
        public string StartStr { get; set; }
        [JsonProperty("e", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string EndStr { get; set; }
        [JsonProperty("d")]
        public string DistanceStr { get; set; }
        [JsonProperty("g", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string GeoLocation { get; set; }
        [JsonProperty("i")]
        public bool IsMoving { get; set; }
        [JsonProperty("t")]
        public string DiffTimeStr { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("b", DefaultValueHandling = DefaultValueHandling.Ignore)]//latitude
        public decimal Latitude { get; set; }
        [JsonProperty("c", DefaultValueHandling = DefaultValueHandling.Ignore)]//longitude
        public decimal Longitude { get; set; }

        private void Set(ReportHistory reportHistory) {
            _reportHistory = reportHistory;
            StartStr = reportHistory.Start.ToTimeDateTime();
            EndStr = reportHistory.End.ToTimeDateTime();
            DistanceStr = _consumption == 0 ? reportHistory.Distance.ToDistanceInKilometers() : reportHistory.Distance.ToDistanceInKilometersAndConsumption(_consumption);
            IsMoving = reportHistory.IsMoving ?? false;
            GeoLocation = reportHistory.GeoLocation;
            DiffTimeStr = ((reportHistory.End ?? reportHistory.Start) - reportHistory.Start).ToTimeDateTime();
            Latitude = reportHistory.Latitude;
            Longitude = reportHistory.Longitude;
        }

        public ReportHistory GetReportHistory() {
            return _reportHistory;
        }
    }

    public class DetailReportItemDay {
        [JsonProperty("g")]
        public string Day { get; set; }
        [JsonProperty("items")]
        public List<DetailReportItem> Items { get; set; }
    }
}
