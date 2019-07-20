using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using Project.DB;
using Project.BL.Models;
using Project.DB.Models;

namespace Project.BL
{
    public class Buisness
    {
        private DataBase _database;
        private User _user;

        private string _errorDisconnect = "Нет подключения к сети!";
        private string _errorLoginOrPassword = "Не правильно введён логин или пароль!";
        private string _errorPasswordsDontMatch = "Пароли не совпадают!";


        public Buisness()
        {
            _database = new DataBase();
        }

        #region Пользователь

        /// <summary>
        /// Авторизация.
        /// </summary>
        /// <param name="error">Ошибка.</param>
        /// <param name="userName">Логин пользователя.</param>
        /// <param name="password">Пароль.</param>
        /// <returns>True, если авторизация прошла успешно.</returns>
        public bool Authorize(out string error, string userName, string password)
        {
            error = "";
            bool result;

            try
            {
                result = _database.Authorize(userName, password);
            }
            catch (WebException ex)
            {
                error = _errorDisconnect;
                return false;
            }


            if (result)
                Protector.SaveData(_database.AccessToken);
            else
                error = _errorLoginOrPassword;

            return result;
        }

        /// <summary>
        /// Аутентификация.
        /// </summary>
        /// <param name="error">Ошибка.</param>
        /// <returns>True, если аутентификация прошла успешно.</returns>
        public bool Authentication(out string error)
        {
            error = "";

            string accessToken = "";

            try
            {
                Protector.GetData(ref accessToken);
            }
            catch (FileNotFoundException ex)
            {
                return false;
            }

            bool result;
            try
            {
                result = _database.Authorize(accessToken);
            }
            catch (WebException ex)
            {
                error = _errorDisconnect;
                return false;
            }

            return result;
        }

        /// <summary>
        /// Получает информацию о пользователе.
        /// </summary>
        /// <param name="error">Ошибка.</param>
        /// <param name="user">Пустой объект типа User.</param>
        /// <returns></returns>
        public HttpStatusCode GetUserInfo(out string error, out User user)
        {
            error = "";
            user = new User();

            HttpResponseMessage response = null;
            try
            {
                response = _database.GetUserInfo(out user);
            }
            // TODO: Проверить работу без подключения к сети.
            catch
            {
                try
                {
                    Protector.GetData(ref user);
                    error = _errorDisconnect;
                    return HttpStatusCode.OK;
                }
                catch (FileNotFoundException fnfex)
                {
                    error = _errorDisconnect;
                    return HttpStatusCode.BadGateway;
                }
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                error = getErrorMessage(response);
                return response.StatusCode;
            }

            // Save
            _user = user;

            Protector.SaveData(user);
            return response.StatusCode;
        }

