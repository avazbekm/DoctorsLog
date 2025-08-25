using DoctorsLog.Pages; // NewRetsepPage'ni ishlatish uchun
using System.Windows;
using System.Windows.Controls;

namespace DoctorsLog.Windows
{
    /// <summary>
    /// Interaction logic for HistoryPatientWindow.xaml
    /// </summary>
    public partial class HistoryPatientWindow : Window
    {
        // Namuna ma'lumotlar sinflari (siz o'z ma'lumotlar bazangizga moslashingiz kerak)
        public class Patient
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public DateTime DateOfBirth { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
        }

        public class Receipt
        {
            public int Id { get; set; }
            public DateTime ReceiptDate { get; set; }
            public string ReceiptName { get; set; }
            public string ReceiptContent { get; set; } // Retseptning to'liq matni
        }

        private Patient _patient;
        private List<Receipt> _receipts;

        public HistoryPatientWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Bemor ma'lumotlarini va retseptlar tarixini oynaga yuklaydi.
        /// </summary>
        /// <param name="patient">Ko'rsatiladigan bemor obyekti.</param>
        public void LoadPatientData(Patient patient)
        {
            _patient = patient;

            // Bemor ma'lumotlarini UIga bog'lash
            tbName.Text = _patient.FullName.Split(' ')[0]; // Ismini ajratib olish
            tbLastName.Text = _patient.FullName.Split(' ')[1]; // Familiyasini ajratib olish
            tbBithday.Text = $"{_patient.DateOfBirth:dd.MM.yyyy}";
            tbAddress.Text = _patient.Address;
            tbPhone.Text = _patient.Phone;

            // Namuna retsept ma'lumotlarini yuklash (siz DBdan olishingiz kerak)
            _receipts = GetSampleReceipts(patient.Id);
            HistoryDataGrid.ItemsSource = _receipts;
        }

        private List<Receipt> GetSampleReceipts(int patientId)
        {
            // Bu yerda ma'lumotlar bazasidan retseptlarni olish kodi bo'lishi kerak.
            // Hozircha namunaviy ma'lumotlar qaytariladi.
            return new List<Receipt>
            {
                new Receipt { Id = 1, ReceiptDate = new DateTime(2025, 05, 10, 10, 30, 0), ReceiptName = "Tomoq og'rig'i retsepti", ReceiptContent = "Bu tomoq og'rig'i uchun yozilgan retsept. \n\nDorilar ro'yxati:\n- Paracetamol (2x1)\n- Strepsils (3x1)"},
                new Receipt { Id = 2, ReceiptDate = new DateTime(2025, 05, 12, 14, 0, 0), ReceiptName = "Bosh og'rig'i retsepti", ReceiptContent = "Bu bosh og'rig'i uchun yozilgan retsept. \n\nDorilar ro'yxati:\n- Ibuprofen (1x1)"},
                new Receipt { Id = 3, ReceiptDate = new DateTime(2025, 05, 15, 09, 45, 0), ReceiptName = "Sovuqqa qarshi retsept", ReceiptContent = "Bu sovuqqa qarshi yozilgan retsept. \n\nDorilar ro'yxati:\n- Askorbin kislotasi (1x1)\n- D vitamini (1x1)"}
            };
        }

        // Retsept nomini bosganda ishlaydigan hodisa
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

        // Yangi retsept qo'shish tugmasi bosilganda ishlaydigan hodisa
        private void btnNewRetsep_Click(object sender, RoutedEventArgs e)
        {
            // DataGridni yashirish
            HistoryDataGridBorder.Visibility = Visibility.Collapsed;
            btnNewRetsep.Visibility = Visibility.Hidden;

            // Frame'ni ko'rsatish
            MainFrame.Visibility = Visibility.Visible;

            // NewRetsepPage'ni yaratish va Frame'ga yuklash
            NewRetsepPage newRetsepPage = new NewRetsepPage();
            MainFrame.Content = newRetsepPage;
        }

        // Pechat tugmasi bosilganda ishlaydigan hodisa (sizning avvalgi kodingizdan olingan)
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            // Pechat logikasi...
        }
    }
}