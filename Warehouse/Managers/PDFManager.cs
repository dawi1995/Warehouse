using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Warehouse.Models.DAL;

namespace Warehouse.Managers
{
    public class PDFManager
    {     
        public static byte[] GenerateOrderPDF(Order order, List<Orders_Positions> orderPositions)
        {
            //Assets
            //Fonts
            XFont titleFont = new XFont("Cambria", 14, XFontStyle.Bold);
            XFont titleTable = new XFont("Cambria", 10, XFontStyle.Bold);
            XFont contentTableSmall = new XFont("Calibri", 8, XFontStyle.Regular);
            XFont contentTableNormal = new XFont("Calibri", 10, XFontStyle.Regular);
            //Lines
            XPen tablePen = new XPen(XColors.Black, 1);


            PdfDocument orderPdf = new PdfDocument();
            orderPdf.Info.Title = "OrderPDF";
            PdfPage firstPage = orderPdf.AddPage();
            XGraphics graph = XGraphics.FromPdfPage(firstPage);

            graph.DrawString("Zlecenia przyjęcia towaru/auftrag".ToUpper(), titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);

            double rightMargin = firstPage.Width.Point - 40;
            //FirstTable
            graph.DrawLine(tablePen, 40, 80, rightMargin, 80);
            graph.DrawLine(tablePen, 40, 160, rightMargin, 160);
            graph.DrawLine(tablePen, 40, 240, rightMargin, 240);
            graph.DrawLine(tablePen, 40, 260, rightMargin, 260);

            graph.DrawLine(tablePen, 40, 80, 40, 260);
            graph.DrawLine(tablePen, 190, 80, 190, 260);
            graph.DrawLine(tablePen, (rightMargin - 190) / 2 + 190, 80, (rightMargin - 190) / 2 + 190, 260);
            graph.DrawLine(tablePen, rightMargin, 80, rightMargin, 260);

            double firstTableHeight = 180;

            //Pierwszy wiersz
            //Zleceniodwaca
            graph.DrawString("Zleceniodawca/Kunde", titleTable, XBrushes.Black, new XRect(44, 83, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Nazwa: " + order.Name, contentTableSmall, XBrushes.Black, new XRect(44, 96, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres: " + order.Address, contentTableSmall, XBrushes.Black, new XRect(44, 107, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("NIP: " + order.VAT_Id, contentTableSmall, XBrushes.Black, new XRect(44, 118, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres e-mail: " + order.Email, contentTableSmall, XBrushes.Black, new XRect(44, 129, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Telefon: ", contentTableSmall, XBrushes.Black, new XRect(44, 140, 0, 0), XStringFormats.TopLeft);

            //Data zlecenia
            graph.DrawString("Data zlecenia/Datum der Bestellung:", titleTable, XBrushes.Black, new XRect(194, 83, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Creation_Date.Date.ToString("dd-MM-yyyy"), contentTableNormal, XBrushes.Black, new XRect(194, 118, 0, 0), XStringFormats.TopLeft);


            //Numer zlecenia
            graph.DrawString("Data zlecenia/Datum der Bestellung:", titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 83, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Order_Number, contentTableNormal, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 118, 0, 0), XStringFormats.TopLeft);

            //Drugi wiersz
            //ETA
            graph.DrawString("ETA:", titleTable, XBrushes.Black, new XRect(44, 163, 0, 0), XStringFormats.TopLeft);
            //graph.DrawString("Data zlecenia/Datum der Bestellung:", contentTableNormal, XBrushes.Black, new XRect(48, 198, 0, 0), XStringFormats.TopLeft);


            //Numer kontenera
            graph.DrawString("Nr kontenera/ Containernummer:", titleTable, XBrushes.Black, new XRect(194, 163, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Container_Id, contentTableNormal, XBrushes.Black, new XRect(194, 198, 0, 0), XStringFormats.TopLeft);

            //Ilość pozycji
            graph.DrawString("Ilość pozycji/ Anzahl der Artikel:", titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 163, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Num_of_Positions.ToString(), contentTableNormal, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 198, 0, 0), XStringFormats.TopLeft);

            //Trzeci wiersz
            //AT
            graph.DrawString(order.ATB, titleTable, XBrushes.Black, new XRect(44, 243, 0, 0), XStringFormats.TopLeft);

            //PIN
            graph.DrawString("PIN: "+order.Pickup_PIN, titleTable, XBrushes.Black, new XRect(194, 243, 0, 0), XStringFormats.TopLeft);

            //Terminal
            graph.DrawString("Terminal: ", titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 243, 0, 0), XStringFormats.TopLeft);


            //graph.DrawString("Zlecenia przyjęcia towaru/auftrag".ToUpper(), titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);
            //graph.DrawString("Zlecenia przyjęcia towaru/auftrag".ToUpper(), titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);
            //graph.DrawString("Zlecenia przyjęcia towaru/auftrag".ToUpper(), titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);
            //graph.DrawString("Zlecenia przyjęcia towaru/auftrag".ToUpper(), titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);
            //graph.DrawString("Zlecenia przyjęcia towaru/auftrag".ToUpper(), titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);
            //graph.DrawString("Zlecenia przyjęcia towaru/auftrag".ToUpper(), titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);
            //graph.DrawString("Zlecenia przyjęcia towaru/auftrag".ToUpper(), titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);
            //graph.DrawString("Zlecenia przyjęcia towaru/auftrag".ToUpper(), titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);




            string uri = HttpContext.Current.Request.Url.AbsoluteUri;
            string pdfFileName = order.Id+"_"+order.Name + DateTime.Now.ToString("dd-MM-yyyy_HHmmss") + ".pdf";
            if (uri.Contains("localhost"))
            {
                pdfFileName = @"C:\Users\dbrzeczek\Desktop\wakexls\" + pdfFileName;
            }

            orderPdf.Save(pdfFileName);
            MemoryStream fileMemoryStream = new MemoryStream(File.ReadAllBytes(pdfFileName));
            byte[] result = fileMemoryStream.ToArray();
            if (!uri.Contains("localhost"))
            {
                File.Delete(pdfFileName);
            }

            return result;
        }
    }
}