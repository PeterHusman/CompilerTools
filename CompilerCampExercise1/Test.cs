namespace CompilerCampExercise1
{
    class Test
    {
        private static string strstr = "\"";

        public bool False = false;

        static int AverageTwoNumbers(int a, int b)
        {
            bool d = a == b;
            int c = a + b;
            return c / 2;
        }

        void LoopSomeTimes()
        {
            int i = 0;
            int num = 3;
            if(i == num)
            {
                return;
            }
            while (i < num)
            {
                i++;
            }
        }

        static void GuessingGame()
        {

        }

        public Test()
        {
        }
    }

    class Prog
    {
        static entrypoint void Main()
        {
            
        }
    }

    class GuessingGame
    {
        int number;
        GuessingGame(int minimum, int maximum, System.Random random)
        {
            number = random.Next(minimum, maximum);
        }
        int guesses = 0;
        int CheckGuess(int guess)
        {
            guesses++;
            if(guess < number)
            {
                return -1;
            }
            else if(guess == number)
            {
                return 0;
            }
            else if(guess > number)
            {
                return 1;
            }
        }

        int GoAllTheWay()
        {
            string s = System.Console.ReadLine();
            int guess = int.Parse(s);

            int check = CheckGuess(guess);
            while(check != 0)
            {
                if(check == -1)
                {
                    System.Console.WriteLine("Guess is too low.");
                }
                else if(check == 1)
                {
                    System.Console.WriteLine("Guess is too high.");
                }

                s = System.Console.ReadLine();
                guess = int.Parse(s);
                check = CheckGuess(guess);
            }
            
            System.Console.WriteLine("Woo! Yay! You win!");
            System.Console.Write("It only took you ");
            System.Console.Write(guesses.ToString());
            System.Console.WriteLine(" guesses!");
            return guesses;
        }
    }

    class Test2
    {
        public int lalala;
    }

    class Data(int One, int Two);
}
