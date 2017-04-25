using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Reflection;

namespace CardChecher
{
    class Program
    {
        static void Main(string[] args)
        {
            //LogWritter log = LogWritter.GetInstance();

            try
            {
                Config config = new Config();
                if (CheckForInternetConnection())
                {
                    SenderDataToSF sender = new SenderDataToSF(config);
                    while (true)
                    {
                        if (CheckForInternetConnection())
                        {
                            try
                            {
                                CSVParser result = new CSVParser(config.item.fileDir, null);
                                LogWritter.GetInstance().WriteLog("Objects to send : " + result.itemsList.Count());
                                if (result.itemsList.Count() > 0)
                                {
                                    sender.SendData(result.itemsList);
                                    LogWritter.GetInstance().WriteLog("Data send successfull");
                                }
                            }
                            catch (NullReferenceException e)
                            {
                                LogWritter.GetInstance().WriteLog("Error : " + e.Message + "\n Stack trace: " + e.ToString());
                                LogWritter.GetInstance().WriteLog(" Application closed  ");
                                Thread.Sleep(60000);
                            }

                        }
                        Thread.Sleep(config.item.timeout * 1000);
                    }
                } else {
                    LogWritter.GetInstance().WriteLog(" Connection is interapted  ");
                    LogWritter.GetInstance().WriteLog(" Application closed  ");
                    Thread.Sleep(60000);
                    var fileName = Assembly.GetExecutingAssembly().Location;
                    System.Diagnostics.Process.Start(fileName);
                    Environment.Exit(0);
                }
  
            } 
            catch (Exception e)
            {
                LogWritter.GetInstance().WriteLog("Error : " + e.Message+ "\n Stack trace: " + e.ToString());
                LogWritter.GetInstance().WriteLog(" Application closed  ");
                Thread.Sleep(60000);
                var fileName = Assembly.GetExecutingAssembly().Location;
                System.Diagnostics.Process.Start(fileName);
                Environment.Exit(0);
            }
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

    }
}
