using System.Drawing.Drawing2D;
using TermoLib;

namespace Termo
{
    public partial class Form1 : Form
    {
        private Dictionary<char, Button> keyboardButtons = [];
        private Button[,] buttonsMatrix = new Button[6, 5];
        private HashSet<Keys> allowedCharacters = new HashSet<Keys>();
        private TermoLib.Termo termo;

        private int currentWord = 0;
        private int currentCharacter = 0;

        private readonly Color currentCharacterBorderColor = Color.DeepSkyBlue;
        private readonly Color characterEmptyBackgroundColor = Color.White;

        public Form1()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            FormPrincipal_Load(this, EventArgs.Empty);

            LoadInputButtons();

            LoadAllowedCharactersSet();

            LoadKeyBoardButtonList();
            keyboardButtons!.Values.ToList().ForEach(btn => SetButtonBorderRadius(btn, 15));

            foreach (var btn in buttonsMatrix)
            {
                SetButtonBorderRadius(btn, 15);
            }

            // Set border radius to backspace and enter buttons
            SetButtonBorderRadius(button53, 15);
            SetButtonBorderRadius(button60, 15);

            termo = new TermoLib.Termo();
        }

        private void LoadKeyBoardButtonList()
        {
            var allowCharacterString = allowedCharacters.Select(c => c.ToString()).ToHashSet();

            foreach(var component in Controls)
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
        }

        private void LoadAllowedCharactersSet()
        {
            for (int i = (int)Keys.A; i <= (int)Keys.Z; i++)
            {
                allowedCharacters.Add((Keys)i);
            }
        }

        private void SetButtonBorderRadius(Button btn, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            // Canto superior esquerdo
            path.AddArc(0, 0, radius, radius, 180, 90);
            // Linha superior
            path.AddLine(radius, 0, btn.Width - radius, 0);
            // Canto superior direito
            path.AddArc(btn.Width - radius, 0, radius, radius, 270, 90);
            // Linha direita
            path.AddLine(btn.Width, radius, btn.Width, btn.Height - radius);
            // Canto inferior direito
            path.AddArc(btn.Width - radius, btn.Height - radius, radius, radius, 0, 90);
            // Linha inferior
            path.AddLine(btn.Width - radius, btn.Height, radius, btn.Height);
            // Canto inferior esquerdo
            path.AddArc(0, btn.Height - radius, radius, radius, 90, 90);
            // Linha esquerda
            path.AddLine(0, btn.Height - radius, 0, radius);

            path.CloseFigure();
            btn.Region = new Region(path);
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
                if (keyboardButtons.TryGetValue(keyBoardLetter.Key, out virtualKeyBoardLetter))
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

            currentCharacter = 0;
            currentWord = 0;

            termo = new TermoLib.Termo();
        }

        // Método para capturar enter
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && IsLineFilled())
            {
                if (CheckWordsMatch())
                {
                    MessageBox.Show("Parabéns, você acertou a palavra!");
                    EndMatch();
                }
                else if (currentWord == 5)
                {
                    MessageBox.Show($"Você errou, a palavra certa era: {termo.DrawedWord}");
                    EndMatch();
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
            if (CheckWordsMatch())
            {
                MessageBox.Show("Parabéns, você acertou a palavra!");
                EndMatch();
            }
            else if (currentWord == 5 && IsLineFilled())
            {
                MessageBox.Show($"Você errou, a palavra certa era: {termo.DrawedWord}");
                EndMatch();
            }
            else if (IsLineFilled())
            {
                ChangeCurrentLine();
            }
        }
        private void VirtualKeyboard_Letter_Click(object sender, EventArgs e)
        {
            Button letter = (Button)sender;

            AddLetter(letter.Text);
        }

        private void VirtualKeyboard_BackSpace_Click(object sender, EventArgs e)
        {
            CleanInputPosition();
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            FormTutorial tutorial = new FormTutorial();

            tutorial.ShowDialog();
        }
    }
}
