using NHunspell;
using System.Drawing.Drawing2D;
using System.Media;
using TermoForms;
using TermoLib;
using static System.Windows.Forms.AxHost;
using Timer = System.Windows.Forms.Timer;

namespace Termo
{
    public partial class Form1 : Form
    {
        private Dictionary<char, Button> keyboardButtons = [];
        private Button[,] buttonsMatrix = new Button[6, 5];
        private HashSet<Keys> allowedCharacters = new HashSet<Keys>();
        private TermoLib.Termo termo;
        private ConfettiManager confettiManager;

        private int currentWord = 0;
        private int currentCharacter = 0;

        private readonly Color currentCharacterBorderColor = Color.DeepSkyBlue;
        private readonly Color characterEmptyBackgroundColor = Color.White;

        private Timer animationTimer;
        private int animationStep = 0;
        private int animationLine = -1;
        private Point[] originalPositions;

        private Hunspell hunspell;

        private const string keySoundPath = "Assets\\key_pressdown_sound.wav";

        public Form1()
        {
            InitializeComponent();
            InitializeForm();

            // inicializa o NHunspell com dicionario portugues
            hunspell = new Hunspell("pt_BR.aff", "pt_BR.dic");

            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            FormPrincipal_Load(this, EventArgs.Empty);

            LoadInputButtons();

            LoadAllowedCharactersSet();

            LoadKeyBoardButtonList();
            keyboardButtons!.Values.ToList().ForEach(btn => Extensions.SetComponentBorderRadius(btn, 15));

            foreach (var btn in buttonsMatrix)
            {
                Extensions.SetComponentBorderRadius(btn, 15);
            }

            // Set border radius to backspace and enter buttons
            Extensions.SetComponentBorderRadius(button53, 15);
            Extensions.SetComponentBorderRadius(button60, 15);
            Extensions.SetComponentBorderRadius(button55, 15);

            // Set border radius to show match info button
            Extensions.SetComponentBorderRadius(btnMatchInfo, 15);

            termo = new TermoLib.Termo();

            ConfigureAnimation();
        }

        private void InitializeForm()
        {
            // Criar o gerenciador de confetes
            confettiManager = new ConfettiManager(this);

            // Habilitar double buffering para reduzir flickering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.DoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Desenhar os confetes
            confettiManager.Draw(e.Graphics);
        }

        private void LoadKeyBoardButtonList()
        {
            var allowCharacterString = allowedCharacters.Select(c => c.ToString()).ToHashSet();

            foreach (var component in Controls)
            {
                if (component is Button keyboardBtn && allowCharacterString.Contains(keyboardBtn.Text))
                {
                    keyboardButtons.Add(keyboardBtn.Text[0], keyboardBtn);
                }
            }
        }

        private void LoadInputButtons()
        {
            // Add buttons from line 1:
            buttonsMatrix[0, 0] = button5;
            buttonsMatrix[0, 1] = button4;
            buttonsMatrix[0, 2] = button3;
            buttonsMatrix[0, 3] = button2;
            buttonsMatrix[0, 4] = button1;

            // Add buttons from line 2:
            buttonsMatrix[1, 0] = button10;
            buttonsMatrix[1, 1] = button9;
            buttonsMatrix[1, 2] = button8;
            buttonsMatrix[1, 3] = button7;
            buttonsMatrix[1, 4] = button6;

            // Add buttons from line 3:
            buttonsMatrix[2, 0] = button15;
            buttonsMatrix[2, 1] = button14;
            buttonsMatrix[2, 2] = button13;
            buttonsMatrix[2, 3] = button12;
            buttonsMatrix[2, 4] = button11;

            // Add buttons from line 4:
            buttonsMatrix[3, 0] = button20;
            buttonsMatrix[3, 1] = button19;
            buttonsMatrix[3, 2] = button18;
            buttonsMatrix[3, 3] = button17;
            buttonsMatrix[3, 4] = button16;

            // Add buttons from line 5:
            buttonsMatrix[4, 0] = button25;
            buttonsMatrix[4, 1] = button24;
            buttonsMatrix[4, 2] = button23;
            buttonsMatrix[4, 3] = button22;
            buttonsMatrix[4, 4] = button21;

            // Add buttons from line 6:
            buttonsMatrix[5, 0] = button30;
            buttonsMatrix[5, 1] = button29;
            buttonsMatrix[5, 2] = button28;
            buttonsMatrix[5, 3] = button27;
            buttonsMatrix[5, 4] = button26;

            originalPositions = new Point[30];

            for (int linha = 0; linha < 6; linha++)
            {
                for (int coluna = 0; coluna < 5; coluna++)
                {
                    // Armazenar posição original
                    originalPositions[linha * 5 + coluna] = buttonsMatrix[linha, coluna].Location;
                }
            }
        }

