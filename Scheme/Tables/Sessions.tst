<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="bbd3d574-a33e-49fb-867d-db3c6811365e" Name="Sessions" Group="System">
	<Description>Открытые сессии.</Description>
	<SchemePhysicalColumn ID="5100aae0-3958-4b1a-b135-57b6640ced19" Name="ID" Type="Guid Not Null">
		<Description>Идентификатор сессии.</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="c4bf3534-a57a-4a6f-8568-b537644b6746" Name="Application" Type="Reference(Typified) Not Null" ReferencedTable="b939817b-bc1f-4a9d-87ef-694336870eed" WithForeignKey="false">
		<Description>Приложение, связанное с сессией.</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="c4bf3534-a57a-006f-4000-0537644b6746" Name="ApplicationID" Type="Guid Not Null" ReferencedColumn="ac166b37-85ea-4bef-b0d2-ad3b95f3af69">
			<Description>Идентификатор приложения. Соответствует идентификатору в сессии, а также идентификаторам в классе Applications.</Description>
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="4f26ce31-e8b7-4186-91e2-36f59580e747" Name="LicenseType" Type="Reference(Typified) Not Null" ReferencedTable="bcc286d4-9d77-4750-8084-15417b966528">
		<Description>Тип лицензии.</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="4f26ce31-e8b7-0086-4000-06f59580e747" Name="LicenseTypeID" Type="Int16 Not Null" ReferencedColumn="7b3eaef7-d50b-4240-9fb2-7e9397088add">
			<Description>Идентификатор типа лицензии.</Description>
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="516c37ff-1d0d-4656-be02-e6b6c308536a" Name="LoginType" Type="Reference(Typified) Not Null" ReferencedTable="44a94501-a954-4ab1-a7f8-47eebb2f869b">
		<Description>Тип входа пользователя в систему.</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="516c37ff-1d0d-0056-4000-06b6c308536a" Name="LoginTypeID" Type="Int16 Not Null" ReferencedColumn="19e48b5c-b2fc-4f2a-b36d-90db3f3ae10e" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="70c01692-63f8-4042-b81f-ff67046b3d89" Name="AccessLevel" Type="Reference(Typified) Not Null" ReferencedTable="648381d6-8647-4ec6-87a4-3cbd6bae380c">
		<Description>Уровень доступа пользователя.</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="70c01692-63f8-0042-4000-0f67046b3d89" Name="AccessLevelID" Type="Int16 Not Null" ReferencedColumn="5c20848a-0f1c-49ea-b6c1-454b0702295f" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="3b795c79-9d17-40fd-a3a6-9a4ec85e707f" Name="User" Type="Reference(Typified) Not Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3" WithForeignKey="false">
		<Description>Пользователь, запросивший открытие сессии.

Внешний ключ не используется, чтобы можно было создать сессию, не связанную с действительным сотрудников Tessa,
например, для интеграции со сторонними справочниками сотрудников.</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="3b795c79-9d17-00fd-4000-0a4ec85e707f" Name="UserID" Type="Guid Not Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="6ddb67f4-64d7-47e1-b74d-68b90f482fb3" Name="UserName" Type="String(128) Not Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964">
			<Description>Отображаемое имя пользователя.</Description>
		</SchemeReferencingColumn>
		<SchemeReferencingColumn ID="56e9dc60-85ef-4ec5-8ac4-b17e33371ad4" Name="UserLogin" Type="String(256) Null" ReferencedColumn="c05ae347-af13-468e-aeda-7c6b541c06b6">
			<Description>Логин пользователя или имя доменного аккаунта.</Description>
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="efb42f4f-1040-4192-8807-8daf7c554c96" Name="DeviceType" Type="Reference(Typified) Not Null" ReferencedTable="8b4cd042-334b-4aee-a623-7d8942aa6897">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="efb42f4f-1040-0092-4000-0daf7c554c96" Name="DeviceTypeID" Type="Int16 Not Null" ReferencedColumn="97971f14-45fc-4fd7-9623-17b38b9853f1">
			<SchemeDefaultConstraint IsPermanent="true" ID="255c96ba-b791-4f51-bcaf-2f2f540825d4" Name="df_Sessions_DeviceTypeID" Value="0" />
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="562aeaf5-9094-4714-9a30-aeff4706b6f1" Name="ServiceType" Type="Reference(Typified) Not Null" ReferencedTable="62c1a795-1688-48a1-b0af-d77032c90bab">
		<Description>Типы сессий, которые определяются типом веб-сервиса: для desktop- или для web-клиентов, или веб-сервис отсутствует (прямое взаимодействие с БД).</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="562aeaf5-9094-0014-4000-0eff4706b6f1" Name="ServiceTypeID" Type="Int16 Not Null" ReferencedColumn="bca8f77c-494c-4ba7-b920-32b46d20172e">
			<SchemeDefaultConstraint IsPermanent="true" ID="dac070e5-bd77-4ea2-acc8-feb5eacd7326" Name="df_Sessions_ServiceTypeID" Value="0" />
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="4d347e4e-e464-40a5-8f78-4e04b62dbcbe" Name="HostIP" Type="AnsiString(39) Null">
		<Description>IP компьютера, открывшего сессию. Длина достаточна для того, чтобы вместить IPv6 адрес.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="a574ff6f-3e24-4696-bd0b-a5a256db5864" Name="HostName" Type="String(128) Null">
		<Description>Имя компьютера, открывшего сессию.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="ec3e5fb3-2f82-4fed-9cd3-56299ccc6594" Name="Created" Type="DateTime Not Null">
		<Description>Дата и время открытия сессии.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="9e56f514-df64-4095-99ca-24306fea5027" Name="Expires" Type="DateTime Not Null">
		<Description>Дата и время истечения срока действия сессии.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="c3bef784-d207-4606-bd47-7f157a5a29c9" Name="IsActive" Type="Boolean Not Null">
		<Description>Признак того, что сессия является активной, т.е. обращения к ней не приведут к ошибкам.

