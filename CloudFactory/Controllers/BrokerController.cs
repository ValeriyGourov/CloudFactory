using System;
using CloudFactory.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CloudFactory.Controllers
{
    /// <summary>
    /// Представляет брокера сообщений, который обрабатывает входящий HTTP-трафик, запрашивает бэкэнд и отдаёт его результат в ответе. Взаимодействие по выполнению запросов и получению ответов выполняется в асинхронном режиме.
    /// </summary>
    /// <remarks>
    /// Расчёт ключа запроса выполняется по формуле md5(HTTP-method + HTTP-path).
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class BrokerController : ControllerBase
    {
        /// <summary>
        /// Брокер сообщений.
        /// </summary>
        private MessageBroker _messageBroker;

        /// <summary>
        /// Основной конструктор, принимающий экземпляр брокера сообщений.
        /// </summary>
        /// <param name="messageBroker">Брокер сообщений.</param>
        public BrokerController(MessageBroker messageBroker)
        {
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
        }

        /// <summary>
        /// Возвращает ответ по полученному ранее ключу запроса.
        /// </summary>
        /// <param name="requestKey">Ключ запроса.</param>
        /// <returns>Ответ, предоставленный бэкэндом. HTTP-код так же устанавливается бэкэндом.</returns>
        /// <response code="400">Пустой ключ запроса.</response>
        [HttpGet("Answer")]
        public ActionResult<string> Answer(string requestKey)
        {
            if (string.IsNullOrWhiteSpace(requestKey))
            {
                return BadRequest("Ключ запроса не может быть пустым.");
            }
            return _messageBroker.GetResponse(requestKey);
        }

        /// <summary>
        /// Тестовый GET-метод.
        /// </summary>
        /// <returns>Статус "200 OK" и сформированный ключ запроса.</returns>
        /// <response code="200">Сформированный ключ запроса.</response>
        [HttpGet("Get1")]
        public ActionResult<string> Get1()
        {
            return _messageBroker.CreateRequest(Request);
        }

        /// <summary>
        /// Тестовый GET-метод.
        /// </summary>
        /// <returns>Статус "200 OK" и сформированный ключ запроса.</returns>
        /// <response code="200">Сформированный ключ запроса.</response>
        [HttpGet("Get2")]
        public ActionResult<string> Get2()
        {
            return _messageBroker.CreateRequest(Request);
        }

        /// <summary>
        /// Тестовый POST-метод.
        /// </summary>
        /// <returns>Статус "200 OK" и сформированный ключ запроса.</returns>
        /// <response code="200">Сформированный ключ запроса.</response>
        [HttpPost("Post1")]
        public ActionResult<string> Post1()
        {
            return _messageBroker.CreateRequest(Request);
        }
    }
}