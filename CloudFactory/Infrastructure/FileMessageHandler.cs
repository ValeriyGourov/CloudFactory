using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CloudFactory.Infrastructure
{
    /// <summary>
    /// Обработчик сообщений, обрабатывающий запросы и ответы в виде файлов на диске.
    /// </summary>
    public class FileMessageHandler : IMessageHandler
    {
        /// <summary>
        /// Полный путь к папке, в которой будут храниться файлы запросов и ответов.
        /// </summary>
        private readonly string _storageFolder;

        /// <summary>
        /// Расширение файлов запросов.
        /// </summary>
        private const string _requestFileExtension = ".req";

        /// <summary>
        /// Расширение файлов ответов.
        /// </summary>
        private const string _responseFileExtension = ".resp";

        /// <summary>
        /// Объект блокировки для предотвращающий конкурентной работы с файлами.
        /// </summary>
        private static readonly object _storageFolderLock = new object();   // Для простоты блокировку делаем для всех операций с файлами, а не в разрезе отдельного запроса.

        /// <summary>
        /// Основной конструктор.
        /// </summary>
        /// <param name="config">Конфигурация приложения.</param>
        /// <param name="env">Информация об окружении размещения.</param>
        public FileMessageHandler(IConfiguration config, IHostingEnvironment env)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            _storageFolder = config.GetValue("StorageFolder", Path.Combine(env.ContentRootPath, "BrokerFolder"));
            if (!Directory.Exists(_storageFolder))
            {
                throw new ApplicationException("Не указана папка брокера.");
            }
        }

        /// <summary>
        /// Обрабатывает входящий запрос и сохраняет его для дальнейшей работы.
        /// </summary>
        /// <param name="request">Входящий HTTP-запрос.</param>
        /// <returns>Сформированный ключ запроса.</returns>
        public string CreateRequest(HttpRequest request)
        {
            string source = request.Method + request.Path;
            string requestKey;
            using (MD5 md5Hash = MD5.Create())
            {
                requestKey = GetMd5Hash(md5Hash, source);
            }

            string filePath = GetRequestFilePath(requestKey);
            lock (_storageFolderLock)
            {
                using (StreamWriter file = new StreamWriter(filePath))
                {
                }
            }

            return requestKey;
        }

        /// <summary>
        /// Формирует ответ для запроса, запрашивающего его по соответствующему ключу запроса.
        /// </summary>
        /// <param name="requestKey">Ключ запроса.</param>
        /// <returns>HTTP-код и тело ответа, предоставленные бэкэндом.</returns>
        public (int statusCode, string answer) GetResponse(string requestKey)
        {
            string requestFilePath = GetRequestFilePath(requestKey);
            string responseFilePath = GetResponseFilePath(requestKey);

            string statusCodeString;
            string answer;
            lock (_storageFolderLock)
            {
                if (!File.Exists(responseFilePath))
                {
                    return (StatusCodes.Status204NoContent, null);
                }

                using (StreamReader file = new StreamReader(responseFilePath))
                {
                    statusCodeString = file.ReadLine();
                    answer = file.ReadToEnd();
                }
            }

            if (!int.TryParse(statusCodeString, out int statusCode))
            {
                return (StatusCodes.Status500InternalServerError, null);
            }

            try
            {
                File.Delete(requestFilePath);
            }
            catch (Exception)
            {
                // TODO: Тут нужно что-то делать.
            }
            try
            {
                File.Delete(responseFilePath);
            }
            catch (Exception)
            {
                // TODO: Тут нужно что-то делать.
            }

            return (statusCode, answer);
        }

        /// <summary>
        /// Возвращает контрольное значение, рассчитанное по алгоритму MD5 на основе входящей строки.
        /// </summary>
        /// <param name="md5Hash">Объект, который выполнить расчёт контрольного значения.</param>
        /// <param name="input">Строка, на основе которой будет рассчитано контрольное значение.</param>
        /// <returns>Рассчитанное контрольное значение.</returns>
        private string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                stringBuilder.Append(data[i].ToString("x2"));
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Формирует путь к файлу запроса.
        /// </summary>
        /// <param name="requestKey">Ключ запроса.</param>
        /// <returns>Путь к файлу запроса.</returns>
        private string GetRequestFilePath(string requestKey) => Path.Combine(_storageFolder, requestKey + _requestFileExtension);

        /// <summary>
        /// Формирует путь к файлу ответа.
        /// </summary>
        /// <param name="requestKey">Ключ запроса.</param>
        /// <returns>Путь к файлу ответа.</returns>
        private string GetResponseFilePath(string requestKey) => Path.Combine(_storageFolder, requestKey + _responseFileExtension);
    }
}
