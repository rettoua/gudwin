using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Smartline.Mapping;
using Smartline.Server.Runtime.Reports;

namespace GisServerService {
    public class Table1 {
        public string A1 { get; set; }
        public string A2 { get; set; }
        public string A3 { get; set; }
        public string A4 { get; set; }
        public string A5 { get; set; }
    }

    public class ReportSet {
        #region Dist report
        public XmlNode GenerateDist(List<ReportAdapted> reportAdapteds) {
            List<Table1> list = reportAdapteds.Select(CreateTable1Dist).ToList();
            var totalReportAdapted = new ReportAdapted {
                Distance = reportAdapteds.Sum(o => o.Distance),
                Moving = reportAdapteds.Sum(o => o.Moving),
                Parking = reportAdapteds.Sum(o => o.Parking),
                AvgSpeed = reportAdapteds.Average(o => o.AvgSpeed)
            };
            Table1 totalTable = CreateTable1Dist(totalReportAdapted);
            totalTable.A1 = "Итого";
            list.Add(totalTable);
            return GenerateXml(list);
        }

        private Table1 CreateTable1Dist(ReportAdapted reportAdapted) {
            var newTable = new Table1 {
                A1 = reportAdapted.Date.ToString("dd.MM.yyyy"),
                A2 = reportAdapted.DistanceStr,
                A3 = reportAdapted.MovingStrCommon,
                A4 = reportAdapted.ParkingStrCommon,
                A5 = reportAdapted.AvgSpeed.ToString(CultureInfo.InvariantCulture)
            };
            return newTable;
        }

        private XmlNode GenerateXml(IEnumerable<Table1> tables) {
            var reportSetNode = GisServiceHelper.ObjectToXml(this).SelectSingleNode("ReportSet");
            foreach (Table1 table1 in tables) {
                XmlNode selectSingleNode = GisServiceHelper.ObjectToXml(table1).SelectSingleNode("Table1");
                if (selectSingleNode != null) { reportSetNode.InnerXml += selectSingleNode.OuterXml; }
            }
            return reportSetNode;
        }
        #endregion

        #region Parking
        public XmlNode GenerateParking(List<DetailReportItemDay> reportItemDay) {
            var list = new List<Table1>();
            foreach (DetailReportItemDay detailReportItemDay in reportItemDay) {
                foreach (DetailReportItem variable in detailReportItemDay.Items) {
                    if (variable.Parking != null) {
                        var table = CreateTable1Parking(variable.Parking);
                        if (table != null) {
                            list.Add(table);
                        }
                    }
                }
            }
            return GenerateXml(list);
        }

        private Table1 CreateTable1Parking(ReportHistoryItem reportHistoryItem) {
            ReportHistory reportHistory = reportHistoryItem.GetReportHistory();
            if (reportHistory == null) {
                return null;
            }
            var newTable = new Table1 {
                A1 = reportHistory.Start.ToString("dd.MM.yyyy HH:mm:ss"),
                A2 = reportHistory.End.HasValue ? reportHistory.End.Value.ToString("dd.MM.yyyy HH:mm:ss") : "",
                A3 = reportHistoryItem.DiffTimeStr,
                A4 = reportHistory.GeoLocation,
            };
            return newTable;
        }
        #endregion
    }
}