        private void LoadAllowedCharactersSet()
        {
            for (int i = (int)Keys.A; i <= (int)Keys.Z; i++)
            {
                allowedCharacters.Add((Keys)i);
            }
        }

        private void ChangeCurrentColumnInputPosition(Keys key)
        {
            int oldColumnPosition = 0;
            int newColumnPosition = 0;

            if (key == Keys.Left && currentCharacter > 0)
            {
                oldColumnPosition = currentCharacter;
                currentCharacter--;
                newColumnPosition = currentCharacter;
            }
            else if (key == Keys.Right && currentCharacter < 4)
            {
                oldColumnPosition = currentCharacter;
                currentCharacter++;
                newColumnPosition = currentCharacter;
            }

            Button btn = buttonsMatrix[currentWord, oldColumnPosition];

            btn.FlatAppearance.BorderColor = characterEmptyBackgroundColor;
            btn.FlatAppearance.BorderSize = 3;

            btn = buttonsMatrix[currentWord, newColumnPosition];

            btn.FlatAppearance.BorderColor = currentCharacterBorderColor;
            btn.FlatAppearance.BorderSize = 3;
        }

        private void ChangeCurrentLine()
        {
            Button btn;

            if (currentWord < 5 && IsLineFilled())
            {
                LockLineInput();

                currentWord++;
                currentCharacter = 0;

                btn = buttonsMatrix[currentWord, currentCharacter];

                btn.FlatAppearance.BorderColor = currentCharacterBorderColor;
                btn.FlatAppearance.BorderSize = 3;
            }
        }

        private void AddLetter(string letter)
        {
            Button btn;

            if (currentCharacter < 5)
            {
                btn = buttonsMatrix[currentWord, currentCharacter];

                if (btn.Text == "")
                {
                    btn.Text = letter;

                    if (currentCharacter < 4)
                    {
                        btn.FlatAppearance.BorderColor = characterEmptyBackgroundColor;
                        btn.FlatAppearance.BorderSize = 3;

                        currentCharacter++;

                        btn = buttonsMatrix[currentWord, currentCharacter];

                        btn.FlatAppearance.BorderColor = currentCharacterBorderColor;
                        btn.FlatAppearance.BorderSize = 3;
                    }

                }
            }
        }

        private bool CheckWordsMatch()
        {
            string word = "";

            for (int i = 0; i < 5; i++)
            {
                word += buttonsMatrix[currentWord, i].Text;
            }

            termo.VerifyWord(word);

            PaintVirtualKeyBoard();
            PaintTableWord();

            return termo.IsWordCorret(word);
        }

        private bool IsValidWord()
        {
            string word = "";

            for (int i = 0; i < 5; i++)
            {
                word += buttonsMatrix[currentWord, i].Text;
            }

            return hunspell.Spell(word.ToLower());
        }

        /*protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                hunspell?.Dispose(); // ← Libera quando o Form for fechado
                components?.Dispose();
            }
            base.Dispose(disposing);
        }*/

        private bool IsLineFilled()
        {
            for (int i = 0; i < 5; i++)
            {
                if (buttonsMatrix[currentWord, i].Text == "")
                {
                    return false;
                }
            }

            return true;
        }

        private void LockLineInput()
        {
            Button btn;

            for (int i = 0; i < 5; i++)
            {
                btn = buttonsMatrix[currentWord, i];
                //btn.FlatAppearance.BorderColor = Color.DarkRed;
                btn.Enabled = false;
            }
        }

        private void CleanInputPosition()
        {
            Button currentInputBtn;

            if (0 < currentCharacter && currentCharacter < 5)
            {
                currentInputBtn = buttonsMatrix[currentWord, currentCharacter];

                if (currentCharacter < 4 || currentInputBtn.Text == "")
                {
                    currentInputBtn.Text = "";
                    currentInputBtn.FlatAppearance.BorderColor = characterEmptyBackgroundColor;

                    currentCharacter--;

                    currentInputBtn = buttonsMatrix[currentWord, currentCharacter];
                    currentInputBtn.Text = "";
                    currentInputBtn.FlatAppearance.BorderColor = currentCharacterBorderColor;
                }
                else
                {
                    currentInputBtn.Text = "";
                    currentInputBtn.FlatAppearance.BorderColor = currentCharacterBorderColor;
                }
            }
        }

