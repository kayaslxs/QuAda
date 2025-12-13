// Dosya Adı: FormFileCrypto.cs (Yeni Dosya)
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace QuAdaCryption
{
    public class FormFileCrypto : Form
    {
        private readonly CipherService _cipherService;
        private readonly bool _isEncryption;
        private Label lblDropZone = null!;
        private Button btnProcess = null!;
        private string _selectedFilePath = "";

        public FormFileCrypto(CipherService cipherService, bool isEncryption)
        {
            _cipherService = cipherService;
            _isEncryption = isEncryption;

            this.Text = isEncryption ? "📁 File Encryption" : "🔓 File Decryption (.KAYA)";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.AllowDrop = true;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 1. Sürükle-Bırak Alanı
            lblDropZone = new Label
            {
                Text = _isEncryption ? "Sürükle & Bırak (Dosya)\n(veya seçmek için tıkla)" : "Sürükle & Bırak (.KAYA Dosyası)\n(veya seçmek için tıkla)",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
                AllowDrop = true,
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
            };
            this.Controls.Add(lblDropZone);
            lblDropZone.DragEnter += LblDropZone_DragEnter;
            lblDropZone.DragDrop += LblDropZone_DragDrop;
            lblDropZone.Click += LblDropZone_Click;

            // 2. İşlem Butonu
            // Font hatası düzeltildi
            btnProcess = new Button
            {
                Text = _isEncryption ? "ŞİFRELE VE KAYDET" : "ŞİFRE ÇÖZ VE KAYDET",
                Dock = DockStyle.Bottom,
                Height = 40,
                Enabled = false,
                BackColor = _isEncryption ? Color.Indigo : Color.DarkSlateGray,
                ForeColor = Color.White,
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
            };
            this.Controls.Add(btnProcess);
            btnProcess.Click += BtnProcess_Click;

            UpdateDropZoneText();
        }

        private void LblDropZone_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data!.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void LblDropZone_DragDrop(object? sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data!.GetData(DataFormats.FileDrop)!;
            if (files.Length > 0)
            {
                SetFilePath(files[0]);
            }
        }

        private void LblDropZone_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = _isEncryption
                    ? "Tüm Dosyalar (*.*)|*.*"
                    : "KAYA Şifreli Dosyalar (*.KAYA)|*.KAYA";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    SetFilePath(openDialog.FileName);
                }
            }
        }

        private void SetFilePath(string path)
        {
            if (!_isEncryption && !path.EndsWith(".KAYA", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Şifre çözme modu, '.KAYA' uzantılı bir dosya gerektirir.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _selectedFilePath = path;
            UpdateDropZoneText();
            btnProcess.Enabled = true;
        }

        private void UpdateDropZoneText()
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                lblDropZone.Text = _isEncryption ? "Sürükle & Bırak (Dosya)\n(veya seçmek için tıkla)" : "Sürükle & Bırak (.KAYA Dosyası)\n(veya seçmek için tıkla)";
            }
            else
            {
                lblDropZone.Text = $"Seçilen Dosya:\n{Path.GetFileName(_selectedFilePath)}";
            }
        }

        private void BtnProcess_Click(object? sender, EventArgs e)
        {
            string savedPath = "";
            bool success = false;

            btnProcess.Enabled = false;

            try
            {
                if (_isEncryption)
                {
                    savedPath = _cipherService.EncryptFile(_selectedFilePath);
                }
                else
                {
                    savedPath = _cipherService.DecryptFile(_selectedFilePath);
                }

                success = !savedPath.StartsWith("ERROR:");

                if (success)
                {
                    ShowSuccessDialog(savedPath);
                }
                else
                {
                    MessageBox.Show(savedPath.Replace("ERROR: ", ""), "İşlem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnProcess.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Beklenmedik bir hata oluştu: {ex.Message}", "Kritik Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnProcess.Enabled = true;
            }
        }

        private void ShowSuccessDialog(string path)
        {
            string operation = _isEncryption ? "Şifreleme" : "Şifre Çözme";

            using (var dialog = new Form
            {
                Text = $"{operation} Başarılı",
                Size = new Size(450, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                Label lblMessage = new Label
                {
                    Text = $"{operation} başarıyla tamamlandı!\nKaydedilen Yer:\n{path}",
                    Location = new Point(10, 10),
                    AutoSize = true,
                    MaximumSize = new Size(430, 0)
                };

                // Font hatası düzeltildi
                Button btnOk = new Button
                {
                    Text = "TAMAM",
                    DialogResult = DialogResult.OK,
                    Location = new Point(10, 120),
                    Width = 100,
                    Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Bold)
                };

                // Font hatası düzeltildi
                Button btnOpenDir = new Button
                {
                    Text = "DİZİNİ AÇ",
                    Location = new Point(120, 120),
                    Width = 150,
                    Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Bold)
                };

                btnOpenDir.Click += (s, e) =>
                {
                    string directory = Path.GetDirectoryName(path);
                    if (Directory.Exists(directory))
                    {
                        Process.Start(new ProcessStartInfo("explorer.exe", directory) { UseShellExecute = true });
                    }
                    else
                    {
                        MessageBox.Show("Hedef dizin bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                dialog.Controls.Add(lblMessage);
                dialog.Controls.Add(btnOk);
                dialog.Controls.Add(btnOpenDir);

                dialog.AcceptButton = btnOk;
                dialog.ShowDialog();
            }

            this.Close();
        }
    }
}