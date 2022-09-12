namespace FedResurs
{
    public class Document
    {
        public string Title { get; private set; }
        public string Id { get; private set; }

        public Document(string title, string id)
        {
            Title = title;
            Id = id;
        }
    }
}