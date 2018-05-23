using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Warehouse.Models.Custom;
using Warehouse.Models.DAL;

namespace Warehouse.Managers
{
    public class PDFManager
    {
        XFont _titleFont;
        XFont _titleTable;
        XFont _contentTableSmall;
        XFont _contentTableNormal;
        XFont _normalText;
        XFont _smallText;
        XFont _titleTableSmall;
        //Lines
        XPen _tablePen;
        public PDFManager()
        {
            //Assets
            //Fonts
            _titleFont = new XFont("Cambria", 14, XFontStyle.Bold);
            _titleTable = new XFont("Cambria", 10, XFontStyle.Bold);
            _titleTableSmall = new XFont("Cambria", 7.4, XFontStyle.Bold);
            _contentTableSmall = new XFont("Calibri", 8, XFontStyle.Regular);
            _contentTableNormal = new XFont("Calibri", 10, XFontStyle.Regular);
            _normalText = new XFont("Calibri", 12, XFontStyle.Regular);
            _smallText = _contentTableNormal;
            //Lines
            _tablePen = new XPen(XColors.Black, 1);
        }
        public byte[] GenerateOrderPDF(Order order, List<Orders_Positions> orderPositions, string creator)
        {
            PdfDocument orderPdf = new PdfDocument();
            orderPdf.Info.Title = "OrderPDF";
            PdfPage firstPage = orderPdf.AddPage();
            XGraphics graph = XGraphics.FromPdfPage(firstPage);

            graph.DrawString("Zlecenia przyjęcia towaru/auftrag".ToUpper(), _titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);

            double rightMargin = firstPage.Width.Point - 40;
            //FirstTable
            graph.DrawLine(_tablePen, 40, 80, rightMargin, 80);
            graph.DrawLine(_tablePen, 40, 160, rightMargin, 160);
            graph.DrawLine(_tablePen, 40, 240, rightMargin, 240);
            graph.DrawLine(_tablePen, 40, 260, rightMargin, 260);

            graph.DrawLine(_tablePen, 40, 80, 40, 260);
            graph.DrawLine(_tablePen, 190, 80, 190, 260);
            graph.DrawLine(_tablePen, (rightMargin - 190) / 2 + 190, 80, (rightMargin - 190) / 2 + 190, 260);
            graph.DrawLine(_tablePen, rightMargin, 80, rightMargin, 260);

            double firstTableHeight = 180;

            //Pierwszy wiersz
            //Zleceniodwaca
            graph.DrawString("Zleceniodawca/Kunde", _contentTableSmall, XBrushes.Black, new XRect(44, 83, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Nazwa: " + order.Name, _contentTableSmall, XBrushes.Black, new XRect(44, 96, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres: " + order.Address, _contentTableSmall, XBrushes.Black, new XRect(44, 107, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("NIP: " + order.VAT_Id, _contentTableSmall, XBrushes.Black, new XRect(44, 118, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres e-mail: " + order.Email, _contentTableSmall, XBrushes.Black, new XRect(44, 129, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Telefon: ", _contentTableSmall, XBrushes.Black, new XRect(44, 140, 0, 0), XStringFormats.TopLeft);

            //Data zlecenia
            graph.DrawString("Data zlecenia/Datum der Bestellung:", _titleTable, XBrushes.Black, new XRect(194, 83, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Creation_Date.Date.ToString("dd-MM-yyyy"), _contentTableNormal, XBrushes.Black, new XRect(194, 118, 0, 0), XStringFormats.TopLeft);


            //Numer zlecenia
            graph.DrawString("Nr zlecenia/Bestellnummer:", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 83, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Order_Number, _contentTableNormal, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 118, 0, 0), XStringFormats.TopLeft);

            //Drugi wiersz
            //ETA
            graph.DrawString("ETA:", _titleTable, XBrushes.Black, new XRect(44, 163, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.ETA == null ? string.Empty: ((DateTime)order.ETA).ToString("dd-MM-yyy"), _contentTableNormal, XBrushes.Black, new XRect(44, 198, 0, 0), XStringFormats.TopLeft);


            //Numer kontenera
            graph.DrawString("Nr kontenera/ Containernummer:", _titleTable, XBrushes.Black, new XRect(194, 163, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Container_Id, _contentTableNormal, XBrushes.Black, new XRect(194, 198, 0, 0), XStringFormats.TopLeft);

            //Ilość pozycji
            graph.DrawString("Ilość pozycji/ Anzahl der Artikel:", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 163, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Num_of_Positions.ToString(), _contentTableNormal, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 198, 0, 0), XStringFormats.TopLeft);

            //Trzeci wiersz
            //AT
            graph.DrawString(order.ATB, _titleTable, XBrushes.Black, new XRect(44, 243, 0, 0), XStringFormats.TopLeft);

            //PIN
            graph.DrawString("PIN: "+order.Pickup_PIN, _titleTable, XBrushes.Black, new XRect(194, 243, 0, 0), XStringFormats.TopLeft);

            //Terminal
            graph.DrawString("Terminal: ", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 243, 0, 0), XStringFormats.TopLeft);


            //Druga tabelka
            double rowHeight = 60;
            double columnWidth = (rightMargin - 40) / 4;
            double lineYPos = rowHeight;
            double lineYPosSecondPage = rowHeight;
            double tableHeight = 0;
            double tableHeightSecondPage = 0;
            int orderPositionsCount = orderPositions.Count;
            if (orderPositionsCount > 6)
            {
                graph.DrawLine(_tablePen, 40, 280, rightMargin, 280);
                for (int i = 0; i < 8; i++)
                {
                    graph.DrawLine(_tablePen, 40, 280 + lineYPos, rightMargin, 280 + lineYPos);
                    graph.DrawString("Nr ATB", _titleTable, XBrushes.Black, new XRect(43, 280 + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(order.ATB, _contentTableNormal, XBrushes.Black, new XRect(43, 280 + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(string.Format("Poz.{0}/Pos.{0}", i+1), _titleTable, XBrushes.Black, new XRect(43 + columnWidth, 280 + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + columnWidth, 280 + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (2 * columnWidth), 280 + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("die Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (2 * columnWidth), 280 + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (2 * columnWidth), 280 + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (3 * columnWidth), 280 + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (3 * columnWidth), 280 + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (3 * columnWidth), 280 + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    lineYPos += rowHeight;
                    tableHeight += rowHeight;
                }

                //Linie pionowe
                graph.DrawLine(_tablePen, 40, 280, 40, 280 + tableHeight);
                graph.DrawLine(_tablePen, 40 + columnWidth, 280, 40 + columnWidth, 280 + tableHeight);
                graph.DrawLine(_tablePen, 40 + (2 * columnWidth), 280, 40 + (2 * columnWidth), 280 + tableHeight);
                graph.DrawLine(_tablePen, 40 + (3 * columnWidth), 280, 40 + (3 * columnWidth), 280 + tableHeight);
                graph.DrawLine(_tablePen, 40 + (4 * columnWidth), 280, 40 + (4 * columnWidth), 280 + tableHeight);


                PdfPage secondPage = orderPdf.AddPage();
                XGraphics graph2 = XGraphics.FromPdfPage(secondPage);
                graph2.DrawLine(_tablePen, 40, 40, rightMargin, 40);
                for (int i = 8; i < orderPositionsCount; i++)
                {
                    graph2.DrawLine(_tablePen, 40, 40 + lineYPosSecondPage, rightMargin, 40 + lineYPosSecondPage);
                    graph2.DrawString("Nr ATB", _titleTable, XBrushes.Black, new XRect(43, 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(order.ATB, _contentTableNormal, XBrushes.Black, new XRect(43, 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(string.Format("Poz.{0}/Pos.{0}", i + 1), _titleTable, XBrushes.Black, new XRect(43 + columnWidth, 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(orderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + columnWidth, 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (2*columnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("die Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (2 * columnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (2 * columnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (3*columnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (3 * columnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (3 * columnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    lineYPos += rowHeight;
                    tableHeight += rowHeight;
                    tableHeightSecondPage += rowHeight;
                    lineYPosSecondPage += rowHeight;
                }

                //Linie pionowe
                graph2.DrawLine(_tablePen, 40, 40, 40, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + columnWidth, 40, 40 + columnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + (2 * columnWidth), 40, 40 + (2 * columnWidth), 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + (3 * columnWidth), 40, 40 + (3 * columnWidth), 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + (4 * columnWidth), 40, 40 + (4 * columnWidth), 40 + tableHeightSecondPage);

                double endOfSecondTableSecondPage = 40 + tableHeightSecondPage;

                //Pod tabelką drugą
                graph2.DrawString("Zlecam przewóz i przyjęcie towaru wykazanego powyżej, do magazynu firmy/ Ich bestelle", _normalText, XBrushes.Black, new XRect(40, endOfSecondTableSecondPage + 20, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("den Transport und die Abnahme der oben genannten Waren im Lager des Unternehmens:", _normalText, XBrushes.Black, new XRect(40, endOfSecondTableSecondPage + 35, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("Direct Transport and Logistic Germany GMBH", _normalText, XBrushes.Black, new XRect(70, endOfSecondTableSecondPage + 65, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("Berzeliusstrasse 11", _normalText, XBrushes.Black, new XRect(70, endOfSecondTableSecondPage + 80, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("22113 Hamburg, Germany", _normalText, XBrushes.Black, new XRect(70, endOfSecondTableSecondPage + 95, 0, 0), XStringFormats.TopLeft);

                //Ramka na osobę zlecającą
                //Poziome
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 95 + 60, rightMargin - 20, endOfSecondTableSecondPage + 95 + 60);
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 95 + 80, rightMargin - 20, endOfSecondTableSecondPage + 95 + 80);
                //Pionowe
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 95 + 60, 60, endOfSecondTableSecondPage + 95 + 80);
                graph2.DrawLine(_tablePen, rightMargin - 20, endOfSecondTableSecondPage + 95 + 60, rightMargin - 20, endOfSecondTableSecondPage + 95 + 80);

                //Tekst w ramce (osoba zlecająca)
                graph2.DrawString(string.Format("Osoba zlecająca/ die bestellende Person: {0}", creator), _normalText, XBrushes.Black, new XRect(63, endOfSecondTableSecondPage + 95 + 63, 0, 0), XStringFormats.TopLeft);
            }
            else
            {
                //Druga Tabelka
                graph.DrawLine(_tablePen, 40, 280, rightMargin, 280);
                for (int i = 0; i < orderPositionsCount; i++)
                {
                    graph.DrawLine(_tablePen, 40, 280 + lineYPos, rightMargin, 280 + lineYPos);
                    graph.DrawString("Nr ATB", _titleTable, XBrushes.Black, new XRect(43, 280 + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(order.ATB, _contentTableNormal, XBrushes.Black, new XRect(43, 280 + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(string.Format("Poz.{0}/Pos.{0}", i + 1), _titleTable, XBrushes.Black, new XRect(43 + columnWidth, 280 + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + columnWidth, 280 + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (2 * columnWidth), 280 + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("die Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (2 * columnWidth), 280 + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (2 * columnWidth), 280 + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (3 * columnWidth), 280 + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (3 * columnWidth), 280 + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (3 * columnWidth), 280 + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    lineYPos += rowHeight;
                    tableHeight += rowHeight;
                }

                //Linie Pionowe
                graph.DrawLine(_tablePen, 40, 280, 40, 280 + tableHeight);
                graph.DrawLine(_tablePen, 40 + columnWidth, 280, 40 + columnWidth, 280 + tableHeight);
                graph.DrawLine(_tablePen, 40 + (2 * columnWidth), 280, 40 + (2 * columnWidth), 280 + tableHeight);
                graph.DrawLine(_tablePen, 40 + (3 * columnWidth), 280, 40 + (3 * columnWidth), 280 + tableHeight);
                graph.DrawLine(_tablePen, 40 + (4 * columnWidth), 280, 40 + (4 * columnWidth), 280 + tableHeight);

                double endOfSecondTable = 280 + tableHeight;

                //Pod tabelką drugą
                graph.DrawString("Zlecam przewóz i przyjęcie towaru wykazanego powyżej, do magazynu firmy/ Ich bestelle", _normalText, XBrushes.Black, new XRect(40, endOfSecondTable + 20, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("den Transport und die Abnahme der oben genannten Waren im Lager des Unternehmens:", _normalText, XBrushes.Black, new XRect(40, endOfSecondTable + 35, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("Direct Transport and Logistic Germany GMBH", _normalText, XBrushes.Black, new XRect(70, endOfSecondTable + 65, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("Berzeliusstrasse 11", _normalText, XBrushes.Black, new XRect(70, endOfSecondTable + 80, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("22113 Hamburg, Germany", _normalText, XBrushes.Black, new XRect(70, endOfSecondTable + 95, 0, 0), XStringFormats.TopLeft);


                //Ramka na osobę zlecającą
                //Poziome
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 95 + 60, rightMargin - 20, endOfSecondTable + 95 + 60);
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 95 + 80, rightMargin - 20, endOfSecondTable + 95 + 80);
                //Pionowe
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 95 + 60, 60, endOfSecondTable + 95 + 80);
                graph.DrawLine(_tablePen, rightMargin - 20, endOfSecondTable + 95 + 60, rightMargin - 20, endOfSecondTable + 95 + 80);

                //Tekst w ramce (osoba zlecająca)
                graph.DrawString(string.Format("Osoba zlecająca/ die bestellende Person: {0}", creator), _normalText, XBrushes.Black, new XRect(63, endOfSecondTable + 95 + 63, 0, 0), XStringFormats.TopLeft);
            }

            string uri = HttpContext.Current.Request.Url.AbsoluteUri;
            string pdfFileName = "Order_"+order.Id+"_"+order.Name+"_" + DateTime.Now.ToString("dd-MM-yyyy_HHmmss") + ".pdf";
            if (uri.Contains("localhost"))
            {
                pdfFileName = @"C:\Users\dawid\Desktop\PDFWarehouse\" + pdfFileName;
                orderPdf.Save(pdfFileName);
            }

            MemoryStream stream = new MemoryStream();
            orderPdf.Save(stream, false);
            byte[] result = stream.ToArray();

            return result;
        }

        public byte[] GenerateDeliveryPDF(Delivery delivery, Order order, List<Orders_Positions> orderPositions, string creator)
        {
            PdfDocument orderPdf = new PdfDocument();
            orderPdf.Info.Title = "DeliveryPDF";
            PdfPage firstPage = orderPdf.AddPage();
            XGraphics graph = XGraphics.FromPdfPage(firstPage);

            graph.DrawString("PRZYJĘCIE TOWARU/ WARENEINGANG".ToUpper(), _titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);

            double rightMargin = firstPage.Width.Point - 40;
            double firstTableRowHeight = 80;
            //FirstTable
            graph.DrawLine(_tablePen, 40, 80, rightMargin, 80);
            graph.DrawLine(_tablePen, 40, 80 + firstTableRowHeight, rightMargin, 80 + firstTableRowHeight);
            graph.DrawLine(_tablePen, 40, 80 + 2 * firstTableRowHeight, rightMargin, 80 + 2 * firstTableRowHeight);
            graph.DrawLine(_tablePen, 40, 80 + 3 * firstTableRowHeight, rightMargin, 80 + 3 * firstTableRowHeight);

            double firstTableEnd = 80 + 3 * firstTableRowHeight;

            graph.DrawLine(_tablePen, 40, 80, 40, firstTableEnd);
            graph.DrawLine(_tablePen, 190, 80, 190, firstTableEnd);
            graph.DrawLine(_tablePen, (rightMargin - 190) / 2 + 190, 80, (rightMargin - 190) / 2 + 190, firstTableEnd);
            graph.DrawLine(_tablePen, rightMargin, 80, rightMargin, firstTableEnd);


            //Pierwszy wiersz
            //Zleceniodwaca
            graph.DrawString("Zleceniodawca/Kunde", _titleTable, XBrushes.Black, new XRect(44, 80+3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Nazwa: " + order.Name, _contentTableSmall, XBrushes.Black, new XRect(44, 80+14, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres: " + order.Address, _contentTableSmall, XBrushes.Black, new XRect(44, 80+25, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("NIP: " + order.VAT_Id, _contentTableSmall, XBrushes.Black, new XRect(44, 80+36, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres e-mail: " + order.Email, _contentTableSmall, XBrushes.Black, new XRect(44, 80+47, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Telefon: ", _contentTableSmall, XBrushes.Black, new XRect(44, 80+58, 0, 0), XStringFormats.TopLeft);

            //Data zlecenia
            graph.DrawString("Data zlecenia/Datum der Bestellung:", _titleTable, XBrushes.Black, new XRect(194, 80+3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Creation_Date.Date.ToString("dd-MM-yyyy"), _contentTableNormal, XBrushes.Black, new XRect(194, 80+3+25, 0, 0), XStringFormats.TopLeft);


            //Numer zlecenia
            graph.DrawString("Numer przyjęcia/Annahmenummer:", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80+3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(delivery.Delivery_Number, _contentTableNormal, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80+3+25, 0, 0), XStringFormats.TopLeft);

            //Drugi wiersz
            //ETA
            graph.DrawString("ETA:", _titleTable, XBrushes.Black, new XRect(44, 80 + firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.ETA == null ? string.Empty : ((DateTime)order.ETA).ToString("dd-MM-yyy"), _contentTableNormal, XBrushes.Black, new XRect(44, 80 + firstTableRowHeight+3+25, 0, 0), XStringFormats.TopLeft);


            //Numer kontenera
            graph.DrawString("Nr kontenera/ Containernummer:", _titleTable, XBrushes.Black, new XRect(194, 80 + firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Container_Id, _contentTableNormal, XBrushes.Black, new XRect(194, 80 + firstTableRowHeight + 3+25, 0, 0), XStringFormats.TopLeft);

            //Ilość pozycji
            graph.DrawString("Nr ATB/ATB-Nummer:", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80 + firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.ATB, _contentTableNormal, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80 + firstTableRowHeight + 3+25, 0, 0), XStringFormats.TopLeft);

            //Trzeci wiersz
            //ATB
            graph.DrawString("Data zlecenia/", _titleTable, XBrushes.Black, new XRect(44, 80 + 2* firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Datum der Bestellung:", _titleTable, XBrushes.Black, new XRect(44, 80 + 2 * firstTableRowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Creation_Date.ToString("dd-MM-yyyy"), _contentTableNormal, XBrushes.Black, new XRect(44, 80 + 2 * firstTableRowHeight + 3+25, 0, 0), XStringFormats.TopLeft);

            //PIN
            graph.DrawString("Nr zlecenia/Bestellnummer:", _titleTable, XBrushes.Black, new XRect(194, 80 + 2 * firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Order_Number, _contentTableNormal, XBrushes.Black, new XRect(194, 80 + 2 * firstTableRowHeight + 3 + 25, 0, 0), XStringFormats.TopLeft);

            //Terminal
            graph.DrawString("Nr rej. samochodu/Autokennzeichen:", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80 + 2 * firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("brak danych", _contentTableNormal, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80 + 2 * firstTableRowHeight + 3 + 25, 0, 0), XStringFormats.TopLeft);


            //Druga tabelka
            //Wiersz tytułowy
            double titleRowHeight = 30;
            graph.DrawLine(_tablePen, 190, firstTableEnd + 20, rightMargin, firstTableEnd + 20);
            graph.DrawLine(_tablePen, 190, firstTableEnd + 20 + titleRowHeight, rightMargin, firstTableEnd + 20 + titleRowHeight);

            graph.DrawLine(_tablePen, 190, firstTableEnd + 20, 190, firstTableEnd + 20 + titleRowHeight);
            graph.DrawLine(_tablePen, (rightMargin - 190) / 2 + 190, firstTableEnd + 20, (rightMargin - 190) / 2 + 190, firstTableEnd + 20 + titleRowHeight);
            graph.DrawLine(_tablePen, rightMargin, firstTableEnd + 20, rightMargin, firstTableEnd + 20 + titleRowHeight);

            graph.DrawString("Ilość wg zlecenia/", _titleTable, XBrushes.Black, new XRect(193, firstTableEnd + 20 + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Menge gemäß der Bestellung", _titleTable, XBrushes.Black, new XRect(193, firstTableEnd + 20 + 3 + 10, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Przyjęto na magazyn/", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 190 + 3, firstTableEnd + 20 + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("An das Lager übernommen", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 190 + 3, firstTableEnd + 20 + 3 + 10, 0, 0), XStringFormats.TopLeft);
            int orderPositionsCount = order.Num_of_Positions;
            double rowHeight = 60;
            int countOfFitIn = 7;
            if (orderPositionsCount < 7)
            {
                countOfFitIn = orderPositionsCount;
            }
            if (orderPositionsCount>17)
            {
                rowHeight = 50;
                countOfFitIn = 9;
            }
  
            double firstColumndWidth = 80;
            double secondColumnWidth = 150 - firstColumndWidth;
            double restColumnWidth = (rightMargin - (40+firstColumndWidth+secondColumnWidth)) / 4;
            double lineYPos = rowHeight;
            double lineYPosSecondPage = rowHeight;
            double tableHeight = 0;
            double tableHeightSecondPage = 0;
            double endOfTitleRow = firstTableEnd + 20 + titleRowHeight;

            graph.DrawLine(_tablePen, 40, endOfTitleRow, rightMargin, endOfTitleRow);
            
            if (orderPositionsCount > 5)
            {
                for (int i = 0; i < countOfFitIn; i++)
                {
                    graph.DrawLine(_tablePen, 40, endOfTitleRow + lineYPos, rightMargin, endOfTitleRow + lineYPos);
                    graph.DrawString("Nr ATB", _titleTable, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(order.ATB, _contentTableNormal, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(string.Format("Poz.{0}/Pos.{0}", i + 1), _titleTable, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowyName", _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);                
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Amount_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Weight_Gross_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    lineYPos += rowHeight;
                    tableHeight += rowHeight;
                }

                //Linie Pionowe
                graph.DrawLine(_tablePen, 40, endOfTitleRow, 40, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth, endOfTitleRow, 40 + firstColumndWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, endOfTitleRow + tableHeight);

                PdfPage secondPage = orderPdf.AddPage();
                XGraphics graph2 = XGraphics.FromPdfPage(secondPage);
                if (orderPositionsCount > countOfFitIn)
                {
                    graph2.DrawLine(_tablePen, 40, 40, rightMargin, 40);
                }

                for (int i = countOfFitIn; i < orderPositionsCount; i++)
                {
                    graph2.DrawLine(_tablePen, 40, 40 + lineYPosSecondPage, rightMargin, 40 + lineYPosSecondPage);
                    graph2.DrawString("Nr ATB", _titleTable, XBrushes.Black, new XRect(43, 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(order.ATB, _contentTableNormal, XBrushes.Black, new XRect(43, 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(string.Format("Poz.{0}/Pos.{0}", i + 1), _titleTable, XBrushes.Black, new XRect(43 + firstColumndWidth, 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(orderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString("testowyName", _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph2.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph2.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2*restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(orderPositions[i].Amount_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2*restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(orderPositions[i].Weight_Gross_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    lineYPos += rowHeight;
                    tableHeight += rowHeight;
                    tableHeightSecondPage += rowHeight;
                    lineYPosSecondPage += rowHeight;
                }

                //Linie Pionowe
                graph2.DrawLine(_tablePen, 40, 40, 40, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth, 40, 40 + firstColumndWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, 40 + tableHeightSecondPage);

                double endOfSecondTableSecondPage = 40 + tableHeightSecondPage;
                //Pod tabelką drugą
                graph2.DrawString("Potwierdzam przyjęcie towaru wykazanego powyżej, do magazynu firmy/Ich bestätige den", _normalText, XBrushes.Black, new XRect(40, endOfSecondTableSecondPage + 20, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("Eingang der oben genannten Waren im Lager des Unternehmens:", _normalText, XBrushes.Black, new XRect(40, endOfSecondTableSecondPage + 35, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("Direct Transport and Logistic Germany GMBH", _normalText, XBrushes.Black, new XRect(70, endOfSecondTableSecondPage + 65, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("Berzeliusstrasse 11", _normalText, XBrushes.Black, new XRect(70, endOfSecondTableSecondPage + 80, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("22113 Hamburg, Germany", _normalText, XBrushes.Black, new XRect(70, endOfSecondTableSecondPage + 95, 0, 0), XStringFormats.TopLeft);

                //Ramka na osobę zlecającą
                //Poziome
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 95 + 50, rightMargin - 20, endOfSecondTableSecondPage + 95 + 50);
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 95 + 70, rightMargin - 20, endOfSecondTableSecondPage + 95 + 70);
                //Pionowe
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 95 + 50, 60, endOfSecondTableSecondPage + 95 + 70);
                graph2.DrawLine(_tablePen, rightMargin - 20, endOfSecondTableSecondPage + 95 + 50, rightMargin - 20, endOfSecondTableSecondPage + 95 + 70);

                //Tekst w ramce (osoba zlecająca)
                graph2.DrawString(string.Format("Osoba przyjmująca/die empfangende Person: {0}", creator), _normalText, XBrushes.Black, new XRect(63, endOfSecondTableSecondPage + 95 + 53, 0, 0), XStringFormats.TopLeft);
            }
            else
            {
                for (int i = 0; i < orderPositionsCount; i++)
                {
                    graph.DrawLine(_tablePen, 40, endOfTitleRow + lineYPos, rightMargin, endOfTitleRow + lineYPos);
                    graph.DrawString("Nr ATB", _titleTable, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(order.ATB, _contentTableNormal, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(string.Format("Poz.{0}/Pos.{0}", i + 1), _titleTable, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString(orderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowyName", _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2*restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Amount_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2*restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Weight_Gross_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    lineYPos += rowHeight;
                    tableHeight += rowHeight;
                }

                //Linie Pionowe
                graph.DrawLine(_tablePen, 40, endOfTitleRow, 40, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth, endOfTitleRow, 40 + firstColumndWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, endOfTitleRow + tableHeight);

                double endOfSecondTable = endOfTitleRow + tableHeight;
                //Pod tabelką drugą
                graph.DrawString("Potwierdzam przyjęcie towaru wykazanego powyżej, do magazynu firmy/Ich bestätige den", _normalText, XBrushes.Black, new XRect(40, endOfSecondTable + 20, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("Eingang der oben genannten Waren im Lager des Unternehmens:", _normalText, XBrushes.Black, new XRect(40, endOfSecondTable + 35, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("Direct Transport and Logistic Germany GMBH", _normalText, XBrushes.Black, new XRect(70, endOfSecondTable + 65, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("Berzeliusstrasse 11", _normalText, XBrushes.Black, new XRect(70, endOfSecondTable + 80, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("22113 Hamburg, Germany", _normalText, XBrushes.Black, new XRect(70, endOfSecondTable + 95, 0, 0), XStringFormats.TopLeft);

                //Ramka na osobę zlecającą
                //Poziome
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 95 + 50, rightMargin - 20, endOfSecondTable + 95 + 50);
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 95 + 70, rightMargin - 20, endOfSecondTable + 95 + 70);
                //Pionowe
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 95 + 50, 60, endOfSecondTable + 95 + 70);
                graph.DrawLine(_tablePen, rightMargin - 20, endOfSecondTable + 95 + 50, rightMargin - 20, endOfSecondTable + 95 + 70);

                //Tekst w ramce (osoba zlecająca)
                graph.DrawString(string.Format("Osoba przyjmująca/die empfangende Person: {0}", creator), _normalText, XBrushes.Black, new XRect(63, endOfSecondTable + 95 + 53, 0, 0), XStringFormats.TopLeft);
            }
            string uri = HttpContext.Current.Request.Url.AbsoluteUri;
            string pdfFileName = "Delivery_"+delivery.Id + "_" + order.Name +"_"+ DateTime.Now.ToString("dd-MM-yyyy_HHmmss") + ".pdf";
            if (uri.Contains("localhost"))
            {
                pdfFileName = @"C:\Users\dawid\Desktop\PDFWarehouse\" + pdfFileName;
                orderPdf.Save(pdfFileName);
            }
            MemoryStream stream = new MemoryStream();
            orderPdf.Save(stream, false);
            byte[] result = stream.ToArray();

            return result;
        }

        public byte[] GenerateDifferenceDeliveryPDF(Delivery delivery, Order order, List<Orders_Positions> orderPositions, Committee committee)
        {
            PdfDocument orderPdf = new PdfDocument();
            orderPdf.Info.Title = "DeliveryDifferencePDF";
            PdfPage firstPage = orderPdf.AddPage();
            XGraphics graph = XGraphics.FromPdfPage(firstPage);

            graph.DrawString("PROTOKÓŁ ROZBIEŻNOŚCI/DISKREPANZBERICHT".ToUpper(), _titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);

            double rightMargin = firstPage.Width.Point - 40;
            double firstTableRowHeight = 75;
            //FirstTable
            graph.DrawLine(_tablePen, 40, 80, rightMargin, 80);
            graph.DrawLine(_tablePen, 40, 80 + firstTableRowHeight, rightMargin, 80 + firstTableRowHeight);
            graph.DrawLine(_tablePen, 40, 80 + 2 * firstTableRowHeight, rightMargin, 80 + 2 * firstTableRowHeight);
            graph.DrawLine(_tablePen, 40, 80 + 3 * firstTableRowHeight, rightMargin, 80 + 3 * firstTableRowHeight);

            double firstTableEnd = 80 + 3 * firstTableRowHeight;

            graph.DrawLine(_tablePen, 40, 80, 40, firstTableEnd);
            graph.DrawLine(_tablePen, 190, 80, 190, firstTableEnd);
            graph.DrawLine(_tablePen, (rightMargin - 190) / 2 + 190, 80, (rightMargin - 190) / 2 + 190, firstTableEnd);
            graph.DrawLine(_tablePen, rightMargin, 80, rightMargin, firstTableEnd);


            //Pierwszy wiersz
            //Zleceniodwaca
            graph.DrawString("Zleceniodawca/Kunde", _titleTable, XBrushes.Black, new XRect(44, 80 + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Nazwa: " + order.Name, _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 14, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres: " + order.Address, _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 25, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("NIP: " + order.VAT_Id, _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 36, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres e-mail: " + order.Email, _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 47, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Telefon: ", _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 58, 0, 0), XStringFormats.TopLeft);

            //Data protokołu rozbieżności
            graph.DrawString("Data protokołu rozbieżności/", _titleTable, XBrushes.Black, new XRect(194, 80 + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Datum des Diskrepanzprotokolls:", _titleTable, XBrushes.Black, new XRect(194, 80 + 3+10, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(DateTime.Now.ToString("dd-MM-yyyy"), _contentTableNormal, XBrushes.Black, new XRect(194, 80 + 3 + 25, 0, 0), XStringFormats.TopLeft);

            ////Data zlecenia
            //graph.DrawString("Data zlecenia/Datum der Bestellung:", _titleTable, XBrushes.Black, new XRect(194, 80 + 3, 0, 0), XStringFormats.TopLeft);
            //graph.DrawString(order.Creation_Date.Date.ToString("dd-MM-yyyy"), _contentTableNormal, XBrushes.Black, new XRect(194, 80 + 3 + 25, 0, 0), XStringFormats.TopLeft);

            //Numer zlecenia
            graph.DrawString("Numer protokołu rozbieżności/", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80 + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Die Protokollnummer der Diskrepanz", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80 + 3 + 10, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(delivery.Delivery_Number, _contentTableNormal, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80 + 3 + 25, 0, 0), XStringFormats.TopLeft);

            ////Numer zlecenia
            //graph.DrawString("Numer przyjęcia/Annahmenummer:", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80 + 3, 0, 0), XStringFormats.TopLeft);
            //graph.DrawString(delivery.Delivery_Number, _contentTableNormal, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80 + 3 + 25, 0, 0), XStringFormats.TopLeft);



            //Drugi wiersz
            //ETA
            graph.DrawString("ETA:", _titleTable, XBrushes.Black, new XRect(44, 80 + firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.ETA == null ? string.Empty : ((DateTime)order.ETA).ToString("dd-MM-yyy"), _contentTableNormal, XBrushes.Black, new XRect(44, 80 + firstTableRowHeight + 3 + 25, 0, 0), XStringFormats.TopLeft);


            //Numer kontenera
            graph.DrawString("Nr kontenera/ Containernummer:", _titleTable, XBrushes.Black, new XRect(194, 80 + firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Container_Id, _contentTableNormal, XBrushes.Black, new XRect(194, 80 + firstTableRowHeight + 3 + 25, 0, 0), XStringFormats.TopLeft);

            //ATB
            graph.DrawString("Nr ATB/ATB-Nummer:", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80 + firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.ATB, _contentTableNormal, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80 + firstTableRowHeight + 3 + 25, 0, 0), XStringFormats.TopLeft);

            //Trzeci wiersz
            //Data zlecenia
            graph.DrawString("Data zlecenia/", _titleTable, XBrushes.Black, new XRect(44, 80 + 2 * firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Datum der Bestellung:", _titleTable, XBrushes.Black, new XRect(44, 80 + 2 * firstTableRowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Creation_Date.ToString("dd-MM-yyyy"), _contentTableNormal, XBrushes.Black, new XRect(44, 80 + 2 * firstTableRowHeight + 3 + 25, 0, 0), XStringFormats.TopLeft);

            //Numer zlecenia
            graph.DrawString("Nr zlecenia/Bestellnummer:", _titleTable, XBrushes.Black, new XRect(194, 80 + 2 * firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(order.Order_Number, _contentTableNormal, XBrushes.Black, new XRect(194, 80 + 2 * firstTableRowHeight + 3 + 25, 0, 0), XStringFormats.TopLeft);

            //Nr rejestracykny samochodu
            graph.DrawString("Nr rej. samochodu/Autokennzeichen:", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80 + 2 * firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("brak danych", _contentTableNormal, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 194, 80 + 2 * firstTableRowHeight + 3 + 25, 0, 0), XStringFormats.TopLeft);


            //Druga tabelka
            //Wiersz tytułowy
            double titleRowHeight = 30;
            graph.DrawLine(_tablePen, 190, firstTableEnd + 20, rightMargin, firstTableEnd + 20);
            graph.DrawLine(_tablePen, 190, firstTableEnd + 20 + titleRowHeight, rightMargin, firstTableEnd + 20 + titleRowHeight);

            graph.DrawLine(_tablePen, 190, firstTableEnd + 20, 190, firstTableEnd + 20 + titleRowHeight);
            graph.DrawLine(_tablePen, (rightMargin - 190) / 2 + 190, firstTableEnd + 20, (rightMargin - 190) / 2 + 190, firstTableEnd + 20 + titleRowHeight);
            graph.DrawLine(_tablePen, rightMargin, firstTableEnd + 20, rightMargin, firstTableEnd + 20 + titleRowHeight);

            graph.DrawString("Ilość wg zlecenia/", _titleTable, XBrushes.Black, new XRect(193, firstTableEnd + 20 + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Menge gemäß der Bestellung", _titleTable, XBrushes.Black, new XRect(193, firstTableEnd + 20 + 3 + 10, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Przyjęto na magazyn/", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 190 + 3, firstTableEnd + 20 + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("An das Lager übernommen", _titleTable, XBrushes.Black, new XRect((rightMargin - 190) / 2 + 190 + 3, firstTableEnd + 20 + 3 + 10, 0, 0), XStringFormats.TopLeft);

            int orderPositionsCount = order.Num_of_Positions;
            //orderPositionsCount = 20;
            double rowHeight = 60;
            int countOfFitIn = 7;
            if (orderPositionsCount < 7)
            {
                countOfFitIn = orderPositionsCount;
            }
            if (orderPositionsCount > 16)
            {
                rowHeight = 50;
                countOfFitIn = 9;
            }

            double firstColumndWidth = 80;
            double secondColumnWidth = 150 - firstColumndWidth;
            double restColumnWidth = (rightMargin - (40 + firstColumndWidth + secondColumnWidth)) / 4;
            double lineYPos = rowHeight;
            double lineYPosSecondPage = rowHeight;
            double tableHeight = 0;
            double tableHeightSecondPage = 0;
            double endOfTitleRow = firstTableEnd + 20 + titleRowHeight;

            graph.DrawLine(_tablePen, 40, endOfTitleRow, rightMargin, endOfTitleRow);

            if (orderPositionsCount > 4)
            {
                for (int i = 0; i < countOfFitIn; i++)
                {
                    graph.DrawLine(_tablePen, 40, endOfTitleRow + lineYPos, rightMargin, endOfTitleRow + lineYPos);
                    graph.DrawString("Nr ATB", _titleTable, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(order.ATB, _contentTableNormal, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(string.Format("Poz.{0}/Pos.{0}", i + 1), _titleTable, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowyName", _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Amount_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Weight_Gross_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    lineYPos += rowHeight;
                    tableHeight += rowHeight;
                }

                //Linie Pionowe
                graph.DrawLine(_tablePen, 40, endOfTitleRow, 40, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth, endOfTitleRow, 40 + firstColumndWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, endOfTitleRow + tableHeight);

                PdfPage secondPage = orderPdf.AddPage();
                XGraphics graph2 = XGraphics.FromPdfPage(secondPage);
                if (orderPositionsCount > countOfFitIn)
                {
                    graph2.DrawLine(_tablePen, 40, 40, rightMargin, 40);
                }

                for (int i = countOfFitIn; i < orderPositionsCount; i++)
                {
                    graph2.DrawLine(_tablePen, 40, 40 + lineYPosSecondPage, rightMargin, 40 + lineYPosSecondPage);
                    graph2.DrawString("Nr ATB", _titleTable, XBrushes.Black, new XRect(43, 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(order.ATB, _contentTableNormal, XBrushes.Black, new XRect(43, 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(string.Format("Poz.{0}/Pos.{0}", i + 1), _titleTable, XBrushes.Black, new XRect(43 + firstColumndWidth, 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(orderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString("testowyName", _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph2.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph2.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2*restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString(orderPositions[i].Amount_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2*restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(orderPositions[i].Weight_Gross_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    lineYPos += rowHeight;
                    tableHeight += rowHeight;
                    tableHeightSecondPage += rowHeight;
                    lineYPosSecondPage += rowHeight;
                }

                //Linie Pionowe
                graph2.DrawLine(_tablePen, 40, 40, 40, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth, 40, 40 + firstColumndWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, 40 + tableHeightSecondPage);

                double endOfSecondTableSecondPage = 40 + tableHeightSecondPage;
                //Pod tabelką drugą
                graph2.DrawString("Komisja w składzie/Der Ausschuss besteht aus:", _normalText, XBrushes.Black, new XRect(40, endOfSecondTableSecondPage + 20, 0, 0), XStringFormats.TopLeft);
                //Pierwsza osoba
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 20 + 30, rightMargin - 20, endOfSecondTableSecondPage + 20 + 30);
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 20 + 50, rightMargin - 20, endOfSecondTableSecondPage + 20 + 50);

                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 20 + 30, 60, endOfSecondTableSecondPage + 20 + 50);
                graph2.DrawLine(_tablePen, rightMargin - 20, endOfSecondTableSecondPage + 20 + 30, rightMargin - 20, endOfSecondTableSecondPage + 20 + 50);
                graph2.DrawString(string.Format("Osoba przyjmująca 1/die empfangende Person 1: {0}", committee.FirstPersonName), _normalText, XBrushes.Black, new XRect(63, endOfSecondTableSecondPage + 20 + 33, 0, 0), XStringFormats.TopLeft);

                //Druga osoba
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 20 + 65, rightMargin - 20, endOfSecondTableSecondPage + 20 + 65);
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 20 + 85, rightMargin - 20, endOfSecondTableSecondPage + 20 + 85);

                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 20 + 65, 60, endOfSecondTableSecondPage + 20 + 85);
                graph2.DrawLine(_tablePen, rightMargin - 20, endOfSecondTableSecondPage + 20 + 65, rightMargin - 20, endOfSecondTableSecondPage + 20 + 85);
                graph2.DrawString(string.Format("Osoba przyjmująca 2/die empfangende Person 2: {0}", committee.SecondPersonName), _normalText, XBrushes.Black, new XRect(63, endOfSecondTableSecondPage + 20 + 68, 0, 0), XStringFormats.TopLeft);

                //Trzecia osoba
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 20 + 100, rightMargin - 20, endOfSecondTableSecondPage + 20 + 100);
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 20 + 120, rightMargin - 20, endOfSecondTableSecondPage + 20 + 120);

                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 20 + 100, 60, endOfSecondTableSecondPage + 20 + 120);
                graph2.DrawLine(_tablePen, rightMargin - 20, endOfSecondTableSecondPage + 20 + 100, rightMargin - 20, endOfSecondTableSecondPage + 20 + 120);
                graph2.DrawString(string.Format("Osoba przyjmująca 3/die empfangende Person 3: {0}", committee.ThirdPersonName), _normalText, XBrushes.Black, new XRect(63, endOfSecondTableSecondPage + 20 + 103, 0, 0), XStringFormats.TopLeft);

                graph2.DrawString(string.Format("Potwierdza rozbieżności w dostawie towaru na podstawie zlecenia nr {0}, do magazynu firmy/ Bestätigt ", delivery.Delivery_Number), _smallText, XBrushes.Black, new XRect(40, endOfSecondTableSecondPage + 20 + 135, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString(string.Format("Abweichungen bei der Lieferung von Waren anhand der Bestellnummer {0}, in das Lager des Unternehmens", delivery.Delivery_Number), _smallText, XBrushes.Black, new XRect(40, endOfSecondTableSecondPage + 20 + 147, 0, 0), XStringFormats.TopLeft);
                //graph2.DrawString("Eingang der oben genannten Waren im Lager des Unternehmens:", _normalText, XBrushes.Black, new XRect(40, endOfSecondTableSecondPage + 35, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("Direct Transport and Logistic Germany GMBH", _normalText, XBrushes.Black, new XRect(65, endOfSecondTableSecondPage + 20 + 147 + 20, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("Berzeliusstrasse 11", _normalText, XBrushes.Black, new XRect(65, endOfSecondTableSecondPage + 20 + 147 + 35, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("22113 Hamburg, Germany", _normalText, XBrushes.Black, new XRect(65, endOfSecondTableSecondPage + 20 + 147 + 50, 0, 0), XStringFormats.TopLeft);

            }
            else
            {
                for (int i = 0; i < orderPositionsCount; i++)
                {
                    graph.DrawLine(_tablePen, 40, endOfTitleRow + lineYPos, rightMargin, endOfTitleRow + lineYPos);
                    graph.DrawString("Nr ATB", _titleTable, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(order.ATB, _contentTableNormal, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(string.Format("Poz.{0}/Pos.{0}", i + 1), _titleTable, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString(orderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowyName", _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph.DrawString("Ilość opakowań/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der Pakete", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString(orderPositions[i].Amount.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2*restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Amount_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTable, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString(orderPositions[i].Weight_Gross.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2*restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(orderPositions[i].Weight_Gross_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    lineYPos += rowHeight;
                    tableHeight += rowHeight;
                }

                //Linie Pionowe
                graph.DrawLine(_tablePen, 40, endOfTitleRow, 40, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth, endOfTitleRow, 40 + firstColumndWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, endOfTitleRow + tableHeight);

                double endOfSecondTable = endOfTitleRow + tableHeight;

                //Pod tabelką drugą
                graph.DrawString("Komisja w składzie/Der Ausschuss besteht aus:", _normalText, XBrushes.Black, new XRect(40, endOfSecondTable + 20, 0, 0), XStringFormats.TopLeft);
                //Pierwsza osoba
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 20 + 30, rightMargin - 20, endOfSecondTable + 20 + 30);
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 20 + 50, rightMargin - 20, endOfSecondTable + 20 + 50);

                graph.DrawLine(_tablePen, 60, endOfSecondTable + 20 + 30, 60, endOfSecondTable + 20 + 50);
                graph.DrawLine(_tablePen, rightMargin - 20, endOfSecondTable + 20 + 30, rightMargin - 20, endOfSecondTable + 20 + 50);
                graph.DrawString(string.Format("Osoba przyjmująca 1/die empfangende Person 1: {0}", committee.FirstPersonName), _normalText, XBrushes.Black, new XRect(63, endOfSecondTable + 20 + 33, 0, 0), XStringFormats.TopLeft);

                //Druga osoba
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 20 + 65, rightMargin - 20, endOfSecondTable + 20 + 65);
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 20 + 85, rightMargin - 20, endOfSecondTable + 20 + 85);

                graph.DrawLine(_tablePen, 60, endOfSecondTable + 20 + 65, 60, endOfSecondTable + 20 + 85);
                graph.DrawLine(_tablePen, rightMargin - 20, endOfSecondTable + 20 + 65, rightMargin - 20, endOfSecondTable + 20 + 85);
                graph.DrawString(string.Format("Osoba przyjmująca 2/die empfangende Person 2: {0}", committee.SecondPersonName), _normalText, XBrushes.Black, new XRect(63, endOfSecondTable + 20 + 68, 0, 0), XStringFormats.TopLeft);

                //Trzecia osoba
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 20 + 100, rightMargin - 20, endOfSecondTable + 20 + 100);
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 20 + 120, rightMargin - 20, endOfSecondTable + 20 + 120);

                graph.DrawLine(_tablePen, 60, endOfSecondTable + 20 + 100, 60, endOfSecondTable + 20 + 120);
                graph.DrawLine(_tablePen, rightMargin - 20, endOfSecondTable + 20 + 100, rightMargin - 20, endOfSecondTable + 20 + 120);
                graph.DrawString(string.Format("Osoba przyjmująca 3/die empfangende Person 3: {0}", committee.ThirdPersonName), _normalText, XBrushes.Black, new XRect(63, endOfSecondTable + 20 + 103, 0, 0), XStringFormats.TopLeft);

                graph.DrawString(string.Format("Potwierdza rozbieżności w dostawie towaru na podstawie zlecenia nr {0}, do magazynu firmy/ Bestätigt ", delivery.Delivery_Number), _smallText, XBrushes.Black, new XRect(40, endOfSecondTable + 20 + 135, 0, 0), XStringFormats.TopLeft);
                graph.DrawString(string.Format("Abweichungen bei der Lieferung von Waren anhand der Bestellnummer {0}, in das Lager des Unternehmens", delivery.Delivery_Number), _smallText, XBrushes.Black, new XRect(40, endOfSecondTable + 20 + 147, 0, 0), XStringFormats.TopLeft);
                //graph2.DrawString("Eingang der oben genannten Waren im Lager des Unternehmens:", _normalText, XBrushes.Black, new XRect(40, endOfSecondTableSecondPage + 35, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("Direct Transport and Logistic Germany GMBH", _normalText, XBrushes.Black, new XRect(65, endOfSecondTable + 20 + 147 + 20, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("Berzeliusstrasse 11", _normalText, XBrushes.Black, new XRect(65, endOfSecondTable + 20 + 147 + 35, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("22113 Hamburg, Germany", _normalText, XBrushes.Black, new XRect(65, endOfSecondTable + 20 + 147 + 50, 0, 0), XStringFormats.TopLeft);

            }
            string uri = HttpContext.Current.Request.Url.AbsoluteUri;
            string pdfFileName = "DeliveryDifference_" + delivery.Id + "_" + order.Name + "_" + DateTime.Now.ToString("dd-MM-yyyy_HHmmss") + ".pdf";
            if (uri.Contains("localhost"))
            {
                pdfFileName = @"C:\Users\dawid\Desktop\PDFWarehouse\" + pdfFileName;
                orderPdf.Save(pdfFileName);
            }
            MemoryStream stream = new MemoryStream();
            orderPdf.Save(stream, false);
            byte[] result = stream.ToArray();

            return result;
        }

        public byte[] GenerateDispatchPDF(DispatchDetailsPDF dispatchInfo,string creator)
        {
            PdfDocument orderPdf = new PdfDocument();
            orderPdf.Info.Title = "DispatchPDF";
            PdfPage firstPage = orderPdf.AddPage();
            XGraphics graph = XGraphics.FromPdfPage(firstPage);

            graph.DrawString("WYDANIE TOWARU/AUSGABE VON WAREN".ToUpper(), _titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);
            double firstColumnWidth = 220;
            double rightMargin = firstPage.Width.Point - 40;
            double firstTableRowHeight = 75;
            //FirstTable
            graph.DrawLine(_tablePen, 40, 80, rightMargin, 80);
            graph.DrawLine(_tablePen, 40, 80 + firstTableRowHeight, rightMargin, 80 + firstTableRowHeight);
            graph.DrawLine(_tablePen, 40, 80 + 2 * firstTableRowHeight, rightMargin, 80 + 2 * firstTableRowHeight);
            graph.DrawLine(_tablePen, 40, 80 + 3 * firstTableRowHeight, (rightMargin - firstColumnWidth) / 2 + firstColumnWidth, 80 + 3 * firstTableRowHeight);

            double firstTableEnd = 80 + 3 * firstTableRowHeight;

            graph.DrawLine(_tablePen, 40, 80, 40, firstTableEnd);
            graph.DrawLine(_tablePen, firstColumnWidth, 80, firstColumnWidth, firstTableEnd);
            graph.DrawLine(_tablePen, (rightMargin - firstColumnWidth) / 2 + firstColumnWidth, 80, (rightMargin - firstColumnWidth) / 2 + firstColumnWidth, firstTableEnd);
            graph.DrawLine(_tablePen, rightMargin, 80, rightMargin, firstTableEnd-firstTableRowHeight);


            //Pierwszy wiersz
            //Zleceniodwaca
            graph.DrawString("Nadawca/Kunde", _titleTable, XBrushes.Black, new XRect(44, 80 + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Nazwa: Direct Transport and Logistic Germany GMBH", _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 14, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres: Berzeliusstrasse 11, 22113 Hamburg, Germany" , _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 25, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("NIP: " , _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 36, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres e-mail: ", _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 47, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Telefon: ", _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 58, 0, 0), XStringFormats.TopLeft);

            //Data wydania
            graph.DrawString("Data wydania/Ausgabedatum:", _titleTable, XBrushes.Black, new XRect(firstColumnWidth+4, 80 + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(dispatchInfo.Creation_Date, _contentTableNormal, XBrushes.Black, new XRect(firstColumnWidth+4, 80 + 3 + 25, 0, 0), XStringFormats.TopLeft);


            //Numer wydania
            graph.DrawString("Numer wydania/Ausgabenummer:", _titleTable, XBrushes.Black, new XRect((rightMargin - firstColumnWidth) / 2 + firstColumnWidth+4, 80 + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(dispatchInfo.Dispatch_Number, _contentTableNormal, XBrushes.Black, new XRect((rightMargin - firstColumnWidth) / 2 + firstColumnWidth+4, 80 + 3 + 25, 0, 0), XStringFormats.TopLeft);

            //Drugi wiersz
            //Odbiorca
            graph.DrawString("Odbiorca/Empfänger:", _titleTable, XBrushes.Black, new XRect(44, 80 + firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Nazwa: "+ dispatchInfo.Receiver.Receiver_Name, _contentTableSmall, XBrushes.Black, new XRect(44, 80 + firstTableRowHeight + 14, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres: "+ dispatchInfo.Receiver.Receiver_Address, _contentTableSmall, XBrushes.Black, new XRect(44, 80 + firstTableRowHeight + 25, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("NIP: "+ dispatchInfo.Receiver.Receiver_VAT_Id, _contentTableSmall, XBrushes.Black, new XRect(44, 80 + firstTableRowHeight + 36, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres e-mail: "+ dispatchInfo.Receiver.Receiver_Email, _contentTableSmall, XBrushes.Black, new XRect(44, 80 + firstTableRowHeight + 47, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Telefon: ", _contentTableSmall, XBrushes.Black, new XRect(44, 80 + firstTableRowHeight + 58, 0, 0), XStringFormats.TopLeft);


            //Miejsce Przeznaczenia
            graph.DrawString("Miejsce przeznaczenia/Ziel:", _titleTable, XBrushes.Black, new XRect(firstColumnWidth+4, 80 + firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(dispatchInfo.Destination, _contentTableNormal, XBrushes.Black, new XRect(firstColumnWidth+4, 80 + firstTableRowHeight + 3 + 25, 0, 0), XStringFormats.TopLeft);

            //Nr rej. samochodu
            graph.DrawString("Nr rej. samochodu/", _titleTable, XBrushes.Black, new XRect((rightMargin - firstColumnWidth) / 2 + firstColumnWidth+4, 80 + firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Autokennzeichen:", _titleTable, XBrushes.Black, new XRect((rightMargin - firstColumnWidth) / 2 + firstColumnWidth + 4, 80 + firstTableRowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
            graph.DrawString(dispatchInfo.Car_Id, _contentTableNormal, XBrushes.Black, new XRect((rightMargin - firstColumnWidth) / 2 + firstColumnWidth+4, 80 + firstTableRowHeight + 3 + 25, 0, 0), XStringFormats.TopLeft);

            //Trzeci wiersz
            //Dane przewoźnika
            graph.DrawString("Dane Przewoźnika/", _titleTable, XBrushes.Black, new XRect(44, 80 + 2 * firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Angaben zum Beförderer:", _titleTable, XBrushes.Black, new XRect(44, 80 + 2 * firstTableRowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Nazwa: " + dispatchInfo.Carrier.Carrier_Name, _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 2 * firstTableRowHeight + 11 + 13, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres: "+ dispatchInfo.Carrier.Carrier_Address, _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 2 * firstTableRowHeight + 11 + 23, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("NIP: "+ dispatchInfo.Carrier.Carrier_VAT_Id, _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 2 * firstTableRowHeight + 11 + 33, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Adres e-mail: "+ dispatchInfo.Carrier.Carrier_Email, _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 2 * firstTableRowHeight + 11 + 43, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Telefon: ", _contentTableSmall, XBrushes.Black, new XRect(44, 80 + 2 * firstTableRowHeight + 11 + 53, 0, 0), XStringFormats.TopLeft);


            //NR dokumentu celnego
            graph.DrawString("Nr dokumentu celnego/", _titleTable, XBrushes.Black, new XRect(firstColumnWidth+4, 80 + 2 * firstTableRowHeight + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Dokumentnummer des Zolls:", _titleTable, XBrushes.Black, new XRect(firstColumnWidth+4, 80 + 2 * firstTableRowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
            //graph.DrawString(order.Order_Number, _contentTableNormal, XBrushes.Black, new XRect(194, 80 + 2 * firstTableRowHeight + 3 + 25, 0, 0), XStringFormats.TopLeft);



            //Druga tabelka
            //Wiersz tytułowy
            double titleRowHeight = 30;
            graph.DrawLine(_tablePen, 190, firstTableEnd + 20, rightMargin, firstTableEnd + 20);
            graph.DrawLine(_tablePen, 190, firstTableEnd + 20 + titleRowHeight, rightMargin, firstTableEnd + 20 + titleRowHeight);

            graph.DrawLine(_tablePen, 190, firstTableEnd + 20, 190, firstTableEnd + 20 + titleRowHeight);
            graph.DrawLine(_tablePen, (rightMargin - 190) / 3 + 190, firstTableEnd + 20, (rightMargin - 190) / 3 + 190, firstTableEnd + 20 + titleRowHeight);
            graph.DrawLine(_tablePen, 2*((rightMargin - 190) / 3) + 190, firstTableEnd + 20, 2 * ((rightMargin - 190) / 3) + 190, firstTableEnd + 20 + titleRowHeight);
            graph.DrawLine(_tablePen, rightMargin, firstTableEnd + 20, rightMargin, firstTableEnd + 20 + titleRowHeight);

            graph.DrawString("Przyjęto na magazyn/", _titleTableSmall, XBrushes.Black, new XRect(193, firstTableEnd + 20 + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("An das Lager übernommen", _titleTableSmall, XBrushes.Black, new XRect(193, firstTableEnd + 20 + 3 + 10, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Stan magazynowy/", _titleTableSmall, XBrushes.Black, new XRect((rightMargin - 190) / 3 + 190 + 3, firstTableEnd + 20 + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Lagerstatus", _titleTableSmall, XBrushes.Black, new XRect((rightMargin - 190) / 3 + 190 + 3, firstTableEnd + 20 + 3 + 10, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("Ilość wydana/", _titleTableSmall, XBrushes.Black, new XRect(2*((rightMargin - 190) / 3) + 190 + 3, firstTableEnd + 20 + 3, 0, 0), XStringFormats.TopLeft);
            graph.DrawString("der ausgegebene Betrag", _titleTableSmall, XBrushes.Black, new XRect(2*((rightMargin - 190) / 3) + 190 + 3, firstTableEnd + 20 + 3 + 10, 0, 0), XStringFormats.TopLeft);
            //int orderPositionsCount = order.Num_of_Positions;
            int orderPositionsCount = dispatchInfo.ListOfOrderPositions.Count;
            double rowHeight = 60;
            int countOfFitIn = 7;
            if (orderPositionsCount < 7)
            {
                countOfFitIn = orderPositionsCount;
            }
            if (orderPositionsCount > 17)
            {
                rowHeight = 50;
                countOfFitIn = 9;
            }

            double firstColumndWidth = 80;
            double secondColumnWidth = 150 - firstColumndWidth;
            double restColumnWidth = (rightMargin - (40 + firstColumndWidth + secondColumnWidth)) / 6;
            double lineYPos = rowHeight;
            double lineYPosSecondPage = rowHeight;
            double tableHeight = 0;
            double tableHeightSecondPage = 0;
            double endOfTitleRow = firstTableEnd + 20 + titleRowHeight;

            graph.DrawLine(_tablePen, 40, endOfTitleRow, rightMargin, endOfTitleRow);

            if (orderPositionsCount > 5)
            {
                for (int i = 0; i < countOfFitIn; i++)
                {
                    graph.DrawLine(_tablePen, 40, endOfTitleRow + lineYPos, rightMargin, endOfTitleRow + lineYPos);
                    graph.DrawString("Nr ATB", _titleTableSmall, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("test", _contentTableNormal, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].ATB, _contentTableNormal, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(string.Format("Poz.{0}/Pos.{0}", i + 1), _titleTableSmall, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowyName", _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph.DrawString("Ilość opakowań/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Pakete", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 20, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Amount_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Weight_Gross_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph.DrawString("Ilość opakowań/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Pakete", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 20, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Amount_Before_Dispatch.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Weight_Before_Dispatch.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Ilość opakowań/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Pakete", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 20, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Amount_Dispatch.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 5 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 5 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 5 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Weight_Dispatch.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 5 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    lineYPos += rowHeight;
                    tableHeight += rowHeight;
                }

                //Linie Pionowe
                graph.DrawLine(_tablePen, 40, endOfTitleRow, 40, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth, endOfTitleRow, 40 + firstColumndWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 5 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 5 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 6 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 6 * restColumnWidth, endOfTitleRow + tableHeight);

                PdfPage secondPage = orderPdf.AddPage();
                XGraphics graph2 = XGraphics.FromPdfPage(secondPage);
                if (orderPositionsCount > countOfFitIn)
                {
                    graph2.DrawLine(_tablePen, 40, 40, rightMargin, 40);
                }

                for (int i = countOfFitIn; i < orderPositionsCount; i++)
                {
                    graph2.DrawLine(_tablePen, 40, 40 + lineYPosSecondPage, rightMargin, 40 + lineYPosSecondPage);
                    graph2.DrawString("Nr ATB", _titleTableSmall, XBrushes.Black, new XRect(43, 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString("test", _contentTableNormal, XBrushes.Black, new XRect(43, 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(dispatchInfo.ListOfOrderPositions[i].ATB, _contentTableNormal, XBrushes.Black, new XRect(43, 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    
                    graph2.DrawString(string.Format("Poz.{0}/Pos.{0}", i + 1), _titleTableSmall, XBrushes.Black, new XRect(43 + firstColumndWidth, 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(dispatchInfo.ListOfOrderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString("testowyName", _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph2.DrawString("Ilość opakowań/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Anzahl der", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Pakete", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 20, 0, 0), XStringFormats.TopLeft);
                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(dispatchInfo.ListOfOrderPositions[i].Amount_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Waga brutto/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Bruttogewicht", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                   
                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(dispatchInfo.ListOfOrderPositions[i].Weight_Gross_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph2.DrawString("Ilość opakowań/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Anzahl der", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Pakete", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 20, 0, 0), XStringFormats.TopLeft);

                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(dispatchInfo.ListOfOrderPositions[i].Amount_Before_Dispatch.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Waga brutto/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Bruttogewicht", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                  
                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(dispatchInfo.ListOfOrderPositions[i].Weight_Before_Dispatch.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Ilość opakowań/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Anzahl der", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Pakete", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 20, 0, 0), XStringFormats.TopLeft);

                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(dispatchInfo.ListOfOrderPositions[i].Amount_Dispatch.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Waga brutto/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 5 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString("Bruttogewicht", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 5 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);

                    //graph2.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 5 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph2.DrawString(dispatchInfo.ListOfOrderPositions[i].Weight_Dispatch.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 5 * restColumnWidth), 40 + lineYPosSecondPage - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                
                    lineYPos += rowHeight;
                    tableHeight += rowHeight;
                    tableHeightSecondPage += rowHeight;
                    lineYPosSecondPage += rowHeight;
                }

                //Linie Pionowe
                graph2.DrawLine(_tablePen, 40, 40, 40, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth, 40, 40 + firstColumndWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 5 * restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + 5 * restColumnWidth, 40 + tableHeightSecondPage);
                graph2.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 6 * restColumnWidth, 40, 40 + firstColumndWidth + secondColumnWidth + 6 * restColumnWidth, 40 + tableHeightSecondPage);

                double endOfSecondTableSecondPage = 40 + tableHeightSecondPage;
                //Pod tabelką drugą
                graph2.DrawString("Potwierdzam wydanie towaru wykazanego powyżej, z magazynu firmy/Ich bestätige den", _normalText, XBrushes.Black, new XRect(40, endOfSecondTableSecondPage + 20, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("Eingang der oben genannten Waren im Lager des Unternehmens:", _normalText, XBrushes.Black, new XRect(40, endOfSecondTableSecondPage + 35, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("Direct Transport and Logistic Germany GMBH", _normalText, XBrushes.Black, new XRect(70, endOfSecondTableSecondPage + 65, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("Berzeliusstrasse 11", _normalText, XBrushes.Black, new XRect(70, endOfSecondTableSecondPage + 80, 0, 0), XStringFormats.TopLeft);
                graph2.DrawString("22113 Hamburg, Germany", _normalText, XBrushes.Black, new XRect(70, endOfSecondTableSecondPage + 95, 0, 0), XStringFormats.TopLeft);

                //Ramka na osobę zlecającą
                //Poziome
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 95 + 50, rightMargin - 20, endOfSecondTableSecondPage + 95 + 50);
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 95 + 70, rightMargin - 20, endOfSecondTableSecondPage + 95 + 70);
                //Pionowe
                graph2.DrawLine(_tablePen, 60, endOfSecondTableSecondPage + 95 + 50, 60, endOfSecondTableSecondPage + 95 + 70);
                graph2.DrawLine(_tablePen, rightMargin - 20, endOfSecondTableSecondPage + 95 + 50, rightMargin - 20, endOfSecondTableSecondPage + 95 + 70);

                //Tekst w ramce (osoba zlecająca)
                graph2.DrawString(string.Format("Osoba wydająca/ausstellende Person: {0}", creator), _normalText, XBrushes.Black, new XRect(63, endOfSecondTableSecondPage + 95 + 53, 0, 0), XStringFormats.TopLeft);
            }
            else
            {
                for (int i = 0; i < orderPositionsCount; i++)
                {
                    graph.DrawLine(_tablePen, 40, endOfTitleRow + lineYPos, rightMargin, endOfTitleRow + lineYPos);
                    graph.DrawString("Nr ATB", _titleTableSmall, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("test", _contentTableNormal, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].ATB, _contentTableNormal, XBrushes.Black, new XRect(43, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    
                    graph.DrawString(string.Format("Poz.{0}/Pos.{0}", i + 1), _titleTableSmall, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                  
                    //graph.DrawString("testowyName", _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Name, _contentTableNormal, XBrushes.Black, new XRect(43 + firstColumndWidth, endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph.DrawString("Ilość opakowań/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Pakete", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 20, 0, 0), XStringFormats.TopLeft);
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Amount_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                 
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Weight_Gross_Received.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph.DrawString("Ilość opakowań/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Pakete", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 20, 0, 0), XStringFormats.TopLeft);

                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Amount_Before_Dispatch.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 2 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                   
                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Weight_Before_Dispatch.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 3 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);

                    graph.DrawString("Ilość opakowań/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Anzahl der", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Pakete", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 20, 0, 0), XStringFormats.TopLeft);

                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Amount_Dispatch.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 4 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Waga brutto/", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 5 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString("Bruttogewicht", _titleTableSmall, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 5 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 10, 0, 0), XStringFormats.TopLeft);

                    //graph.DrawString("testowe", _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 5 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    graph.DrawString(dispatchInfo.ListOfOrderPositions[i].Weight_Dispatch.ToString(), _contentTableNormal, XBrushes.Black, new XRect(43 + (firstColumndWidth + secondColumnWidth + 5 * restColumnWidth), endOfTitleRow + lineYPos - rowHeight + 3 + 30, 0, 0), XStringFormats.TopLeft);
                    lineYPos += rowHeight;
                    tableHeight += rowHeight;
                }

                //Linie Pionowe
                graph.DrawLine(_tablePen, 40, endOfTitleRow, 40, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth, endOfTitleRow, 40 + firstColumndWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 2 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 3 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 4 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 5 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 5 * restColumnWidth, endOfTitleRow + tableHeight);
                graph.DrawLine(_tablePen, 40 + firstColumndWidth + secondColumnWidth + 6 * restColumnWidth, endOfTitleRow, 40 + firstColumndWidth + secondColumnWidth + 6 * restColumnWidth, endOfTitleRow + tableHeight);

                double endOfSecondTable = endOfTitleRow + tableHeight;
                //Pod tabelką drugą
                graph.DrawString("Potwierdzam wydanie towaru wykazanego powyżej, z magazynu firmy/Ich bestätige den", _normalText, XBrushes.Black, new XRect(40, endOfSecondTable + 20, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("Eingang der oben genannten Waren im Lager des Unternehmens:", _normalText, XBrushes.Black, new XRect(40, endOfSecondTable + 35, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("Direct Transport and Logistic Germany GMBH", _normalText, XBrushes.Black, new XRect(70, endOfSecondTable + 65, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("Berzeliusstrasse 11", _normalText, XBrushes.Black, new XRect(70, endOfSecondTable + 80, 0, 0), XStringFormats.TopLeft);
                graph.DrawString("22113 Hamburg, Germany", _normalText, XBrushes.Black, new XRect(70, endOfSecondTable + 95, 0, 0), XStringFormats.TopLeft);

                //Ramka na osobę zlecającą
                //Poziome
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 95 + 50, rightMargin - 20, endOfSecondTable + 95 + 50);
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 95 + 70, rightMargin - 20, endOfSecondTable + 95 + 70);
                //Pionowe
                graph.DrawLine(_tablePen, 60, endOfSecondTable + 95 + 50, 60, endOfSecondTable + 95 + 70);
                graph.DrawLine(_tablePen, rightMargin - 20, endOfSecondTable + 95 + 50, rightMargin - 20, endOfSecondTable + 95 + 70);

                //Tekst w ramce (osoba zlecająca)
                graph.DrawString(string.Format("Osoba wydająca/ausstellende Person: {0}", creator), _normalText, XBrushes.Black, new XRect(63, endOfSecondTable + 95 + 53, 0, 0), XStringFormats.TopLeft);
            }
            string uri = HttpContext.Current.Request.Url.AbsoluteUri;
            string pdfFileName = "Dispatch_" + dispatchInfo.Id + "_" + dispatchInfo.Dispatch_Number.Replace("/", "_") + "_" + DateTime.Now.ToString("dd-MM-yyyy_HHmmss") + ".pdf";
            if (uri.Contains("localhost"))
            {
                pdfFileName = @"C:\Users\dawid\Desktop\PDFWarehouse\" + pdfFileName;
                orderPdf.Save(pdfFileName);
            }
            MemoryStream stream = new MemoryStream();
            orderPdf.Save(stream, false);
            byte[] result = stream.ToArray();

            return result;
        }

        public byte[] GenerateCMR()
        {
            try
            {
                PdfDocument PDFDoc = PdfSharp.Pdf.IO.PdfReader.Open(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\CMRPDF.pdf"), PdfDocumentOpenMode.Modify);
                PDFDoc.Info.Title = "CMRPDF";
                PdfPage firstPage = PDFDoc.Pages[0];
                XGraphics graph = XGraphics.FromPdfPage(firstPage);
                //graph.DrawString("WYDANIE TOWARU/AUSGABE VON WAREN".ToUpper(), _titleFont, XBrushes.Black, new XRect(firstPage.Width.Point / 2, 40, 0, 0), XStringFormats.TopCenter);
                //for (int Pg = 0; Pg < PDFDoc.Pages.Count; Pg++)
                //{
                //    orderPdf.AddPage(PDFDoc.Pages[Pg]);
                //}

                string uri = HttpContext.Current.Request.Url.AbsoluteUri;
                string pdfFileName = "CMR_" + "_" + "_" + DateTime.Now.ToString("dd-MM-yyyy_HHmmss") + ".pdf";
                if (uri.Contains("localhost"))
                {
                    pdfFileName = @"C:\Users\dawid\Desktop\PDFWarehouse\" + pdfFileName;
                    PDFDoc.Save(pdfFileName);
                }


                MemoryStream stream = new MemoryStream();
                PDFDoc.Save(stream, false);
                byte[] result = stream.ToArray();

                return result;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.ToString());
            }
           
        }
    }
}