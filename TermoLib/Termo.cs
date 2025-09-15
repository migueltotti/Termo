using System.Runtime.CompilerServices;

namespace TermoLib
{
    public class Termo  
    {
        public List<string> Words = [];
        //public char[,] table;
        public List<List<Letter>> Table = [];
        public string DrawedWord;
        public Dictionary<char, TypedStatus> Keyboard = [];
        int CurrentWord = 0;

        public Termo()
        {
            LoadWordsFromFile("palavras_5_letras_sem_acentos.txt");

            LoadKeyboard();

            DrawWord();
        }

        public void LoadWordsFromFile(string fileName)
        {
            Words = File.ReadAllLines(fileName).ToList();
        }

        private void LoadKeyboard()
        {
            for(int i = 65; i <= 90; i++)
            {
                Keyboard.Add((char)i, TypedStatus.NOT_TYPED);
            }
        }

        public void DrawWord()
        {
            Random rdn = new Random();
            var index = rdn.Next(0, Words.Count);

            DrawedWord = Words[index];
        }

        public void VerifyWord(string word)
        {
            if (word.Length != 5)
            {
                throw new ArgumentException("Incorrect word length");
            }

            var characters = new List<Letter>();
            var targetLetters = DrawedWord.ToCharArray().ToList();
            var tempKeyboard = new Dictionary<char, TypedStatus>();

            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] == DrawedWord[i])
                {
                    characters.Add(new Letter(word[i], TypedStatus.RIGHT_POSITION));
                    tempKeyboard[word[i]] = TypedStatus.RIGHT_POSITION;
                    targetLetters.RemoveAt(targetLetters.IndexOf(word[i]));
                }
                else
                {
                    characters.Add(null); 
                }
            }

            for (int i = 0; i < word.Length; i++)
            {
                if (characters[i] == null)
                {
                    if (targetLetters.Contains(word[i]))
                    {
                        characters[i] = new Letter(word[i], TypedStatus.WRONG_POSITION);
                        if (!tempKeyboard.ContainsKey(word[i]) || tempKeyboard[word[i]] != TypedStatus.RIGHT_POSITION)
                        {
                            tempKeyboard[word[i]] = TypedStatus.WRONG_POSITION;
                        }

                        targetLetters.RemoveAt(targetLetters.IndexOf(word[i]));
                    }
                    else
                    {
                        characters[i] = new Letter(word[i], TypedStatus.NOT_IN_WORD);
                        if (!tempKeyboard.ContainsKey(word[i]))
                        {
                            tempKeyboard[word[i]] = TypedStatus.NOT_IN_WORD;
                        }
                    }
                }
            }
            foreach (var kvp in tempKeyboard)
            {
                if (!Keyboard.ContainsKey(kvp.Key) ||
                    Keyboard[kvp.Key] == TypedStatus.NOT_TYPED ||
                    (Keyboard[kvp.Key] == TypedStatus.NOT_IN_WORD && kvp.Value != TypedStatus.NOT_IN_WORD) ||
                    (Keyboard[kvp.Key] == TypedStatus.WRONG_POSITION && kvp.Value == TypedStatus.RIGHT_POSITION))
                {
                    Keyboard[kvp.Key] = kvp.Value;
                }
            }
            Table.Add(characters);
            CurrentWord++;
        }

        public bool IsWordCorret(string word)
        {
            return DrawedWord.Equals(word);
        }
    }
}
