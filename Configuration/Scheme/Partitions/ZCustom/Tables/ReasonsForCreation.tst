<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="8e00fba3-f00a-4755-9689-a28d56e27999" Name="ReasonsForCreation" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<Description>Причины создания</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="8e00fba3-f00a-0055-2000-028d56e27999" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="8e00fba3-f00a-0155-4000-028d56e27999" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="190d6c43-8194-4a53-bc3d-a0b6afcd374a" Name="Name" Type="String(Max) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="8e00fba3-f00a-0055-5000-028d56e27999" Name="pk_ReasonsForCreation" IsClustered="true">
		<SchemeIndexedColumn Column="8e00fba3-f00a-0155-4000-028d56e27999" />
	</SchemePrimaryKey>
</SchemeTable>