        /// <summary>
        /// Удаление текущей сессии.
        /// </summary>
        /// <param name="error">Ошибка.</param>
        /// <returns>True, если сессия удалилась</returns>
        public bool Logout(out string error)
        {
            error = "";

            try
            {
                _database.Logout();
            }
            catch (WebException ex)
            {
                error = _errorDisconnect;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Изменение пароля.
        /// </summary>
        /// <param name="oldPassword">Старый пароль.</param>
        /// <param name="newPassword">Новый пароль.</param>
        /// <returns></returns>
        public HttpStatusCode ChangePassword(out string error, string oldPassword, string newPassword, string confirmPassword)
        {
            error = "";

            if (newPassword != confirmPassword)
            {
                error = _errorPasswordsDontMatch;
                return HttpStatusCode.BadRequest;
            }

            HttpResponseMessage response = null;
            try
            {
                response = _database.ChangePassword(oldPassword, newPassword);
            }
            catch (WebException ex)
            {
                return HttpStatusCode.BadGateway;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                error = getErrorMessage(response);
                return response.StatusCode;
            }
            return response.StatusCode;
        }

        #endregion

        #region Расписание

        /// <summary>
        /// Получение расписание всей группы.
        /// </summary>
        /// <param name="error">Ошибка.</param>
        /// <param name="days">Список дней с расписанием.</param>
        /// <param name="group">Номер группы.</param>
        /// <returns></returns>
        public HttpStatusCode GetGroupShedule(out string error, out List<Day> days, string group)
        {
            error = "";
            Shedule shedule = new Shedule();
            HttpResponseMessage response = null;
            days = null;

            Protector.GetData(ref shedule);
            if (shedule.SheduleId == 0)
            {
                try
                {
                    response = _database.GetShedule(out shedule, shedule != null ? shedule.SheduleId : 0);
                }
                catch
                {
                    error = _errorDisconnect;
                    return HttpStatusCode.BadGateway;
                }
            }

            try { days = shedule.SheduleGroups.First(q => q.Name == group).Days.Where(w => w.Pairs.Count != 0).ToList(); }
            catch { return HttpStatusCode.BadGateway; }

            Protector.SaveData(shedule);
            return HttpStatusCode.OK;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error">Ошибка.</param>
        /// <param name="days">Список дней с расписанием.</param>
        /// <returns></returns>
        public HttpStatusCode GetUserShedule(out string error, out List<Day> days, out Shedule _shedule)
        {
            error = "";
            Shedule shedule = new Shedule();
            _shedule = null;
            HttpResponseMessage response = null;
            days = null;

            Protector.GetData(ref shedule);
            if (shedule.SheduleId == 0)
            {
                try
                {
                    response = _database.GetShedule(out shedule, shedule != null ? shedule.SheduleId : 0);
                }
                catch
                {
                    error = _errorDisconnect;
                    return HttpStatusCode.BadGateway;
                }
            }

            try { days = shedule.SheduleGroups.First(q => q.Name == _user.GroupName).Days.Where(w => w.Pairs.Count != 0).ToList(); }
            catch { return HttpStatusCode.BadGateway; }

            _shedule = shedule;
            Protector.SaveData(shedule);
            return HttpStatusCode.OK;
        }

        /// <summary>
        /// Получает изменение в расписании.
        /// </summary>
        /// <param name="pair">Список пар</param>
        /// <returns></returns>
        public HttpStatusCode GetChanges(out string error, out List<Change> pair, out ChangeList changes, string group)
        {
            error = "";
            HttpResponseMessage response = null;
            changes = null;
            pair = null;

            try
            {
                response = _database.GetChanges(out changes);
                pair = changes.Changes.Where(q => q.GroupName == group).ToList();
            }
            // TODO: Проверить работу без подключения к сети.
            catch
            {
                try
                {
                    Protector.GetData(ref changes);
                    error = _errorDisconnect;
                    return HttpStatusCode.OK;
                }
                catch (FileNotFoundException fnfex)
                {
                    error = _errorDisconnect;
                    return HttpStatusCode.BadGateway;
                }
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                error = getErrorMessage(response);
                return response.StatusCode;
            }

            Protector.SaveData(changes);
            return response.StatusCode;
        }

        /// <summary>
        /// Получает изменение в расписании пользователя.
        /// </summary>
        /// <param name="pair">Список пар</param>
        /// <returns></returns>
        public HttpStatusCode GetUserChanges(out string error, out List<Change> pair, out ChangeList changes)
        {
            error = "";
            HttpResponseMessage response = null;
            changes = null;
            pair = null;

            try
            {
                response = _database.GetChanges(out changes);
                pair = changes.Changes.Where(q => q.GroupName == _user.GroupName).ToList();
            }
            // TODO: Проверить работу без подключения к сети.
            catch
            {
                try
                {
                    Protector.GetData(ref changes);
                    error = _errorDisconnect;
                    return HttpStatusCode.OK;
                }
                catch (FileNotFoundException fnfex)
                {
                    error = _errorDisconnect;
                    return HttpStatusCode.BadGateway;
                }
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                error = getErrorMessage(response);
                return response.StatusCode;
            }

            Protector.SaveData(changes);
            return response.StatusCode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        /// <param name="sheduleGroup"></param>
        /// <returns></returns>
        public HttpStatusCode GetGroups(out string error, out List<string> sheduleGroup)
        {
            error = "";
            Shedule shedule = new Shedule();
            HttpResponseMessage response = null;
            sheduleGroup = null;

            try
            {
                response = _database.GetShedule(out shedule, shedule != null ? shedule.SheduleId : 0);
                sheduleGroup = shedule.SheduleGroups.Select(q => q.Name).ToList();
            }
            // TODO: Проверить работу без подключения к сети.
            catch
            {
                error = _errorDisconnect;
                return HttpStatusCode.BadGateway;
            }

            return response.StatusCode;
        }

        #endregion

        #region Домашнее задание

        /// <summary>
        /// Получает всё домашнее задание.
        /// </summary>
        /// <param name="error">Ошибка.</param>
        /// <param name="hometask">Домашнее задание.</param>
        /// <returns></returns>
        public HttpStatusCode GetHometask(out string error, out List<Hometask> hometask)
        {
            error = "";
            HttpResponseMessage response = null;
            hometask = new List<Hometask>();

            var newHometask = new List<Hometask>();

            try
            {
                Protector.GetData(ref hometask);
            }
            catch (FileNotFoundException fnfex)
            {

            }

            try
            {
                response = _database.GetHometasks(out newHometask, (hometask != null && hometask.Count != 0) ? hometask.Select(q => q.HometaskId).Last() : 0);
            }
            // TODO: Проверить работу без подключения к сети.
            catch
            {

            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                error = getErrorMessage(response);
                return response.StatusCode;
            }

            if (hometask == null)
                hometask = new List<Hometask>();
            hometask.AddRange(newHometask);

            Protector.SaveData(hometask);
            return response.StatusCode;
        }

        /// <summary>
        /// Получает список домашних заданий удовлетворяющий параметру
        /// </summary>
        /// <param name="error">Ошибка.</param>
        /// <param name="hometask">Домашнее задание.</param>
        /// <param name="lesson">Предмет.</param>
        /// <returns></returns>
        public HttpStatusCode GetHometask(out string error, List<Hometask> hometask, string lesson)
        {
            error = "";
            HttpResponseMessage response = null;
            hometask = new List<Hometask>();

            try
            {
                response = _database.GetHometasks(out hometask);
                var result = hometask.Where(h => h.Lesson == lesson);
            }
            // TODO: Проверить работу без подключения к сети.
            catch
            {
                try
                {
                    //Protector.GetData(ref hometask);
                    error = _errorDisconnect;
                    return HttpStatusCode.OK;
                }
                catch (FileNotFoundException fnfex)
                {
                    error = _errorDisconnect;
                    return HttpStatusCode.BadGateway;
                }
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                error = getErrorMessage(response);
                return response.StatusCode;
            }

            //Protector.SaveData(hometask);
            return response.StatusCode;
        }

        /// <summary>
        /// Запись домашнего задания.
        /// </summary>
        /// <param name="error">Ошибка.</param>
        /// <param name="lesson">Предмет.</param>
        /// <param name="value">Текст домашнего задания.</param>
        /// <returns></returns>
        public HttpStatusCode PostHometask(out string error, Hometask hometask)
        {
            error = "";

            HttpResponseMessage response = null;

            try
            {
                response = _database.PostHometasks(hometask);
            }
            catch (WebException ex)
            {
                error = _errorDisconnect;
                return HttpStatusCode.BadGateway;
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                error = getErrorMessage(response);
                return response.StatusCode;
            }

            return response.StatusCode;
        }

        /// <summary>
        /// Удаление домашнего задания
        /// </summary>
        /// <param name="error">Ошибка</param>
        /// <param name="hometaskId"></param>
        /// <returns></returns>
        public HttpStatusCode DeleteHometask(out string error, int hometaskId)
        {
            error = "";
        
            HttpResponseMessage response = null;
            try
            {
                response = _database.DeleteHometask(hometaskId);
            }
            catch (WebException ex)
            {
                return HttpStatusCode.BadGateway;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                error = getErrorMessage(response);
                return response.StatusCode;
            }

            return response.StatusCode;
        }

        #endregion

        #region Новости

        /// <summary>
        /// Получение новостей.
        /// </summary>
        /// <param name="error">Ошибка.</param>
        /// <param name="news">Новости.</param>
        /// <returns></returns>
        public HttpStatusCode GetNews(out string error, out List<news> news)
        {
            error = "";
            news = null;
            HttpResponseMessage response = null;

            try
            {
                response = _database.GetNews(out news);
            }
            // TODO: Проверить работу без подключения к сети.
            catch
            {
                try
                {
                    Protector.GetData(ref news);
                    error = _errorDisconnect;
                    return HttpStatusCode.OK;
                }
                catch (FileNotFoundException fnfex)
                {
                    error = _errorDisconnect;
                    return HttpStatusCode.BadGateway;
                }
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                error = getErrorMessage(response);
                return response.StatusCode;
            }

            Protector.SaveData(news);
            return response.StatusCode;
        }

        #endregion

        private string getErrorMessage(HttpResponseMessage response)
        {
            string jsonError = response.Content.ReadAsStringAsync().Result;

            string modelState = "ModelState\":{\"\":[\"";
            string endModelState = ".\"]}}";

            int modelStateIndex = jsonError.IndexOf(modelState);
            if (modelStateIndex > 0)
            {
                int startIndex = modelStateIndex + modelState.Length;
                int count = jsonError.IndexOf(endModelState) - startIndex + 1;
                return jsonError.Substring(startIndex, count);
            }       

            var serverError = JsonConvert.DeserializeObject<ShortError>(jsonError);
            return serverError.Message;
        }
    }
}
