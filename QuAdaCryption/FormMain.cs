// Dosya Adı: FormMain.cs
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text.Json;
using QRCoder;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Diagnostics; // Process.Start için
using ZXing;
using NAudio.Wave; // NAudio ile gömülü MP3 oynatma


namespace QuAdaCryption
{
    public partial class FormMain : Form
    {
        private readonly CipherService _cipherService = new CipherService();
        private bool isDarkMode = false;

        private const AnchorStyles AnchorAll = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        private TabControl mainTabControl = null!;
        private TextBox txtPlainText = null!;
        private TextBox txtCipherCode = null!;
        private TextBox txtCipherToDecode = null!;
        private TextBox txtDecodedText = null!;
        private Button btnEncrypt = null!;
        private Button btnDecrypt = null!;
        private Button btnShowQR = null!;
        private Button btnScanQR = null!;
        // YENİ BUTONLAR
        private Button btnFileEncrypt = null!;
        private Button btnFileDecrypt = null!;

        private MenuStrip mainMenu = null!;
        private ToolStripMenuItem settingsToolStripMenuItem = null!;
        private ToolStripMenuItem darkThemeToolStripMenuItem = null!;
        private ToolStripMenuItem exportKeyToolStripMenuItem = null!;
        private ToolStripMenuItem aboutToolStripMenuItem = null!;


        public FormMain()
        {
            this.Text = "🔐 QuAdaCrypter by KAYA SLXS";
            this.Size = new Size(1000, 750);
            this.MinimumSize = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeCustomComponents();
            AttachEventHandlers();
            ApplyTheme(isDarkMode);
        }

        private void InitializeCustomComponents()
        {
            mainMenu = new MenuStrip();
            settingsToolStripMenuItem = new ToolStripMenuItem("Settings");
            darkThemeToolStripMenuItem = new ToolStripMenuItem("Dark Mod/Light Mod");
            exportKeyToolStripMenuItem = new ToolStripMenuItem("Export Your Own Key (JSON)");
            aboutToolStripMenuItem = new ToolStripMenuItem("About");

            settingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                darkThemeToolStripMenuItem,
                exportKeyToolStripMenuItem
            });
            mainMenu.Items.AddRange(new ToolStripItem[] { settingsToolStripMenuItem, aboutToolStripMenuItem });
            this.Controls.Add(mainMenu);

            mainTabControl = new TabControl
            {
                Name = "mainTabControl",
                Dock = DockStyle.Fill,
                Location = new Point(0, 24)
            };
            this.Controls.Add(mainTabControl);
            mainTabControl.BringToFront();

            TabPage encryptTab = new TabPage("Encryption");
            mainTabControl.Controls.Add(encryptTab);

            txtPlainText = new TextBox
            {
                Name = "txtPlainText",
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorAll
            };
            txtPlainText.BringToFront();

            txtCipherCode = new TextBox
            {
                Name = "txtCipherCode",
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorAll
            };

            btnEncrypt = new Button
            {
                Name = "btnEncrypt",
                Text = "ENCRYPT",
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = new Font(this.Font, FontStyle.Bold),
                BackColor = Color.Green,
                ForeColor = Color.White
            };

            btnShowQR = new Button
            {
                Name = "btnShowQR",
                Text = "CREATE QR CODE",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.Teal,
                ForeColor = Color.White
            };

