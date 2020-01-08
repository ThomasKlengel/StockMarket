using System;
using System.Collections.Generic;
using System.Linq;

namespace StockMarket
{
    class PdfReader
    {
        /// <summary>
        /// Reads PDF file by a given path.
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="pageCount">The number of pages to read (0=all, 1 by default) </param>
        /// <returns></returns>
        public static DocumentTree PdfToText(string path, int pageCount=1 )
        {
            var pages = new DocumentTree();
            using (iText.Kernel.Pdf.PdfReader reader = new iText.Kernel.Pdf.PdfReader(path))
            {
                using (iText.Kernel.Pdf.PdfDocument pdfDocument = new iText.Kernel.Pdf.PdfDocument(reader))
                {
                    var strategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.LocationTextExtractionStrategy();

                    // set up pages to read
                    int pagesToRead = 1;
                    if (pageCount > 0)
                    {
                        pagesToRead = pageCount;
                    }
                    if (pagesToRead > pdfDocument.GetNumberOfPages() || pageCount==0)
                    {
                        pagesToRead = pdfDocument.GetNumberOfPages();
                    }

                    // for each page to read...
                    for (int i = 1; i <= pagesToRead; ++i)
                    {
                        // get the page and save it
                        var page = pdfDocument.GetPage(i);
                        var txt = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page, strategy);
                        pages.Add(txt);
                    }
                    pdfDocument.Close();
                    reader.Close();
                }
            }
            return pages;
        }

    }
    
    /// <summary>
    /// A class representing parts of a PDF document.
    /// </summary>
    class DocumentTree
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentTree()
        {
            Pages = new List<DocumentPage>();
        }

        private List<DocumentPage> _pages;
        /// <summary>
        /// The pages of the document
        /// </summary>
        public List<DocumentPage> Pages
        {
            get { return _pages; }
            set { _pages = value; }
        }

        /// <summary>
        /// Adds a <see cref="DocumentPage"/> to the document.
        /// </summary>
        /// <param name="page">The text of the <see cref="DocumentPage"/>.</param>
        public void Add(string page)
        {
            Pages.Add(new DocumentPage(page));
        }
    }

    /// <summary>
    /// A class representing a single page of a document
    /// </summary>
    class DocumentPage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageContent">The pages content as text</param>
        public DocumentPage(string pageContent)
        {
            // set the content to the input
            CompletePage = pageContent;

            // split the content by lines
            var splitter = new string[] { "\n" };
            foreach (var line in CompletePage.Split(splitter, StringSplitOptions.None))
            {
                // add lines to the page if the content is not empty
                if (!string.IsNullOrWhiteSpace(line))
                {                    
                    _lines.Add(new Line(line));
                }
            }

        }

        private List<Line> _lines = new List<Line>();
        /// <summary>
        /// The lines of text of the <see cref="DocumentPage"/>
        /// </summary>
        public List<Line> Lines
        {
            get
            {
                return _lines;
            }            
        }

        /// <summary>
        /// The text of the complete <see cref="DocumentPage"/>.
        /// </summary>
        private string CompletePage;
    }

    /// <summary>
    /// A class representing a single line of text
    /// </summary>
    class Line
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Line(string lineContent)
        {
            CompleteLine = lineContent;
        }

        /// <summary>
        /// The words of the <see cref="Line"/>.
        /// </summary>
        public List<string> Words
        {
            get
            {
                return CompleteLine.Split(" ".ToArray()).Where((word)=> { return !string.IsNullOrWhiteSpace(word); }).ToList();
            }
        }

        /// <summary>
        /// The complete text of the <see cref="Line"/>.
        /// </summary>
        private string CompleteLine;

        public override string ToString()
        {
            return CompleteLine;
        }
    }
}
