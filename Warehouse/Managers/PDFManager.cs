﻿using PdfSharp.Drawing;
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
        XFont _titleFont;
        XFont _titleTable;
        XFont _contentTableSmall;
        XFont _contentTableNormal;
        XFont _normalText;
        //Lines
        XPen _tablePen;
        public PDFManager()
        {
            //Assets
            //Fonts
            _titleFont = new XFont("Cambria", 14, XFontStyle.Bold);
            _titleTable = new XFont("Cambria", 10, XFontStyle.Bold);
            _contentTableSmall = new XFont("Calibri", 8, XFontStyle.Regular);
            _contentTableNormal = new XFont("Calibri", 10, XFontStyle.Regular);
            _normalText = new XFont("Calibri", 12, XFontStyle.Regular);
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
            graph.DrawString(order.Date_Of_Arrival == null ? string.Empty: ((DateTime)order.Date_Of_Arrival).ToString("dd-MM-yyy"), _contentTableNormal, XBrushes.Black, new XRect(44, 198, 0, 0), XStringFormats.TopLeft);


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

            double rowHeight = 60;
            double columnWidth = (rightMargin - 40) / 4;
            double lineYPos = rowHeight;
            double lineYPosSecondPage = rowHeight;
            double tableHeight = 0;
            double tableHeightSecondPage = 0;
            int orderPositionsCount = orderPositions.Count;
            if (orderPositionsCount > 6)
            {
                //Druga Tabelka
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
            string pdfFileName = order.Id+"_"+order.Name + DateTime.Now.ToString("dd-MM-yyyy_HHmmss") + ".pdf";
            if (uri.Contains("localhost"))
            {
                pdfFileName = @"C:\Users\dawid\Desktop\PDFWarehouse\" + pdfFileName;
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