        private void EndMatch()
        {
            //ShowMatchsInformationCard();

            foreach (var btn in buttonsMatrix)
            {
                btn.Enabled = true;
                btn.FlatAppearance.BorderColor = characterEmptyBackgroundColor;
                btn.BackColor = Color.FromArgb(33, 33, 33);
                btn.Text = "";
            }

            var firtsLetterButton = buttonsMatrix[0, 0];
            firtsLetterButton.FlatAppearance.BorderColor = currentCharacterBorderColor;
            firtsLetterButton.FlatAppearance.BorderSize = 3;

            foreach (var btn in keyboardButtons.Values)
            {
                //btn.FlatAppearance.BorderColor = Color.FromArgb(33, 33, 33);
                btn.BackColor = Color.FromArgb(224, 224, 224);
                btn.ForeColor = Color.FromArgb(33, 33, 33);
            }

            lblWL.Text = "";

            currentCharacter = 0;
            currentWord = 0;

            termo = new TermoLib.Termo();
        }

        private void PaintTableWord()
        {
            Button tableLetter;
            var currentWordFromTable = termo.Table[currentWord];

            for (int i = 0; i < 5; i++)
            {
                tableLetter = buttonsMatrix[currentWord, i];

                var color = PaintLetter(currentWordFromTable[i].Status, false); // false = não é teclado virtual

                tableLetter.BackColor = color;
                tableLetter.FlatAppearance.BorderColor = color;
                tableLetter.ForeColor = Color.White;
            }
        }

        private void PaintVirtualKeyBoard()
        {
            Button virtualKeyBoardLetter;

            foreach (var keyBoardLetter in termo.Keyboard)
            {
                if (keyboardButtons.TryGetValue(keyBoardLetter.Key, out virtualKeyBoardLetter!))
                {
                    var color = PaintLetter(keyBoardLetter.Value, true);

                    virtualKeyBoardLetter.BackColor = color;
                    virtualKeyBoardLetter.FlatAppearance.BorderColor = color;
                    if (color == ColorTranslator.FromHtml("#333333"))
                    {
                        virtualKeyBoardLetter.ForeColor = Color.White;
                    }
                    else if (color == Color.FromArgb(224, 224, 224))
                    {
                        virtualKeyBoardLetter.ForeColor = Color.Black;
                    }
                    else
                    {
                        virtualKeyBoardLetter.ForeColor = Color.White;
                    }
                }
            }
        }

        private Color PaintLetter(TypedStatus letterTypedStatus, bool isVirtualKeyboard = false)
        {
            return letterTypedStatus switch
            {
                TypedStatus.NOT_TYPED => isVirtualKeyboard ? Color.FromArgb(224, 224, 224) : characterEmptyBackgroundColor,
                TypedStatus.NOT_IN_WORD => ColorTranslator.FromHtml("#333333"),
                TypedStatus.WRONG_POSITION => ColorTranslator.FromHtml("#B48900"),
                TypedStatus.RIGHT_POSITION => ColorTranslator.FromHtml("#388E3C"),
                _ => throw new NotImplementedException(),
            };
        }

        private void PlayKeySound()
        {
            using var player = new SoundPlayer(keySoundPath);

            player.Play();
        }

        private void ShowMatchsInformationCard()
        {
            using CardForm card = new CardForm();

            // Exibe o card como modal
            DialogResult result = card.ShowDialog(this);
        }

        private async Task DoInvalidWordAnimation()
        {
            if (animationLine != -1) return; // Evitar animações simultâneas

            animationLine = currentWord;
            animationStep = 0;

            // Primeiro: mudança de cor para vermelho
            await ChangeLineColor(animationLine, Color.FromArgb(220, 20, 60), Color.DarkRed, 100); // Vermelho

            // Depois: animação de shake
            animationTimer.Start();
        }

