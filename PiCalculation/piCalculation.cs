using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * https://github.com/karan/Projects#numbers
 * Find PI to the Nth Digit - Enter a number and have the program generate PI up to that many decimal places. Keep a limit to how far the program will go
 * (though so far only 15 digits after comma)
 */
namespace PiCalculation
{
	class piCalculation
	{
		static void Main()
		{
			Console.WriteLine(Math.PI.ToString() + " (constant)");
			Console.WriteLine(CalculatePi().ToString("N30") + " (calculated)");

			Console.ReadKey();
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

			decimal pow = Convert.ToDecimal(Math.Pow(3, i));

			return sign / ((2 * i + 1) * pow);
		}
	}
}
