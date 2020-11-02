using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClassLibrary
{
    public class ServerAsync : AbstractServer
    {
        byte[] msgLogin;
        byte[] msgPass;
        byte[] msgGood;
        byte[] msgBad;
        string login;
        string password;

        public delegate void TransmissionDataDelegate(NetworkStream stream);

        public ServerAsync(IPAddress IP, int port) : base(IP, port)
        {
            this.msgLogin = new ASCIIEncoding().GetBytes("Podaj login: ");
            this.msgPass = new ASCIIEncoding().GetBytes("Podaj haslo: ");
            this.msgGood = new ASCIIEncoding().GetBytes("Witaj na serwerze!");
            this.msgBad = new ASCIIEncoding().GetBytes("Bledny login lub haslo!\r\n");
        }

        protected void signIn(NetworkStream stream, byte[] buffer)
        {
            readPass();
            char[] trim = { (char)0x0 };
            while (true)
            {
                stream.Write(msgLogin, 0, msgLogin.Length);
                int dlugosc = stream.Read(buffer, 0, buffer.Length);
                if (Encoding.ASCII.GetString(buffer, 0, dlugosc) == "\r\n")
                {
                    stream.Read(buffer, 0, buffer.Length);
                }
                string login = Encoding.ASCII.GetString(buffer).Trim(trim);
                Array.Clear(buffer, 0, buffer.Length);

                stream.Write(msgPass, 0, msgPass.Length);
                dlugosc = stream.Read(buffer, 0, buffer.Length);
                if (Encoding.ASCII.GetString(buffer, 0, dlugosc) == "\r\n")
                {
                    stream.Read(buffer, 0, buffer.Length);
                }
                string password = Encoding.ASCII.GetString(buffer).Trim(trim);
                Array.Clear(buffer, 0, buffer.Length);

                if (login == this.login && password == this.password)
                {
                    stream.Write(msgGood, 0, msgGood.Length);
                    break;
                }
                else
                {
                    stream.Write(msgBad, 0, msgBad.Length);
                }
            }
        }

        protected void readPass()
        {
            using(System.IO.StreamReader file = new StreamReader(@"pass.txt"))
            {
                login = file.ReadLine();
                password = file.ReadLine();
            }       
        }

        protected void writePass()
        {
            string[] lines = { this.login, this.password };
            System.IO.File.WriteAllLines(@"pass.txt", lines);
        }



        protected void choice(NetworkStream stream, byte[] buffer)
        {
            char[] trim = { (char)0x0 };
            byte[] choiceText = new ASCIIEncoding().GetBytes("Co chcesz zrobic?\r\n1. Zalogowac sie.\r\n2. Zmienic login i haslo.\r\nWybor: ");
            byte[] wrongChoice = new ASCIIEncoding().GetBytes("Zly numer\r\n");
            bool flag = true;
            while(true)
            {
                if (flag == true)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    stream.Write(choiceText, 0, choiceText.Length);
                    int dlugosc = stream.Read(buffer, 0, buffer.Length);
                    if (Encoding.ASCII.GetString(buffer, 0, dlugosc) == "\r\n")
                    {
                        stream.Read(buffer, 0, buffer.Length);
                    }
                    string num = Encoding.ASCII.GetString(buffer).Trim(trim);
                    Array.Clear(buffer, 0, buffer.Length);
                    if (num == "1" || num == "1\n")
                    {
                        signIn(stream, buffer);
                        flag = false;
                    }
                    else if(num == "2" || num == "2\n")
                    {
                        stream.Write(msgLogin, 0, msgLogin.Length);
                        dlugosc = stream.Read(buffer, 0, buffer.Length);
                        if (Encoding.ASCII.GetString(buffer, 0, dlugosc) == "\r\n")
                        {
                            stream.Read(buffer, 0, buffer.Length);
                        }
                        this.login = Encoding.ASCII.GetString(buffer).Trim(trim);
                        Array.Clear(buffer, 0, buffer.Length);

                        stream.Write(msgPass, 0, msgPass.Length);
                        dlugosc = stream.Read(buffer, 0, buffer.Length);
                        if (Encoding.ASCII.GetString(buffer, 0, dlugosc) == "\r\n")
                        {
                            stream.Read(buffer, 0, buffer.Length);
                        }
                        this.password = Encoding.ASCII.GetString(buffer).Trim(trim);
                        Array.Clear(buffer, 0, buffer.Length);
                        writePass();
                    }
                    else
                    {
                        stream.Write(wrongChoice, 0, wrongChoice.Length);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        protected override void AcceptClient()
        {
            while (true)
            {
                TcpClient tcpClient = TcpListener.AcceptTcpClient();
                Stream = tcpClient.GetStream();
                TransmissionDataDelegate transmissionDelegate = new TransmissionDataDelegate(BeginDataTransmission);
                transmissionDelegate.BeginInvoke(Stream, TransmissionCallback, tcpClient);
            }
        }

        private void TransmissionCallback(IAsyncResult ar)
        {

        }

        protected override void BeginDataTransmission(NetworkStream stream)
        {
            byte[] buffer = new byte[Buffer_size];
            try
            {
                choice(stream, buffer);
            }
            catch (System.IO.IOException e) { }
        }

        public override void Start()
        {
            StartListening();
            AcceptClient();
        }
    }
}
