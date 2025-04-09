using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace LexBohWPF
{
    public class clsWordDocument
    {
        string path = "";
        WordprocessingDocument WPDocument;
        Body body;
        public List<clsWordDocumentEntry> entries;

        public clsWordDocument(string path)
        {
            this.path = path;
            this.WPDocument = WordprocessingDocument.Open(path, false);
            this.body = this.WPDocument.MainDocumentPart.Document.Body;
            this.read_entries();

        }
        private void read_entries()
        {
            this.entries = new List<clsWordDocumentEntry>();
            foreach (Paragraph p in this.body.Elements<Paragraph>())
            {
                clsWordDocumentEntry newEntry = new clsWordDocumentEntry(p);
                if (newEntry.lemma != "")
                    this.entries.Add(newEntry);
            }
        }
    }
    public class clsWordDocumentEntry
    {
        public Paragraph p;
        public string lemma = "";
        public clsWordDocumentEntry(Paragraph _p)
        {
            this.p = _p;
            bool reading = false;
            foreach (Run r in this.p.Descendants<Run>())
            {
                if (r.RunProperties.Bold == null && this.lemma != "")
                    return;
                else if (r.RunProperties.Bold != null)
                    this.lemma += r.InnerText;
            }
        }

    }
}
