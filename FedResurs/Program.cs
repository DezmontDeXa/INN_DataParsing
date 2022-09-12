namespace FedResurs
{
    public class Program
    {
        public static void Main()
        {
            //Console.ReadLine();
            var parser = new FedResursParser<FedResursInnParseResult>();
            var result = parser.Parse("644007996012");

            //FedResursInnParseResult result = new FedResursInnParseResult("", "", "", File.ReadAllText("test.html"));

            return;
        }
    }
}