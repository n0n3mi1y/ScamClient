using System;
using System.Collections.Generic;
using System.Linq;
using Global.ZennoLab.Json;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.ProjectModel;
using static ScamClient.TitaniumInstanceApiResponse;

namespace ScamClient
{
    public class ScamClient
    {
        private string ApiEndpoint { get;}
        private readonly IZennoPosterProjectModel _project;
        private string _titaniumEndpoint;

        /// <summary>
        /// Инициализация клиента
        /// </summary>
        /// <param name="project">Объект проекта (для вывода лога)</param>
        /// <param name="apiEndpoint">URL развернутого SCAM API</param>
        public ScamClient(IZennoPosterProjectModel project, string apiEndpoint)
        {
            var uri = new Uri(apiEndpoint);
            
            this.ApiEndpoint = uri.AbsoluteUri + "scamapi";
            this._project = project;
            
        }

        public void Ok()
        {
            return;
        }

        /// <summary>
        /// Создание нового инстанса Titanium
        /// </summary>
        /// <param name="proxy_row">Строка прокси в формате ZennoPoster <b>protocol://log:pas@host:port</b></param>
        /// <param name="proxyKillDelay">Таймаут жизни инстанса. Инстанс автоматически будет отключен по таймауту, если через него не будут проходить запросы через указанное количество секунд</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public NewInstanceOk NewInstance(string proxy_row = null, int proxyKillDelay = 300)
        {
            var resp = ScamGet($"{ApiEndpoint}/newInstance?proxy_row={proxy_row}&proxyKillDelay={proxyKillDelay}");
            if (!string.IsNullOrEmpty(resp))
            {
                if (JsonConvert.DeserializeObject<NewInstanceBase>(resp).IsSuccess)
                {
                    var deserialized = JsonConvert.DeserializeObject<NewInstanceOk>(resp);
                    return deserialized;
                }
                else
                {
                    var deserialized = JsonConvert.DeserializeObject<NewInstanceBad>(resp);
                    _project.SendErrorToLog("Не удалось создать новый инстанс Titanium, ошибка API: " + deserialized.ErrorText);
                    
                }
            }

            else _project.SendErrorToLog("Не удалось создать новый инстанс Titanium, ответ API: " + resp);
            throw new Exception("Ошибка при создании нового инстанса Titanium");
        }

        /// <summary>
        /// Объявление эндпоинт инстанса Titanium в рамках клиента. Обязательная процедура перед выполнением любых действий
        /// </summary>
        /// <param name="instanceEndpoint"></param>
        public void AttachTitaniumInstance(string instanceEndpoint)
        {
            _titaniumEndpoint = instanceEndpoint;
        }

        /// <summary>
        /// Запросить перечень всех инстансов Titanium
        /// </summary>
        /// <returns>Перечень инстансов</returns>
        /// <exception cref="Exception"></exception>
        public List<GetInstances> GetInstances()
        {
            var resp = ScamGet($"{ApiEndpoint}/getInstances");  
            if (!string.IsNullOrEmpty(resp))
            {
                var deserialized = JsonConvert.DeserializeObject<List<GetInstances>>(resp);
                return deserialized;
            }
            else _project.SendErrorToLog("Не удалось запросить перечень инстансов Titanium, ответ API: " + resp);
            throw new Exception("Ошибка при запросе перечня инстансов");
        }

        /// <summary>
        /// Замена конечного URL. 
        /// </summary>
        /// <param name="replaceData">Данные для подмены</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public AddDomainForChangeResponse AddDomainForChange(DomainsForReplace replaceData)
        {
            if (string.IsNullOrEmpty(_titaniumEndpoint))
            {
                _project.SendErrorToLog($"Не объявлен эндпоинт инстанса. Предварительно воспользуйтесь методом AttachTitaniumInstance");
                throw new Exception("Инстанс Titanium не объявлен");
            }

            var resp = ScamPost($"{ApiEndpoint}/addDomainForChange?endPoint={_titaniumEndpoint}",
                JsonConvert.SerializeObject(replaceData));
            if (!string.IsNullOrEmpty(resp))
            {
                var deserialized = JsonConvert.DeserializeObject<AddDomainForChangeResponse>(resp);
                if (deserialized.IsSuccess)
                    return deserialized;
                else
                {
                    _project.SendErrorToLog($"Не удалось выполнить подмену url, ошибка: "  + deserialized.ErrorText); 
                    
                }
            }
            else _project.SendErrorToLog("Не удалось выполнить подмену url, ответ API: " + resp);
            throw new Exception("Ошибка при AddDomainForChange");
        }

