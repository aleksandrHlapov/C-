using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;

// <summary>
// https://github.com/karan/Projects#numbers
// Find PI to the Nth Digit
// </summary>
namespace PiCalculation
{
	/// <summary>
	/// Compute pi digits using spigot algorithm, Gosper's formula
	/// </summary>
	class PiCalculation
	{
		private static ulong[] numerators;
		private static ulong[] denominators;
		private static ulong[] remainders;
		private static ulong[] carriedOver;

		static void Main()
		{
			Console.WriteLine(CalculatePi(requestNumberOfPiDigits()));
			Console.ReadKey();
		}

		/// <summary>
		/// Request user to specify number of pi digits. Simple validation will be applied, application will close unless user type positive integer.
		/// </summary>
		/// <returns>user type result</returns>
		private static uint requestNumberOfPiDigits()
		{
			Console.WriteLine("Please specify number of pi digits to calculate.");
			Console.WriteLine("Please note that due to this algorithm don't use arbitrary large math it could calculate accurately up to 99 996 pi digits.");
			Console.WriteLine("Algorithm will continue calculation if you wish to get more digits but this won't be pi digits any more.");
			Console.WriteLine();
			Console.WriteLine("You should input positive integer (1, 2, 3, …). Otherwise application will just close.");

			uint requestedNumberOfDigits = 0;
			bool isInputPositiveInteger = uint.TryParse(Console.ReadLine(), out requestedNumberOfDigits);

			if(!isInputPositiveInteger || 0 == requestedNumberOfDigits)
			{
				System.Environment.Exit(0);
			}

			return requestedNumberOfDigits;
		}

		/// <summary>
		/// Main pi calculation method, aggregate all steps
		/// </summary>
		/// <param name="requestedPiDigitsNumber">user should specify number of digits</param>
		/// <returns>pi as string</returns>
		public static string CalculatePi(uint requestedPiDigitsNumber)
		{
			//initialization starts
			BigInteger rawPi = 0;

			//using this approach i will use same number of boxes for each pi digit. Simple but slow approach.
			//If i need 100 digits – going to use 91 box. Which is huge overkill to calculate, for example, 10th digit which require only 10 boxes
			uint boxesNumber = calculateBoxesNumber(requestedPiDigitsNumber); 

			numerators = bakeNumeratorsArray(boxesNumber);
			denominators = bakeDenominatorsArray(boxesNumber);
			remainders = initialiseReminders(boxesNumber);

			//should be initialized with zeros
			carriedOver = new ulong[boxesNumber];
			//initialization complete

			//calculation
			for (int i = 0; i < requestedPiDigitsNumber; i++)//for each requested digit…
			{
				ulong nextDigit = nextPiDigit(boxesNumber);//…calculate all boxes
				showLoadingMessage(i);//hate waiting. 11+k digits calculation may take some time
				rawPi = rawPi * 10 + nextDigit;
			}

			//right now pi is just positive integer (31415926…), need to apply regular form ({integers}{separator}{decimals})
			return assemblePi(rawPi);
		}

