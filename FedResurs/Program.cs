namespace FedResurs
{
    public class Program
    {
        public static void Main()
        {
            //Console.ReadLine();
            var parser = new FedResursParser<FedResursInnParseResult>();
            var result = parser.Parse("220806581020");

            //FedResursInnParseResult result = new FedResursInnParseResult("", "", "", File.ReadAllText("test.html"));

            return;
        }
    }
}