        /// <summary>
        /// Замена текстовых данных в запросе
        /// </summary>
        /// <param name="replaceData">Данные для замены</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public AddChangeBodyResponse AddChangeRequest(ChangeBody replaceData)
        {
            if (string.IsNullOrEmpty(_titaniumEndpoint))
            {
                _project.SendErrorToLog($"Не объявлен эндпоинт инстанса. Предварительно воспользуйтесь методом AttachTitaniumInstance");
                throw new Exception("Инстанс Titanium не объявлен");
            }

            var resp = ScamPost($"{ApiEndpoint}/addChangeRequest?endPoint={_titaniumEndpoint}",
                JsonConvert.SerializeObject(replaceData));
            if (!string.IsNullOrEmpty(resp))
            {
                var deserialized = JsonConvert.DeserializeObject<AddChangeBodyResponse>(resp);
                if (deserialized.IsSuccess)
                    return deserialized;
                else
                {
                    _project.SendErrorToLog($"Не удалось выполнить подмену request, ошибка: " + deserialized.ErrorText);

                }
            }
            else _project.SendErrorToLog("Не удалось выполнить подмену request, ответ API: " + resp);
            throw new Exception("Ошибка при AddChangeRequest");
        }

        /// <summary>
        /// Замена текстовых данных в ответе
        /// </summary>
        /// <param name="replaceData">Данные для замены</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public AddChangeBodyResponse AddChangeResponse(ChangeBody replaceData)
        {
            if (string.IsNullOrEmpty(_titaniumEndpoint))
            {
                _project.SendErrorToLog($"Не объявлен эндпоинт инстанса. Предварительно воспользуйтесь методом AttachTitaniumInstance");
                throw new Exception("Инстанс Titanium не объявлен");
            }

            var resp = ScamPost($"{ApiEndpoint}/addChangeResponse?endPoint={_titaniumEndpoint}",
                JsonConvert.SerializeObject(replaceData));
            if (!string.IsNullOrEmpty(resp))
            {
                var deserialized = JsonConvert.DeserializeObject<AddChangeBodyResponse>(resp);
                if (deserialized.IsSuccess)
                    return deserialized;
                else
                {
                    _project.SendErrorToLog($"Не удалось выполнить подмену request, ошибка: " + deserialized.ErrorText);

                }
            }
            else _project.SendErrorToLog("Не удалось выполнить подмену request, ответ API: " + resp);
            throw new Exception("Ошибка при AddChangeBodyResponse");
        }

        /// <summary>
        /// Очистка массива с трафиком
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public GetTrafficResponse ClearTraffic()
        {
            if (string.IsNullOrEmpty(_titaniumEndpoint))
            {
                _project.SendErrorToLog($"Не объявлен эндпоинт инстанса. Предварительно воспользуйтесь методом AttachTitaniumInstance");
                throw new Exception("Инстанс Titanium не объявлен");
            }

            var resp = ScamGet($"{ApiEndpoint}/clearTraffic?endPoint={_titaniumEndpoint}");
            if (!string.IsNullOrEmpty(resp))
            {
                var deserialized = JsonConvert.DeserializeObject<GetTrafficResponse>(resp);
                if (deserialized.IsSuccess)
                {
                    _project.SendInfoToLog($"Трафик успешно очищен");
                    return deserialized;
                }
                else
                {
                    _project.SendErrorToLog($"Трафик не был очищен! Возможно, инстанс уже не существует.");
                }
            }
            else _project.SendErrorToLog("Пришел пустой ответ при попытке выполнить очистку трафика");

            throw new Exception("Ошибка при попытке очистки трафика");
        }

