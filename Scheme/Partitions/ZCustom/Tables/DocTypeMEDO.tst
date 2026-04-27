<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="06684526-4757-45fa-a9b8-bd1598c22d09" Name="DocTypeMEDO" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<Description>Вид документа МЭДО</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="06684526-4757-00fa-2000-0d1598c22d09" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="06684526-4757-01fa-4000-0d1598c22d09" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="5297e59d-0b65-4aa6-a896-78842779205a" Name="Name" Type="String(256) Null" />
	<SchemePhysicalColumn ID="d2459c96-fef7-45ba-89f4-50e36b6f1fd1" Name="Index" Type="String(128) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="06684526-4757-00fa-5000-0d1598c22d09" Name="pk_DocTypeMEDO" IsClustered="true">
		<SchemeIndexedColumn Column="06684526-4757-01fa-4000-0d1598c22d09" />
	</SchemePrimaryKey>
</SchemeTable>