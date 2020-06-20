using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSA
{
    class RSA_MAIN
    {
        /// <summary>
        /// Точка входа приложения
        /// </summary>
        static void Main(string[] args)
        {
           
            Console.WriteLine("ЭЦП RSA");
            Console.WriteLine("Введите номер действия:");
            Console.WriteLine("0) Выход из программы.");
            Console.WriteLine("1) Создать файлы с закрытым и открытым ключом.");
            Console.WriteLine("2) \"Подписать\" файл.");
            Console.WriteLine("3) Проверить \"подпись\" файла.");
            int a = Int32.Parse(Console.ReadLine());
            do
            {
                switch (a)
                {
                    case 0:
                        {
                            return;
                        }
                    case 1: CreateFilesOfKeys(); break;
                    case 2:
                        {
                            Console.WriteLine("Введите имя подписываемого файла:");
                            string filename = Console.ReadLine();
                            CreateFilesOfSign(filename);
                            break;
                        }
                    case 3:
                        {
                            Console.WriteLine("Введите имя проверяемого файла:");
                            string filename = Console.ReadLine();
                            VerificationSignature(filename);
                            break;
                        }
                }
                Console.WriteLine("Введите номер действия:");
                a = Int32.Parse(Console.ReadLine());
            }
            while (a != 0);
            
        }
      
        /// <summary>
        /// Создает файлы "OpenKey.txt" с открытым ключом и "Secret.txt" c закрытым
        /// </summary>
        static void CreateFilesOfKeys()
        {
            RSA rsa = new RSA();
            BigInt32 E = new BigInt32(rsa.e);
            BigInt32 D = new BigInt32(rsa.d);
            BigInt32 N = new BigInt32(rsa.n);

            FileStream Stream = new FileStream("OpenKey.txt", FileMode.Create, FileAccess.Write);
            StreamWriter Writer = new StreamWriter(Stream);
            Writer.WriteLine(E);
            Writer.WriteLine(N);
            Writer.Close();
            Stream.Close();

            Stream = new FileStream("SecretKey.txt", FileMode.Create, FileAccess.Write);
            Writer = new StreamWriter(Stream);
            Writer.WriteLine(D);
            Writer.WriteLine(N);
            Writer.Close();
            Stream.Close();

            Console.WriteLine("Файлы созданы.");
        }
        /// <summary>
        /// Создание ЭЦП файла
        /// </summary>
        /// <param name="filename">Имя файла, который мы хотим подписать</param>
        static void CreateFilesOfSign(string filename)
        {
            Encoding enc = Encoding.Default;
            byte[] test_byte = File.ReadAllBytes(filename);    // байтовое представление исходного файла
       
            FileStream Stream = new FileStream("SecretKey.txt", FileMode.Open, FileAccess.Read);
            StreamReader Reader = new StreamReader(Stream);
            string D = Reader.ReadLine();
            string N = Reader.ReadLine();
            Reader.Close();
            Stream.Close();

            BigInt32 d = new BigInt32(D);
            BigInt32 n = new BigInt32(N);
            BigInt32 Sign = RSA.CreateSignature(test_byte, d, n);      // ЭЦП файла
            string FileNameIsSignature = "Sign_" + filename;
            
            Stream = new FileStream(FileNameIsSignature, FileMode.Create, FileAccess.Write);
            StreamWriter Writer = new StreamWriter(Stream);
            Writer.WriteLine(Sign);
            Writer.WriteLine(filename);
            Writer.Close();
            Stream.Close();

            Stream = new FileStream(FileNameIsSignature, FileMode.Append, FileAccess.Write);
            BinaryWriter WriterByte = new BinaryWriter(Stream);
            WriterByte.Write(test_byte);
            WriterByte.Close();
            Console.WriteLine("Cоздан файл: {0}",FileNameIsSignature);
        }
        /// <summary>
        /// Проверка цифровой подписи файла на подлинность
        /// </summary>
        /// <param name="filename"> Имя подписанного файла</param>
        static void VerificationSignature(string filename)
        {
            FileStream Stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader Reader = new StreamReader(Stream);
            string Sign = Reader.ReadLine();        //подпись
            string Test_file = Reader.ReadLine();     //имя проверяемого файла
            Reader.Close();
            Stream.Close();

            Stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader ReaderBinary = new BinaryReader(Stream);
            FileInfo f = new FileInfo(filename);

            int fil = (int)f.Length - (Sign.Length + Test_file.Length);
            byte[] s1 = ReaderBinary.ReadBytes(Sign.Length + 2);      //
            byte[] s2 = ReaderBinary.ReadBytes(Test_file.Length + 2); //
            byte[] test_byte = ReaderBinary.ReadBytes(fil);          // байтовое представление проверяемых данные

            Stream = new FileStream("OpenKey.txt", FileMode.Open, FileAccess.Read);
            Reader = new StreamReader(Stream);
            string E = Reader.ReadLine();
            string N = Reader.ReadLine();
            Reader.Close();
            Stream.Close();

            Encoding enc = Encoding.Default;

         
            BigInt32 Signature = new BigInt32(Sign);        //подпись
            BigInt32 e = new BigInt32(E);                   //открытый ключ
            BigInt32 n = new BigInt32(N);                   //модуль
            bool finish = false;
            
            finish = RSA.VerifySignature(test_byte, Signature, e, n);  // проверка ЭЦП             
            if (finish)
            {
                Console.WriteLine("Подпись верна.");
                string str_new = "New_" + Test_file;
                Stream = new FileStream(str_new, FileMode.Create, FileAccess.Write);
                BinaryWriter Writer = new BinaryWriter(Stream);
                Writer.Write(test_byte);
                Writer.Close();
                Stream.Close();
                Console.WriteLine("Создан файл: {0}",str_new);
            }
            else
                Console.WriteLine("Подпись не верна");
        }
    }
}
