<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="a18478e4-3e6d-4e46-a594-2cd784cadb37" Name="ComissionPerformerBox" Group="Custom">
	<SchemePhysicalColumn ID="8b8c2a61-33af-460b-a5cc-28f60eec8a53" Name="ID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="656e235c-7255-4ac2-81cd-5d2fd41a044b" Name="CardID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="4c829f67-4857-43db-9951-9146b92c83d7" Name="CurrentUserID" Type="Guid Not Null">
		<Description>Идентификатор текущего пользователя</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="9f4f8b2b-28e4-4823-9eb2-90b3a901ced9" Name="LogDate" Type="Date Null">
		<Description>Дата записи для логирования</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="4813b17e-7aa3-4ed4-ba2a-7847776bec4b" Name="Order" Type="Int32 Not Null">
		<Description>Порядок решений в списке</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="9dbeccf6-6ba8-4072-ad86-90d7af1c65ab" Name="df_ComissionPerformerBox_Order" Value="0" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="dc0546e4-270b-4404-8e9e-e35f8e69b6a5" Name="UserID" Type="Guid Null" />
	<SchemePhysicalColumn ID="aea4c6d0-445f-4734-894d-b493a0c0caa6" Name="UserName" Type="String(512) Null" />
	<SchemePhysicalColumn ID="35ac7987-2f61-45ff-9696-f3d1f9882900" Name="IsResponsible" Type="Boolean Null">
		<Description>Статус основного-главного исполнителя</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="ac3f40d5-fe5b-4189-85a1-673cac16612a" Name="df_ComissionPerformerBox_IsResponsible" Value="false" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey ID="1def4cc7-09a4-491c-9ae6-387a35e8ffe6" Name="pk_ComissionPerformerBox">
		<SchemeIndexedColumn Column="8b8c2a61-33af-460b-a5cc-28f60eec8a53" />
	</SchemePrimaryKey>
</SchemeTable>