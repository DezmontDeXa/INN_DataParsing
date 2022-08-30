using Newtonsoft.Json;
using Shared;

namespace Kommersant
{
    public class Program
    {
        public static void Main()
        {
            var parser = new KommersantParser<IParsedData>("bcb5af909bfa980bb11f63f03bf23b82");
            var result230214326100 = parser.Parse("550306846702"); 
            return;
        }
    }
}