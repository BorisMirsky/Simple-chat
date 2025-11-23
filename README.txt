
Адаптация такого гайда
https://www.youtube.com/watch?v=x4svcLhhu9s


Он ставит 3ю версию tailwindcss, потом init,
но сейчас по дефолту 4я и там по другому - понижать версию либо вообще 
не надо init, я пока что не понял и на этом месте я остановился.


- При создании страниц не было шаблона для .jsx
  Создал как .js, потом добавил, не это ли причина проблем которые ниже?

- Слетела кодировка, много лет такого не было:
            '?' вместо кириллицы (неверно создал .jsx ?)
  Пока не понимаю

- чакра ui: не карточка как в примере и вообще не работает
  Второй раз пытаюсь работать с ч. и снова неудача. Надо другой UI?

- Ошибка при отправке сообщения
StackExchange.Redis.RedisConnectionException: 
'The message timed out in the backlog attempting to send 
because no connection became available (5000ms) 
- Last Connection Exception: 
It was not possible to connect to the redis server(s). 

Решение (?):
 вместо IDistibutedCache взять дефолтный IMemoryCache или ConcurrentDictionary

##################################
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5260",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7083;http://localhost:5260",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
