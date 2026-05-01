<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="57e7926b-8fad-4d24-bb1c-9d1d7f36a489" Name="SubStageType" Group="Custom">
	<Description>Этапы при ручном создании маршрутов пользователями</Description>
	<SchemePhysicalColumn ID="9863c446-b64c-4875-b59a-d84508bff7b7" Name="ID" Type="Int16 Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="b6bb08e6-3241-4d98-a734-997c07a55eb0" Name="df_SubStageType_ID" Value="0" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="6a4b1a82-b637-4ea8-82d3-9a49f1005ff4" Name="Name" Type="String(128) Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="534213c9-f2ac-46ec-b150-48d621cde822" Name="df_SubStageType_Name" Value="Согласование" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey ID="1146f1a6-90b5-4fa2-9329-a753b3611dbf" Name="pk_SubStageType">
		<SchemeIndexedColumn Column="9863c446-b64c-4875-b59a-d84508bff7b7" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="9863c446-b64c-4875-b59a-d84508bff7b7">0</ID>
		<Name ID="6a4b1a82-b637-4ea8-82d3-9a49f1005ff4">Согласование</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="9863c446-b64c-4875-b59a-d84508bff7b7">1</ID>
		<Name ID="6a4b1a82-b637-4ea8-82d3-9a49f1005ff4">Утверждение</Name>
	</SchemeRecord>
</SchemeTable>