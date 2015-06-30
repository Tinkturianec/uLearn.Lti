# LTI Sample

Это пример реализации ToolProvider-а на C# по стандарту LTI 1.0.

Чтобы встроить в курс на Edx свою нестандартную проверялку задач, нужно сделать следующее:

1. Создать новый веб-сервис (Tool Provider), который будет по запросу edx (или другой так называемый Tool Consumer), 
будет аутентифицировать edx и студента, отдавать html разметку проверялки задач, которую нужно встроить в iFrame edx-а, 
и после взаимодействия со студентом, отдельным запросом отправлять в edx набранные баллы.

2. При создании курса в Edx Studio, чтобы заработала интеграция с TP, нужно:
  1. В настройках курса добавить "lti" в список модулей (Advanced Module List)
  2. В настройках курса добавить client_key и client_secret в список известных (LTI Passports) в формате "id:client_key:client_secret", где id - любая строка, она будет использоваться в настройках модуля.
  3. Добавить модуль LTI в юнит (Advanced -> LTI)
  4. В настройках модуля указать URL ToolProvader-а (LTI URL) и id из п.2.2 (LTI ID)
  5. В настройках модуля указать, что не нужно открывать ответ TP в новом окне/вкладке (Open in New Page = false) и указать, что модуль нужно оценить (Scored = true)

Порядок работы Tool Provider-а:

1. Разработчик ToolProvider-а создает пару client_key и client_secret. Регистрирует их в Edx (п.2.2 выше).
В этом проекте client_key и client_secret хранятся в таблице Consumers и называются ConsumerKey и ConsumerSecret.

2. Браузер студента отправляет запрос к ToolProvider-e с аутентификационной информацией о студенте 
с подписью, сформированной сервером edx-а. 
Tool Provider проверяет подпись (OAuth) edx с использоваием client_key и client_secret 
(проверка реализована в LtiAuthenticationExtensions). 

3. Создание и аутентификация пользователя. 
Если пользователя нет в нашей базе, то он в неё добавляется, ему генерируется имя. 
Далее всё общение проходит с аутентифицированным пользователем (см. SecurityHandler).  
Все это реализовано в пакете LtiLibrary.AspNet.Identity.Owin, но по техническим причинам подключить его не получается.
Поэтому код скопирован из этого пакета в файл SecurityHandler и немного подправлен 
(изменили схему генерации имен пользователей и убраны мешающие зависимости пакета).

4. ToolProvider запоминает URL и ID запроса, по которому нужно отправлять баллы, после взаимодействия с пользователем.

5. В ответ на запрос edx, ToolProvider возвращает HTML (со ссылками на CSS и JS и тп), как это делает обычное веб-приложение.

6. В процессе работы ToolProvider решает зачесть задачу и отправить (или изменить) баллы в edx. (см. HomeController.SubmitScore). 
FullStack инсталляция Edx настроена неправильно — она просит отправить результаты на http**s**://**localhost**, 
при этом edx находится на другой виртульной машине, на которой не настроен https.
Сейчас в коде есть костыль для обхода этого бага.

Ссылки:

1. Настройка LTI Module в Edx:  https://edx-partner-course-staff.readthedocs.org/en/latest/exercises_tools/lti_component.html
2. Сайт автора пакета на C# про LTI: http://andyfmiller.com/
3. Статья про OAuth в LTI: http://andyfmiller.com/2013/02/10/does-lti-use-oauth/
3. стандарт LTI v1.1.1: http://www.imsglobal.org/LTI/v1p1p1/ltiIMGv1p1p1.html
4. LtiLibrary.Core: https://www.nuget.org/packages/LtiLibrary.Core/
5. LtiLibrary.Owin.Security.Lti: https://www.nuget.org/packages/LtiLibrary.Owin.Security.Lti
6. LtiLibrary.AspNet.Identity.Owin: https://github.com/andyfmiller/LtiLibrary.Owin
