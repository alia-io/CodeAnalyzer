using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeAnalyzer
{
    class InputReader
    {
        public string[] FormattedInput { get; private set; }
            // Format (if options are present): "/S", "/R", "/X", "[path]"

        public string ErrorMessage { get; private set; }

        public InputReader()
        {
            this.FormattedInput = new string[4] { "", "", "", "" };
            this.ErrorMessage = "";
        }


        /* Returns true if successful; returns false if invalid argument */
        public bool FormatInput(string[] args)
        {
            if (args.Length < 1)
            {
                this.ErrorMessage = "Needs more command line arguments.";
                return false;
            }

            this.ErrorMessage = "\nArguments must include a valid directory path." +
                "\nPaths with spaces must be surrounded by quotation marks." +
                "\nAdditional arguments are optional. Valid arguments include:" +
                "\n\t/S - inlude subdirectories" +
                "\n\t/R - analyze relationship data" +
                "\n\t/X - print data to XML document";

            for (int i = 0; i < args.Length; i++)
            {
                if (!this.SetInputField(args[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /* Returns true if successful; returns false if invalid argument */
        private bool SetInputField(string arg)
        {
            /* Test Option input */

            if (arg.ToLower().Equals("/s"))
            {
                if (this.FormattedInput[0].Equals(""))
                {
                    this.FormattedInput[0] = "/S";
                    return true;
                }
                return false;
            }

            if (arg.ToLower().Equals("/r"))
            {
                if (this.FormattedInput[1].Equals(""))
                {
                    this.FormattedInput[1] = "/R";
                    return true;
                }
                return false;
            }

            if (arg.ToLower().Equals("/x"))
            {
                if (this.FormattedInput[2].Equals(""))
                {
                    this.FormattedInput[2] = "/X";
                    return true;
                }
                return false;
            }

            /* Test Directory Path input */
            return this.TestDirectoryPath(arg);
        }

        /* Returns true if successful; returns false if invalid argument */
        private bool TestDirectoryPath(string arg)
        {
            string directoryPath = "";

            try
            {
                directoryPath = Path.GetFullPath(arg);
            }
            catch (Exception)
            {
                return false;
            }

            if (Directory.Exists(directoryPath))
            {
                if (this.FormattedInput[3].Equals(""))
                {
                    this.FormattedInput[3] = directoryPath;
                    return true;
                }
                return false;
            }

            return false;
        }
    }

    class OutputWriter
    {

    }
}
