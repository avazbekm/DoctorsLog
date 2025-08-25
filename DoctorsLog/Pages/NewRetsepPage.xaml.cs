using DoctorsLog.Windows;
using System.IO; // Stream uchun
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace DoctorsLog.Pages
{
    public partial class NewRetsepPage : Page
    {
        public NewRetsepPage()
        {
            InitializeComponent();
            PopulateFontSizeComboBox();

            RichTextEditor.SelectionChanged += RichTextEditor_SelectionChanged;
            RichTextEditor.TextInput += RichTextEditor_TextInput;
            RichTextEditor.GotFocus += RichTextEditor_GotFocus;
        }

        private void PopulateFontSizeComboBox()
        {
            FontSizeComboBox.Items.Clear();
            double[] fontSizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            foreach (double size in fontSizes)
            {
                FontSizeComboBox.Items.Add(size);
            }
            FontSizeComboBox.SelectedItem = 14.0;

            RichTextEditor.Document.Blocks.Clear();
            var paragraph = new Paragraph();
            var run = new Run("") { FontSize = 14.0 };
            paragraph.Inlines.Add(run);
            RichTextEditor.Document.Blocks.Add(paragraph);
            RichTextEditor.CaretPosition = run.ElementStart;
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem is double selectedSize)
            {
                if (!RichTextEditor.Selection.IsEmpty)
                {
                    RichTextEditor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, selectedSize);
                }
                else
                {
                    var caretPosition = RichTextEditor.CaretPosition;
                    var paragraph = caretPosition.Paragraph ?? new Paragraph();
                    var newRun = new Run("") { FontSize = selectedSize };
                    paragraph.Inlines.Add(newRun);
                    RichTextEditor.CaretPosition = newRun.ElementStart;
                    if (!RichTextEditor.Document.Blocks.Contains(paragraph))
                    {
                        RichTextEditor.Document.Blocks.Add(paragraph);
                    }
                }
            }
        }

        private void RichTextEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var fontSizeProperty = RichTextEditor.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            if (fontSizeProperty != DependencyProperty.UnsetValue)
            {
                double fontSize = (double)fontSizeProperty;
                FontSizeComboBox.SelectedItem = FontSizeComboBox.Items.Contains(fontSize) ? fontSize : null;
            }
            else
            {
                var caretPosition = RichTextEditor.CaretPosition;
                var currentRun = caretPosition.GetAdjacentElement(LogicalDirection.Backward) as Run;
                if (currentRun != null && currentRun.FontSize > 0)
                {
                    FontSizeComboBox.SelectedItem = FontSizeComboBox.Items.Contains(currentRun.FontSize) ? currentRun.FontSize : null;
                }
                else
                {
                    if (FontSizeComboBox.SelectedItem != null)
                    {
                        var paragraph = caretPosition.Paragraph ?? new Paragraph();
                        var newRun = new Run("") { FontSize = (double)FontSizeComboBox.SelectedItem };
                        paragraph.Inlines.Add(newRun);
                        RichTextEditor.CaretPosition = newRun.ElementStart;
                        if (!RichTextEditor.Document.Blocks.Contains(paragraph))
                        {
                            RichTextEditor.Document.Blocks.Add(paragraph);
                        }
                    }
                }
            }
        }

        private void RichTextEditor_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem is double selectedSize)
            {
                var caretPosition = RichTextEditor.CaretPosition;
                var currentRun = caretPosition.GetAdjacentElement(LogicalDirection.Forward) as Run;
                var paragraph = caretPosition.Paragraph ?? new Paragraph();

                if (currentRun == null || currentRun.FontSize != selectedSize)
                {
                    var newRun = new Run("") { FontSize = selectedSize };
                    paragraph.Inlines.Add(newRun);
                    newRun.Text = e.Text;
                    RichTextEditor.CaretPosition = newRun.ElementEnd;
                    if (!RichTextEditor.Document.Blocks.Contains(paragraph))
                    {
                        RichTextEditor.Document.Blocks.Add(paragraph);
                    }
                }
                else
                {
                    caretPosition.InsertTextInRun(e.Text);
                    RichTextEditor.CaretPosition = caretPosition.GetPositionAtOffset(e.Text.Length);
                }
                e.Handled = true;
            }
        }

        private void RichTextEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem is double selectedSize)
            {
                var caretPosition = RichTextEditor.CaretPosition;
                var currentRun = caretPosition.GetAdjacentElement(LogicalDirection.Forward) as Run;
                var paragraph = caretPosition.Paragraph ?? new Paragraph();

                if (currentRun == null || currentRun.FontSize != selectedSize)
                {
                    var newRun = new Run("") { FontSize = selectedSize };
                    paragraph.Inlines.Add(newRun);
                    RichTextEditor.CaretPosition = newRun.ElementStart;
                    if (!RichTextEditor.Document.Blocks.Contains(paragraph))
                    {
                        RichTextEditor.Document.Blocks.Add(paragraph);
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string richTextContent = new TextRange(RichTextEditor.Document.ContentStart, RichTextEditor.Document.ContentEnd).Text;
            MessageBox.Show("Matn saqlandi:\n" + richTextContent);
        }

        private void btnPechat_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDlg = new PrintDialog();
            if (printDlg.ShowDialog() == true)
            {
                // A4 formatga mos FlowDocument yaratamiz
                FlowDocument doc = new FlowDocument();
                // A4 qog'ozining standart o'lchamlarini hisobga olgan holda chegara qo'yamiz
                doc.PageWidth = 794;
                doc.PageHeight = 1122;
                doc.PagePadding = new Thickness(72); // Har tomondan 0.75 dyuymlik chegara
                doc.ColumnGap = 0;
                doc.ColumnWidth = doc.PageWidth - doc.PagePadding.Left - doc.PagePadding.Right;
                doc.FontFamily = new System.Windows.Media.FontFamily("Times New Roman");
                doc.FontSize = 12;

                // Rekvizitlar uchun ikki ustunli jadval yaratamiz
                Table rekvizitTable = new Table();
                rekvizitTable.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });
                rekvizitTable.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });
                rekvizitTable.Margin = new Thickness(0, 0, 0, 30);

                TableRowGroup rowGroup = new TableRowGroup();
                TableRow row = new TableRow();

                // Chap rekvizit matni (rasmdagi lotincha matn)
                string leftRekvizit =
                    "Фаргона вилояти Бувайда тумани\n" +
                    "Янгикургон ШФЙ, Янгикургон ш-си,\n" +
                    "Янгиқўрғон куча 14-уй «Янгикургон\n" +
                    "малхам медик» х/к \n\n" +
                    "ЭНДОКРИНОЛОГИЯ булими \n" +
                    "Тел: +99890-628-4444";

                // O'ng rekvizit matni (rasmdagi kirilcha matn)
                string rightRekvizit =
                    "Farg'ona viloyati Buvayda tumani\n" +
                    "Yangig'o'rg'on SHF'Y, Yangig'o'rg'on sh-si,\n" +
                    "Yangig'o'rg'on ko'chasi 14-uy\n" +
                    "«Yangig'o'rg'on malxam medik» x/k\n\n" +
                    "ENDOKRINOLOGIYA bo'limi\n" +
                    "Tel: +99890-628-4444";

                // Chap hujayraga matnni joylashtiramiz
                Paragraph leftParagraph = new Paragraph(new Run(leftRekvizit))
                {
                    FontSize = 14,
                    TextAlignment = TextAlignment.Left,
                    LineHeight = 14
                };
                TableCell leftCell = new TableCell(leftParagraph);
                leftCell.BorderThickness = new Thickness(0);
                row.Cells.Add(leftCell);

                // O'ng hujayraga matnni joylashtiramiz
                Paragraph rightParagraph = new Paragraph(new Run(rightRekvizit))
                {
                    FontSize = 14,
                    TextAlignment = TextAlignment.Right,
                    LineHeight = 14
                };
                TableCell rightCell = new TableCell(rightParagraph);
                rightCell.BorderThickness = new Thickness(0);
                row.Cells.Add(rightCell);

                rowGroup.Rows.Add(row);
                rekvizitTable.RowGroups.Add(rowGroup);
                doc.Blocks.Add(rekvizitTable);

                // RichTextEditor'dagi matnni olish va hujjatga qo'shish
                FlowDocument tempDoc = new FlowDocument();
                TextRange sourceRange = new TextRange(RichTextEditor.Document.ContentStart, RichTextEditor.Document.ContentEnd);

                using (MemoryStream stream = new MemoryStream())
                {
                    sourceRange.Save(stream, DataFormats.Xaml);
                    stream.Position = 0;

                    TextRange destRange = new TextRange(tempDoc.ContentStart, tempDoc.ContentEnd);
                    destRange.Load(stream, DataFormats.Xaml);
                }

                foreach (Block block in tempDoc.Blocks.ToList())
                {
                    doc.Blocks.Add(block);
                }

                IDocumentPaginatorSource document = doc;
                printDlg.PrintDocument(document.DocumentPaginator, "Retsept");
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            HistoryPatientWindow historyPatientWindow = new HistoryPatientWindow();
            // Frame'ni yashirish
            historyPatientWindow.MainFrame.Visibility = Visibility.Collapsed;
            // DataGrid va "Yangi Retsept" tugmasini ko'rsatish
            historyPatientWindow.HistoryDataGridBorder.Visibility = Visibility.Visible;
            historyPatientWindow.btnNewRetsep.Visibility = Visibility.Visible;

            // Frame ichidagi kontentni tozalash
            historyPatientWindow.MainFrame.Content = null;
        }
    }
}