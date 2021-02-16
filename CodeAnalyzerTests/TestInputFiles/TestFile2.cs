﻿
using System;
using System.Collections.Generic;

/* Albahari, Joseph; Johannsen, Eric.C# 8.0 in a Nutshell (p. 337). O'Reilly Media. Kindle Edition. */

namespace Tests
{
    public class Lists
    {
        public void ExampleList()
        {
            var words = new List<string>(); // New string-typed list
            words.Add(" melon");
            words.Add(" avocado");
            words.AddRange(new[] { "banana", "plum" });
            words.Insert(0, "lemon"); // Insert at start
            words.InsertRange(0, new[] { "peach", "nashi" }); // Insert at start

            words.Remove(" melon"); words.RemoveAt(3); // Remove the 4th element
            words.RemoveRange(0, 2); // Remove first 2 elements

            // Remove all strings starting in 'n':
            words.RemoveAll(s => s.StartsWith(" n"));

            Console.WriteLine(words[0]); // first word
            Console.WriteLine(words[words.Count - 1]); // last word
            foreach (string s in words) Console.WriteLine(s); // all words
            List<string> subset = words.GetRange(1, 2); // 2nd-> 3rd words

            string[] wordsArray = words.ToArray(); // Creates a new typed array

            // Copy first two elements to the end of an existing array: 
            string[] existing = new string[1000];
            words.CopyTo(0, existing, 998, 2);

            List<string> upperCaseWords = words.ConvertAll(s => s.ToUpper());
            List<int> lengths = words.ConvertAll(s => s.Length);
        }
    }

}