using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CardChecher
{
    class SenderDataToSF
    {
        Config config;
        TokenWrapper token;

        public SenderDataToSF(Config inputConfig)
        {
            this.config = inputConfig;
            this.CreateToken();
        }


        public void CreateToken()
        {
            string LoginUrl = "https://" + this.config.item.domain + ".salesforce.com/services/oauth2/token";
            //the line below enables TLS1.1 and TLS1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
            {
               new KeyValuePair<string, string>("grant_type", "password"),
               new KeyValuePair<string, string>("client_id", this.config.item.client_id),
               new KeyValuePair<string, string>("client_secret", this.config.item.client_secret),
               new KeyValuePair<string, string>("password", this.config.item.password+this.config.item.token),
               new KeyValuePair<string, string>("username", this.config.item.username)
            });
            HttpResponseMessage response;
            using (HttpClient client = new HttpClient())
            {
                response = client.PostAsync(LoginUrl, content).Result;
            }
            TokenWrapper tokenTMP = JsonConvert.DeserializeObject<TokenWrapper>(response.Content.ReadAsStringAsync().Result);
            if (tokenTMP.error != null)
            {
                throw new System.Security.SecurityException(" Security error,"+ tokenTMP.error +" error description "+ tokenTMP.error_description + " check the configuration ");
            }
            else
            {
                token = tokenTMP;
            } 

        }

        public void saveLastItem(RowItem lastRow)
        {
            using (StreamWriter writer = new StreamWriter(@"lastitem.json", false))
            {
                writer.Write(JsonConvert.SerializeObject(lastRow));
                writer.Close();
            }
        }

        public void SendData(List <RowItem> itemList)
        {
           
            List<ResponseWrapper> result;
            if (this.token.access_token != null)
            {
                try
                {
                    String jsonBody = JsonConvert.SerializeObject(itemList);
                    WebRequest request = (HttpWebRequest)WebRequest.Create(this.config.item.endpoint);
                    request.Method = "POST";
                    byte[] byteArray = Encoding.UTF8.GetBytes(jsonBody);
                    request.ContentType = "application/json";
                    request.Headers.Add("Authorization", "Bearer " + this.token.access_token);
                    request.ContentLength = byteArray.Length;
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                    //WebResponse response = request.GetResponse();
                    result = JsonConvert.DeserializeObject<List<ResponseWrapper>>(new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd());
                    this.saveLastItem(itemList[itemList.Count-1]);

                }
                catch (WebException ex)
                {
                    result = JsonConvert.DeserializeObject<List<ResponseWrapper>>(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
                }
                ResponseWrapper responseFromSF = result[0];
                if (responseFromSF.message.Equals("Session expired or invalid"))
                {
                    this.CreateToken();
                    this.SendData(itemList);
                }
                else
                {
                    if (!responseFromSF.message.Equals("success"))
                    {
                        LogWritter.GetInstance().WriteLog("Error from send request method: " + responseFromSF);
                    }
                    else
                    {
                        LogWritter.GetInstance().WriteLog("Response : "+ responseFromSF);
                    }
                }
            } 
            else
            {
                throw new System.Security.SecurityException(" Access token not created ");
            }
            
        }
  }


    class TokenWrapper
    {
        public string access_token { get; set; }
        public string instance_url { get; set; }
        public string id { get; set; }
        public string token_type { get; set; }
        public string issued_at { get; set; }
        public string signature { get; set; }
        public string error { get; set; }
        public string error_description { get; set; }

    }

    class ResponseWrapper
    {
        public string message { get; set; }
        public string errorCode { get; set; }

        public override string ToString()
        {
            return "message " + message + " errorCode " + errorCode;
        }
    }
}