        /// <summary>
        /// Запрос трафика
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public GetTrafficResponse GetTraffic()
        {
            if (string.IsNullOrEmpty(_titaniumEndpoint))
            {
                _project.SendErrorToLog($"Не объявлен эндпоинт инстанса. Предварительно воспользуйтесь методом AttachTitaniumInstance");
                throw new Exception("Инстанс Titanium не объявлен");
            }

            var resp = ScamGet($"{ApiEndpoint}/getTraffic?endPoint={_titaniumEndpoint}");
            if (!string.IsNullOrEmpty(resp))
            {
                var deserialized = JsonConvert.DeserializeObject<GetTrafficResponse>(resp);
                if (deserialized.IsSuccess)
                {
                    _project.SendInfoToLog($"Трафик успешно получен");
                    return deserialized;
                }
                else
                {
                    _project.SendErrorToLog($"Трафик не был получен! Возможно, инстанс уже не существует.");
                }
            }
            else _project.SendErrorToLog("Пришел пустой ответ при попытке выполнить получение трафика");

            throw new Exception("Ошибка при попытке получить трафика");
        }

        /// <summary>
        /// Приватный метод для GET запроса
        /// </summary>
        /// <param name="url">Целевой URL</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string ScamGet(string url)
        {
            var resp = ZennoPoster.HTTP.Request(method: HttpMethod.GET,
                url: url
            );
            if (!string.IsNullOrEmpty(resp))
                return resp;
            else
            {
                _project.SendErrorToLog($"[GET] Получили пустой ответ при запросе к {url}");
                throw new Exception("Ошибка при запросе.");
            }
        }
        /// <summary>
        /// Приватный метод для POST запроса
        /// </summary>
        /// <param name="url">Целевой URL</param>
        /// <param name="data">Данные</param>
        /// <returns></returns>
        private string ScamPost(string url, string data)
        {
            return ZennoPoster.HTTP.Request(method: HttpMethod.POST,
                url: url,
                content: data,
                contentPostingType: "application/json"
            );
        }

    }

    public class TitaniumInstanceApiResponse
    {
        /// <summary>
        /// Базовый объект ответа о создании нового инстанса
        /// </summary>
        public class NewInstanceBase
        {
            /// <summary>
            /// Удачное ли создание
            /// </summary>
            public bool IsSuccess { get; set; } = false;

        }

        /// <summary>
        /// Объект удачного ответа в случае создания нового инстанса
        /// </summary>
        public class NewInstanceOk : NewInstanceBase
        {
            /// <summary>
            /// Данные нового инстанса
            /// </summary>
            public TitaniumInstance InstanceData { get; set; } = null;
        }

        /// <summary>
        /// Объект неудоачного ответа в случае создания нового инстанса
        /// </summary>
        public class NewInstanceBad : NewInstanceBase
        {
            /// <summary>
            /// Текст ошибки
            /// </summary>
            public string ErrorText { get; set; } = string.Empty;
        }

        /// <summary>
        /// Объект ответа с инстансами
        /// </summary>
        public class GetInstances
        {
            /// <summary>
            /// Эндпоинт инстанса
            /// </summary>
            public string EndPoint { get; set; } = string.Empty;
            /// <summary>
            /// Данные инстанса
            /// </summary>
            public TitaniumInstance Instance { get; set; } = null;
        }

        /// <summary>
        /// Объект с данными трафика
        /// </summary>
        public class GetTrafficResponse
        {
            /// <summary>
            /// Удачный ли запрос
            /// </summary>
            public bool IsSuccess { get; set; } = false;

            /// <summary>
            /// Список с данными инстанса
            /// </summary>
            public List<TrafficRecord> TrafficRecords { get; set; } = new List<TrafficRecord>();
        }

        /// <summary>
        /// Объект ответа при взаимодействии с доменами для изменения
        /// </summary>
        public class AddDomainForChangeResponse
        {
            /// <summary>
            /// Удачное ли добавление
            /// </summary>
            public bool IsSuccess { get; set; } = false;

