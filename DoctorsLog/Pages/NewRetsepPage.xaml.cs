using DoctorsLog.Entities;
using DoctorsLog.Services.Persistence;
using DoctorsLog.Windows;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace DoctorsLog.Pages
{
    public partial class NewRetsepPage : Page
    {
        private readonly IAppDbContext db;
        private Patient patient;
        public NewRetsepPage(IAppDbContext db, Patient patient)
        {
            this.db = db;
            this.patient = patient;
            InitializeComponent();
            PopulateFontSizeComboBox();
            _ = InitializeComponentAsync();

            RichTextEditor.SelectionChanged += RichTextEditor_SelectionChanged;
            RichTextEditor.TextInput += RichTextEditor_TextInput;
            RichTextEditor.GotFocus += RichTextEditor_GotFocus;
        }

        private async Task InitializeComponentAsync()
        {
            RecipesComboBox.ItemsSource = await db.RecipeTemplates.Select(rt => rt.Type).ToListAsync();
        }

        private void PopulateFontSizeComboBox()
        {
            FontSizeComboBox.Items.Clear();
            double[] fontSizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            foreach (double size in fontSizes)
            {
                FontSizeComboBox.Items.Add(size);
            }
            FontSizeComboBox.SelectedItem = 20.0;

            RichTextEditor.Document.Blocks.Clear();
            var paragraph = new Paragraph();
            var run = new Run("") { FontSize = 20.0 };
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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Recipe recipe = new()
            {
                Type = (RecipesComboBox.SelectedValue as string)! ?? "янги муолажа",
                Content = new TextRange(RichTextEditor.Document.ContentStart, RichTextEditor.Document.ContentEnd).Text,
                CreatedAt = DateTime.Now,
                PatientId = patient.Id
            };
            await db.Recipes.AddAsync(recipe);
            if (0 < await db.SaveAsync())
            {
                new SuccessDialog().ShowDialog();
                await ToBackAsync();
            }
            else
                MessageBox.Show("Qandaydir xatolik yuz berdi, iltimos ishlab chiquvchiga murojaat qiling", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void BtnPechat_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDlg = new();
            if (printDlg.ShowDialog() == true)
            {
                // A4 formatga mos FlowDocument yaratamiz
                FlowDocument doc = new()
                {
                    // A4 qog'ozining standart o'lchamlarini hisobga olgan holda chegara qo'yamiz
                    PageWidth = 794,
                    PageHeight = 1122,
                    PagePadding = new Thickness(72), // Har tomondan 0.75 dyuymlik chegara
                    ColumnGap = 0
                };
                doc.ColumnWidth = doc.PageWidth - doc.PagePadding.Left - doc.PagePadding.Right;
                doc.FontFamily = new System.Windows.Media.FontFamily("Times New Roman");
                doc.FontSize = 20;

                // Rekvizitlar uchun ikki ustunli jadval yaratamiz
                Table rekvizitTable = new();
                rekvizitTable.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });
                rekvizitTable.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });
                rekvizitTable.Margin = new Thickness(0, 0, 0, 30);

                TableRowGroup rowGroup = new();
                TableRow row = new();

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
                    FontSize = 16,
                    TextAlignment = TextAlignment.Left,
                    LineHeight = 14
                };
                TableCell leftCell = new TableCell(leftParagraph);
                leftCell.BorderThickness = new Thickness(0);
                row.Cells.Add(leftCell);

                // O'ng hujayraga matnni joylashtiramiz
                Paragraph rightParagraph = new Paragraph(new Run(rightRekvizit))
                {
                    FontSize = 16,
                    TextAlignment = TextAlignment.Right,
                    LineHeight = 14
                };
                TableCell rightCell = new(rightParagraph)
                {
                    BorderThickness = new Thickness(0)
                };
                row.Cells.Add(rightCell);

                rowGroup.Rows.Add(row);
                rekvizitTable.RowGroups.Add(rowGroup);
                doc.Blocks.Add(rekvizitTable);

                // RichTextEditor'dagi matnni olish va hujjatga qo'shish
                FlowDocument tempDoc = new();
                TextRange sourceRange = new(RichTextEditor.Document.ContentStart, RichTextEditor.Document.ContentEnd);

                using (MemoryStream stream = new())
                {
                    sourceRange.Save(stream, DataFormats.Xaml);
                    stream.Position = 0;

                    TextRange destRange = new(tempDoc.ContentStart, tempDoc.ContentEnd);
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

        private async void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            await ToBackAsync();
        }

        private async Task ToBackAsync()
        {
            // HistoryPatientWindow ga murojaat qilish
            var parentWindow = (HistoryPatientWindow)Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.MainFrame.Content = null; // Page ni "yopish"
                parentWindow.MainFrame.Visibility = Visibility.Collapsed;

                parentWindow.HistoryDataGridBorder.Visibility = Visibility.Visible;
                parentWindow.btnNewRetsep.Visibility = Visibility.Visible;
                await parentWindow.InitializeRecipeFieldsAsync();
            }
        }


        private void RecipesComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (RecipesComboBox.SelectedItem is string selectedTemplate)
            {
                var template = db.RecipeTemplates.FirstOrDefault(rt => rt.Type == selectedTemplate);
                if (template != null)
                {
                    RichTextEditor.Document.Blocks.Clear();
                    var paragraph = new Paragraph();
                    var run = new Run(template.Content) { FontSize = 14.0 };
                    paragraph.Inlines.Add(run);
                    RichTextEditor.Document.Blocks.Add(paragraph);
                    RichTextEditor.CaretPosition = run.ElementEnd;
                }
            }
        }
    }
}