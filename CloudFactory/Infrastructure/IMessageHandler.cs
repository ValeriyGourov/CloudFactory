using Microsoft.AspNetCore.Http;

namespace CloudFactory.Infrastructure
{
    /// <summary>
    /// Контракт обработчика сообщений, используемый брокером сообщений для управления потоком сообщений.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Обрабатывает входящий запрос и сохраняет его для дальнейшей работы.
        /// </summary>
        /// <param name="request">Входящий HTTP-запрос.</param>
        /// <returns>Сформированный ключ запроса.</returns>
        string CreateRequest(HttpRequest request);

        /// <summary>
        /// Формирует ответ для запроса, запрашивающего его по соответствующему ключу запроса.
        /// </summary>
        /// <param name="requestKey">Ключ запроса.</param>
        /// <returns>HTTP-код и тело ответа, предоставленные бэкэндом.</returns>
        (int statusCode, string answer) GetResponse(string requestKey);
    }
}
