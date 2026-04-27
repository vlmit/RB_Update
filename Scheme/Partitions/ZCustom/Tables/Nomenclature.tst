<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="27454eac-5316-4d34-baa6-31e0379447f6" Name="Nomenclature" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="27454eac-5316-0034-2000-01e0379447f6" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="27454eac-5316-0134-4000-01e0379447f6" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="5c735299-9018-43ee-a545-6424cfe8fb9b" Name="DepartmentName" Type="String(2048) Null">
		<Description>Название отдела</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="ecbfd5ce-7268-4bfc-98ec-d903d80124a6" Name="Index" Type="String(128) Null">
		<Description>№ дела</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="3b87132b-cdeb-4f31-8ebf-4b60e3b85eb0" Name="Name" Type="String(512) Null">
		<Description>Заголовок номенклатуры</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="d40b2ee8-568f-47af-924b-ca7dce6422bf" Name="Article" Type="String(128) Null">
		<Description>Статья</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="f68f7145-4a21-4be2-b776-3d7f89182f00" Name="Duration" Type="String(128) Null">
		<Description>Срок хранения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="db1d439d-2e90-4233-8127-b2cdc6a027cf" Name="Description" Type="String(512) Null">
		<Description>Описание</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="28042afc-28f3-4e82-8fa7-4b5519f55334" Name="ExternalID" Type="String(128) Null">
		<Description>Поле для указания ID из SP по номенклатуре</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="180b6459-50ad-444c-abae-cab5cc237f95" Name="Year" Type="Int16 Null">
		<Description>Год</Description>
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="27454eac-5316-0034-5000-01e0379447f6" Name="pk_Nomenclature" IsClustered="true">
		<SchemeIndexedColumn Column="27454eac-5316-0134-4000-01e0379447f6" />
	</SchemePrimaryKey>
</SchemeTable>