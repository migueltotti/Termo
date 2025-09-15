using TermoLib;

namespace TermoTeste
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Termo termo = new Termo();

            termo.LoadWordsFromFile("words.txt");

            Console.WriteLine(String.Join("\n", termo.Words.Take(5)));
        }

        [TestMethod]
        public void TestMethod2()
        {
            Termo termo = new Termo();
            PrintGame(termo);
            Console.WriteLine($"\nPALAVRA_SORTEADA: {termo.DrawedWord}\n");
            termo.VerifyWord("APOIO");
            PrintGame(termo);

            termo.VerifyWord("NADAR");
            PrintGame(termo);

            termo.VerifyWord("PINTO");
            PrintGame(termo);
        }

        private void PrintGame(Termo termo)
        {
            Console.WriteLine("T A B U L E I R O");
            foreach (var word in termo.Table)
            {
                foreach(var letter in word)
                {
                    Console.Write($"{letter.Character}: {letter.Status} | ");
                }

                Console.WriteLine();
            }

            Console.WriteLine("\nT E C L A D O");
            foreach (var key in termo.Keyboard)
            {
                Console.Write($"{key.Key}: {key.Value} | ");
            }
            Console.WriteLine();
        }
    }
}