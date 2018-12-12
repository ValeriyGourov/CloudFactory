using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudFactory.Infrastructure
{
    /// <summary>
    /// Брокер сообщений, обрабатывающий входящие запросы и предоставляющий ответы клиентам.
    /// </summary>
    /// <remarks>
    /// Брокер сообщений существует в единственном экземпляре на всё приложение.
    /// </remarks>
    public class MessageBroker
    {
        /// <summary>
        /// Одиночный экземпляр брокера. Поддерживает отложенную инициализацию.
        /// </summary>
        private static readonly Lazy<MessageBroker> _instance = new Lazy<MessageBroker>(() => new MessageBroker());

        /// <summary>
        /// Обработчик сообщений, используемый брокером для управления потоком сообщений.
        /// </summary>
        private static IMessageHandler _messageHandler;

        /// <summary>
        /// Закрытый конструктор по умолчанию. Экземпляр брокера создаётся через фабричный метод.
        /// </summary>
        private MessageBroker()
        {
        }

        /// <summary>
        /// Фабричный метод для получения (и создания при необходимости) экземпляра брокера.
        /// </summary>
        /// <param name="messageHandler">Обработчик сообщений, используемый брокером для управления потоком сообщений.</param>
        /// <returns>Одиночный экземпляр брокера.</returns>
        public static MessageBroker GetInstance(IMessageHandler messageHandler)
        {
            if (!_instance.IsValueCreated)
            {
                _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            }

            return _instance.Value;
        }

        /// <summary>
        /// Обрабатывает входящий запрос и сохраняет его для дальнейшей работы.
        /// </summary>
        /// <param name="request">Входящий HTTP-запрос.</param>
        /// <returns>Сформированный ключ запроса.</returns>
        public string CreateRequest(HttpRequest request)
        {
            return _messageHandler.CreateRequest(request);
        }

        /// <summary>
        /// Формирует ответ для запроса, запрашивающего его по соответствующему ключу запроса.
        /// </summary>
        /// <param name="requestKey">Ключ запроса.</param>
        /// <returns>Ответ, предоставленный бэкэндом. HTTP-код так же устанавливается бэкэндом.</returns>
        public ActionResult<string> GetResponse(string requestKey)
        {
            (int statusCode, string answer) = _messageHandler.GetResponse(requestKey);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return new ContentResult
                {
                    Content = answer,
                    StatusCode = statusCode
                };
            }
            else
            {
                return new StatusCodeResult(statusCode);
            }
        }
    }
}
