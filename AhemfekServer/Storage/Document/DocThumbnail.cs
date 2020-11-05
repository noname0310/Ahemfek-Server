using System;

namespace AhemfekServer.Storage.Document
{
    [Serializable]
    class DocThumbnail
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string Content { get; set; }

        private const int MaxShowContentLength = 20;

        public DocThumbnail()
        {

        }

        public DocThumbnail(string id, string date, string content)
        {
            Id = id;
            Date = date;
            Content = content;
        }

        public DocThumbnail(Doc doc)
        {
            Id = doc.User.Id;
            Date = doc.Time;
            Content = doc.Content[0][(MaxShowContentLength <= doc.Content[0].Length) ? ..MaxShowContentLength : ..^0];
        }
    }
}
