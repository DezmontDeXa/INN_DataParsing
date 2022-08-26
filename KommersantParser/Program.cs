using Newtonsoft.Json;
using Shared;

namespace Kommersant
{
    public class Program
    {
        public static void Main()
        {
            var parser = new KommersantParser<IParsedData>("bcb5af909bfa980bb11f63f03bf23b82");
            var result230214326100 = parser.Parse("230214326100"); 
            var result860222742476 = parser.Parse("860222742476");
            return;
        }
    }
}