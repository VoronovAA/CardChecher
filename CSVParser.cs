using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CardChecher
{   

    class CSVParser
       
    {
        public List<RowItem> itemsList { get; set; }
        private RowItem maxRow;
        private int takes;
        private void Parse(String fileDir, string targetDate, Boolean withLastItem)
        {
            bool rowFound = false;
            StreamReader reader = null;
            RowItem lastRow = new RowItem();
            if ((File.Exists(@"lastitem.json")) && (withLastItem == true))
            {
                reader = new StreamReader(File.Open(@"lastitem.json", FileMode.Open, FileAccess.Read));
                lastRow = JsonConvert.DeserializeObject<RowItem>(reader.ReadToEnd());
                reader.Close();
            }
            String fullPath = "";
            if (targetDate == null)
            {
                fullPath = fileDir + "GKMS.csv";
            } 
            else
            {
                
                fullPath = fileDir + "GK" + DateTime.Parse(targetDate).ToString("ddMMyyyy") + ".csv";
            }

            if (this.takes < 31 || !withLastItem)
            {
                this.takes++;
                if (File.Exists(fullPath))
                {
                    FileStream fileStreamIn = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    FileInfo fi = new FileInfo(fullPath);
                    //reader = new StreamReader(fileStreamIn);
                    byte[] bytes = new byte[fi.Length];
                    string[] lines;
                    int bytesRead = fileStreamIn.Read(bytes, 0, bytes.Length);
                    String fileString = System.Text.Encoding.ASCII.GetString(bytes);
                    lines = fileString.Split('\n');
           
                    if (this.maxRow == null && lastRow != null)
                    {
                        this.maxRow = lastRow;
                    }
                    foreach (String eachRow in lines) //(!reader.EndOfStream)
                    {
                        if (!String.IsNullOrWhiteSpace(eachRow) && !eachRow.Contains("Event date"))
                        {
                            var line = eachRow;
                            RowItem row = new RowItem(line);
                            if (row.Equals(lastRow) || lastRow.id == 0)
                            {
                                rowFound = true;
                            }
                            if (row.Greater(lastRow))
                            {
                                this.itemsList.Add(row);
                            }
                            if (row.Greater(this.maxRow))
                            {
                                this.maxRow = row;
                            }
                        }
                    }
                    fileStreamIn.Dispose();
                }
                
                if (!rowFound)
                {
                    DateTime startDate = DateTime.Today;
                    if (targetDate != null)
                    {
                        startDate = DateTime.Parse(targetDate);
                    }
                    startDate = startDate.AddDays(-1);
                    //String targetFileDate = startDate.ToString("ddMMyyyy");
                    this.Parse(fileDir, startDate.ToString(), true);
                }
            }
            else
            {
                this.Parse(fileDir, targetDate, false);
            }
        }

        public CSVParser(String fileDir, string targetDate)
        {
            this.itemsList = new List<RowItem>();
            this.maxRow = new RowItem();
            this.takes = 0;
            this.Parse(fileDir, targetDate, true);


        }
    }

    class RowItem
    {
        public string inputDateTime { get; set; }
        public int direct { get; set; }
        public int eventNum { get; set; }
        public string keyNum { get; set; }
        public Int64 id { get; set; }

        public RowItem()
        {
            DateTime today = DateTime.Today;
            today = today.AddHours(-1);
            this.inputDateTime = today.ToString("yyyy-MM-dd HH:mm:ss zz");
            this.id = 0;
        }
        public RowItem(string inputLine)
        {

            if (!String.IsNullOrWhiteSpace(inputLine)) {
                var inputArray = inputLine.Split(';');
                DateTime dateTimeTmp;
                if (DateTime.TryParse(inputArray[0], out dateTimeTmp))
                {
                    //DateTime dateTimeTmp = DateTime.Parse(inputArray[0]);
                    this.inputDateTime = dateTimeTmp.ToString("yyyy-MM-dd HH:mm:ss zz");
                    this.direct = int.Parse(inputArray[1]);
                    this.eventNum = int.Parse(inputArray[2]);
                    this.keyNum = inputArray[3];
                    this.id = Int64.Parse(inputArray[inputArray.Count() - 2]);
                }
            }
          
        }

        public bool Greater(RowItem obj)
        {
            DateTime thisDateTime = DateTime.Parse((String)this.inputDateTime);
            DateTime objDateTime = DateTime.Parse((String)obj.inputDateTime);
            int result = DateTime.Compare(thisDateTime, objDateTime);
            if (result < 0) {
                return false;
            } else if (result > 0)
            {
                return true;
            } else
            {
                return this.id > obj.id;
            }
        }

        public bool Equals(RowItem obj)
        {
            DateTime thisDateTime = DateTime.Parse((String)this.inputDateTime);
            DateTime objDateTime = DateTime.Parse((String)obj.inputDateTime);
            int result = DateTime.Compare(thisDateTime, objDateTime);
            if (result == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return inputDateTime + ":" + direct + ":" + eventNum + ":" + keyNum + ":" + id;
        }

    }
}
