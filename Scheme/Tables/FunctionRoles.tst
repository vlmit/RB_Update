<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="a59078ce-8acf-4c45-a49a-503fa88a0580" Name="FunctionRoles" Group="System">
	<Description>Функциональные роли заданий, такие как "автор", "исполнитель", "контролёр" и др.</Description>
	<SchemePhysicalColumn ID="bd4fdcea-8042-488a-94c9-770b49357cfe" Name="ID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="8e682682-9729-4231-8152-046c69337615" Name="Name" Type="String(128) Not Null">
		<Description>Уникальное имя функциональной роли.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="f8b3afc6-cea7-4a98-b907-e716e0a426c6" Name="Caption" Type="String(128) Not Null">
		<Description>Отображаемое название функциональной роли. Может быть строкой локализации $Alias</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="196f68f6-d645-4aab-a016-33078a421fcd" Name="CanBeDeputy" Type="Boolean Not Null">
		<Description>Признак того, что пользователь может входить в функциональную роль как заместитель. В противном случае проверяется только явное включение в роль (RoleUsers.IsDeputy = false).</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="368bd3db-960d-4c66-acc5-4900b53d5518" Name="df_FunctionRoles_CanBeDeputy" Value="true" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey ID="31b5332d-8c51-4a4b-bc67-e86a6aecb411" Name="pk_FunctionRoles">
		<SchemeIndexedColumn Column="bd4fdcea-8042-488a-94c9-770b49357cfe" />
	</SchemePrimaryKey>
	<SchemeIndex ID="78b1e4b7-c323-4fda-951c-4d2c2e8a3970" Name="ndx_FunctionRoles_Name" IsUnique="true">
		<SchemeIndexedColumn Column="8e682682-9729-4231-8152-046c69337615" />
	</SchemeIndex>
	<SchemeRecord>
		<ID ID="bd4fdcea-8042-488a-94c9-770b49357cfe">6bc228a0-e5a2-4f15-bf6d-c8e744533241</ID>
		<Name ID="8e682682-9729-4231-8152-046c69337615">Author</Name>
		<Caption ID="f8b3afc6-cea7-4a98-b907-e716e0a426c6">$Enum_FunctionRoles_Author</Caption>
		<CanBeDeputy ID="196f68f6-d645-4aab-a016-33078a421fcd">true</CanBeDeputy>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="bd4fdcea-8042-488a-94c9-770b49357cfe">f726ab6c-a279-4d79-863a-47253e55ccc1</ID>
		<Name ID="8e682682-9729-4231-8152-046c69337615">Performer</Name>
		<Caption ID="f8b3afc6-cea7-4a98-b907-e716e0a426c6">$Enum_FunctionRoles_Performer</Caption>
		<CanBeDeputy ID="196f68f6-d645-4aab-a016-33078a421fcd">true</CanBeDeputy>
	</SchemeRecord>
</SchemeTable>