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

    class clsDocument
    {
        public interface intSegment
        {
            bool bold { get; set; }
            bool italic { get; set; }
            bool underline { get; set; }
            string text { get; set; }
        }


        public class clsReadRuns
        {
            /*
             *  První krok aplikace: načíst runs a upravit je
             */
            public class clsRunlikeSegment : intSegment
            {
                /*
                 *  prvek, do kterého se budou načítat wRun - kousky text s vlastním formátováním. 
                 *  Nás z formátování zajímá jenom bold/kurzíva/podtržení + text
                 *  Nemůžeme je ale načíst jak leží a beží, protože budeme muset provést nějaké úpravy
                 *  u mezer - u těch je formátování všelijaké, ale my ho musíme sjednotit. A manipulovat s 
                 *  celým objektem Worprocessing je pro tyto účely zbytečně složité.
                 *  Tato třída tedy slouží při prvním načítání souborů
                 */
                public bool _bold;
                public bool _italic;
                public bool _underline;
                public string _text;
                public bool bold { get => _bold; set => _bold = value; }
                public bool italic { get => _italic; set => _italic = value; }
                public bool underline { get => _underline; set => _underline = value; }
                public string text { get => _text; set => _text = value; }


                public clsRunlikeSegment next_segment;
                public clsRunlikeSegment prev_segment;
                public clsRunlikeSegment(clsRunlikeSegment _prev, Run _r)
                {
                    this.prev_segment = _prev;
                    if (_prev != null)
                        this.prev_segment.next_segment = this;
                    this.bold = (_r.RunProperties.Bold != null);
                    this.italic = (_r.RunProperties.Italic != null);
                    this.underline = (_r.RunProperties.Underline != null);
                    this.text = _r.InnerText;

                }

            }
            public List<clsRunlikeSegment> segments = new List<clsRunlikeSegment>();

            public clsReadRuns(clsWordDocumentEntry _e)
            {
                clsRunlikeSegment prev = null;

                foreach (Run r in _e.p.Descendants<Run>())
                {
                    clsRunlikeSegment rls = new clsRunlikeSegment(prev, r);
                    segments.Add(rls);
                    prev = rls;
                }
                this.adjustSpaces();
            }
            private void adjustSpaces()
            {  /*
                *  zde ošetříme případy, kdy jsou vícemezerové segmenty rozděleny mezi více runů, protože 
                *  se liší formátováním. Všechny takové sloučíme směrem k první mezeře.
                *  Dále ošetříme případy, kdy máme 1 mezeru s formátováním jiným než předchozí i následující segment
                *  A konečně nahradíme občas se vyskytující nbsp za obyčejnou mezeru
                */
                foreach (clsRunlikeSegment r in this.segments)
                {
                    r.text = r.text.Replace(@"\u00A0\", " ");//nbsp - > obyč mezera
                    clsRunlikeSegment n = r.next_segment;
                    clsRunlikeSegment p = r.prev_segment;
                    if (r.text.EndsWith(" "))
                    {
                        while (n != null && n.text[0] == ' ')
                        {
                            r.text += " ";
                            n.text = n.text.Substring(1);
                            if (n.text == "")
                            {
                                n = n.next_segment;
                                n.prev_segment = r;//prozatím "vyignorujeme" prázdný prvek, odstraníme je pak všechny naráz
                                r.next_segment = n;
                            }
                        }
                        if (r.text == " ")//speciální případ: jediná mezera s odlišným formátováním -> sloučíme s předchozím segmentem
                        {
                            if (p != null)
                                p.text += " ";
                            r.text = "";

                        }
                    }
                }
                this.segments.RemoveAll(this.rls_is_empty);//odstraníme prázdné

            }
            private bool rls_is_empty(clsRunlikeSegment r)
            {
                return r.text == "";
            }

        }
        public class clsIsolateSeparators
        {
            public class clsSeparatedSegment : intSegment
            {
                public bool _bold;
                public bool _italic;
                public bool _underline;
                public string _text;
                public bool bold { get => _bold; set => _bold = value; }
                public bool italic { get => _italic; set => _italic = value; }
                public bool underline { get => _underline; set => _underline = value; }
                public string text { get => _text; set => _text = value; }
                public bool is_separator = false;

                public clsSeparatedSegment()
                {
                }
                public clsSeparatedSegment(clsReadRuns.clsRunlikeSegment r, int textStartIndex = 0, int length = -1)
                {
                    this._bold = r.bold;
                    this._italic = r.italic;
                    this._underline = r.underline;
                    if (textStartIndex == 0 && length == -1) //kopírujeme celý element
                        this._text = r.text;
                    else
                    {
                        this._text = r._text.Substring(textStartIndex, length);
                    }
                    //return this;
                }
            }

            public List<clsSeparatedSegment> segments = new List<clsSeparatedSegment>();

            public char[] _1_separators = new char[] { ';', ':', '(', '[', '{', ')', ']', '}', '|', '+' };

            public void clsReadRuns(List<clsReadRuns.clsRunlikeSegment> segments)
            {
                foreach (clsReadRuns.clsRunlikeSegment r in segments)
                {
                    char[] chArr = r.text.ToCharArray();
                    int lastIndex = 0;
                    for (int i = 0; i < chArr.Length; i++)
                    {
                        /* separators:
                         * mezery (víc než 1): "  ", "   ", "    "
                         * ; : () [] {} |
                         * +
                         * + jakákoliv změna formátování
                         * Tedy každý runlikeSegment (který je vymezen změnou formátování) musí být ještě dále rozdělen podle těchto separátorů
                         */

                        string ch = chArr[i].ToString();
                        if (chArr[i] == ' ')
                        {
                            int j = i + 1;
                            while (j < chArr.Length && chArr[j] == ' ')
                                ch += chArr[j].ToString();
                        }

                        if (ch.LastIndexOfAny(_1_separators) > -1 || ch.Length > 1)
                        {
                            this.segments.Add(new clsSeparatedSegment(r, lastIndex, (i - 1) - lastIndex));
                            this.segments.Add(new clsSeparatedSegment(r, i, ch.Length));
                            lastIndex = i + 1;
                        }



                    }
                }
            }
        }

        private clsWordDocumentEntry e;
        public clsReadRuns readRuns;


        public clsDocument(clsWordDocumentEntry _e)
        {
            this.e = _e;
            this.readRuns = new clsReadRuns(_e);

        }

    }
}
