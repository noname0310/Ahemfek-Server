using System;
using System.Collections.Generic;
using System.Linq;

namespace AhemfekServer.Storage.Document
{
    class DocFolder
    {
        public string Name { get; private set; }
        public IReadOnlyCollection<KeyValuePair<DateTime, Doc>> NewestDocuments => _newestDocuments;
        public IReadOnlyCollection<KeyValuePair<int, Doc>> PopularDocuments => _popularDocuments;

        private readonly SortedList<DateTime, Doc> _newestDocuments;
        private readonly SortedList<int, Doc> _popularDocuments;

        public DocFolder(string name)
        {
            Name = name;
            _newestDocuments = new SortedList<DateTime, Doc>();
            _popularDocuments = new SortedList<int, Doc>();
        }

        public bool TryAddDoc(Doc doc)
        {
            DateTime dateTime = DateTime.Parse(doc.Time);
            if (_newestDocuments.ContainsKey(dateTime))
                return false;
            else
            {
                _newestDocuments.Add(dateTime, doc);
                _popularDocuments.Add(doc.Likes, doc);
                return true;
            }
        }

        public Doc RemoveDoc(string docName)
        {
            Doc doc = _newestDocuments.Values.Where(item => item.Title == docName).FirstOrDefault();
            _newestDocuments.Values.Remove(doc);
            _popularDocuments.Values.Remove(doc);
            return doc;
        }

        public bool TryLike(string docName)
        {
            Doc doc = _newestDocuments.Where(pair => pair.Value.Title == docName).FirstOrDefault().Value;
            Doc popdoc = _popularDocuments.Where(pair => pair.Value.Title == docName).FirstOrDefault().Value;
            
            if (doc == default)
                return false;

            doc.Likes++;
            if (!ReferenceEquals(doc, popdoc))
                popdoc.Likes++;

            for (int i = 0; i < _popularDocuments.Count; i++)
            {
                if (_popularDocuments[i].Title == docName)
                {
                    _popularDocuments.RemoveAt(i);
                    _popularDocuments.Add(doc.Likes, doc);
                    break;
                }
            }
            return true;
        }

        public bool TryUnlike(string docName)
        {
            Doc doc = _newestDocuments.Where(pair => pair.Value.Title == docName).FirstOrDefault().Value;
            Doc popdoc = _popularDocuments.Where(pair => pair.Value.Title == docName).FirstOrDefault().Value;

            if (doc == default)
                return false;

            doc.Likes--;
            if (!ReferenceEquals(doc, popdoc))
                popdoc.Likes--;

            for (int i = 0; i < _popularDocuments.Count; i++)
            {
                if (_popularDocuments[i].Title == docName)
                {
                    _popularDocuments.RemoveAt(i);
                    _popularDocuments.Add(doc.Likes, doc);
                    break;
                }
            }
            return true;
        }

        public List<DocThumbnail> GetDocThumbList(int startIndex, int count, DocOrder docOrder)
        {
            List<DocThumbnail> pagedDocsThum = new List<DocThumbnail>();
            switch (docOrder)
            {
                case DocOrder.Newest:
                    for (int i = startIndex; i < startIndex + count; i++)
                    {
                        if (_newestDocuments.Count < i + 1)
                            break;
                        pagedDocsThum.Add(new DocThumbnail(_newestDocuments.Values[i]));
                    }
                    break;
                case DocOrder.Popular:
                    for (int i = startIndex; i < startIndex + count; i++)
                    {
                        if (_popularDocuments.Count < i + 1)
                            break;
                        pagedDocsThum.Add(new DocThumbnail(_newestDocuments.Values[i]));
                    }
                    break;
                default:
                    break;
            }
            return pagedDocsThum;
        }
    }
}
