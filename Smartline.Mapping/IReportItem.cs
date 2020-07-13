namespace Smartline.Mapping {
    public interface IReportItem {
        decimal AvgSpeed { get; set; }
        decimal MaxSpeed { get; set; }
        int? Distance { get; set; }
        int Parking { get; set; }
        int Moving { get; set; }
    }
}
