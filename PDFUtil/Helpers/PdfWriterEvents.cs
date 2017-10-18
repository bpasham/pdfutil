using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace PDFUtil.Helpers
{
    class PdfWriterEvents : IPdfPageEvent
    {
        string watermarkText = string.Empty;
        int _pages;
        public PdfWriterEvents(string watermark, int pages) 
        {
            watermarkText = watermark;
            _pages = pages;
        }

        public void OnOpenDocument(PdfWriter writer, Document document) { }
        public void OnCloseDocument(PdfWriter writer, Document document) { }
        public void OnStartPage(PdfWriter writer, Document document) {}        
        public void OnEndPage(PdfWriter writer, Document document)
        {
            float fontSize = 40;
            float xPosition = 300;
            float yPosition = 400;
            float angle = 45;
            try
            {
                //if (document.PageNumber >_pages) return;
                PdfContentByte under = writer.DirectContentUnder;
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, BaseFont.EMBEDDED);
                under.BeginText();
                under.SetColorFill(BaseColor.LIGHT_GRAY);
                under.SetFontAndSize(baseFont, fontSize);
                under.ShowTextAligned(PdfContentByte.ALIGN_CENTER, watermarkText, xPosition, yPosition, angle);
                under.EndText();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
        public void OnParagraph(PdfWriter writer, Document document, float paragraphPosition) { }
        public void OnParagraphEnd(PdfWriter writer, Document document, float paragraphPosition) { }
        public void OnChapter(PdfWriter writer, Document document, float paragraphPosition, Paragraph title) { }
        public void OnChapterEnd(PdfWriter writer, Document document, float paragraphPosition) { }
        public void OnSection(PdfWriter writer, Document document, float paragraphPosition, int depth, Paragraph title) { }
        public void OnSectionEnd(PdfWriter writer, Document document, float paragraphPosition) { }
        public void OnGenericTag(PdfWriter writer, Document document, Rectangle rect, String text) { }

    }
}
