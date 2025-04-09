using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;




namespace LexBohWPF
{
    class clsWPFRenderer
    {
        public class clsRTFSegment : clsDocument.intSegment
        {
            private bool _bold;
            private bool _italic;
            private bool _underline;
            private string _text;
            private Color _bcolor;
            private Color _fcolor;
            public TextElement te;
            private clsDocument.intSegment _logical_segment;
            public bool bold { get => _bold; set => _bold = value; }
            public bool italic { get => _italic; set => _italic = value; }
            public bool underline { get => _underline; set => _underline = value; }
            public string text { get => _text; set => _text = value; }
            public clsDocument.intSegment logical_segment
            {
                get => _logical_segment; set => _logical_segment = value;
            }

            public clsRTFSegment(clsDocument.intSegment s)
            {
                this.logical_segment = s;
                this.text = s.text;
                this.bold = s.bold;
                this.italic = s.italic;
                this.underline = s.underline;
            }

        }
        private RichTextBox _target = null;
        private List<clsRTFSegment> _data = null;
        public RichTextBox target
        {
            set => this._target = value;
            get => this._target;
        }
        public void set_data(List<clsDocument.intSegment> segments)
        {
            _data = new List<clsRTFSegment>();
            foreach (clsDocument.intSegment s in segments)
            {
                _data.Add(new clsRTFSegment(s));
            }
        }
        public List<clsRTFSegment> data
        {
            get => this._data;
        }
        public void render()
        {
            if (this.target != null)
            {
                this.target.FontSize = 24;
                FlowDocument d = new FlowDocument();
                Paragraph p = new Paragraph();
                d.Blocks.Add(p);

                foreach (clsRTFSegment seg in this._data)
                {
                    Run r = new Run(seg.text);
                    if (seg.bold == true)
                        r.FontWeight = FontWeights.Bold;
                    if (seg.italic == true)
                        r.FontStyle = FontStyles.Italic;
                    if (seg.underline == true)
                        r.TextDecorations = TextDecorations.Underline;
                    p.Inlines.Add(r);
                    //p.Inlines.Add(seg.text);
                }
                target.Document = d;
            }
        }
        public clsWPFRenderer(RichTextBox rtf)
        {
            this.target = rtf;
        }
    }
}