        private void ConfigureAnimation()
        {
            animationTimer = new Timer
            {
                Interval = 50 // 50ms para animação suave
            };
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private async Task ChangeLineColor(int line, Color backColor, Color borderColor, int duration)
        {
            // Mudar cor de todos os botões da linha
            for (int column = 0; column < 5; column++)
            {
                var currentButton = buttonsMatrix[line, column];

                currentButton.BackColor = backColor;
                currentButton.FlatAppearance.BorderColor = borderColor;
            }

            // Aguardar duração especificada
            await Task.Delay(duration);
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (animationLine == -1) return;

            int lineShift = 0;

            // Padrão de shake: esquerda, direita, esquerda, centro
            switch (animationStep)
            {
                case 0:
                    lineShift = -8; // Esquerda
                    break;
                case 1:
                    lineShift = 8;  // Direita
                    break;
                case 2:
                    lineShift = -6; // Esquerda (menor)
                    break;
                case 3:
                    lineShift = 6;  // Direita (menor)
                    break;
                case 4:
                    lineShift = -4; // Esquerda (ainda menor)
                    break;
                case 5:
                    lineShift = 4;  // Direita (ainda menor)
                    break;
                case 6:
                    lineShift = 0;  // Volta ao centro
                    break;
            }

            // Aplicar deslocamento a toda linha
            for (int column = 0; column < 5; column++)
            {
                Point originalPosition = originalPositions[animationLine * 5 + column];
                buttonsMatrix[animationLine, column].Location = new Point(
                    originalPosition.X + lineShift,
                    originalPosition.Y
                );
            }

            animationStep++;

            // Finalizar animação
            if (animationStep > 6)
            {
                animationTimer.Stop();
                EndAnimation();
            }
        }

        private async void EndAnimation()
        {
            // Restaurar posições originais
            for (int collumn = 0; collumn < 5; collumn++)
            {
                buttonsMatrix[animationLine, collumn].Location = originalPositions[animationLine * 5 + collumn];
            }

            // Aguardar um pouco antes de restaurar a cor
            await Task.Delay(200);

            // Restaurar cor original
            await ChangeLineColor(animationLine, Color.FromArgb(33, 33, 33), Color.White, 0);

            var currentButton = buttonsMatrix[currentWord, currentCharacter];
            currentButton.FlatAppearance.BorderColor = currentCharacterBorderColor;
            currentButton.FlatAppearance.BorderSize = 3;
            currentButton.BackColor = Color.FromArgb(33, 33, 33);

            animationLine = -1;
        }

        // Método para capturar enter
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            PlayKeySound();

            if (keyData == Keys.Enter && IsLineFilled())
            {
                if (CheckWordsMatch())
                {
                    //MessageBox.Show("Parabéns, você acertou a palavra!");
                    confettiManager.StartAnimation(125);
                    lblWL.Text = "Vitória";
                    lblWL.ForeColor = ColorTranslator.FromHtml("#388E3C");
                    //EndMatch();
                }
                else if (currentWord == 5)
                {
                    MessageBox.Show($"Você errou, a palavra certa era: {termo.DrawedWord}");
                    lblWL.Text = "Derrota";
                    lblWL.ForeColor = Color.Red;
                    //EndMatch();
                }
                else if(!IsValidWord())
                {
                    DoInvalidWordAnimation();
                }
                else
                {
                    ChangeCurrentLine();
                }
                return true;
            }
            else if (keyData is Keys.Left or Keys.Right)
            {
                ChangeCurrentColumnInputPosition(keyData);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            PlayKeySound();

            if (allowedCharacters.Contains(e.KeyCode))
            {
                AddLetter(e.KeyCode.ToString());
            }
            else if (e.KeyCode is Keys.Back)
            {
                CleanInputPosition();
            }
        }

        private void VirtualKeyboard_Enter_Click(object sender, EventArgs e)
        {
            PlayKeySound();
            if (IsLineFilled() && IsValidWord())
            {
                if (CheckWordsMatch())
                {
                    //MessageBox.Show("Parabéns, você acertou a palavra!");
                    confettiManager.StartAnimation(125);
                    lblWL.Text = "Vitória";
                    lblWL.ForeColor = ColorTranslator.FromHtml("#388E3C");
                    //EndMatch();
                }
                else if (currentWord == 5)
                {
                    MessageBox.Show($"Você errou, a palavra certa era: {termo.DrawedWord}");
                    lblWL.Text = "Derrota";
                    lblWL.ForeColor = Color.Red;
                    //EndMatch();
                }
                else
                {
                    ChangeCurrentLine();
                }
            }
            else if(IsLineFilled() && !IsValidWord())
            {
                DoInvalidWordAnimation();
            }
        }

        private void VirtualKeyboard_Letter_Click(object sender, EventArgs e)
        {
            PlayKeySound();

            Button letter = (Button)sender;

            AddLetter(letter.Text);
        }

        private void VirtualKeyboard_BackSpace_Click(object sender, EventArgs e)
        {
            PlayKeySound();

            CleanInputPosition();
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            FormTutorial tutorial = new FormTutorial();

            tutorial.ShowDialog();
        }

        private void BtnShowMatchInfo_Click(object sender, EventArgs e)
        {
            ShowMatchsInformationCard();
        }

        private void button55_Click(object sender, EventArgs e)
        {
            EndMatch();
        }
    }
}