Сессия может стать неактивной и перестать использовать лицензию, если лицензия конкурентная, сессия длительное время не использовалась и при этом не была закрыта. В этом случае при открытии новой сессии текущая сессия становится неактивной, а при обращении к текущей сессии будет запрошен возврат лицензии. Если лицензию не получилось вернуть (т.к. количество конкурентных лицензий недостаточно), то сессия закрывается.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="20a0ec37-ed50-42ef-bc41-8d2f56f218ea" Name="LastActivity" Type="DateTime Not Null">
		<Description>Дата и время последнего запроса к серверу, определяющего активность клиента.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="56bfc450-7e9a-48e4-ac12-aec283b5f0f9" Name="Culture" Type="Int32 Not Null">
		<Description>Код LCID для культуры региональных настроек пользователя.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="249ae65e-cbd4-4df1-b40f-f590ae3943a1" Name="UICulture" Type="Int32 Not Null">
		<Description>Код LCID для культуры языка интерфейса.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="e0f4d1f1-b354-4f7d-a060-71daa3f63241" Name="UtcOffsetMinutes" Type="Int32 Not Null">
		<Description>Количество минут для смещения часового пояса пользователя относительно UTC.
Может быть как положительным, так и отрицательным числом или нулём.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="104a6d8f-f075-4550-876f-163a116b8111" Name="OSName" Type="String(128) Null">
		<Description>Имя операционной системы, с которой подключался пользователь в момент создания сессии.
Равно Null, если информация по ОС недоступна.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="e1d7dea9-027a-4629-9737-adb574fbf380" Name="UserAgent" Type="String(512) Null">
		<Description>Строка UserAgent браузера, доступная при подключении web-клиента.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="ca3732e5-f817-44cc-9fa6-dcfc7b3e3521" Name="TimeZoneUtcOffset" Type="Int32 Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="966d65c1-aa46-452c-a39f-badacc4a3d78" Name="df_Sessions_TimeZoneUtcOffset" Value="0" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="64eb386b-19e6-4184-ab05-6e78cbab65fa" Name="Client64Bit" Type="Boolean Null">
		<Description>Признак того, что клиентское приложение является 64-битным. True - 64-битное приложение, False - 32-битное приложение, Null - разрядность неизвестна.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="2120a471-1807-4c9b-a766-97e14f3941b7" Name="Client64BitOS" Type="Boolean Null">
		<Description>Признак того, что операционная система клиента является 64-битной. True - 64-битная ОС, False - 32-битная ОС, Null - разрядность неизвестна.</Description>
	</SchemePhysicalColumn>
	<SchemePrimaryKey ID="6d66b30f-330c-4de3-9dab-07394ae042a0" Name="pk_Sessions" IsClustered="true">
		<SchemeIndexedColumn Column="5100aae0-3958-4b1a-b135-57b6640ced19" />
	</SchemePrimaryKey>
</SchemeTable>