		/// <summary>
		/// Core method: calculate all boxes to compute next pi digit
		/// </summary>
		/// <param name="boxesNumber">number of boxes required to calculate all digits</param>
		/// <returns>next pi digit. Could be greater than 9, in this case increase previous digit by 1 and attach 0 as last pi digit.</returns>
		/// <remarks>
		/// <list type="number">
		/// <listheader><term>Spigot algorithm</term></listheader>
		/// <item><term><c>multiplier = floor (previous box sum / previous box denominator)</c></term>
		/// <description>This is general rule. First and last multipliers, however, use different logic. First multiplier equal to 0.
		/// Last multiplier = floor (previous box sum / base)</description></item>
		/// <item><term><c>carried over = multiplier * previous box numerator</c></term>
		/// <description>First carried over = 0 because no previous calculations</description></item>
		/// <item><term><c>sum = carried over + remainder * base</c></term>
		/// <description>base is a power of 10, base = 10 mean calculate 1 pi digit at a time, base = 100 – 2 digits and so on.</description></item>
		/// <item><term><c>new remainder = sum modulo denominator</c></term>
		/// <description>i.e. in this step we should update this box remainder using Euclidean division.<para />
		/// Example: new remainder = sum modulo denominator = 2800 % 816 = 352. Same case using regular representation:
		/// sum = denominator * next box multiplier + new remainder = 816 * 3 + 352 = 2800. Here "next box multiplier" is from step 1
		/// </description></item>
		/// <item><term><c>pi digit = floor (sum / base)</c></term>
		/// <description>I.e. this is last step, return expression, it should be done once loop through boxes is over</description></item>
		/// </list>
		/// </remarks>
		private static ulong nextPiDigit(uint boxesNumber)
		{
			ulong sum = 0;

			for (int i = 0; i < boxesNumber; i++)//for each box
			{
				ulong remainder = remainders[i] * 10;
				ulong denominator = denominators[i];
				ulong thisIterationCarriedOver = carriedOver[i];

				sum = remainder + thisIterationCarriedOver;

				if (i + 1 == boxesNumber)
				{
					denominator = 10;
				}

				remainder = sum % denominator;
				remainders[i] = remainder;

				if (i + 1 < boxesNumber)
				{
					ulong multiplier = sum / denominator;
					ulong numerator = numerators[i];
					carriedOver[i + 1] = numerator * multiplier;
				}
			}

			return sum / 10;
		}

		private static void showLoadingMessage(int digitNumber)
		{
			if (0 == digitNumber % 1000 && digitNumber > 0)
			{
				Console.WriteLine(digitNumber / 1000 + "k digits calculated (loading…)");
			}
		}

		/// <summary>
		/// Simply use your system number decimal separator to convert pi to double (3.14…) from integer (314…)
		/// </summary>
		/// <param name="rawPi">integer pi representation (314…)</param>
		/// <returns>double pi representation (3.14…) as a string</returns>
		private static string assemblePi(BigInteger rawPi)
		{
			string decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			string pi = rawPi.ToString();

			if(rawPi > 3)
			{
				pi = pi.Insert(1, decimalSeparator);
			}

			return pi;
		}

		/// <summary>
		/// In a nutshell this is memory allocation, this method calculate number of sum terms:
		/// pi = 3 + 1/60 (8 + 6/168 (13 + 15/330 (18 + …))) ~ box{1}(box{2}(box{3}(…box{N - 2}(box{N - 1}(box{N}))))).
		/// Here box{1} = {3, 0/0}, box{2} = {8, 1/60}, box{3} = {13, 6/168}, …, box{N} = {biggest remainder, biggest numerator / biggest denominator}<para />
		/// Gosper's formula allow to use 0.9 boxes per digit + 1 box. So to get 10 digits need
		/// boxesNumber = floor(0.9 * 10) + 1 = 10. To get 100 digits – boxesNumber = floor(0.9 * 100) + 1 = 91
		/// </summary>
		/// <param name="requestedPiDigitsNumber">number of pi digits user requested to calculate</param>
		/// <returns>positive integer, number of iterations required to calculate every pi digit</returns>
		private static uint calculateBoxesNumber(uint requestedPiDigitsNumber)
		{
			return requestedPiDigitsNumber * 9 / 10 + 1;
		}

