using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    class FileProcessor
    {
        public File FileData { get; }
        public List<Class> ClassList { get; }

        public FileProcessor()
        {
            this.FileData = new File();
            this.ClassList = new List<Class>();
        }

        public void ProcessFile(string filePath, bool populateClassList)
        {

            /* Test */



            /* End of test */


            if (populateClassList)
            {
                this.ProcessRelationshipData(filePath);
            }
            else
            {
                this.ProcessFunctionData(filePath);
            }
        }

        /* This option is used to process the file when the /R option was not provided */
        private void ProcessFunctionData(string filePath)
        {

        }

        /* This option is used to process the file when the /R option was provided */
        private void ProcessRelationshipData(string filePath)
        {

        }
    }

    class ClassProcessor
    {

    }
}
