using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Project.DB.Models;

namespace Project.DB
{
    /// <summary>
    /// Класс для доступа к данным.
    /// </summary>
    public class DataBase
    {

        private string _appPath = "https://makeeducationbetter.ru";
        //private string _appPath = "http://localhost:49460";

        private string _accessToken;

        /// <summary>
        /// Получает ключ доступа к API.
        /// </summary>
        /// <value>
        /// Ключ доступа.
        /// </value>
        public string AccessToken
        {
            get { return _accessToken; }
            private set { _accessToken = value; }
        }

        #region Пользователь

        /// <summary>
        /// Проверяет данные пользователя.
        /// </summary>
        /// <param name="user">Данные пользователя.</param>
        /// <returns>Ответ от сервера.</returns>
        public HttpResponseMessage CheckUserInfo(User user)
        {
            using (var client = new HttpClient())
            {
                var response = client.PostAsJsonAsync(_appPath + "/api/Account/CheckUserInfo", user).Result;
                return response;
            }
        }

        /// <summary>
        /// Регистрирует пользователя.
        /// </summary>
        /// <param name="user">Данные пользователя.</param>
        /// <param name="password">Пароль пользователя длиною больше 6 символов.</param>
        /// <returns>Ответ от сервера.</returns>
        public HttpResponseMessage RegisterUser(User user, string password)
        {
            var registerModel = new
            {
                Login = user.Login,
                Name = user.Name,
                Surname = user.SurName,
                Group = user.GroupName,
                MiddleName = user.MiddleName,
                Password = password,
                ConfirmPassword = password
            };

            using (var client = new HttpClient())
            {
                var response = client.PostAsJsonAsync(_appPath + "/api/Account/Register", registerModel).Result;
                return response;
            }
        }

        /// <summary>
        /// Авторизация.
        /// </summary>
        /// <param name="userName">Логин пользователя.</param>
        /// <param name="password">Пароль.</param>
        /// <returns>True, если авторизация прошла успешно.</returns>
        public bool Authorize(string userName, string password)
        {
            var pairs = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>( "grant_type", "password" ),
                    new KeyValuePair<string, string>( "username", userName ),
                    new KeyValuePair<string, string> ( "Password", password )
                };
            var content = new FormUrlEncodedContent(pairs);

            using (var client = new HttpClient())
            {
                var response =
                  client.PostAsync(_appPath + "/Token", content).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    return false;

                var result = response.Content.ReadAsStringAsync().Result;

                // Десериализация полученного JSON-объекта
                Dictionary<string, string> tokenDictionary =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

                _accessToken = tokenDictionary["access_token"];
                return true;
            }
        }

