using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;

namespace MinerCommander_Client
{
    class Program
    {
      public static  Process[] processes;
        public static int miner;
        public static  string строканасервере;
        private static void ЗаписьЛога(string текстлога)
        {
            string путь = Environment.CurrentDirectory + "\\Logs\\" + "LOG_" + DateTime.Now.ToString("MM-dd-yy") + ".txt";

            using (FileStream fstream = new FileStream(путь, FileMode.Append))
            {
            }

            System.IO.StreamWriter writer = new System.IO.StreamWriter(путь, true);
            writer.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : " + текстлога);
            writer.Close();
        }
        static void Main(string[] args)
        {
           
            try {
              
                System.IO.Directory.CreateDirectory("Logs");
                Thread.Sleep(2100);
            XDocument xDocument = XDocument.Load("minercommander.xml");
                int минуты = Convert.ToInt32(xDocument.Element("miner").Element("commander").Attribute("reftime").Value);
                int миллисекунды = минуты * 60000;
                Console.WriteLine("Callback время " + минуты.ToString());
                Console.WriteLine("Timer started ");
                System.Timers.Timer timer = new System.Timers.Timer(миллисекунды);
                timer.Elapsed += HandleTimer;
                timer.Start();
                var myTask = ПолучитьСтрокуAsync();
                Console.Read();
            }
            catch (Exception ex){
                Console.ForegroundColor = ConsoleColor.Red; // устанавливаем цвет
                Console.WriteLine("Ошибка инициализации :" +ex.Message);
                ЗаписьЛога("Ошибка инициализации :" + ex.Message);
                Console.Read();
            }
        }
        private static void HandleTimer(Object source, System.Timers.ElapsedEventArgs e)
        {
            ЗаписьЛога("Новый запрос к XML файлу..");

            var myTask= ПолучитьСтрокуAsync();

        }

      
        protected static void SaveXML(string element, string attribut, string value)
        {
            XDocument xmlFile = XDocument.Load("minercommander.xml");

            var query = from c in xmlFile.Elements("miner").Elements(element) select c;

            foreach (XElement book in query)
            {
                book.Attribute(attribut).Value = value;
            }

            xmlFile.Save("minercommander.xml");
        }
        private static void ЗаписьСтроки(string строка)
        {

            try { 
            string путь = "start.bat";

            using (FileStream fstream = new FileStream(путь, FileMode.Create))
            {
            }
            System.IO.StreamWriter writer = new System.IO.StreamWriter(путь, true);
            writer.WriteLine(строка);
            writer.Close();
            Console.WriteLine("bat файл обновлен - закрываем поток.");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Ошибка при записи bat файла : " +ex.Message);
            }
        }
        private static bool ЗапущенМайнер()
        {
            miner = 0;
            try {
                ЗаписьЛога("Поиск процесса майнинга");
                processes = Process.GetProcessesByName("EthDcrMiner64");
                foreach (Process proc in processes)
            {
                    miner++;
                ЗаписьЛога("Найден процесс EthDcrMiner64");
                //proc.CloseMainWindow();
                //proc.WaitForExit();
            }
            processes = Process.GetProcessesByName("cmd");
            foreach (Process proc in processes)
            {
                    miner++;
                    ЗаписьЛога("Найден процесс cmd");
                    //proc.CloseMainWindow();
                    //proc.WaitForExit();
                }
            
            if (miner == 0)
            {
                ЗаписьЛога("Запущенный майнер не найден. Запуск майнера");
                return false;
                    
            }
            else
            {
                ЗаписьЛога("Запущенный майнер найден.");
                return true;
            }
            }
            catch {
                return true;
            }
        }
        public static async Task<Stream> GetYandexXML()
        {
            try {
                XDocument xDocument = XDocument.Load("minercommander.xml");
                var client = new WebDAVClient.Client(new NetworkCredential { UserName = xDocument.Element("miner").Element("yandex").Attribute("login").Value, Password = xDocument.Element("miner").Element("yandex").Attribute("password").Value });
            client.Server = xDocument.Element("miner").Element("yandex").Attribute("server").Value;
            client.BasePath = "/";
            var stream = await client.Download(xDocument.Element("miner").Element("yandex").Attribute("filename").Value);
            return stream;
            }
            catch(Exception ex)
            {
                ЗаписьЛога("При загрузки с Яндекс Диска произошла ошибка: " + ex.Message + ex.InnerException.Message);
                Console.ForegroundColor = ConsoleColor.Red; // устанавливаем цвет
                Console.WriteLine("При загрузки с Яндекс Диска произошла ошибка: " + ex.Message);
                return null;
            }
        }
        private static void ОчисткаЛогов()
        {
            try { 
            int колводоговоров = System.IO.Directory.GetFiles( Environment.CurrentDirectory + "\\Logs\\").Length;
            Thread.Sleep(2000);
            if (колводоговоров > 20)
            {
                Console.WriteLine("Проверка старых лог файлов...");
                Thread.Sleep(1000);
                DirectoryInfo dirInfo = new DirectoryInfo(Environment.CurrentDirectory + "\\Logs\\");

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    int left = (DateTime.Now - file.CreationTime).Days;

                    if (left > 10)
                    {
                            Console.WriteLine("Удаление старых лог файлов");
                            file.Delete();
                    }
                }
            }
            }
            catch
            {

            }
        }
        private static async Task ПолучитьСтрокуAsync()
        {
            начало:
            ОчисткаЛогов();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Gray; // устанавливаем цвет
            Console.WriteLine("Последний запрос: {0}\n",
            DateTime.Now.ToString("hh:mm:ss"));
            Console.ForegroundColor = ConsoleColor.DarkYellow; // устанавливаем цвет
            Console.WriteLine("*******************MINER COMMANDER*******************");
            Console.WriteLine("*******************ver.1.0.0.4***********************");
            Console.ForegroundColor = ConsoleColor.Green; // устанавливаем цвет
            Console.WriteLine("Получаем строку");
            try
            {
                XDocument xDocument = XDocument.Load("minercommander.xml");
                XDocument doc;
                
                string текущаястрока = File.ReadAllText("start.bat");
                ЗаписьЛога("Текущая строка : ");
                ЗаписьЛога(текущаястрока);
                string minercommand = xDocument.Element("miner").Element("GPU").Attribute("name").Value;
                //Получить строку на сервере и сравнить
                if(Convert.ToBoolean(xDocument.Element("miner").Element("yandex").Attribute("use").Value) == true)
                {
                    Console.WriteLine("Загрузка строки с ЯндексДиска");
                    //Берем с яндекс диска
                    Stream docc = await GetYandexXML();
                    if (docc != null) { 
                    doc = XDocument.Load(docc);
                    строканасервере = doc.Element("miners").Element(minercommand).Value;
                    SaveXML("commander", "laststr", строканасервере);
                    }
                    //Если с яндекса не удалось пробуем получить по стандарту
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red; // устанавливаем цвет
                        Console.WriteLine("XML файл с яндекс диска недоступен.");
                        if(Convert.ToBoolean(xDocument.Element("miner").Element("yandex").Attribute("usedefault").Value) == true)  //Если с яндекса не удалось пробуем получить по стандарту
                        {
                            Console.ForegroundColor = ConsoleColor.Green; // устанавливаем цвет
                            Console.WriteLine("Попытка получить строку с параметра urlxml");
                            doc = XDocument.Load(xDocument.Element("miner").Element("commander").Attribute("urlxml").Value);
                            строканасервере = doc.Element("miners").Element(minercommand).Value;
                            SaveXML("commander", "laststr", строканасервере);
                            goto далее;
                        }
                        else
                        {
                            Console.WriteLine("Обновление остановлено. Новая попытка спустя время след. обновления .... ");
                            return;
                        }
                    }
                }
                else { 
                doc = XDocument.Load(xDocument.Element("miner").Element("commander").Attribute("urlxml").Value);
                    строканасервере = doc.Element("miners").Element(minercommand).Value;
                    SaveXML("commander", "laststr", строканасервере);
                }
             далее:
                Console.ForegroundColor = ConsoleColor.Green; // устанавливаем цвет
                ЗаписьЛога("Строка на сервере: ");
                ЗаписьЛога(строканасервере);
                Console.WriteLine("Получена строка:");
                Console.WriteLine(строканасервере);
                Console.WriteLine("Сравниваем строки батника");
                Thread.Sleep(900);
                if (строканасервере.Trim()==текущаястрока.Trim())
                {
                    Console.ForegroundColor = ConsoleColor.Green; // устанавливаем цвет
                    Console.WriteLine("Bat файл не нуждается в обновление.");
                    ЗаписьЛога("Строки совпали");
                   if( ЗапущенМайнер() == false)
                    {
                        Console.ForegroundColor = ConsoleColor.Red; // устанавливаем цвет
                        Console.WriteLine("Не удалось найти запущенный майнер");
                        Console.ForegroundColor = ConsoleColor.Green; // устанавливаем цвет
                        Console.WriteLine("Запуск start.bat");
                        Process.Start("start.bat");
                        Console.WriteLine("Майнер запущен");
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red; // устанавливаем цвет
                    Console.WriteLine("Строки не совпали, перезаписываем строку и перезапускаем майнер");
                    Console.ForegroundColor = ConsoleColor.Green; // устанавливаем цвет
                    try
                    {
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Green; // устанавливаем цвет
                            Console.WriteLine("Закрываем майнер");
                            Thread.Sleep(1100);

                            processes = Process.GetProcessesByName("EthDcrMiner64");
                            foreach (Process proc in processes)
                            {
                                proc.Kill();
                                ЗаписьЛога("Процесс EthDcrMiner64 закрыт");
                                //proc.CloseMainWindow();
                                //proc.WaitForExit();
                            }
                            processes = Process.GetProcessesByName("cmd");
                            foreach (Process proc in processes)
                            {
                                proc.Kill();
                                ЗаписьЛога("Процесс cmd закрыт");
                                //proc.CloseMainWindow();
                                //proc.WaitForExit();
                            }
                            Console.ForegroundColor = ConsoleColor.Green; // устанавливаем цвет
                            Console.WriteLine("Майнер закрыт");
                            Console.WriteLine("Обновление bat файла запуска майнера ...");
                            Thread.Sleep(1200);
                            ЗаписьСтроки(строканасервере);
                            ЗаписьЛога("Новая строка записана");
                            ЗаписьЛога("Запуск майнера");
                            Thread.Sleep(500);
                            Process.Start("start.bat");
                            ЗаписьЛога("Майнер запущен");
                            Console.WriteLine("Майнер запущен");
                        }
                        catch (System.NullReferenceException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red; // устанавливаем цвет
                            Console.WriteLine("Запущеный майнер не найден");
                            ЗаписьЛога("Запущеный майнер не найден");
                            Console.ForegroundColor = ConsoleColor.Green; // устанавливаем цвет
                            Console.WriteLine("Обновляем bat файл и запускаем майнер");
                         
                            Thread.Sleep(1200);
                            ЗаписьСтроки(строканасервере);
                            ЗаписьЛога("Строка записана запуск майнера");
                            Thread.Sleep(500);
                            Process.Start("start.bat");
                            ЗаписьЛога("Майнер запущен");
                        }
                    }
                    catch(Exception ex) {
                        Console.ForegroundColor = ConsoleColor.Red; // устанавливаем цвет
                        Console.WriteLine("Ошибка при перезапуске майнера:"+ex.Message);
                        ЗаписьЛога("Ошибка при перезапуске майнера:" + ex.Message);
                        Console.WriteLine("Пробуем еще раз");
                        goto начало;
                    }
                }
              
            }
            catch(Exception ex) {
                XDocument xDocument = XDocument.Load("minercommander.xml");
                Console.ForegroundColor = ConsoleColor.Red; // устанавливаем цвет
                Console.WriteLine("Ошибка приложения:" + ex.Message + ex.HResult);
                ЗаписьЛога("Ошибка приложения:" + ex.Message + ex.HResult);
                if (ЗапущенМайнер() == false)
                {
                    Console.ForegroundColor = ConsoleColor.Red; // устанавливаем цвет
                    Console.WriteLine("Не удалось найти запущенный майнер");
                    Console.ForegroundColor = ConsoleColor.Green; // устанавливаем цвет
                    Console.WriteLine("Загрузка последней строки");
                    ЗаписьСтроки(xDocument.Element("miner").Element("commander").Attribute("laststr").Value);
                    Process.Start("start.bat");
                    Console.WriteLine("Майнер запущен");
                }
            }
            
        }
    }
}
