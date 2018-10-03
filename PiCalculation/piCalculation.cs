using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

/*
 * https://github.com/karan/Projects#numbers
 * Find PI to the Nth Digit - Enter a number and have the program generate PI up to that many decimal places. Keep a limit to how far the program will go
 * (though so far only 25 digits after comma)
 */
namespace PiCalculation
{
	class piCalculation
	{
		private static int maxDigitsNumber = 25;
		private static string maxDigitsNumberString = maxDigitsNumber.ToString();

		static void Main()
		{
			int input = requestPiDigits();
			Console.WriteLine(input);

			Console.WriteLine(Math.PI.ToString() + " (constant)");
			Console.WriteLine(CalculatePi().ToString("N" + input) + " (calculated)");

			Console.ReadKey();
		}

		private static int requestPiDigits()
		{
			Console.WriteLine("Number (positive integer less than " + maxDigitsNumberString + ") of pi digits after comma ");
			int input;
			
			if (!int.TryParse(Console.ReadLine(), out input) || input < 0 || input > maxDigitsNumber)
			{
				Console.WriteLine("Please use only int values between 0 and " + maxDigitsNumberString);
				requestPiDigits();
			}

			return input;
		}

		/*
		 * i'm using https://en.wikipedia.org/wiki/Approximations_of_%CF%80#Middle_Ages Madhava–Leibniz series to calculate pi:
		 * pi = sqrt(12) * (1 - 1 / (3 * 3) + 1 / (5 * 9) - 1 / (7 * 27) + …)
		 * i.e. accuracy depend on number of iterations.
		 */
		static decimal CalculatePi()
		{
			int termsNumber = 56; //27 terms enough to get double that is equal to Math.PI constant. Using decimals max iteration number is 56

			decimal sum = 1;

			for (int i = 0; i < termsNumber; i++)
			{
				sum += CalculateTerm(i + 1);
			}

			decimal sqrt12 = Convert.ToDecimal(Math.Sqrt(12));
			decimal pi = sqrt12 * sum;

			return pi;
		}

		/*
		 * Madhava–Leibniz series terms: - 1 / (3 * 3), 1 / (5 * 9), - 1 / (7 * 27), …
		 */
		static decimal CalculateTerm(int i)
		{
			int sign = -1;

			if (i % 2 == 0)
			{
				sign = 1;
			}

			decimal a = 2 * i + 1;
			decimal b = Convert.ToDecimal(Math.Pow(3, i));

			return sign / (a * b);

		}
	}
}
