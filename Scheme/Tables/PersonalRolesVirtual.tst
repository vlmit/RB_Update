<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="e86b07e5-da20-487b-a55e-0ed56606bddf" Name="PersonalRolesVirtual" Group="Roles" IsVirtual="true" InstanceType="Cards" ContentType="Entries">
	<Description>Виртуальные поля для карточки сотрудника.</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="e86b07e5-da20-007b-2000-0ed56606bddf" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="e86b07e5-da20-017b-4000-0ed56606bddf" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="a97a0420-e82e-4908-aa18-fa44be513dd7" Name="Language" Type="Reference(Typified) Null" ReferencedTable="1ed36bf1-2ebf-43da-acb2-1ddb3298dbd8">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="a97a0420-e82e-0008-4000-0a44be513dd7" Name="LanguageID" Type="Int16 Null" ReferencedColumn="f13de4a3-34d7-4e7b-95b6-f34372ed724c" />
		<SchemeReferencingColumn ID="7b694aab-fd14-4ff3-8576-96255289e62b" Name="LanguageCaption" Type="String(256) Null" ReferencedColumn="40a3d47c-40f7-48bd-ab8e-edef2f84094d" />
		<SchemeReferencingColumn ID="a25bc09f-a67c-4112-8ac0-abfbaf9656f3" Name="LanguageCode" Type="AnsiStringFixedLength(2) Null" ReferencedColumn="9e7084bb-c1dc-4ace-90c9-800dbcf7f3c2" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="3ac6b21e-0f15-46c5-a4e2-494c99354399" Name="Password" Type="String(128) Null">
		<Description>Пароль, вводимый сотрудником для типа входа "Пользователь Tessa" или Null, если пароль не задан (для других типов входа).

Физически хранится только хеш от пароля и идентификатора пользователя в поле PersonalRoles.PasswordHash.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="04496793-f41a-4e0e-ba2b-49ffe8abef50" Name="PasswordRepeat" Type="String(128) Null">
		<Description>Повтор пароля, вводимого сотрудником для типа входа "Пользователь Tessa" или Null, если пароль не задан (для других типов входа).
Повтор пароля необходим, что пользователь не ошибся при вводе пароля первый раз.

Физически хранится только хеш от пароля и идентификатора пользователя в поле PersonalRoles.PasswordHash.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="951e2083-a75d-44ad-b3f5-59f49691e9a1" Name="Settings" Type="String(Max) Null">
		<Description>Настройки сотрудника, сериализованные в JSON. Настройки могут быть добавлены в типовом и в проектном решении.</Description>
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="e86b07e5-da20-007b-5000-0ed56606bddf" Name="pk_PersonalRolesVirtual" IsClustered="true">
		<SchemeIndexedColumn Column="e86b07e5-da20-017b-4000-0ed56606bddf" />
	</SchemePrimaryKey>
</SchemeTable>