        /// <summary>
        /// Авторизация по ключу доступа.
        /// </summary>
        /// <param name="accessToken">Ключ доступа.</param>
        /// <returns>True, если авторизация прошла успешно.</returns>
        public bool Authorize(string accessToken)
        {
            using (var client = createClient(accessToken))
            {
                var response = client.GetAsync(_appPath + "/api/Account/UserInfo").Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _accessToken = accessToken;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Получает информацию о пользователе.
        /// </summary>
        /// <param name="user">Пустой объект типа User.</param>
        /// <returns>Ответ от сервера.</returns>
        public HttpResponseMessage GetUserInfo(out User user)
        {
            using (var client = createClient(_accessToken))
            {
                var response = client.GetAsync(_appPath + "/api/Account/UserInfo").Result;
                var result = response.Content.ReadAsStringAsync().Result;
                user = JsonConvert.DeserializeObject<User>(result);
                return response;
            }
        }

        /// <summary>
        /// Удаление текущей сессии.
        /// </summary>
        public void Logout()
        {
            using (var client = createClient(_accessToken))
            {
                var response = client.GetAsync(_appPath + "/api/Account/Logout").Result;
                _accessToken = "";
            }
        }

        /// <summary>
        /// Изменение пароля.
        /// </summary>
        /// <param name="oldPassword">Старый пароль.</param>
        /// <param name="newPassword">Новый пароль.</param>
        /// <returns></returns>
        public HttpResponseMessage ChangePassword(string oldPassword, string newPassword)
        {
            var changePasswordModel = new
            {
                OldPassword = oldPassword,
                NewPassword = newPassword,
                ConfirmPassword = newPassword
            };
            using (var client = createClient(_accessToken))
            {
                var response = client.PostAsJsonAsync(_appPath + "/api/Account/ChangePassword", changePasswordModel).Result;

                return response;
            }
        }

        #endregion

        #region Расписание

        /// <summary>
        /// Получает всё расписание.
        /// </summary>
        /// <param name="shedule">Расписание.</param>
        /// <returns>Ответ от сервера.</returns>
        public HttpResponseMessage GetShedule(out Shedule shedule, int lastId = 0)
        {
            using (var client = createClient(_accessToken))
            {
                var response = client.GetAsync(_appPath + "/api/Shedule?lastId=" + lastId.ToString()).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                shedule = JsonConvert.DeserializeObject<Shedule>(result);
                return response;
            }
        }

        /// <summary>
        /// Получает изменение в расписании.
        /// </summary>
        /// <param name="pairs">Список пар</param>
        /// <returns>Ответ от сервера.</returns>
        public HttpResponseMessage GetChanges(out ChangeList pairs)
        {
            using (var client = createClient(_accessToken))
            {
                var response = client.GetAsync(_appPath + "/api/Changes").Result;
                var result = response.Content.ReadAsStringAsync().Result;
                pairs = JsonConvert.DeserializeObject<ChangeList>(result);
                return response;
            }
        }

        #endregion

        #region Домашнее задание

        /// <summary>
        /// Получает всё доступное пользователю расписание.
        /// </summary>
        /// <param name="hometask">The hometask.</param>
        /// <returns></returns>
        public HttpResponseMessage GetHometasks(out List<Hometask> hometask, int lastId = 0)
        {
            using (var client = createClient(_accessToken))
            {
                var response = client.GetAsync(_appPath + "/api/Hometasks?lastId=" + lastId.ToString()).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                hometask = JsonConvert.DeserializeObject<List<Hometask>>(result);

                return response;
            }
        }

        /// <summary>
        /// Запись домашнего задания.
        /// </summary>
        /// <param name="hometask">Домашнее задание.</param>
        /// <returns>Ответ от сервера.</returns>
        public HttpResponseMessage PostHometasks(Hometask hometask)
        {
            using (var client = createClient(_accessToken))
            {
                var response = client.PostAsJsonAsync(_appPath + "/api/Hometasks", hometask).Result;
                return response;
            }
        }

        /// <summary>
        /// Удаление домашнего задания
        /// </summary>
        /// <param name="hometaskId"></param>
        /// <returns></returns>
        public HttpResponseMessage DeleteHometask(int hometaskId)
        {
            using (var client = createClient(_accessToken))
            {
                var response = client.DeleteAsync(_appPath + "/api/Hometasks/" + hometaskId.ToString()).Result;
                return response;
            }
        }

        #endregion

        #region Новости

        /// <summary>
        /// Получение новостей.
        /// </summary>
        /// <param name="news">Новости.</param>
        /// <returns>Ответ от сервера.</returns>
        public HttpResponseMessage GetNews(out List<news> news)
        {
            using (var client = createClient(_accessToken))
            {
                var response = client.GetAsync(_appPath + "/api/News").Result;
                var result = response.Content.ReadAsStringAsync().Result;
                news = JsonConvert.DeserializeObject<List<news>>(result);
                return response;
            }
        }

        #endregion

        #region Настройки

        public HttpResponseMessage PostNotificationKey(NotificationKey notificationKey)
        {
            using (var client = createClient(_accessToken))
            {
                var response = client.PostAsJsonAsync(_appPath + "/api/Settings/NotificationKey", notificationKey).Result;

                return response;
            }
        }

        public HttpResponseMessage PostVkApiKey(string key)
        {
            using (var client = createClient(_accessToken))
            {
                var response = client.GetAsync(_appPath + "/api/Settings/VkApiKey?key=" + key).Result;

                return response;
            }
        }
        #endregion

        /// <summary>
        /// HTTP-клиент с ключом доступа.
        /// </summary>
        /// <param name="accessToken">Ключ доступа.</param>
        /// <returns>HTTP-клиент.</returns>
        private HttpClient createClient(string accessToken = "")
        {
            var client = new HttpClient();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                client.DefaultRequestHeaders.Authorization =
                  new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }
            return client;
        }
    }
}
