Установка плагинов Chronos из сборки Tessa.Extensions.Chronos:

1. Убедитесь, что сервис Chronos остановлен.
2. В папке сервиса Chronos скопируйте содержимое папки Plugins\Tessa в новую папку Plugins\Tessa.Extensions.Chronos
3. Удалите подпапку Plugins\Tessa.Extensions.Chronos\configuration.
4. Удалите следующие файлы в папке Plugins\Tessa.Extensions.Chronos: Tessa.Chronos.dll, Tessa.Extensions.Default.Chronos.dll, unoconv.
5. Убедитесь, что в xml-файлах плагина, расположенных в папке Extensions\Tessa.Extensions.Chronos\configuration, требуемые плагины включены, т.е. у них указано disabled="false". В плагине-примере ExamplePlugin по умолчанию указано disable="true", замените его на "false".
6. Соберите проект Tessa.Extensions.Chronos в Visual Studio.
7. После сборки появится папка Bin\Tessa.Extensions.Chronos (относительно папки с файлом .sln), скопируйте её содержимое с заменой в Plugins\Tessa.Extensions.Chronos.
8. Запустите сервис Chronos в окне консоли. Названия ваших плагинов в сборке Tessa.Extensions.Chronos должны быть выведены на экране.

По умолчанию это ExamplePlugin, который также выполняет запись в log.txt на уровне Trace. Вы можете включить этот уровень логирования в NLog.config в папке с Chronos,
заменив строку: <logger name="*" minlevel="Trace" writeTo="file" />

В дальнейшем для обновления плагина достаточно шагов:
1. Остановите сервис Chronos.
2. Соберите проект Tessa.Extensions.Chronos в Visual Studio.
3. Скопируйте содержимое папки Bin\Tessa.Extensions.Chronos с заменой в Plugins\Tessa.Extensions.Chronos.
4. Запустите сервис Chronos.

Копирование можно автоматизировать при сборке, записав скрипт копирования в Extensions\Tessa.Extensions.Chronos\post-build.bat.
