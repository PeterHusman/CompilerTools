﻿namespace CompilerCampExercise1
{
    class Test
    {
        private static string strstr = "\"";

        public int LetsGo = 1+ 2 - 3 * 4 /5 + 6;

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
        GuessingGame(int minimum, int maximum, Random random)
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
            string s = Console.ReadLine();
            int guess = int.Parse(s);

            int check = CheckGuess(guess);
            while(check != 0)
            {
                if(check == -1)
                {
                    Console.WriteLine("Guess is too low.");
                }
                else if(check == 1)
                {
                    Console.WriteLine("Guess is too high.");
                }

                s = Console.ReadLine();
                guess = int.Parse(s);
                check = CheckGuess(guess);
            }
            
            Console.WriteLine("Woo! Yay! You win!");
            Console.Write("It only took you ");
            Console.Write(guesses.ToString());
            Console.WriteLine(" guesses!");
            return guesses;
        }
    }

    class Test2
    {
        public int lalala;
    }
}
