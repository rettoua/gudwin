namespace Smartline.Mapping {
    public interface ISensor {
        int Id { get; set; }
        bool Available { get; set; }
        string Name { get; set; }
    }
}
