﻿using System;
using System.Threading.Tasks;

using Xenial.Delicious.Reporters;

using static Xenial.Tasty;

namespace ReturnCodes
{
    internal class Program
    {
        static Program() => ConsoleReporter.Register();

        private static async Task<int> Main(string[] args)
        {
            It("1 + 2 should be 3", () =>
            {
                var calculation = 1 + 2;
                var isThree = calculation == 3;
                return (isThree, $"1 + 2 should be 3 but actually was {calculation}");
            });

            return await Run(args); //Tell Tasty to execute the test cases and return an exit-code
        }
    }
}
