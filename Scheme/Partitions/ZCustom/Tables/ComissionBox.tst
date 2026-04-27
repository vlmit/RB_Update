<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="2352d490-e303-408d-8319-6cb10e4f0748" Name="ComissionBox" Group="Custom">
	<SchemePhysicalColumn ID="b510bf4b-6dcc-46c3-be69-399fe4d33eaf" Name="ID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="bb440918-1320-4d93-8bc8-4cfb20f388c7" Name="CardID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="481709f0-2ae0-437c-a1ef-fa3e67ac3e8b" Name="CurrentUserID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="870d01d8-b0f4-4210-a833-f25d3de72bd4" Name="ComText" Type="String(Max) Null">
		<Description>Текст поручения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="38835fa1-e9dd-48cf-9209-b55a032ce43a" Name="ComDate" Type="Date Null">
		<Description>Срок поручения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="cd71c4ab-d4cd-40f0-88f7-076490d94012" Name="ControllerUserID" Type="Guid Null" />
	<SchemePhysicalColumn ID="7c0d9466-eae7-43ca-a897-63cc75fc331a" Name="ControllerUserName" Type="String(512) Null" />
	<SchemePhysicalColumn ID="4190ab7f-0df6-4d45-a393-a7bb83c274e9" Name="LogDate" Type="Date Null" />
	<SchemePhysicalColumn ID="f1944506-f3b4-4f97-99bd-8bce3e58d67d" Name="ParentTaskCardID" Type="Guid Null" />
	<SchemePrimaryKey ID="d9ac30c5-df16-4f45-b56e-5ca8555cea4d" Name="pk_ComissionBox">
		<SchemeIndexedColumn Column="b510bf4b-6dcc-46c3-be69-399fe4d33eaf" />
	</SchemePrimaryKey>
</SchemeTable>