            SplitContainer encryptSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 200,
            };

            encryptSplit.Panel1.Controls.Add(new Label { Text = "Enter Text", Dock = DockStyle.Top });
            encryptSplit.Panel1.Controls.Add(txtPlainText);

            Panel encryptButtonPanel = new Panel { Dock = DockStyle.Bottom, Height = 130 };
            encryptButtonPanel.BackColor = Color.Transparent;

            Label lblEncrypt = new Label
            {
                Text = "Click the green button to start the encryption process:",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };
            Label lblQR = new Label
            {
                Text = "Generate the encrypted text as a QR code:",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };

            encryptButtonPanel.Controls.Add(btnEncrypt);
            encryptButtonPanel.Controls.Add(lblEncrypt);
            encryptButtonPanel.Controls.Add(btnShowQR);
            encryptButtonPanel.Controls.Add(lblQR);

            encryptSplit.Panel2.Controls.Add(txtCipherCode);
            encryptSplit.Panel2.Controls.Add(new Label { Text = "Encrypted Text", Dock = DockStyle.Top });
            encryptSplit.Panel2.Controls.Add(encryptButtonPanel);

            encryptTab.Controls.Add(encryptSplit);


            TabPage decryptTab = new TabPage("Decryption");
            mainTabControl.Controls.Add(decryptTab);

            txtCipherToDecode = new TextBox
            {
                Name = "txtCipherToDecode",
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorAll
            };
            txtCipherToDecode.BringToFront();

            txtDecodedText = new TextBox
            {
                Name = "txtDecodedText",
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorAll
            };

            btnDecrypt = new Button
            {
                Name = "btnDecrypt",
                Text = "DECODE",
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = new Font(this.Font, FontStyle.Bold),
                BackColor = Color.OrangeRed,
                ForeColor = Color.White
            };

            btnScanQR = new Button
            {
                Name = "btnScanQR",
                Text = "Solve with QR Code (Image File)",
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = new Font(this.Font, FontStyle.Regular),
                BackColor = Color.Navy,
                ForeColor = Color.White,
                Location = new Point(0, 40) // Konum ayarı
            };

            // YENİ BUTONLAR
            btnFileEncrypt = new Button
            {
                Name = "btnFileEncrypt",
                Text = "FILE ENCRYPT",
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = new Font(this.Font, FontStyle.Bold),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Location = new Point(0, 80) // Konum ayarı
            };

            btnFileDecrypt = new Button
            {
                Name = "btnFileDecrypt",
                Text = "FILE DECRYPT (.KAYA)",
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = new Font(this.Font, FontStyle.Bold),
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                Location = new Point(0, 0) // Konum ayarı
            };


            SplitContainer decryptSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 200,
            };

            decryptSplit.Panel1.Controls.Add(new Label { Text = "Enter Encrypted Text", Dock = DockStyle.Top });
            decryptSplit.Panel1.Controls.Add(txtCipherToDecode);

            // Panel Yüksekliğini Butonları alacak şekilde artır
            Panel decryptBottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 210 };
            decryptBottomPanel.BackColor = Color.Transparent;

            // Labellar ve Butonların Eklendiği Sıra

            Label lblDecrypt = new Label
            {
                Text = "Click the orange button to start the decoding process:",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblScanQR = new Label
            {
                Text = "Scan the QR code file (PNG/JPG) to automatically decode it:",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 40)
            };

            Label lblFileCrypto = new Label
            {
                Text = "File Operations:",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 120)
            };


            decryptBottomPanel.Controls.Add(btnDecrypt); // 0
            decryptBottomPanel.Controls.Add(lblDecrypt); // Üstte
            decryptBottomPanel.Controls.Add(btnScanQR);  // 40
            decryptBottomPanel.Controls.Add(lblScanQR);  // Üstte

            decryptBottomPanel.Controls.Add(btnFileEncrypt); // 80
            decryptBottomPanel.Controls.Add(btnFileDecrypt); // 120
            decryptBottomPanel.Controls.Add(lblFileCrypto); // Üstte


            decryptSplit.Panel2.Controls.Add(txtDecodedText);
            decryptSplit.Panel2.Controls.Add(new Label { Text = "Decoded Text:", Dock = DockStyle.Top });
            decryptSplit.Panel2.Controls.Add(decryptBottomPanel);

            decryptTab.Controls.Add(decryptSplit);
        }

        private void AttachEventHandlers()
        {
            btnEncrypt.Click += BtnEncrypt_Click;
            btnDecrypt.Click += BtnDecrypt_Click;
            btnScanQR.Click += BtnScanQR_Click;

            // YENİ EVENT HANDLER'LAR
            btnFileEncrypt.Click += BtnFileEncrypt_Click;
            btnFileDecrypt.Click += BtnFileDecrypt_Click;

            btnShowQR.Click += BtnShowQR_Click;
            darkThemeToolStripMenuItem.Click += DarkThemeToolStripMenuItem_Click;
            exportKeyToolStripMenuItem.Click += ExportKeyToolStripMenuItem_Click;
            aboutToolStripMenuItem.Click += AboutToolStripMenuItem_Click;
        }


        private void BtnEncrypt_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPlainText.Text))
            {
                MessageBox.Show("Please enter a text to encrypt.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string plaintext = txtPlainText.Text;
            txtCipherCode.Text = _cipherService.Encrypt(plaintext);
            MessageBox.Show("The text has been successfully encrypted!", "Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDecrypt_Click(object? sender, EventArgs e)
        {
            string ciphertext = txtCipherToDecode.Text.Trim();

            if (string.IsNullOrEmpty(ciphertext))
            {
                MessageBox.Show("Please enter a code to solve it.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // V3 Kontrolü: 4 basamağın katı olmalı
            if (ciphertext.Length % 4 != 0)
            {
                MessageBox.Show("The entered code must consist of a multiple of four digits. Please check your password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            txtDecodedText.Text = _cipherService.Decrypt(ciphertext);

            const string expectedError = "ERROR: The encryption code must consist of a multiple of four digits.";

            if (txtDecodedText.Text.Contains('?'))
            {
                MessageBox.Show("The code has been decrypted, but some characters (‘?’) not present in your key were found. Please check your code or key.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (txtDecodedText.Text == expectedError)
            {
                MessageBox.Show(txtDecodedText.Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDecodedText.Text = "";
            }
            else
            {
                MessageBox.Show("The code has been successfully decrypted!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ---------------------------------------------------------------------
        // DOSYA ŞİFRELEME/ÇÖZME EVENTLERİ
        // ---------------------------------------------------------------------

        private void BtnFileEncrypt_Click(object? sender, EventArgs e)
        {
            ShowFileCryptoForm(true);
        }

        private void BtnFileDecrypt_Click(object? sender, EventArgs e)
        {
            ShowFileCryptoForm(false);
        }

        private void ShowFileCryptoForm(bool isEncryption)
        {
            using (var form = new FormFileCrypto(_cipherService, isEncryption))
            {
                form.ShowDialog();
            }
        }

        // ---------------------------------------------------------------------
        // DİĞER EVENTLER
        // ---------------------------------------------------------------------

        private void BtnScanQR_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "QR Code Images (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp";
                openDialog.Title = "Select QR Code Image";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string scannedCode = _cipherService.DecodeQRImage(openDialog.FileName);

                        if (!string.IsNullOrEmpty(scannedCode) && scannedCode != "ERROR")
                        {
                            txtCipherToDecode.Text = scannedCode;
                            BtnDecrypt_Click(sender, e);
                        }
                        else if (scannedCode == "ERROR")
                        {
                            MessageBox.Show("The QR code could not be scanned from the image. Please select a valid code image.", "Scan Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("No encrypted text was found in the image.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while scanning the QR code: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void DarkThemeToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;
            ApplyTheme(isDarkMode);
        }

        private void ExportKeyToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "JSON File (*.json)|*.json";
                saveDialog.Title = "Export Encryption Key";
                saveDialog.FileName = "cipher_key.json";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var key = _cipherService.GetEncryptionKey();
                        // 4 basamaklı kodlar otomatik olarak serialize edilecektir.
                        string jsonString = JsonSerializer.Serialize(key, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(saveDialog.FileName, jsonString);
                        MessageBox.Show("The key has been successfully exported.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while exporting the key: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnShowQR_Click(object? sender, EventArgs e)
        {
            string code = txtCipherCode.Text;
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Please encrypt a text first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(5);

                using (var form = new Form
                {
                    Text = "QR Code",
                    Size = new Size(qrCodeImage.Width + 50, qrCodeImage.Height + 80),
                    StartPosition = FormStartPosition.CenterScreen,
                    MaximizeBox = false,
                    MinimizeBox = false
                })
                {
                    PictureBox pb = new PictureBox
                    {
                        Image = qrCodeImage,
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.CenterImage
                    };
                    form.Controls.Add(pb);
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating the QR code: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AboutToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            // Gömülü About formu: assembly içindeki "song.mp3" Embedded Resource olarak çalınır
            using (var aboutForm = new Form
            {
                Text = "About QuAda Crypter",
                Size = new Size(400, 220),
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false,
                FormBorderStyle = FormBorderStyle.FixedDialog
            })
            {
                var lblInfo = new Label
                {
                    Text = "QuAda Crypter \nVersion Enchanted 2.0\nThis Logalical Encryption System developed by KAYA SLXS\nGitHub: kayaslxs",
                    Dock = DockStyle.Top,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Padding = new Padding(10),
                    Height = 110
                };

                var btnPlay = new Button
                {
                    Text = "Play",
                    Dock = DockStyle.Bottom,
                    Height = 40,
                    Font = new Font(this.Font, FontStyle.Bold),
                    BackColor = Color.Orange,
                    ForeColor = Color.Black
                };

                aboutForm.Controls.Add(btnPlay);
                aboutForm.Controls.Add(lblInfo);

                // Local playback vars
                IWavePlayer? waveOut = null;
                AudioFileReader? audioFile = null;
                Stream? resStream = null;
                string? tempFilePath = null;
                bool isPlaying = false;

                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                // try to find the embedded resource that ends with song.mp3
                string? resourceName = Array.Find(asm.GetManifestResourceNames(),
                    n => n.EndsWith("song.mp3", StringComparison.OrdinalIgnoreCase));

                Action startPlayback = () =>
                {
                    try
                    {
                        if (isPlaying) return;

                        if (resourceName == null)
                        {
                            // show available resources for debugging
                            var names = string.Join("\n", asm.GetManifestResourceNames());
                            MessageBox.Show($"'song.mp3' gömülü kaynak olarak bulunamadı. Mevcut kaynaklar:\n{names}", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        resStream = asm.GetManifestResourceStream(resourceName);
                        if (resStream == null)
                        {
                            MessageBox.Show("Gömülü kaynak okunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Write embedded stream to temp file because Mp3FileReader can fail on files with sample rate/frame changes.
                        tempFilePath = Path.Combine(Path.GetTempPath(), $"QuAdaCryption_song_{Guid.NewGuid()}.mp3");
                        using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                        {
                            resStream.CopyTo(fs);
                        }
                        // dispose resStream early since we've written it to disk
                        resStream.Dispose();
                        resStream = null;

                        audioFile = new AudioFileReader(tempFilePath);
                        waveOut = new WaveOutEvent();
                        waveOut.Init(audioFile);

                        waveOut.PlaybackStopped += (s2, ev2) =>
                        {
                            isPlaying = false;
                            try
                            {
                                waveOut?.Dispose();
                                waveOut = null;
                                audioFile?.Dispose();
                                audioFile = null;
                            }
                            catch { }

                            // delete temp file
                            try { if (tempFilePath != null && File.Exists(tempFilePath)) File.Delete(tempFilePath); } catch { }
                            tempFilePath = null;

                            if (!aboutForm.IsDisposed && aboutForm.IsHandleCreated)
                            {
                                aboutForm.BeginInvoke(new Action(() => btnPlay.Text = "Play"));
                            }
                        };

                        waveOut.Play();
                        isPlaying = true;
                        if (!aboutForm.IsDisposed && aboutForm.IsHandleCreated)
                        {
                            aboutForm.BeginInvoke(new Action(() => btnPlay.Text = "Stop"));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Müzik çalınırken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // cleanup on error
                        try { audioFile?.Dispose(); audioFile = null; } catch { }
                        try { waveOut?.Dispose(); waveOut = null; } catch { }
                        try { if (tempFilePath != null && File.Exists(tempFilePath)) File.Delete(tempFilePath); } catch { }
                        tempFilePath = null;
                        try { resStream?.Dispose(); resStream = null; } catch { }
                    }
                };

                Action stopPlayback = () =>
                {
                    try { waveOut?.Stop(); } catch { }
                };

                btnPlay.Click += (s, ev) =>
                {
                    if (!isPlaying) startPlayback(); else stopPlayback();
                };

                aboutForm.FormClosing += (s, ev) =>
                {
                    try { waveOut?.Stop(); waveOut?.Dispose(); waveOut = null; } catch { }
                    try { audioFile?.Dispose(); audioFile = null; } catch { }
                    try { resStream?.Dispose(); resStream = null; } catch { }
                    try { if (tempFilePath != null && File.Exists(tempFilePath)) File.Delete(tempFilePath); } catch { }
                    tempFilePath = null;
                };

                if (isDarkMode)
                {
                    aboutForm.BackColor = Color.FromArgb(45, 45, 48);
                    aboutForm.ForeColor = Color.White;
                    lblInfo.BackColor = Color.FromArgb(45, 45, 48);
                    lblInfo.ForeColor = Color.White;
                }

                // Otomatik başlat
                startPlayback();

                aboutForm.ShowDialog();
            }
        }

        private void ApplyTheme(bool dark)
        {
            if (dark)
            {
                this.BackColor = Color.FromArgb(28, 28, 28);
                this.ForeColor = Color.White;
                UpdateControlColors(this.Controls, Color.FromArgb(45, 45, 48), Color.White);
            }
            else
            {
                this.BackColor = SystemColors.Control;
                this.ForeColor = SystemColors.ControlText;
                UpdateControlColors(this.Controls, Color.White, SystemColors.ControlText);
            }
        }

        private void UpdateControlColors(Control.ControlCollection controls, Color backColor, Color foreColor)
        {
            foreach (Control control in controls)
            {
                if (control is TextBox || control is TabControl || control is SplitContainer)
                {
                    control.BackColor = backColor;
                    control.ForeColor = foreColor;
                }
                else if (control is Panel)
                {
                    control.ForeColor = foreColor;
                }
                else if (control is Label)
                {
                    control.ForeColor = foreColor;
                }
                else if (control is Button)
                {
                }

                if (control.HasChildren)
                {
                    UpdateControlColors(control.Controls, backColor, foreColor);
                }
            }
        }
    }
}