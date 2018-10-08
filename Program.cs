using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace ConsoleApplication1
{
    class Program
    {
        static string GetHTMLCode(string urlAddress)
        {
            string data = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }

            return data;
        }

        static void CompareData(string url, string senderAccount, string password)
        {
            string oldData = "", newData = "";
            int mSecs = 600000; // Cada 10 minutos
            int initI, endI;

            while (true)
            {
                if (oldData.Equals(""))
                    Console.WriteLine("Obteniendo HTML de: " + url);
                else
                    Console.WriteLine("Comprobando cambios en la URL: " + url);

                newData = GetHTMLCode(url);

                // Parte de los tickets web
                initI = newData.IndexOf("id=\"progress-container\"");
                 endI = newData.IndexOf("END Related Cards");

                newData = newData.Substring(initI, endI - initI);

                // Si oldData no está vacío...
                if (String.Compare(oldData, "") != 0)
                {
                    // Si los strings son distintos...
                    if(String.Compare(oldData, newData) == 0)
                    {
                        Console.WriteLine("No hay cambios en la URL");
                    }

                    else
                    {
                        MailMessage mail = new MailMessage();
                        mail.To.Add(senderAccount);
                        mail.From = new MailAddress(senderAccount);
                        mail.Subject = "Comprobador web";

                        mail.Body = "Correo enviado automáticamente.";

                        mail.IsBodyHtml = true;
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "smtp.gmail.com"; //Or Your SMTP Server Address
                        smtp.Credentials = new System.Net.NetworkCredential
                             (senderAccount, password); // ***use valid credentials***
                        smtp.Port = 587;

                        //Or your Smtp Email ID and Password
                        smtp.EnableSsl = true;
                        smtp.Send(mail);

                        Console.WriteLine("\nCorreo enviado a " + senderAccount);
                        return;
                    }
                }

                oldData = newData;

                Console.WriteLine("\n------ Durmiendo el hilo " + mSecs/1000/60 + " minutos ------\n");
                Thread.Sleep(mSecs);
            }
        }

        static void Main(string[] args)
        {
            Console.Title = "Comprobador web";

            CompareData("https://www.ticketea.com/organizers/elhormiguero/", "CUENTA@gmail.com", "CONTRASEÑA");

            Console.WriteLine("\nFin del programa");
            Console.Read();
        }
    }
}