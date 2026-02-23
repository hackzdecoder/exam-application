public class FruitsModel
{
    public int FruitId { get; set; }
    public string FruitName { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stocks { get; set; }
}