		/// <summary>
		/// In spigot algorithm, Gosper's formula, numerators array is a set of constants
		/// (box{1}, box{2}, …): 0, 1, 6, 15, … == 0, 1, 2 * 3, 3 * 5, …, (i + 1) * (2i + 1)
		/// pi = 3 + numerator[box{2}]/60 (8 + numerator[boxesNumber{3}]/168 (13 + numerator[boxesNumber{4}]/330 (18 + …)))
		/// <para>Keep in mind that numerators and denominators starts with 0 (box1), while reminders starts with 3 (box1)</para>
		/// </summary>
		/// <param name="boxesNumber">number of boxes required to calculate all digits</param>
		/// <returns>array of constants</returns>
		/// <remarks>
		/// According to algorithm, calculation should be done "from inside", box{N} (biggest numerator, denominator, remainder values) should be calculated first.
		/// That is why i build this array reversed: array[0] == box{N}, array[1] = box{N-1}, …
		/// </remarks>
		private static ulong[] bakeNumeratorsArray(uint boxesNumber)
		{
			ulong[] numeratorsArray = new ulong[boxesNumber];
			//min boxesNumber is 1 due to user input validation. Cycle starts at boxesNumber == 2
			ulong multiplier1 = boxesNumber - 1;//… 5, 4, 3, 2, 1.
			ulong multiplier2 = 2 * boxesNumber - 3;//…, 9, 7, 5, 3, 1.

			for (int i = 0; i < boxesNumber - 1; i++)//first element, box{N}, will remain equal to 0
			{
				numeratorsArray[i] = multiplier1 * multiplier2;//…, 45, 28, 15, 6, 1.
				multiplier1--;
				multiplier2 -= 2;
			}

			return numeratorsArray;
		}

		/// <summary>
		/// In spigot algorithm, Gosper's formula, numerators array is a set of constants
		/// (box{1}, box{2}, …): 0, 60, 168, 330, 546, … == 0, 4 * 5 * 3, 7 * 8 * 3, 10 * 11 * 3, 13 * 14 * 3, …, i * (i + 1) * 3
		/// pi = 3 + 1/denominator[2] (8 + 6/denominator[3] (13 + 15/denominator[4] (18 + …)))
		/// <para>Keep in mind that numerators and denominators starts with 0 (box1), while reminders starts with 3 (box1)</para>
		/// </summary>
		/// <param name="boxesNumber">number of boxes required to calculate all digits</param>
		/// <returns>array of constants</returns>
		/// <remarks>
		/// According to algorithm, calculation should be done "from inside", box{N} (biggest numerator, denominator, remainder values) should be calculated first.
		/// That is why i build this array reversed: array[0] == box{N}, array[1] = box{N-1}, …
		/// </remarks>
		private static ulong[] bakeDenominatorsArray(uint boxesNumber)
		{
			ulong[] denominatorsArray = new ulong[boxesNumber];

			//min boxesNumber is 1 due to user input validation. Cycle starts at boxesNumber == 2
			ulong multiplier = 4 + 3 * (boxesNumber - 2);//4, 7, 10, …

			for (int i = 0; i < boxesNumber - 1; i++)//first element, box{N}, will remain equal to 0
			{
				denominatorsArray[i] = multiplier * (multiplier + 1) * 3;
				multiplier -= 3;
			}

			return denominatorsArray;
		}

		/// <summary>
		/// In spigot algorithm, Gosper's formula, numerators array is a set of constants
		/// (box{1}, box{2}, …): 3, 8, 13, 18, … == 3, 8, 13, 18, …, (i + 5)
		/// pi = remainder{1} + 1/60 (remainder{2} + 6/168 (remainder{3} + 15/330 (remainder{4} + …)))
		/// <para>Keep in mind that numerators and denominators starts with 0 (box1), while reminders starts with 3 (box1)</para>
		/// </summary>
		/// <param name="boxesNumber">number of boxes required to calculate all digits</param>
		/// <returns>array of constants</returns>
		/// <remarks>
		/// According to algorithm, calculation should be done "from inside", box{N} (biggest numerator, denominator, remainder values) should be calculated first.
		/// That is why i build this array reversed: array[0] == box{N}, array[1] = box{N-1}, …
		/// </remarks>
		private static ulong[] initialiseReminders(uint boxesNumber)
		{
			ulong[] initialRemaindersArray = new ulong[boxesNumber];

			for (uint i = 0; i < boxesNumber; i++)
			{
				initialRemaindersArray[i] = 3 + 5 * (boxesNumber - i - 1);//box2 → 3; box3 → 8; box4 → 13; …
			}

			return initialRemaindersArray;
		}
	}
}
