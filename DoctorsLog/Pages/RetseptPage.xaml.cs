namespace DoctorsLog.Pages;

using DoctorsLog.Entities;
using DoctorsLog.Services.Persistence;
using DoctorsLog.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

public partial class RetseptPage : Page
{
    private readonly IAppDbContext db;
    private long? id = null;
    public RetseptPage(IAppDbContext db)
    {
        this.db = db;
        InitializeComponent();
        PopulateFontSizeComboBox();

        // RichTextBox hodisalari
        RichTextEditor.SelectionChanged += RichTextEditor_SelectionChanged;
        RichTextEditor.TextInput += RichTextEditor_TextInput;
        RichTextEditor.GotFocus += RichTextEditor_GotFocus;

        RecipesComboBox.ItemsSource = db.RecipeTemplates.Select(rt => rt.Type).ToList();
    }

    private void PopulateFontSizeComboBox()
    {
        // Standart shrift o‘lchamlarini ComboBoxga yuklash
        FontSizeComboBox.Items.Clear();
        double[] fontSizes = [8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72];
        foreach (double size in fontSizes)
        {
            FontSizeComboBox.Items.Add(size);
        }
        FontSizeComboBox.SelectedItem = 14.0; // Dastlabki qiymat 14

        // RichTextBox ning Document ob‘yektiga dastlabki shrift hajmini 14 o‘rnatish
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
            // Tanlangan matn bo‘lsa, faqat unga shrift hajmini qo‘llash
            if (!RichTextEditor.Selection.IsEmpty)
            {
                RichTextEditor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, selectedSize);
            }
            else
            {
                // Kursor joyiga yangi Run qo‘shish
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
            // Kursor joyidagi Run yoki Paragraphning shrift hajmini aniqlash
            var caretPosition = RichTextEditor.CaretPosition;
            if (caretPosition.GetAdjacentElement(LogicalDirection.Backward) is Run currentRun && currentRun.FontSize > 0)
                FontSizeComboBox.SelectedItem = FontSizeComboBox.Items.Contains(currentRun.FontSize) ? currentRun.FontSize : null;
            else
                // ComboBox da tanlangan hajmni (14) saqlash
                if (FontSizeComboBox.SelectedItem != null)
            {
                var paragraph = caretPosition.Paragraph ?? new Paragraph();
                var newRun = new Run("") { FontSize = (double)FontSizeComboBox.SelectedItem };
                paragraph.Inlines.Add(newRun);
                RichTextEditor.CaretPosition = newRun.ElementStart;
                if (!RichTextEditor.Document.Blocks.Contains(paragraph))
                    RichTextEditor.Document.Blocks.Add(paragraph);
            }
        }
    }

    private void RichTextEditor_TextInput(object sender, TextCompositionEventArgs e)
    {
        if (FontSizeComboBox.SelectedItem is double selectedSize)
        {
            var caretPosition = RichTextEditor.CaretPosition;
            var paragraph = caretPosition.Paragraph ?? new Paragraph();

            // Joriy Run ning shrift hajmi tanlangan hajmdan farq qilsa, yangi Run yaratish
            if (caretPosition.GetAdjacentElement(LogicalDirection.Forward) is not Run currentRun
                                                            || currentRun.FontSize != selectedSize)
            {
                var newRun = new Run("") { FontSize = selectedSize };
                paragraph.Inlines.Add(newRun);
                newRun.Text = e.Text; // Yangi matnni Run ga qo‘shish
                RichTextEditor.CaretPosition = newRun.ElementEnd;
                if (!RichTextEditor.Document.Blocks.Contains(paragraph))
                {
                    RichTextEditor.Document.Blocks.Add(paragraph);
                }
            }
            else
            {
                // Joriy Run ga matn qo‘shish
                caretPosition.InsertTextInRun(e.Text);
                RichTextEditor.CaretPosition = caretPosition.GetPositionAtOffset(e.Text.Length);
            }

            e.Handled = true; // Standart matn kiritishni to‘xtatish
        }
    }

    private void RichTextEditor_GotFocus(object sender, RoutedEventArgs e)
    {
        if (FontSizeComboBox.SelectedItem is double selectedSize)
        {
            var caretPosition = RichTextEditor.CaretPosition;
            var paragraph = caretPosition.Paragraph ?? new Paragraph();

            if (caretPosition.GetAdjacentElement(LogicalDirection.Forward) is not Run currentRun
                                                                || currentRun.FontSize != selectedSize)
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

    private void SaveButton_Click()
    {
        if (string.IsNullOrEmpty(RecipesComboBox.SelectedItem.ToString()))
        {
            System.Windows.MessageBox.Show("Retsep tanlanmagan!", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        string richTextContent = new TextRange(RichTextEditor.Document.ContentStart, RichTextEditor.Document.ContentEnd).Text;
        db.RecipeTemplates.First(rt => rt.Type == RecipesComboBox.SelectedItem).Content = richTextContent;
        new SuccessDialog().ShowDialog();
    }

    private void ActionButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(tbNameRecipe.Text))
            SaveButton_Click();
        else
            AddButton_Click();
    }

    private void AddButton_Click()
    {
        var recipeTemplate = db.RecipeTemplates.FirstOrDefault(r => r.Type == tbNameRecipe.Text);
        if (recipeTemplate is not null)
        {
            System.Windows.MessageBox.Show("Bunday retsept turi avval qo'shilgan!", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        recipeTemplate = new RecipeTemplate()
        {
            Type = tbNameRecipe.Text,
            Content = new TextRange(RichTextEditor.Document.ContentStart, RichTextEditor.Document.ContentEnd).Text
        };

        db.RecipeTemplates.Add(recipeTemplate);
        if (0 < db.SaveAsync().Result)
            new SuccessDialog().ShowDialog();
        else
        {
            System.Windows.MessageBox.Show("Xatolik yuz berdi!", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        RecipesComboBox.SelectedItem = tbNameRecipe;
        tbNameRecipe.Text = string.Empty;
    }

    private void RecipesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RecipesComboBox.SelectedItem is null)
        {
            if (string.IsNullOrEmpty(tbNameRecipe.Text))
                btnRecipeAction.Visibility = Visibility.Hidden;

            return;
        }

        btnRecipeAction.Content = "Saqlash";
        btnRecipeAction.Visibility = Visibility.Visible;
        tbNameRecipe.Text = string.Empty;

        var template = db.RecipeTemplates.FirstOrDefault(rt => rt.Type == RecipesComboBox.SelectedItem)!;
        id = template.Id;
        RichTextEditor.Document.Blocks.Clear();
        var paragraph = new Paragraph();
        var run = new Run(template.Content) { FontSize = 14.0 };
        paragraph.Inlines.Add(run);
        RichTextEditor.Document.Blocks.Add(paragraph);
        RichTextEditor.CaretPosition = run.ElementStart;
    }

    private void TbNameRecipe_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbNameRecipe.Text))
        {
            if (RecipesComboBox.SelectedItem is null)
                btnRecipeAction.Visibility = Visibility.Hidden;

            return;
        }

        btnRecipeAction.Content = "Yaratish";
        btnRecipeAction.Visibility = Visibility.Visible;
        RecipesComboBox.SelectedItem = null;
    }

    private void Separator_ColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color> e)
    {

    }
}