using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardChecher
{
    class LogWritter
    {
        static LogWritter obj = new LogWritter();

        private LogWritter() {

        }

        public void WriteLog(String logItem)
        {
            DateTime now = DateTime.Now;
            String dateString = now.Date.ToString().Split(' ')[0];
            dateString = dateString.Replace(".","");
            string fileName = "Card Checker " + dateString + ".log";
            using (StreamWriter writer = new StreamWriter(fileName, true))
            {
                
                Console.WriteLine(now + " ::: " + logItem);
                writer.WriteLine(now + " ::: "+logItem);
            }
        }

        public static LogWritter GetInstance()
        {
            return obj;
        }

    }
}
