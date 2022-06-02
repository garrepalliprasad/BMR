using System;
using System.Net.Http;

namespace Calculator.WebAPP.Helper
{
    public class CalculatorAPI
    {
        public HttpClient Initial()
        {
            var client=new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44387/");
            client.Timeout=TimeSpan.FromMinutes(5);
            return client;
        }
    }
}
