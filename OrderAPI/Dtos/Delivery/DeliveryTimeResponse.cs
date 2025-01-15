namespace OrderAPI.Dtos
{
    public class DeliveryTimeResponse
    {
        public string Status { get; set; }
        public List<Row> Rows { get; set; }
    }

    public class Row
    {
        public List<Element> Elements { get; set; }
    }

    public class Element
    {
        public Distance Distance { get; set; }
        public Duration Duration { get; set; }
    }

    public class Distance
    {
        public string Text { get; set; }
        public int Value { get; set; } 
    }

    public class Duration
    {
        public string Text { get; set; }
        public int Value { get; set; } 
    }
}