            /// <summary>
            /// Текст ошибки
            /// </summary>
            public string ErrorText { get; set; } = string.Empty;

            /// <summary>
            /// Перечень активных доменов для подмены
            /// </summary>
            public List<DomainsForReplace> DomainsForReplaceData { get; set; } = new List<DomainsForReplace>();
        }

        /// <summary>
        /// Объект ответа при добавлении данных для подмены в запросе
        /// </summary>
        public class AddChangeBodyResponse
        {
            /// <summary>
            /// Удачное ли добавление
            /// </summary>
            public bool IsSuccess { get; set; } = false;
            /// <summary>
            /// Текст ошибки
            /// </summary>
            public string ErrorText { get; set; } = string.Empty;
            /// <summary>
            /// Перечень всех объектов для подмены
            /// </summary>
            public List<ChangeBody> ChangeBodyArray { get; set; } = new List<ChangeBody>();
        }
    }
    public class DomainsForReplace
    {
        public string DomainRegex { get; set; } = string.Empty;
        public string RegexFromReplace { get; set; } = string.Empty;
        public string TextToReplace { get; set; } = string.Empty;

        /// <summary>
        /// Данные для подмены
        /// </summary>
        /// <param name="domainRegex">Регулярное выражение для определения url, который необходимо заменить</param>
        /// <param name="regexFromReplace">Регулярное выражение для участка строки, который надо подменить</param>
        /// <param name="textToReplace">Текст, на который необходимо заменить участок строки</param>
        public DomainsForReplace(string domainRegex, string regexFromReplace, string textToReplace)
        {
            this.DomainRegex = domainRegex;
            this.RegexFromReplace = regexFromReplace;
            this.TextToReplace = textToReplace;
        }
    }
    public class ChangeBody
    {
        public string DomainRegex { get; set; } = string.Empty;
        public List<string> RegexesFromReplace { get; set; } = new List<string>();
        public List<string> TextToReplace { get; set; } = new List<string>();

        /// <summary>
        /// Объект данных, хранящий в себе данные для подмены запроса или ответа. Количество
        /// </summary>
        /// <param name="domainRegex">Регулярное выражение для поиска URL, к которому необходимо применить изменение</param>
        /// <param name="regexFromReplace">Список регулярных выражений для поиска данных подмены</param>
        /// <param name="textToReplace">Список текстовых значений, на которые необходимо подменить</param>
        public ChangeBody(string domainRegex, IEnumerable<string> regexFromReplace, IEnumerable<string> textToReplace)
        {
            this.DomainRegex = domainRegex;
            this.RegexesFromReplace = regexFromReplace?.ToList() ?? new List<string>();
            this.TextToReplace = textToReplace?.ToList() ?? new List<string>();
        }

    }
    public class TrafficRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RequestUrl { get; set; }
        public string RequestMethod { get; set; }
        public DateTime RequestTime { get; set; }
        public DateTime ResponseTime { get; set; }
        public TimeSpan ResponseDuration { get; set; }
        public int ResponseStatusCode { get; set; }
        public string ResponseStatusDescription { get; set; }

        // Заголовки
        public Dictionary<string, string> RequestHeaders { get; set; }
        public Dictionary<string, string> ResponseHeaders { get; set; }

        // Тела запроса и ответа
        public string RequestBodyRaw { get; set; }
        public string RequestBodyBase64 { get; set; }
        public string ResponseBodyRaw { get; set; }
        public string ResponseBodyBase64 { get; set; }

        // Дополнительная информация
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
        public string ClientIpAddress { get; set; }
    }

    public class TitaniumInstance
    {
        public string ProxyEndpoint { get; set; } = string.Empty;
        public string CertificatePath { get; set; }
        public string CertificatePemData { get; set; }

        private List<DomainsForReplace> DomainsForReplaces { get; set; } = new List<DomainsForReplace>();
        private List<ChangeBody> ChangeRequests { get; set; } = new List<ChangeBody>();
        private List<ChangeBody> ChangeResponses { get; set; } = new List<ChangeBody>();

    }
}
