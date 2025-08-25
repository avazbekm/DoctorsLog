using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using System.IO;
using System.Windows.Xps;
using System.Printing;

namespace DoctorsLog.Windows
{
    /// <summary>
    /// Interaction logic for HistoryPatientWindow.xaml
    /// </summary>
    public partial class HistoryPatientWindow : Window
    {
        // Sample data classes (you should adapt these to your database)
        public class Patient
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public DateTime DateOfBirth { get; set; }
        }

        public class Receipt
        {
            public int Id { get; set; }
            public DateTime ReceiptDate { get; set; }
            public string ReceiptName { get; set; }
            public string ReceiptContent { get; set; } // Full text of the receipt
        }

        // Private fields to hold data
        private Patient _patient;
        private List<Receipt> _receipts;

        public HistoryPatientWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads patient data and their receipt history into the window.
        /// Call this method after creating an instance of the window.
        /// </summary>
        /// <param name="patient">The patient object to display.</param>
        public void LoadPatientData(Patient patient)
        {
            _patient = patient;

            // Bind patient information to the UI
            PatientNameTextBlock.Text = $"F.I.SH.: {_patient.FullName}";
            PatientDateOfBirthTextBlock.Text = $"Tug'ilgan sanasi: {_patient.DateOfBirth:dd.MM.yyyy}";

            // Load sample receipt data (you should get this from your DB)
            _receipts = GetSampleReceipts(patient.Id);
            HistoryDataGrid.ItemsSource = _receipts;
        }

        private List<Receipt> GetSampleReceipts(int patientId)
        {
            // This is where the code to fetch receipts from your database should go.
            // For now, it returns sample data.
            return new List<Receipt>
            {
                new Receipt { Id = 1, ReceiptDate = new DateTime(2025, 05, 10, 10, 30, 0), ReceiptName = "Tomoq og'rig'i retsepti", ReceiptContent = "Bu tomoq og'rig'i uchun yozilgan retsept. \n\nDorilar ro'yxati:\n- Paracetamol (2x1)\n- Strepsils (3x1)"},
                new Receipt { Id = 2, ReceiptDate = new DateTime(2025, 05, 12, 14, 0, 0), ReceiptName = "Bosh og'rig'i retsepti", ReceiptContent = "Bu bosh og'rig'i uchun yozilgan retsept. \n\nDorilar ro'yxati:\n- Ibuprofen (1x1)"},
                new Receipt { Id = 3, ReceiptDate = new DateTime(2025, 05, 15, 09, 45, 0), ReceiptName = "Sovuqqa qarshi retsept", ReceiptContent = "Bu sovuqqa qarshi yozilgan retsept. \n\nDorilar ro'yxati:\n- Askorbin kislotasi (1x1)\n- D vitamini (1x1)"}
            };
        }

        // Receipt name button click event
        private void ReceiptNameButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var receiptId = (int)button.Tag;

            var receipt = _receipts.Find(r => r.Id == receiptId);

            if (receipt != null)
            {
                string fullReceipt = $"Retsept nomi: {receipt.ReceiptName}\n" +
                                     $"Sanasi: {receipt.ReceiptDate:dd.MM.yyyy HH:mm}\n\n" +
                                     $"{receipt.ReceiptContent}";

                MessageBox.Show(fullReceipt, "Retsept to'liq matni", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Print button click event
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDlg = new PrintDialog();

            if (printDlg.ShowDialog() == true)
            {
                FlowDocument doc = new FlowDocument();
                doc.PagePadding = new Thickness(50);
                doc.FontSize = 12;

                Paragraph title = new Paragraph(new Run("Bemorning retseptlari tarixi"))
                {
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center
                };
                doc.Blocks.Add(title);

                Paragraph patientInfo = new Paragraph(new Run($"F.I.SH: {_patient.FullName}\nTug'ilgan sanasi: {_patient.DateOfBirth:dd.MM.yyyy}"))
                {
                    FontSize = 14,
                    Margin = new Thickness(0, 10, 0, 10)
                };
                doc.Blocks.Add(patientInfo);

                Table table = new Table();
                table.CellSpacing = 5;
                table.Columns.Add(new TableColumn() { Width = new GridLength(50) });
                table.Columns.Add(new TableColumn() { Width = new GridLength(200) });
                table.Columns.Add(new TableColumn() { Width = new GridLength(300) });

                table.RowGroups.Add(new TableRowGroup());
                TableRow headerRow = new TableRow();
                table.RowGroups[0].Rows.Add(headerRow);

                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("T/r"))) { FontWeight = FontWeights.Bold });
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Retsept sanasi"))) { FontWeight = FontWeights.Bold });
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Retsept nomi"))) { FontWeight = FontWeights.Bold });

                int counter = 1;
                foreach (var receipt in _receipts)
                {
                    TableRow row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(counter.ToString()))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(receipt.ReceiptDate.ToString("yyyy-MM-dd HH:mm")))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(receipt.ReceiptName))));
                    table.RowGroups[0].Rows.Add(row);
                    counter++;
                }

                doc.Blocks.Add(table);

                IDocumentPaginatorSource document = doc;
                printDlg.PrintDocument(document.DocumentPaginator, "Bemorning retseptlari");
            }
        }
    }
}